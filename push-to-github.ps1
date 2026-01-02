# Script para fazer push do ERP Orama para GitHub
# Execute após criar o repositório no GitHub

Write-Host "🚀 Fazendo push do ERP Orama para GitHub..." -ForegroundColor Green

# Verificar se o remote já existe
$remoteExists = git remote get-url origin 2>$null
if (-not $remoteExists) {
    Write-Host "Configurando remote origin..." -ForegroundColor Yellow
    git remote add origin https://github.com/Lukzera1325/OramaERP.git
}

# Fazer push
Write-Host "Fazendo push para GitHub..." -ForegroundColor Yellow
git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Push realizado com sucesso!" -ForegroundColor Green
    Write-Host "📂 Repositório disponível em: https://github.com/Lukzera1325/OramaERP" -ForegroundColor Cyan
} else {
    Write-Host "❌ Erro no push. Verifique se o repositório foi criado no GitHub." -ForegroundColor Red
    Write-Host "🔗 Acesse: https://github.com/Lukzera1325 e crie o repositório 'OramaERP'" -ForegroundColor Yellow
}

Write-Host "`nPressione qualquer tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")