/**
 * JavaScript para funcionalidades de clientes
 * Inclui máscaras, validações e integrações com APIs externas
 */

// Validação de CPF (função global)
function validarCPF(cpf) {
    cpf = cpf.replace(/\D/g, '');
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;
    
    let soma = 0;
    for (let i = 0; i < 9; i++) {
        soma += parseInt(cpf.charAt(i)) * (10 - i);
    }
    let resto = 11 - (soma % 11);
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.charAt(9))) return false;
    
    soma = 0;
    for (let i = 0; i < 10; i++) {
        soma += parseInt(cpf.charAt(i)) * (11 - i);
    }
    resto = 11 - (soma % 11);
    if (resto === 10 || resto === 11) resto = 0;
    return resto === parseInt(cpf.charAt(10));
}

// Validação de CNPJ (função global)
function validarCNPJ(cnpj) {
    cnpj = cnpj.replace(/\D/g, '');
    if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) return false;
    
    let tamanho = cnpj.length - 2;
    let numeros = cnpj.substring(0, tamanho);
    let digitos = cnpj.substring(tamanho);
    let soma = 0;
    let pos = tamanho - 7;
    
    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }
    
    let resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    if (resultado !== parseInt(digitos.charAt(0))) return false;
    
    tamanho = tamanho + 1;
    numeros = cnpj.substring(0, tamanho);
    soma = 0;
    pos = tamanho - 7;
    
    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }
    
    resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    return resultado === parseInt(digitos.charAt(1));
}

// Função para mostrar toast/notificação (global)
function showToast(message, type) {
    type = type || 'info';
    
    if (!$('#toastContainer').length) {
        $('body').append('<div id="toastContainer" class="position-fixed top-0 end-0 p-3" style="z-index: 9999;"></div>');
    }
    
    var toastId = 'toast-' + Date.now();
    var bgClass = type === 'success' ? 'bg-success' : 
                  type === 'warning' ? 'bg-warning' : 
                  type === 'error' ? 'bg-danger' : 'bg-info';
    
    var icon = type === 'success' ? 'check-circle' : 
               type === 'warning' ? 'exclamation-triangle' : 
               type === 'error' ? 'times-circle' : 'info-circle';
    
    var toastHtml = '<div id="' + toastId + '" class="toast ' + bgClass + ' text-white" role="alert">' +
                    '<div class="toast-body"><i class="fas fa-' + icon + ' me-2"></i>' + message + '</div></div>';
    
    $('#toastContainer').append(toastHtml);
    
    var toastElement = new bootstrap.Toast(document.getElementById(toastId), { delay: 4000 });
    toastElement.show();
    
    document.getElementById(toastId).addEventListener('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Função para buscar dados do CNPJ (global)
function buscarDadosCNPJ(cnpj) {
    var cnpjLimpo = cnpj.replace(/\D/g, '');
    
    if (cnpjLimpo.length !== 14) {
        showToast('CNPJ deve ter 14 dígitos!', 'error');
        return;
    }
    
    var btnBuscar = $('#btnConsultarCnpj');
    var iconOriginal = btnBuscar.html();
    
    btnBuscar.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');
    $('#Nome').val('Buscando dados...');
    
    $.getJSON('/api/cnpj/' + cnpjLimpo, function(data) {
        console.log('Dados recebidos da API:', data);
        
        if (data.status === 'OK') {
            $('#Nome').val(data.nome || '');
            $('#NomeFantasia').val(data.fantasia || '');
            $('#Email').val(data.email || '');
            $('#Telefone').val(data.telefone || '');
            
            if (data.cep) {
                var cepLimpo = data.cep.replace(/\D/g, '');
                var cepFormatado = cepLimpo.replace(/(\d{5})(\d{3})/, '$1-$2');
                $('#cep, #Cep').val(cepFormatado);
            }
            
            $('#Logradouro').val(data.logradouro || '');
            $('#Numero').val(data.numero || '');
            $('#Complemento').val(data.complemento || '');
            $('#Bairro').val(data.bairro || '');
            $('#Cidade').val(data.municipio || '');
            $('#Uf').val(data.uf || '');
            
            showToast('Dados do CNPJ carregados com sucesso!', 'success');
            
            if (data.cep && (!data.logradouro || !data.bairro || !data.municipio)) {
                buscarEnderecoPorCep(data.cep);
            }
        } else {
            $('#Nome').val('');
            showToast('CNPJ não encontrado na Receita Federal!', 'warning');
        }
    }).fail(function(xhr) {
        $('#Nome').val('');
        
        var mensagem = 'Erro ao consultar CNPJ!';
        if (xhr.status === 429) {
            mensagem = 'Muitas consultas. Tente novamente em alguns segundos.';
        } else if (xhr.status === 404) {
            mensagem = 'CNPJ não encontrado!';
        } else if (xhr.responseJSON && xhr.responseJSON.erro) {
            mensagem = xhr.responseJSON.erro;
        }
        
        showToast(mensagem, 'error');
        console.error('Erro na consulta CNPJ:', xhr);
    }).always(function() {
        btnBuscar.prop('disabled', false).html(iconOriginal);
    });
}

// Função para buscar endereço por CEP (global)
function buscarEnderecoPorCep(cep) {
    var cepLimpo = cep.replace(/\D/g, '');
    
    if (cepLimpo.length === 8) {
        $.getJSON('https://viacep.com.br/ws/' + cepLimpo + '/json/', function(data) {
            if (!data.erro) {
                if (!$('#Logradouro').val()) $('#Logradouro').val(data.logradouro || '');
                if (!$('#Bairro').val()) $('#Bairro').val(data.bairro || '');
                if (!$('#Cidade').val()) $('#Cidade').val(data.localidade || '');
                if (!$('#Uf').val()) $('#Uf').val(data.uf || '');
            }
        });
    }
}

$(document).ready(function() {
    // Máscaras
    $('#cep, #Cep').mask('00000-000');
    $('#Telefone').mask('(00) 0000-0000');
    $('#Celular').mask('(00) 00000-0000');
    $('#Uf').mask('AA');
    
    // Controle de tipo de pessoa
    $('#tipoPessoa').change(function() {
        var tipoPessoa = $(this).val();
        var isJuridica = tipoPessoa == '2';
        
        if (isJuridica) {
            $('#divNomeFantasia').show();
            $('#btnConsultarCnpj').show();
            $('#labelCpfCnpj').text('CNPJ');
            $('#labelNome').text('Razão Social');
            $('#labelRgIe').text('Inscrição Estadual');
            $('#cpfCnpj').unmask().mask('00.000.000/0000-00');
            $('#cpfCnpj').attr('placeholder', '00.000.000/0000-00');
        } else if (tipoPessoa == '1') {
            $('#divNomeFantasia').hide();
            $('#NomeFantasia').val('');
            $('#btnConsultarCnpj').hide();
            $('#labelCpfCnpj').text('CPF');
            $('#labelNome').text('Nome Completo');
            $('#labelRgIe').text('RG');
            $('#cpfCnpj').unmask().mask('000.000.000-00');
            $('#cpfCnpj').attr('placeholder', '000.000.000-00');
        } else {
            $('#divNomeFantasia').hide();
            $('#btnConsultarCnpj').hide();
            $('#labelCpfCnpj').text('CPF/CNPJ');
            $('#labelNome').text('Nome/Razão Social');
            $('#labelRgIe').text('RG/IE');
            $('#cpfCnpj').unmask();
        }
    });
    
    // Botão para buscar CNPJ manualmente
    $('#btnConsultarCnpj').click(function() {
        var cnpj = $('#cpfCnpj').val();
        var cnpjLimpo = cnpj.replace(/\D/g, '');
        
        console.log('Botão buscar clicado. CNPJ:', cnpj, 'Limpo:', cnpjLimpo);
        
        if (cnpjLimpo.length === 14) {
            if (validarCNPJ(cnpjLimpo)) {
                buscarDadosCNPJ(cnpjLimpo);
            } else {
                showToast('CNPJ inválido! Verifique os dígitos.', 'error');
            }
        } else {
            showToast('Digite um CNPJ completo (14 dígitos)!', 'warning');
        }
    });
    
    // Inicializar tipo de pessoa
    $('#tipoPessoa').trigger('change');
    
    // Formatação automática ao digitar/colar
    $('#cpfCnpj').on('input', function() {
        var valor = $(this).val().replace(/\D/g, '');
        
        // Auto-detectar tipo baseado no tamanho
        if (valor.length > 11) {
            if ($('#tipoPessoa').val() != '2') {
                $('#tipoPessoa').val('2').trigger('change');
            }
        }
    });
    
    // Validar ao sair do campo
    $('#cpfCnpj').blur(function() {
        var valor = $(this).val();
        var valorLimpo = valor.replace(/\D/g, '');
        var tipoPessoa = $('#tipoPessoa').val();
        
        if (!valorLimpo) return;
        
        console.log('Blur - Valor:', valor, 'Limpo:', valorLimpo, 'Tamanho:', valorLimpo.length);
        
        var valido = false;
        
        if (valorLimpo.length === 11) {
            // CPF
            var cpfFormatado = valorLimpo.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
            $(this).val(cpfFormatado);
            valido = validarCPF(valorLimpo);
            
            if (!valido) {
                $(this).addClass('is-invalid').removeClass('is-valid');
                showToast('CPF inválido!', 'error');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
            }
        } else if (valorLimpo.length === 14) {
            // CNPJ
            var cnpjFormatado = valorLimpo.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
            $(this).val(cnpjFormatado);
            valido = validarCNPJ(valorLimpo);
            
            console.log('Validação CNPJ:', valorLimpo, 'Resultado:', valido);
            
            if (!valido) {
                $(this).addClass('is-invalid').removeClass('is-valid');
                showToast('CNPJ inválido!', 'error');
            } else {
                $(this).removeClass('is-invalid').addClass('is-valid');
                // Buscar dados automaticamente
                if (tipoPessoa == '2') {
                    buscarDadosCNPJ(valorLimpo);
                }
            }
        } else if (valorLimpo.length > 0) {
            $(this).addClass('is-invalid').removeClass('is-valid');
            showToast('CPF deve ter 11 dígitos ou CNPJ deve ter 14 dígitos!', 'error');
        }
    });
    
    // Busca CEP
    $('#cep, #Cep').blur(function() {
        var cep = $(this).val().replace(/\D/g, '');
        
        if (cep.length === 8) {
            $('#Logradouro, #Bairro, #Cidade, #Uf').val('');
            $('#Logradouro').val('Buscando...');
            
            $.getJSON('https://viacep.com.br/ws/' + cep + '/json/', function(data) {
                if (!data.erro) {
                    $('#Logradouro').val(data.logradouro);
                    $('#Bairro').val(data.bairro);
                    $('#Cidade').val(data.localidade);
                    $('#Uf').val(data.uf);
                    $('#Numero').focus();
                    showToast('Endereço encontrado!', 'success');
                } else {
                    $('#Logradouro').val('');
                    showToast('CEP não encontrado!', 'warning');
                }
            }).fail(function() {
                $('#Logradouro').val('');
                showToast('Erro ao buscar CEP!', 'error');
            });
        }
    });
    
    // Botão de buscar CEP
    $('#btnConsultarCep').click(function() {
        $('#cep, #Cep').trigger('blur');
    });
});
