// Variáveis globais
let produtos = [];
let editandoItem = false;

// Inicialização
$(document).ready(function() {
    carregarProdutos();
    configurarEventos();
});

// Configurar eventos
function configurarEventos() {
    // Busca de produtos com autocomplete
    $('#produtoId').select2({
        placeholder: 'Digite para buscar produtos...',
        allowClear: true,
        ajax: {
            url: '/Compras/BuscarProdutos',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    termo: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map(function(item) {
                        return {
                            id: item.id,
                            text: `${item.codigo} - ${item.nome}`,
                            preco: item.preco,
                            estoque: item.estoque
                        };
                    })
                };
            },
            cache: true
        }
    });

    // Quando selecionar um produto, preencher o preço
    $('#produtoId').on('select2:select', function (e) {
        var data = e.params.data;
        if (data.preco) {
            $('#valorUnitario').val(data.preco.toFixed(2));
        }
    });

    // Calcular valor total automaticamente
    $('#quantidade, #valorUnitario').on('input', function() {
        calcularValorTotal();
    });
}

// Carregar produtos
function carregarProdutos() {
    $.get('/Compras/BuscarProdutos', { termo: '' })
        .done(function(data) {
            produtos = data;
        })
        .fail(function() {
            console.error('Erro ao carregar produtos');
        });
}

// Calcular valor total do item
function calcularValorTotal() {
    const quantidade = parseFloat($('#quantidade').val()) || 0;
    const valorUnitario = parseFloat($('#valorUnitario').val()) || 0;
    const valorTotal = quantidade * valorUnitario;
    
    // Exibir valor total (opcional - pode adicionar um campo readonly)
    console.log('Valor Total:', valorTotal.toFixed(2));
}

// Adicionar novo item
function adicionarItem() {
    editandoItem = false;
    limparFormularioItem();
    $('#itemModalTitle').text('Adicionar Item');
    $('#itemModal').modal('show');
}

// Editar item existente
function editarItem(itemId) {
    editandoItem = true;
    $('#itemModalTitle').text('Editar Item');
    
    // Buscar dados do item via AJAX
    $.get(`/Compras/ObterItens/${compraId}`)
        .done(function(response) {
            if (response.success) {
                const item = response.data.find(i => i.id === itemId);
                if (item) {
                    preencherFormularioItem(item);
                    $('#itemModal').modal('show');
                }
            }
        })
        .fail(function() {
            mostrarMensagem('Erro ao carregar dados do item', 'error');
        });
}

// Preencher formulário com dados do item
function preencherFormularioItem(item) {
    $('#itemId').val(item.id);
    
    // Criar option para o produto selecionado
    const option = new Option(`${item.produtoCodigo} - ${item.produtoNome}`, item.produtoId, true, true);
    $('#produtoId').append(option).trigger('change');
    
    $('#quantidade').val(item.quantidade);
    $('#valorUnitario').val(item.valorUnitario);
    $('#observacoes').val(item.observacoes || '');
}

// Limpar formulário do item
function limparFormularioItem() {
    $('#itemId').val('0');
    $('#produtoId').val(null).trigger('change');
    $('#quantidade').val('');
    $('#valorUnitario').val('');
    $('#observacoes').val('');
}

// Salvar item (adicionar ou editar)
function salvarItem() {
    if (!validarFormularioItem()) {
        return;
    }

    const itemData = {
        id: parseInt($('#itemId').val()) || 0,
        compraId: compraId,
        produtoId: parseInt($('#produtoId').val()),
        quantidade: parseFloat($('#quantidade').val()),
        valorUnitario: parseFloat($('#valorUnitario').val()),
        observacoes: $('#observacoes').val()
    };

    const url = editandoItem ? '/Compras/AtualizarItem' : '/Compras/AdicionarItem';
    
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(itemData),
        success: function(response) {
            if (response.success) {
                $('#itemModal').modal('hide');
                mostrarMensagem(response.message, 'success');
                recarregarItens();
            } else {
                mostrarMensagem(response.message, 'error');
            }
        },
        error: function() {
            mostrarMensagem('Erro ao salvar item', 'error');
        }
    });
}

// Remover item
function removerItem(itemId) {
    if (!confirm('Deseja remover este item?')) {
        return;
    }

    $.post('/Compras/RemoverItem', { itemId: itemId })
        .done(function(response) {
            if (response.success) {
                mostrarMensagem(response.message, 'success');
                recarregarItens();
            } else {
                mostrarMensagem(response.message, 'error');
            }
        })
        .fail(function() {
            mostrarMensagem('Erro ao remover item', 'error');
        });
}

// Recarregar lista de itens
function recarregarItens() {
    window.location.reload();
}

// Validar formulário do item
function validarFormularioItem() {
    const produtoId = $('#produtoId').val();
    const quantidade = $('#quantidade').val();
    const valorUnitario = $('#valorUnitario').val();

    if (!produtoId) {
        mostrarMensagem('Selecione um produto', 'error');
        $('#produtoId').focus();
        return false;
    }

    if (!quantidade || parseFloat(quantidade) <= 0) {
        mostrarMensagem('Informe uma quantidade válida', 'error');
        $('#quantidade').focus();
        return false;
    }

    if (!valorUnitario || parseFloat(valorUnitario) <= 0) {
        mostrarMensagem('Informe um valor unitário válido', 'error');
        $('#valorUnitario').focus();
        return false;
    }

    return true;
}

// Aprovar compra
function aprovarCompra(compraId) {
    if (!confirm('Deseja aprovar esta compra?')) {
        return;
    }

    $.post('/Compras/Aprovar', { id: compraId })
        .done(function(response) {
            if (response.success) {
                mostrarMensagem(response.message, 'success');
                setTimeout(() => window.location.reload(), 1500);
            } else {
                mostrarMensagem(response.message, 'error');
            }
        })
        .fail(function() {
            mostrarMensagem('Erro ao aprovar compra', 'error');
        });
}

// Receber compra
function receberCompra(compraId) {
    if (!confirm('Deseja receber esta compra? Esta ação irá:\n- Dar entrada no estoque\n- Gerar contas a pagar\n- Não poderá ser desfeita')) {
        return;
    }

    $.post('/Compras/Receber', { id: compraId })
        .done(function(response) {
            if (response.success) {
                mostrarMensagem(response.message, 'success');
                setTimeout(() => window.location.reload(), 2000);
            } else {
                mostrarMensagem(response.message, 'error');
            }
        })
        .fail(function() {
            mostrarMensagem('Erro ao receber compra', 'error');
        });
}

// Cancelar compra
function cancelarCompra(compraId) {
    if (!confirm('Deseja cancelar esta compra?')) {
        return;
    }

    $.post('/Compras/Cancelar', { id: compraId })
        .done(function(response) {
            if (response.success) {
                mostrarMensagem(response.message, 'success');
                setTimeout(() => window.location.reload(), 1500);
            } else {
                mostrarMensagem(response.message, 'error');
            }
        })
        .fail(function() {
            mostrarMensagem('Erro ao cancelar compra', 'error');
        });
}

// Mostrar mensagem
function mostrarMensagem(mensagem, tipo) {
    const alertClass = tipo === 'success' ? 'alert-success' : 'alert-danger';
    const icon = tipo === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-triangle';
    
    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="${icon} me-2"></i>${mensagem}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Remover alertas existentes
    $('.alert').remove();
    
    // Adicionar novo alerta no topo da página
    $('body').prepend(alertHtml);
    
    // Auto-remover após 5 segundos
    setTimeout(() => {
        $('.alert').fadeOut();
    }, 5000);
}