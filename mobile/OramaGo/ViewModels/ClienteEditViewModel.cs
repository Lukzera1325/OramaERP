using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using OramaGo.Models;
using OramaGo.Services;

namespace OramaGo.ViewModels;

[QueryProperty(nameof(ClienteId), "ClienteId")]
[QueryProperty(nameof(IsNew), "IsNew")]
public partial class ClienteEditViewModel : BaseViewModel
{
    private readonly IClienteService _clienteService;
    private readonly INavigationService _navigationService;
    private readonly IPermissionService _permissionService;
    private readonly ClienteLocalValidator _validator;

    [ObservableProperty]
    private int clienteId;

    [ObservableProperty]
    private bool isNew = true;

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string cpfCnpj = string.Empty;

    [ObservableProperty]
    private string rgIe = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string telefone = string.Empty;

    [ObservableProperty]
    private string celular = string.Empty;

    [ObservableProperty]
    private string cep = string.Empty;

    [ObservableProperty]
    private string endereco = string.Empty;

    [ObservableProperty]
    private string numero = string.Empty;

    [ObservableProperty]
    private string complemento = string.Empty;

    [ObservableProperty]
    private string bairro = string.Empty;

    [ObservableProperty]
    private string cidade = string.Empty;

    [ObservableProperty]
    private string estado = string.Empty;

    [ObservableProperty]
    private DateTime? dataNascimento;

    [ObservableProperty]
    private decimal limiteCredito;

    [ObservableProperty]
    private bool bloqueado;

    [ObservableProperty]
    private string motivoBloqueio = string.Empty;

    [ObservableProperty]
    private string observacoes = string.Empty;

    [ObservableProperty]
    private bool hasValidationErrors;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    public ClienteEditViewModel(
        IClienteService clienteService,
        INavigationService navigationService,
        IPermissionService permissionService)
    {
        _clienteService = clienteService;
        _navigationService = navigationService;
        _permissionService = permissionService;
        _validator = new ClienteLocalValidator();
        
        Title = "Novo Cliente";
    }

    public override async Task InitializeAsync()
    {
        if (!IsNew && ClienteId > 0)
        {
            await LoadClienteAsync();
        }
        
        Title = IsNew ? "Novo Cliente" : "Editar Cliente";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            ClearValidationErrors();
            SetBusy(true);

            var cliente = CreateClienteFromForm();
            
            // Validar dados
            var validationResult = await _validator.ValidateAsync(cliente);
            if (!validationResult.IsValid)
            {
                ShowValidationErrors(validationResult.Errors.First().ErrorMessage);
                return;
            }

            // Salvar cliente
            if (IsNew)
            {
                await _clienteService.CreateAsync(cliente);
                await _navigationService.DisplayAlertAsync("Sucesso", "Cliente criado com sucesso!", "OK");
            }
            else
            {
                cliente.Id = ClienteId;
                await _clienteService.UpdateAsync(cliente);
                await _navigationService.DisplayAlertAsync("Sucesso", "Cliente atualizado com sucesso!", "OK");
            }

            await _navigationService.NavigateBackAsync();
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao salvar cliente: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        var hasChanges = !string.IsNullOrEmpty(Nome) || !string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(CpfCnpj);
        
        if (hasChanges)
        {
            var confirm = await _navigationService.DisplayConfirmAsync(
                "Cancelar",
                "Deseja realmente cancelar? As alterações serão perdidas.",
                "Sim",
                "Não");

            if (!confirm) return;
        }

        await _navigationService.NavigateBackAsync();
    }

    [RelayCommand]
    private async Task SearchCepAsync()
    {
        if (string.IsNullOrWhiteSpace(Cep) || Cep.Length < 8) return;

        try
        {
            SetBusy(true);
            
            // TODO: Implementar busca de CEP via API quando disponível
            // Por enquanto, simular busca
            await Task.Delay(1000);
            
            await _navigationService.DisplayAlertAsync("Info", "Funcionalidade de busca de CEP será implementada em versão futura", "OK");
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadClienteAsync()
    {
        try
        {
            SetBusy(true);

            var cliente = await _clienteService.GetByIdAsync(ClienteId);
            if (cliente != null)
            {
                Nome = cliente.Nome;
                CpfCnpj = cliente.CpfCnpj ?? string.Empty;
                RgIe = cliente.RgIe ?? string.Empty;
                Email = cliente.Email ?? string.Empty;
                Telefone = cliente.Telefone ?? string.Empty;
                Celular = cliente.Celular ?? string.Empty;
                Cep = cliente.Cep ?? string.Empty;
                Endereco = cliente.Endereco ?? string.Empty;
                Numero = cliente.Numero ?? string.Empty;
                Complemento = cliente.Complemento ?? string.Empty;
                Bairro = cliente.Bairro ?? string.Empty;
                Cidade = cliente.Cidade ?? string.Empty;
                Estado = cliente.Estado ?? string.Empty;
                DataNascimento = cliente.DataNascimento;
                LimiteCredito = cliente.LimiteCredito;
                Bloqueado = cliente.Bloqueado;
                MotivoBloqueio = cliente.MotivoBloqueio ?? string.Empty;
                Observacoes = cliente.Observacoes ?? string.Empty;
            }
            else
            {
                await _navigationService.DisplayAlertAsync("Erro", "Cliente não encontrado", "OK");
                await _navigationService.NavigateBackAsync();
            }
        }
        catch (Exception ex)
        {
            await _navigationService.DisplayAlertAsync("Erro", $"Erro ao carregar cliente: {ex.Message}", "OK");
            await _navigationService.NavigateBackAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private ClienteLocal CreateClienteFromForm()
    {
        return new ClienteLocal
        {
            Nome = Nome.Trim(),
            CpfCnpj = CpfCnpj.Trim(),
            RgIe = RgIe.Trim(),
            Email = Email.Trim(),
            Telefone = Telefone.Trim(),
            Celular = Celular.Trim(),
            Cep = Cep.Trim(),
            Endereco = Endereco.Trim(),
            Numero = Numero.Trim(),
            Complemento = Complemento.Trim(),
            Bairro = Bairro.Trim(),
            Cidade = Cidade.Trim(),
            Estado = Estado.Trim(),
            DataNascimento = DataNascimento,
            LimiteCredito = LimiteCredito,
            Bloqueado = Bloqueado,
            MotivoBloqueio = MotivoBloqueio.Trim(),
            Observacoes = Observacoes.Trim()
        };
    }

    private void ClearValidationErrors()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
    }

    private void ShowValidationErrors(string message)
    {
        ValidationMessage = message;
        HasValidationErrors = true;
    }

    partial void OnClienteIdChanged(int value)
    {
        if (!IsNew && value > 0)
        {
            Task.Run(async () => await LoadClienteAsync());
        }
    }
}

public class ClienteLocalValidator : AbstractValidator<ClienteLocal>
{
    public ClienteLocalValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.CpfCnpj)
            .MaximumLength(20).WithMessage("CPF/CNPJ deve ter no máximo 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.CpfCnpj));

        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito deve ser maior ou igual a zero");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Celular)
            .MaximumLength(20).WithMessage("Celular deve ter no máximo 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Celular));

        RuleFor(x => x.Cep)
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Cep));
    }
}