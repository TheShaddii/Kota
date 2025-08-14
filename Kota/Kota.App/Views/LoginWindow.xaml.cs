using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Kota.App.ViewModels;

namespace Kota.App
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            // Set default values for testing
            UsernameTextBox.Text = "admin";
            PasswordBox.Password = "ChangeMe!123";
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show loading
                LoadingTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = false;

                // Get services and open main window
                var app = (App)Application.Current;
                var mainWindow = app.Host.Services.GetRequiredService<MainWindow>();

                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Login failed: {ex.Message}";
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingTextBlock.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = true;
            }
        }
    }
}