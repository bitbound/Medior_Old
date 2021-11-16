using Medior.BaseTypes;
using Medior.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private bool _isLoggedIn;
        private string _username;

        public DashboardViewModel(IAuthService authService)
        {
            _authService = authService;
            _isLoggedIn = authService.IsSignedIn;
            _username = authService.Username;

            // TODO: Add email claim and use that.
            // TODO: Use Messenger to update these values.
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public async Task LogIn(IntPtr windowHandle)
        {
            await _authService.SignInInteractive(windowHandle);
        }

        public async Task LogOut()
        {
            await _authService.SignOut();
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string AccountButtonContent
        {
            get => IsLoggedIn ? Username : "Logged Out";
        }
    }
}
