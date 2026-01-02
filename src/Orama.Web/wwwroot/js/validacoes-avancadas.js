// Validações avançadas para formulários do ERP Orama
$(document).ready(function () {
    // Aplicar máscaras automaticamente
    aplicarMascaras();

    // Validações em tempo real
    configurarValidacoesTempoReal();

    // Validações de negócio
    configurarValidacoesNegocio();
});

// Aplicar máscaras nos campos
function aplicarMascaras() {
    // CPF/CNPJ dinâmico
    $('input[data-mask="cpf-cnpj"]').on('input', function () {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length <= 11) {
            $(this).mask('000.000.000-00');
        } else {
            $(this).mask('00.000.000/0000-00');
        }
    });

    // Telefone dinâmico
    $('input[data-mask="telefone"]').on('input', function () {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length <= 10) {
            $(this).mask('(00) 0000-0000');
        } else {
            $(this).mask('(00) 00000-0000');
        }
    });

    // CEP
    $('input[data-mask="cep"]').mask('00000-000');

    // Moeda
    $('input[data-mask="money"]').mask('#.##0,00', {
        reverse: true,
        translation: {
            '#': { pattern: /[0-9]/ }
        }
    });

    // Percentual
    $('input[data-mask="percent"]').mask('##0,00%', {
        reverse: true,
        translation: {
            '#': { pattern: /[0-9]/ }
        }
    });
}

// Validações em tempo real
function configurarValidacoesTempoReal() {
    // Validação de email
    $('input[type="email"]').on('blur', function () {
        var email = $(this).val();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (email && !emailRegex.test(email)) {
            mostrarErroValidacao($(this), 'Email inválido');
        } else {
            limparErroValidacao($(this));
        }
    });

    // Validação de CPF/CNPJ
    $('input[data-validate="cpf-cnpj"]').on('blur', function () {
        var valor = $(this).val().replace(/\D/g, '');
        var valido = false;

        if (valor.length === 11) {
            valido = validarCPF(valor);
        } else if (valor.length === 14) {
            valido = validarCNPJ(valor);
        }

        if (valor && !valido) {
            mostrarErroValidacao($(this), 'CPF/CNPJ inválido');
        } else {
            limparErroValidacao($(this));
        }
    });

    // Validação de valores monetários
    $('input[data-validate="money"]').on('blur', function () {
        var valor = parseFloat($(this).val().replace(/[^\d,]/g, '').replace(',', '.'));
        var min = parseFloat($(this).data('min')) || 0;
        var max = parseFloat($(this).data('max')) || Infinity;

        if (isNaN(valor) || valor < min || valor > max) {
            mostrarErroValidacao($(this), `Valor deve estar entre ${min.toFixed(2)} e ${max.toFixed(2)}`);
        } else {
            limparErroValidacao($(this));
        }
    });
}

// Validações de negócio
function configurarValidacoesNegocio() {
    // Validar estoque suficiente em vendas
    $('.quantidade-venda').on('change', function () {
        var quantidade = parseFloat($(this).val()) || 0;
        var estoqueDisponivel = parseFloat($(this).data('estoque')) || 0;

        if (quantidade > estoqueDisponivel) {
            mostrarErroValidacao($(this), `Estoque insuficiente. Disponível: ${estoqueDisponivel}`);
        } else {
            limparErroValidacao($(this));
        }
    });

    // Validar datas de vencimento
    $('input[data-validate="data-futura"]').on('change', function () {
        var dataInformada = new Date($(this).val());
        var hoje = new Date();
        hoje.setHours(0, 0, 0, 0);

        if (dataInformada < hoje) {
            mostrarErroValidacao($(this), 'Data deve ser futura');
        } else {
            limparErroValidacao($(this));
        }
    });

    // Validar margem de lucro
    $('.preco-custo, .preco-venda').on('change', function () {
        var container = $(this).closest('.produto-precos');
        var precoCusto = parseFloat(container.find('.preco-custo').val()) || 0;
        var precoVenda = parseFloat(container.find('.preco-venda').val()) || 0;

        if (precoCusto > 0 && precoVenda > 0) {
            var margem = ((precoVenda - precoCusto) / precoVenda) * 100;
            var margemMinima = 10; // 10% mínimo

            if (margem < margemMinima) {
                mostrarAvisoValidacao(container.find('.preco-venda'),
                    `Margem baixa: ${margem.toFixed(1)}%. Recomendado: mín. ${margemMinima}%`);
            } else {
                limparErroValidacao(container.find('.preco-venda'));
            }
        }
    });
}

// Validar CPF
function validarCPF(cpf) {
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;

    var soma = 0;
    for (var i = 0; i < 9; i++) {
        soma += parseInt(cpf.charAt(i)) * (10 - i);
    }
    var resto = 11 - (soma % 11);
    var digito1 = resto < 2 ? 0 : resto;

    if (parseInt(cpf.charAt(9)) !== digito1) return false;

    soma = 0;
    for (var i = 0; i < 10; i++) {
        soma += parseInt(cpf.charAt(i)) * (11 - i);
    }
    resto = 11 - (soma % 11);
    var digito2 = resto < 2 ? 0 : resto;

    return parseInt(cpf.charAt(10)) === digito2;
}

// Validar CNPJ
function validarCNPJ(cnpj) {
    if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) return false;

    var tamanho = cnpj.length - 2;
    var numeros = cnpj.substring(0, tamanho);
    var digitos = cnpj.substring(tamanho);
    var soma = 0;
    var pos = tamanho - 7;

    for (var i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    var resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    if (resultado != digitos.charAt(0)) return false;

    tamanho = tamanho + 1;
    numeros = cnpj.substring(0, tamanho);
    soma = 0;
    pos = tamanho - 7;

    for (var i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
    return resultado == digitos.charAt(1);
}

// Mostrar erro de validação
function mostrarErroValidacao(elemento, mensagem) {
    elemento.addClass('is-invalid');
    elemento.removeClass('is-valid');

    var feedback = elemento.siblings('.invalid-feedback');
    if (feedback.length === 0) {
        elemento.after(`<div class="invalid-feedback">${mensagem}</div>`);
    } else {
        feedback.text(mensagem);
    }
}

// Mostrar aviso de validação
function mostrarAvisoValidacao(elemento, mensagem) {
    elemento.addClass('is-warning');
    elemento.removeClass('is-invalid is-valid');

    var feedback = elemento.siblings('.warning-feedback');
    if (feedback.length === 0) {
        elemento.after(`<div class="warning-feedback text-warning small">${mensagem}</div>`);
    } else {
        feedback.text(mensagem);
    }
}

// Limpar erro de validação
function limparErroValidacao(elemento) {
    elemento.removeClass('is-invalid is-warning');
    elemento.addClass('is-valid');
    elemento.siblings('.invalid-feedback, .warning-feedback').remove();
}

// Validação antes do submit
function validarFormulario(form) {
    var valido = true;

    // Validar campos obrigatórios
    $(form).find('[required]').each(function () {
        if (!$(this).val()) {
            mostrarErroValidacao($(this), 'Campo obrigatório');
            valido = false;
        }
    });

    // Validar campos com erro
    if ($(form).find('.is-invalid').length > 0) {
        valido = false;
    }

    if (!valido) {
        Swal.fire({
            icon: 'error',
            title: 'Formulário Inválido',
            text: 'Por favor, corrija os erros antes de continuar.',
            confirmButtonText: 'OK'
        });
    }

    return valido;
}

// Auto-completar endereço por CEP
function buscarEnderecoPorCEP(cep, callback) {
    cep = cep.replace(/\D/g, '');

    if (cep.length === 8) {
        $.getJSON(`https://viacep.com.br/ws/${cep}/json/`, function (data) {
            if (!data.erro) {
                callback(data);
            }
        });
    }
}

// Configurar busca de CEP
$(document).on('blur', 'input[data-cep]', function () {
    var cep = $(this).val();
    var form = $(this).closest('form');

    buscarEnderecoPorCEP(cep, function (endereco) {
        form.find('input[name*="Logradouro"], input[name*="Endereco"]').val(endereco.logradouro);
        form.find('input[name*="Bairro"]').val(endereco.bairro);
        form.find('input[name*="Cidade"]').val(endereco.localidade);
        form.find('input[name*="Uf"], select[name*="Uf"]').val(endereco.uf);
    });
});