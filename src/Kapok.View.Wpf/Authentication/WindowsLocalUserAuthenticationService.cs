using System.Security.Principal;
using Res = Kapok.View.Wpf.Resources.Authentication.WindowsLocalUserAuthenticationService;

namespace Kapok.Acl.Windows;

public sealed class WindowsLocalUserAuthenticationService : ISilentAuthenticationService, IAuthenticationService
{
    public string ProviderName => "WindowsLocalUser";

    /// <summary>
    /// Login via the current windows user.
    /// </summary>
    /// <returns>
    /// Returns the login provider specific key for the user logged in.
    ///
    /// The return value is null when the user is an anonymous.
    /// </returns>
    public Task Login()
    {
        UserName = null;
        UserAccountId = null;

        var currentUser = WindowsIdentity.GetCurrent();

        if (currentUser.IsSystem)
            throw new UnauthorizedAccessException(Res.Login_SystemUsersNotAllowed);

        // skip all users or states which are not allowed for a login
        if (currentUser.IsAnonymous)
            return Task.CompletedTask;

        // Guest users are always recognized as anonymous users.
        if (currentUser.IsGuest)
            return Task.CompletedTask;

        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException(Res.Login_NoUserLoggedIn);

        if (currentUser.User == null)
            throw new UnauthorizedAccessException(Res.Login_FailedToIdentifyCurrentUser);

        UserName = currentUser.Name;
        UserAccountId = currentUser.User.ToString();

        return Task.CompletedTask;
    }

    public string? UserName { get; private set; }

    public string? UserAccountId { get; private set; }

    #region IAuthenticationService

    string? IAuthenticationService.UserEmail => null;

    Task IAuthenticationService.Logout()
    {
        return Task.CompletedTask;
    }

    #endregion

    #region ISilentAuthenticationService

    async Task<bool> ISilentAuthenticationService.SilentLogin()
    {
        try
        {
            await Login();
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return true;
    }

    #endregion
}