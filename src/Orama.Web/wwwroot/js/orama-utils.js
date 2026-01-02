// Utilitários do ERP Orama
$(document).ready(function () {
    // Configurar auto-complete para produtos
    configurarAutoCompleteProdutos();

    // Configurar auto-complete para clientes
    configurarAutoCompleteClientes();

    // Configurar auto-complete para fornecedores
    configurarAutoCompleteFornecedores();

    // Configurar busca de CEP
    configurarBuscaCEP();

    // Configurar validações
    configurarValidacoes();

    // Configurar cálculos automáticos
    configurarCalculosAutomaticos();
});

// Auto-complete para produtos
function configurarAutoCompleteProdutos() {
    $('.produto-autocomplete').each(function () {
        var input = $(this);
        var container = input.closest('.produto-container');

        input.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Utils/BuscarProdutos',
                    data: { termo: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return {
                                label: item.codigo + ' - ' + item.nome,
                                value: item.nome,
                                produto: item
                            };
                        }));
                    }
                });
            },
            minLength: 2,
            select: function (_, ui) {
                var produto = ui.item.produto;

                // Preencher campos relacionados
                container.find('.produto-id').val(produto.id);
                container.find('.produto-codigo').val(produto.codigo);
                container.find('.produto-nome').val(produto.nome);
                container.find('.preco-unitario').val(produto.precoVenda.toFixed(2));
                container.find('.estoque-disponivel').text(produto.estoqueAtual + ' ' + produto.unidade);

                // Atualizar cálculos
                calcularTotalItem(container);
            }
        });
    });
}

// Auto-complete para clientes
function configurarAutoCompleteClientes() {
    $('.cliente-autocomplete').each(function () {
        var input = $(this);
        var container = input.closest('.cliente-container');

        input.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Utils/BuscarClientes',
                    data: { termo: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return {
                                label: item.nome + ' - ' + item.cpfCnpj,
                                value: item.nome,
                                cliente: item
                            };
                        }));
                    }
                });
            },
            minLength: 2,
            select: function (_, ui) {
                var cliente = ui.item.cliente;

                // Preencher campos relacionados
                container.find('.cliente-id').val(cliente.id);
                container.find('.cliente-nome').val(cliente.nome);
                container.find('.cliente-cpfcnpj').val(cliente.cpfCnpj);
                container.find('.cliente-email').val(cliente.email);
                container.find('.cliente-telefone').val(cliente.telefone);
            }
        });
    });
}

// Auto-complete para fornecedores
function configurarAutoCompleteFornecedores() {
    $('.fornecedor-autocomplete').each(function () {
        var input = $(this);
        var container = input.closest('.fornecedor-container');

        input.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Utils/BuscarFornecedores',
                    data: { termo: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return {
                                label: item.nome + ' - ' + item.cnpj,
                                value: item.nome,
                                fornecedor: item
                            };
                        }));
                    }
                });
            },
            minLength: 2,
            select: function (_, ui) {
                var fornecedor = ui.item.fornecedor;

                // Preencher campos relacionados
                container.find('.fornecedor-id').val(fornecedor.id);
                container.find('.fornecedor-nome').val(fornecedor.nome);
                container.find('.fornecedor-cnpj').val(fornecedor.cnpj);
                container.find('.fornecedor-email').val(fornecedor.email);
                container.find('.fornecedor-telefone').val(fornecedor.telefone);
            }
        });
    });
}

// Busca de CEP
function configurarBuscaCEP() {
    $(document).on('blur', '.cep-input', function () {
        var cep = $(this).val().replace(/\D/g, '');
        var container = $(this).closest('.endereco-container');

        if (cep.length === 8) {
            // Mostrar loading
            container.find('.cep-loading').show();

            $.ajax({
                url: '/Utils/BuscarCep',
                data: { cep: cep },
                success: function (data) {
                    if (data && !data.erro) {
                        container.find('.logradouro-input').val(data.logradouro);
                        container.find('.bairro-input').val(data.bairro);
                        container.find('.cidade-input').val(data.localidade);
                        container.find('.uf-input').val(data.uf);

                        // Focar no número
                        container.find('.numero-input').focus();
                    } else {
                        mostrarAlerta('CEP não encontrado', 'warning');
                    }
                },
                error: function () {
                    mostrarAlerta('Erro ao buscar CEP', 'error');
                },
                complete: function () {
                    container.find('.cep-loading').hide();
                }
            });
        }
    });
}

// Validações
function configurarValidacoes() {
    // Validação de CPF
    $(document).on('blur', '.cpf-input', function () {
        var cpf = $(this).val();
        var input = $(this);

        if (cpf) {
            $.ajax({
                url: '/Utils/ValidarCpf',
                data: { cpf: cpf },
                success: function (data) {
                    if (data.valido) {
                        input.removeClass('is-invalid').addClass('is-valid');
                        input.siblings('.invalid-feedback').hide();
                    } else {
                        input.removeClass('is-valid').addClass('is-invalid');
                        input.siblings('.invalid-feedback').text(data.mensagem).show();
                    }
                }
            });
        }
    });

    // Validação de CNPJ
    $(document).on('blur', '.cnpj-input', function () {
        var cnpj = $(this).val();
        var input = $(this);

        if (cnpj) {
            $.ajax({
                url: '/Utils/ValidarCnpj',
                data: { cnpj: cnpj },
                success: function (data) {
                    if (data.valido) {
                        input.removeClass('is-invalid').addClass('is-valid');
                        input.siblings('.invalid-feedback').hide();
                    } else {
                        input.removeClass('is-valid').addClass('is-invalid');
                        input.siblings('.invalid-feedback').text(data.mensagem).show();
                    }
                }
            });
        }
    });
}

// Cálculos automáticos
function configurarCalculosAutomaticos() {
    // Calcular total do item quando quantidade ou preço mudar
    $(document).on('change', '.quantidade-input, .preco-unitario-input, .desconto-input', function () {
        var container = $(this).closest('.item-container');
        calcularTotalItem(container);
        calcularTotalGeral();
    });

    // Verificar estoque quando quantidade mudar
    $(document).on('change', '.quantidade-input', function () {
        var container = $(this).closest('.item-container');
        var produtoId = container.find('.produto-id').val();
        var quantidade = parseFloat($(this).val()) || 0;

        if (produtoId && quantidade > 0) {
            verificarEstoque(produtoId, quantidade, container);
        }
    });
}

// Calcular total do item
function calcularTotalItem(container) {
    var quantidade = parseFloat(container.find('.quantidade-input').val()) || 0;
    var precoUnitario = parseFloat(container.find('.preco-unitario-input').val()) || 0;
    var percentualDesconto = parseFloat(container.find('.desconto-input').val()) || 0;

    var subtotal = quantidade * precoUnitario;
    var desconto = subtotal * (percentualDesconto / 100);
    var total = subtotal - desconto;

    container.find('.subtotal-item').text(subtotal.toFixed(2));
    container.find('.desconto-item').text(desconto.toFixed(2));
    container.find('.total-item').text(total.toFixed(2));

    return { subtotal, desconto, total };
}

// Calcular total geral
function calcularTotalGeral() {
    var itens = [];

    $('.item-container').each(function () {
        var container = $(this);
        var quantidade = parseFloat(container.find('.quantidade-input').val()) || 0;
        var precoUnitario = parseFloat(container.find('.preco-unitario-input').val()) || 0;
        var percentualDesconto = parseFloat(container.find('.desconto-input').val()) || 0;

        if (quantidade > 0 && precoUnitario > 0) {
            itens.push({
                quantidade: quantidade,
                precoUnitario: precoUnitario,
                percentualDesconto: percentualDesconto
            });
        }
    });

    if (itens.length > 0) {
        $.ajax({
            url: '/Utils/CalcularTotais',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ itens: itens }),
            success: function (data) {
                $('.subtotal-geral').text(data.subtotal.toFixed(2));
                $('.desconto-geral').text(data.totalDesconto.toFixed(2));
                $('.impostos-geral').text(data.totalImpostos.toFixed(2));
                $('.total-geral').text(data.total.toFixed(2));
                $('.quantidade-itens').text(data.quantidadeItens);
            }
        });
    }
}

// Verificar estoque
function verificarEstoque(produtoId, quantidade, container) {
    $.ajax({
        url: '/Utils/VerificarEstoque',
        data: { produtoId: produtoId, quantidade: quantidade },
        success: function (data) {
            var alertaEstoque = container.find('.alerta-estoque');

            if (!data.suficiente) {
                alertaEstoque.removeClass('d-none alert-success')
                    .addClass('alert-warning')
                    .html(`<i class="fas fa-exclamation-triangle"></i> Estoque insuficiente! Disponível: ${data.disponivel} ${data.unidade}`);
            } else {
                alertaEstoque.removeClass('d-none alert-warning')
                    .addClass('alert-success')
                    .html(`<i class="fas fa-check"></i> Estoque OK: ${data.disponivel} ${data.unidade}`);
            }
        }
    });
}

// Adicionar novo item
function adicionarNovoItem() {
    var template = $('.item-template').html();
    var novoItem = $(template);
    var index = $('.item-container').length;

    // Atualizar índices dos campos
    novoItem.find('input, select').each(function () {
        var name = $(this).attr('name');
        if (name) {
            $(this).attr('name', name.replace('[0]', '[' + index + ']'));
        }
    });

    $('.itens-container').append(novoItem);

    // Reconfigurar auto-completes
    configurarAutoCompleteProdutos();

    return novoItem;
}

// Remover item
function removerItem(botao) {
    var container = $(botao).closest('.item-container');
    container.remove();
    calcularTotalGeral();
}

// Mostrar alerta
function mostrarAlerta(mensagem, tipo) {
    var classe = 'alert-info';
    var icone = 'fas fa-info-circle';

    switch (tipo) {
        case 'success':
            classe = 'alert-success';
            icone = 'fas fa-check-circle';
            break;
        case 'warning':
            classe = 'alert-warning';
            icone = 'fas fa-exclamation-triangle';
            break;
        case 'error':
            classe = 'alert-danger';
            icone = 'fas fa-exclamation-circle';
            break;
    }

    var alerta = `
        <div class="alert ${classe} alert-dismissible fade show" role="alert">
            <i class="${icone}"></i> ${mensagem}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('.alertas-container').prepend(alerta);

    // Auto-remover após 5 segundos
    setTimeout(function () {
        $('.alert').first().alert('close');
    }, 5000);
}

// Formatação de moeda
function formatarMoeda(valor) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(valor);
}

// Formatação de número
function formatarNumero(valor, decimais = 2) {
    return new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: decimais,
        maximumFractionDigits: decimais
    }).format(valor);
}

// Confirmar ação
function confirmarAcao(mensagem, callback) {
    Swal.fire({
        title: 'Confirmação',
        text: mensagem,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sim, confirmar!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed && callback) {
            callback();
        }
    });
}

// Loading overlay
function mostrarLoading() {
    if ($('.loading-overlay').length === 0) {
        $('body').append(`
            <div class="loading-overlay">
                <div class="loading-spinner"></div>
            </div>
        `);
    }
    $('.loading-overlay').show();
}

function ocultarLoading() {
    $('.loading-overlay').hide();
}