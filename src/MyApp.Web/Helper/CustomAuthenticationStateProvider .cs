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
            var result = await _sessionStorage.GetAsync<string>("username");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, result.Value)
                }, "apiauth_type");

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
        }
        catch
        {
            // gagal baca session, fallback ke anonymous
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
