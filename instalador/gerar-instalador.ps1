# Gera o instalador do Lotofácil: publica o app para Windows 64 bits, com o .NET incluído, e compila o Lotofacil.iss.
# Pré-requisito: Inno Setup 6 (winget install JRSoftware.InnoSetup).
# Uso: pwsh instalador/gerar-instalador.ps1  ->  artefatos/Lotofacil-Setup-<versão>.exe

$ErrorActionPreference = 'Stop'

$raiz = Resolve-Path (Join-Path $PSScriptRoot '..')
[xml] $propriedades = Get-Content (Join-Path $raiz 'Directory.Build.props')
$versao = $propriedades.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $versao) { throw 'Versão não encontrada no Directory.Build.props.' }

$artefatos = Join-Path $raiz 'artefatos'
$publicacao = Join-Path $artefatos 'publicacao'
if (Test-Path $publicacao) { Remove-Item $publicacao -Recurse -Force }

# Distribuicao=true faz o app abrir sem janela de console.
dotnet publish (Join-Path $raiz 'src\Lotofacil.Web') -c Release -r win-x64 --self-contained true -p:Distribuicao=true -o $publicacao
if ($LASTEXITCODE -ne 0) { throw 'Falha ao publicar o app.' }

$compilador = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $compilador) { throw 'Inno Setup 6 não encontrado. Instale com: winget install JRSoftware.InnoSetup' }

& $compilador "/DVersao=$versao" "/DPublicacao=$publicacao" "/DSaida=$artefatos" (Join-Path $PSScriptRoot 'Lotofacil.iss')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o instalador.' }

Write-Output "Instalador gerado: $(Join-Path $artefatos "Lotofacil-Setup-$versao.exe")"
