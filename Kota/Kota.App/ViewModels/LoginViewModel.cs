using System.Windows.Input;
using System.IO;
using Kota.Services.Authentication;
using Kota.App.Services;
using Kota.Data;

namespace Kota.App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(
            IAuthService authService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;

            LoginCommand = new RelayCommand(async () => await LoginAsync(), () => CanLogin);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool CanLogin => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;

        public ICommand LoginCommand { get; }

        public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Database is already initialized at app startup, just authenticate
                var user = await _authService.AuthenticateAsync(Username, Password);
                if (user == null)
                {
                    ErrorMessage = "Invalid username or password.";
                    return;
                }

                _currentUserService.SetCurrentUser(user);

                var requiresPasswordChange = await _authService.IsDefaultPasswordAsync(Username);

                LoginSuccess?.Invoke(this, new LoginSuccessEventArgs(user, requiresPasswordChange));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class LoginSuccessEventArgs : EventArgs
    {
        public LoginSuccessEventArgs(Domain.Entities.User user, bool requiresPasswordChange)
        {
            User = user;
            RequiresPasswordChange = requiresPasswordChange;
        }

        public Domain.Entities.User User { get; }
        public bool RequiresPasswordChange { get; }
    }
}