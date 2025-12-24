/**
 * Orama ERP - JavaScript Principal
 * Funcionalidades gerais do sistema
 */

// Aguarda o carregamento completo da página
document.addEventListener('DOMContentLoaded', function() {
    
    // Inicializar tooltips do Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Inicializar popovers do Bootstrap
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts após 5 segundos
    setTimeout(function() {
        var alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(function(alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Adicionar classe fade-in aos cards
    var cards = document.querySelectorAll('.card');
    cards.forEach(function(card, index) {
        setTimeout(function() {
            card.classList.add('fade-in');
        }, index * 100);
    });

    // Confirmar exclusões
    var deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            var message = this.getAttribute('data-confirm-delete') || 'Tem certeza que deseja excluir este item?';
            if (confirm(message)) {
                window.location.href = this.href;
            }
        });
    });

    // Máscara para CNPJ
    var cnpjInputs = document.querySelectorAll('input[data-mask="cnpj"]');
    cnpjInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            var value = e.target.value.replace(/\D/g, '');
            value = value.replace(/^(\d{2})(\d)/, '$1.$2');
            value = value.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3');
            value = value.replace(/\.(\d{3})(\d)/, '.$1/$2');
            value = value.replace(/(\d{4})(\d)/, '$1-$2');
            e.target.value = value;
        });
    });

    // Máscara para CPF
    var cpfInputs = document.querySelectorAll('input[data-mask="cpf"]');
    cpfInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            var value = e.target.value.replace(/\D/g, '');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
            e.target.value = value;
        });
    });

    // Máscara para CEP
    var cepInputs = document.querySelectorAll('input[data-mask="cep"]');
    cepInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            var value = e.target.value.replace(/\D/g, '');
            value = value.replace(/^(\d{5})(\d)/, '$1-$2');
            e.target.value = value;
        });
    });

    // Máscara para telefone
    var phoneInputs = document.querySelectorAll('input[data-mask="phone"]');
    phoneInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            var value = e.target.value.replace(/\D/g, '');
            if (value.length <= 10) {
                value = value.replace(/(\d{2})(\d)/, '($1) $2');
                value = value.replace(/(\d{4})(\d)/, '$1-$2');
            } else {
                value = value.replace(/(\d{2})(\d)/, '($1) $2');
                value = value.replace(/(\d{5})(\d)/, '$1-$2');
            }
            e.target.value = value;
        });
    });

    // Formatação de moeda
    var currencyInputs = document.querySelectorAll('input[data-mask="currency"]');
    currencyInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            var value = e.target.value.replace(/\D/g, '');
            value = (value / 100).toFixed(2) + '';
            value = value.replace(".", ",");
            value = value.replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
            e.target.value = 'R$ ' + value;
        });
    });
});

/**
 * Funções utilitárias
 */

// Mostrar loading
function showLoading(element) {
    if (element) {
        element.innerHTML = '<div class="spinner-santorini mx-auto"></div>';
        element.disabled = true;
    }
}

// Esconder loading
function hideLoading(element, originalText) {
    if (element) {
        element.innerHTML = originalText;
        element.disabled = false;
    }
}

// Validar CNPJ
function validarCNPJ(cnpj) {
    cnpj = cnpj.replace(/[^\d]+/g, '');
    
    if (cnpj.length !== 14) return false;
    
    // Elimina CNPJs inválidos conhecidos
    if (cnpj === "00000000000000" || 
        cnpj === "11111111111111" || 
        cnpj === "22222222222222" || 
        cnpj === "33333333333333" || 
        cnpj === "44444444444444" || 
        cnpj === "55555555555555" || 
        cnpj === "66666666666666" || 
        cnpj === "77777777777777" || 
        cnpj === "88888888888888" || 
        cnpj === "99999999999999")
        return false;
    
    // Valida DVs
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
    if (resultado != digitos.charAt(1)) return false;
    
    return true;
}

// Validar CPF
function validarCPF(cpf) {
    cpf = cpf.replace(/[^\d]+/g, '');
    if (cpf.length !== 11 || !!cpf.match(/(\d)\1{10}/)) return false;
    cpf = cpf.split('').map(el => +el);
    const rest = (count) => (cpf.slice(0, count-12)
        .reduce((soma, el, index) => (soma + el * (count-index)), 0)*10) % 11 % 10;
    return rest(10) === cpf[9] && rest(11) === cpf[10];
}

// Consultar CEP
async function consultarCEP(cep, callback) {
    cep = cep.replace(/\D/g, '');
    
    if (cep.length !== 8) {
        alert('CEP deve conter 8 dígitos');
        return;
    }
    
    try {
        const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        const data = await response.json();
        
        if (data.erro) {
            alert('CEP não encontrado');
            return;
        }
        
        if (callback) {
            callback(data);
        }
    } catch (error) {
        console.error('Erro ao consultar CEP:', error);
        alert('Erro ao consultar CEP. Tente novamente.');
    }
}

// Função para consultar CNPJ (será implementada com a API)
async function consultarCNPJ(cnpj, callback) {
    cnpj = cnpj.replace(/\D/g, '');
    
    if (!validarCNPJ(cnpj)) {
        alert('CNPJ inválido');
        return;
    }
    
    // TODO: Implementar consulta à API da Receita Federal
    // Por enquanto, apenas um placeholder
    console.log('Consultando CNPJ:', cnpj);
    
    if (callback) {
        // Dados fictícios para demonstração
        callback({
            cnpj: cnpj,
            razao_social: 'Empresa Exemplo LTDA',
            nome_fantasia: 'Empresa Exemplo',
            logradouro: 'Rua das Flores, 123',
            bairro: 'Centro',
            municipio: 'São Paulo',
            uf: 'SP',
            cep: '01234-567'
        });
    }
}