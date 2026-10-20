$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        throw 'Node.js 20 veya sonrası gerekli. https://nodejs.org adresinden kurun.'
    }
    # Windows CPU binaries are included in the locked packages; no install scripts are needed.
    npm ci --ignore-scripts --no-fund --no-audit
    if ($LASTEXITCODE -ne 0) { throw 'Kokoro paketleri kurulamadı.' }
    node server.mjs --warmup
    if ($LASTEXITCODE -ne 0) { throw 'Kokoro modeli hazırlanamadı.' }
    Write-Output 'Kokoro hazır. MAUI Windows uygulamasını yeniden başlatabilirsiniz.'
} finally { Pop-Location }
