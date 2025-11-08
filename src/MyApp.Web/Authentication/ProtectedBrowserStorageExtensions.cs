using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MyApp.Web.Authentication
{
    public static class ProtectedBrowserStorageExtensions
    {
        public static bool IsPrerendering(this ProtectedBrowserStorage storage)
        {
            try
            {
                // During prerendering, accessing JSRuntime will throw an exception
                // We can use this to detect if we're prerendering
                storage.GetAsync<string>("test").GetAwaiter().GetResult();
                return false;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering"))
            {
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}