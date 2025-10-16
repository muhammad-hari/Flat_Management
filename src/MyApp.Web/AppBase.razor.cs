using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class AppBase : ComponentBase
{
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }

    protected string Username { get; private set; } = "";
    protected string Role { get; private set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthenticationAsync();
    }

    protected async Task CheckAuthenticationAsync(string requiredRole = null)
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated ?? false)
        {
            Username = user.Identity.Name;
            Role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No Role";

            if (!string.IsNullOrEmpty(requiredRole) && Role != requiredRole)
            {
                Navigation.NavigateTo("/forbidden", true);
            }
        }
        else
        {
            Username = "Guest";
            Navigation.NavigateTo("/login", true);
        }
    }
}
