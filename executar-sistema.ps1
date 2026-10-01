# Script para executar o sistema Orama ERP
Write-Host "=== EXECUTANDO ORAMA ERP ===" -ForegroundColor Cyan

# Verificar se o .NET está disponível
$dotnetPath = $null

# Tentar encontrar o .NET em locais comuns
$possiblePaths = @(
    "C:\Program Files\dotnet\dotnet.exe",
    "C:\Program Files (x86)\dotnet\dotnet.exe",
    "$env:USERPROFILE\.dotnet\dotnet.exe",
    "$env:ProgramFiles\dotnet\dotnet.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $dotnetPath = $path
        break
    }
}

if (-not $dotnetPath) {
    Write-Host "❌ .NET não encontrado!" -ForegroundColor Red
    Write-Host "Por favor, instale o .NET 8 SDK:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host "✅ .NET encontrado em: $dotnetPath" -ForegroundColor Green

# Navegar para o diretório do projeto
$projectPath = "src\Orama.Web"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Diretório do projeto não encontrado: $projectPath" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Set-Location $projectPath

Write-Host "📁 Diretório atual: $(Get-Location)" -ForegroundColor Yellow

# Restaurar pacotes
Write-Host "📦 Restaurando pacotes NuGet..." -ForegroundColor Yellow
& $dotnetPath restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar pacotes" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Compilar o projeto
Write-Host "🔨 Compilando o projeto..." -ForegroundColor Yellow
& $dotnetPath build --configuration Debug --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na compilação" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green

# Verificar se há migrations pendentes
Write-Host "🗄️ Verificando banco de dados..." -ForegroundColor Yellow
try {
    & $dotnetPath ef database update --no-build
    Write-Host "✅ Banco de dados atualizado!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Aviso: Não foi possível atualizar o banco de dados" -ForegroundColor Yellow
    Write-Host "Certifique-se de que o PostgreSQL está rodando e a connection string está correta" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🚀 INICIANDO O SISTEMA..." -ForegroundColor Green
Write-Host "📍 URL: http://localhost:5050" -ForegroundColor Cyan
Write-Host "Use uma conta individual provisionada para este ambiente." -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione Ctrl+C para parar o servidor" -ForegroundColor Yellow
Write-Host ""

# Executar o projeto
& $dotnetPath run --no-build --urls=http://localhost:5050

Set-Location "..\..\"
