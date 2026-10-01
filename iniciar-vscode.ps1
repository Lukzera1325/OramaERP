# Script de Inicialização Rápida - Orama ERP no VS Code
# Execute: .\iniciar-vscode.ps1

Write-Host "🚀 INICIANDO ORAMA ERP NO VS CODE" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Verificar se o VS Code está instalado
if (!(Get-Command "code" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ VS Code não encontrado. Instale o Visual Studio Code primeiro." -ForegroundColor Red
    exit 1
}

# Verificar se o .NET está instalado
if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET SDK não encontrado. Instale o .NET 8 SDK primeiro." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Verificando dependências..." -ForegroundColor Yellow

# Restaurar pacotes NuGet
Write-Host "📦 Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar pacotes." -ForegroundColor Red
    exit 1
}

# Build do projeto
Write-Host "🔨 Compilando projeto..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na compilação." -ForegroundColor Red
    exit 1
}

# Abrir VS Code
Write-Host "🎯 Abrindo VS Code..." -ForegroundColor Yellow
code .

Write-Host ""
Write-Host "✅ SETUP COMPLETO!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "1. Instale as extensões recomendadas (VS Code irá sugerir)" -ForegroundColor White
Write-Host "2. Pressione Ctrl+Shift+P e digite 'Tasks: Run Task'" -ForegroundColor White
Write-Host "3. Selecione 'run-web' para executar o sistema" -ForegroundColor White
Write-Host "4. Acesse http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "🔑 LOGIN:" -ForegroundColor Cyan
Write-Host "Use uma conta individual provisionada para este ambiente." -ForegroundColor White
Write-Host ""
Write-Host "📚 Consulte o GUIA_MIGRACAO_VSCODE.md para mais detalhes" -ForegroundColor Yellow
