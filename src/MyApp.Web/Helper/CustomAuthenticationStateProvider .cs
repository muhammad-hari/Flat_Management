using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var usernameResult = await _sessionStorage.GetAsync<string>("username");
            var loginTimeResult = await _sessionStorage.GetAsync<DateTime>("loginTime");

            if (usernameResult.Success && !string.IsNullOrEmpty(usernameResult.Value))
            {
                var loginTime = loginTimeResult.Value;
                // cek expired (misal 30 menit)
                if ((DateTime.UtcNow - loginTime).TotalMinutes > 30)
                {
                    // session expired
                    await _sessionStorage.DeleteAsync("username");
                    await _sessionStorage.DeleteAsync("role");
                    await _sessionStorage.DeleteAsync("loginTime");
                    return new AuthenticationState(_anonymous);
                }

                var roleResult = await _sessionStorage.GetAsync<string>("role");
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usernameResult.Value),
                    new Claim(ClaimTypes.Role, roleResult.Value ?? "Anonymous")
                }, "apiauth_type");

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
        }
        catch
        {
            // fallback ke anonymousreturn new AuthenticationState(_anonymous);
            return new AuthenticationState(_anonymous);
        }

        return new AuthenticationState(_anonymous);
    }


    public async Task NotifyUserAuthentication(string username)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username)
        }, "apiauth_type");

        var user = new ClaimsPrincipal(identity);

        await _sessionStorage.SetAsync("username", username);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogout()
    {
        await _sessionStorage.DeleteAsync("username");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
