// IngilizceProje/MAUIClient/ViewModels/LoginViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IngilizceProje.Application.DTOs;
using IngilizceProjeMAUI.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace IngilizceProjeMAUI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe = true;

        [ObservableProperty]
        private bool _isRegisterMode;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var remember = Preferences.Get("remember_me", true);
                RememberMe = remember;
                if (remember)
                {
                    var savedEmail = Preferences.Get("saved_email", string.Empty);
                    var savedPassword = Preferences.Get("saved_password", string.Empty);

                    if (string.IsNullOrEmpty(savedEmail))
                    {
                        try { savedEmail = await SecureStorage.GetAsync("saved_email"); } catch { }
                    }
                    if (string.IsNullOrEmpty(savedPassword))
                    {
                        try { savedPassword = await SecureStorage.GetAsync("saved_password"); } catch { }
                    }

                    if (!string.IsNullOrEmpty(savedEmail)) Email = savedEmail;
                    if (!string.IsNullOrEmpty(savedPassword)) Password = savedPassword;
                }
            }
            catch
            {
                // Fallback
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void ToggleRememberMe()
        {
            RememberMe = !RememberMe;
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        public async Task QuickDemoLoginAsync()
        {
            Email = "demo";
            Password = "Demo123!";
            IsRegisterMode = false;
            await AuthenticateAsync();
        }

        [RelayCommand]
        private async Task AuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Lütfen e-posta/kullanıcı adı ve şifre alanlarını doldurun.";
                return;
            }

            if (IsRegisterMode && string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage = "Kayıt için kullanıcı adı gereklidir.";
                return;
            }

            if (IsRegisterMode && Password.Trim().Length < 3)
            {
                ErrorMessage = "Şifreniz en az 3 karakter olmalıdır.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (IsRegisterMode)
                {
                    var regResult = await _apiService.RegisterWithDetailsAsync(new RegisterDto
                    {
                        Email = Email.Trim(),
                        UserName = UserName.Trim(),
                        Password = Password.Trim()
                    });

                    if (regResult.Success)
                    {
                        // Auto-login after registration
                        var loginIdentifier = !string.IsNullOrWhiteSpace(UserName) ? UserName.Trim() : Email.Trim();
                        var loginResult = await _apiService.LoginWithDetailsAsync(new LoginDto
                        {
                            EmailOrUserName = loginIdentifier,
                            Password = Password.Trim()
                        });

                        if (loginResult.Success)
                        {
                            await SaveOrClearRememberMeCredentialsAsync();
                            await Shell.Current.GoToAsync("//MainPage");
                        }
                        else
                        {
                            IsRegisterMode = false;
                            ErrorMessage = "Kayıt başarılı! Lütfen belirlediğiniz şifre ile giriş yapın.";
                        }
                    }
                    else
                    {
                        ErrorMessage = regResult.Message;
                    }
                }
                else
                {
                    var loginResult = await _apiService.LoginWithDetailsAsync(new LoginDto
                    {
                        EmailOrUserName = Email.Trim(),
                        Password = Password.Trim()
                    });

                    if (loginResult.Success)
                    {
                        await SaveOrClearRememberMeCredentialsAsync();
                        await Shell.Current.GoToAsync("//MainPage");
                    }
                    else
                    {
                        ErrorMessage = loginResult.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Bir hata oluştu: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveOrClearRememberMeCredentialsAsync()
        {
            try
            {
                if (RememberMe)
                {
                    Preferences.Set("remember_me", true);
                    if (!string.IsNullOrWhiteSpace(Email))
                    {
                        Preferences.Set("saved_email", Email.Trim());
                        try { await SecureStorage.SetAsync("saved_email", Email.Trim()); } catch { }
                    }
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        Preferences.Set("saved_password", Password.Trim());
                        try { await SecureStorage.SetAsync("saved_password", Password.Trim()); } catch { }
                    }
                }
                else
                {
                    Preferences.Set("remember_me", false);
                    Preferences.Remove("saved_email");
                    Preferences.Remove("saved_password");
                    try
                    {
                        SecureStorage.Remove("saved_email");
                        SecureStorage.Remove("saved_password");
                    }
                    catch { }
                }
            }
            catch
            {
                // Ignore storage exceptions
            }
            await Task.CompletedTask;
        }
    }
}
