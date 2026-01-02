using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using OramaGo.Models;
using OramaGo.Services;
using System.ComponentModel.DataAnnotations;

namespace OramaGo.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly LoginRequestValidator _validator;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string senha = string.Empty;

    [ObservableProperty]
    private bool lembrarMe;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string emailError = string.Empty;

    [ObservableProperty]
    private bool hasEmailError;

    [ObservableProperty]
    private string senhaError = string.Empty;

    [ObservableProperty]
    private bool hasSenhaError;

    public bool IsNotBusy => !IsBusy;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _validator = new LoginRequestValidator();
        
        Title = "Login";
    }

    public override async Task InitializeAsync()
    {
        // Verificar se já está autenticado
        if (await _authService.IsAuthenticatedAsync())
        {
            await NavigateToDashboard();
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            ClearErrors();
            SetBusy(true);

            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Senha = Senha,
                LembrarMe = LembrarMe
            };

            // Validar dados
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                ShowValidationErrors(validationResult.Errors);
                return;
            }

            // Fazer login
            var response = await _authService.LoginAsync(request);

            if (response.Sucesso)
            {
                await NavigateToDashboard();
            }
            else
            {
                ShowError(response.Erro ?? "Erro desconhecido ao fazer login");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erro interno: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task FillAdminCredentialsAsync()
    {
        Email = "admin@orama.com.br";
        Senha = "Admin@123";
        LembrarMe = true;
    }

    [RelayCommand]
    private async Task FillVendedorCredentialsAsync()
    {
        Email = "vendedor@orama.com.br";
        Senha = "123456";
        LembrarMe = true;
    }

    private async Task NavigateToDashboard()
    {
        await _navigationService.NavigateToAsync("//dashboard");
    }

    private void ClearErrors()
    {
        HasError = false;
        ErrorMessage = string.Empty;
        HasEmailError = false;
        EmailError = string.Empty;
        HasSenhaError = false;
        SenhaError = string.Empty;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ShowValidationErrors(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        foreach (var error in errors)
        {
            switch (error.PropertyName)
            {
                case nameof(LoginRequest.Email):
                    EmailError = error.ErrorMessage;
                    HasEmailError = true;
                    break;
                case nameof(LoginRequest.Senha):
                    SenhaError = error.ErrorMessage;
                    HasSenhaError = true;
                    break;
            }
        }
    }

    protected new void SetBusy(bool busy, string? loadingMessage = null)
    {
        base.SetBusy(busy, loadingMessage);
        OnPropertyChanged(nameof(IsNotBusy));
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres");
    }
}