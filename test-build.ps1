# Script para testar a compilação do projeto Orama ERP
Write-Host "=== TESTE DE COMPILAÇÃO - ORAMA ERP ===" -ForegroundColor Cyan

# Verificar se o .NET está instalado
Write-Host "Verificando instalação do .NET..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET encontrado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET não encontrado. Instale o .NET 8 SDK" -ForegroundColor Red
    exit 1
}

# Navegar para o diretório do projeto
Set-Location "src/Orama.Web"

# Restaurar pacotes
Write-Host "Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar o projeto
Write-Host "Compilando o projeto..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration Debug --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Compilação bem-sucedida!" -ForegroundColor Green
    
    # Verificar se há migrations pendentes
    Write-Host "Verificando migrations..." -ForegroundColor Yellow
    try {
        dotnet ef database update --dry-run
        Write-Host "✓ Migrations verificadas" -ForegroundColor Green
    } catch {
        Write-Host "⚠ Erro ao verificar migrations" -ForegroundColor Yellow
    }
    
    Write-Host "=== PROJETO PRONTO PARA EXECUÇÃO ===" -ForegroundColor Green
    Write-Host "Para executar: dotnet run" -ForegroundColor Cyan
    Write-Host "URL: https://localhost:5001" -ForegroundColor Cyan
    Write-Host "Use uma conta individual provisionada para este ambiente." -ForegroundColor Cyan
} else {
    Write-Host "✗ Erro na compilação" -ForegroundColor Red
    exit 1
}

Set-Location "..\..\"
