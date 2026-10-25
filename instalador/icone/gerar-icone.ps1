# Gera o ícone do Lotofácil (desenho próprio, sem a marca da Caixa): um volante estilizado com bolinhas marcadas.
# Saídas: src/Lotofacil.Web/lotofacil.ico (16 a 256 px) e src/Lotofacil.Web/wwwroot/favicon.png (64 px).
# Uso: pwsh instalador/icone/gerar-icone.ps1

Add-Type -AssemblyName System.Drawing

$raiz = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$pastaWeb = Join-Path $raiz 'src\Lotofacil.Web'
$corFundo = [System.Drawing.ColorTranslator]::FromHtml('#594AE2')
$corMarcada = [System.Drawing.Color]::White
$corLivre = [System.Drawing.Color]::FromArgb(90, 255, 255, 255)

# Bolinhas marcadas no volante 5 × 5 (linha, coluna) e no volante simplificado 3 × 3 dos tamanhos pequenos.
$marcadas5 = @('0,0', '0,3', '1,1', '1,4', '2,2', '2,0', '3,3', '3,1', '4,4', '4,2')
$marcadas3 = @('0,0', '1,1', '2,2', '0,2', '2,0')

function Desenhar([int] $tamanho) {
    $bitmap = [System.Drawing.Bitmap]::new($tamanho, $tamanho, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $grafico = [System.Drawing.Graphics]::FromImage($bitmap)
    $grafico.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $grafico.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $grafico.Clear([System.Drawing.Color]::Transparent)

    # Fundo: quadrado de cantos arredondados ocupando todo o ícone.
    $raio = $tamanho * 0.22
    $caminho = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $caminho.AddArc(0, 0, 2 * $raio, 2 * $raio, 180, 90)
    $caminho.AddArc($tamanho - 2 * $raio, 0, 2 * $raio, 2 * $raio, 270, 90)
    $caminho.AddArc($tamanho - 2 * $raio, $tamanho - 2 * $raio, 2 * $raio, 2 * $raio, 0, 90)
    $caminho.AddArc(0, $tamanho - 2 * $raio, 2 * $raio, 2 * $raio, 90, 90)
    $caminho.CloseFigure()
    $grafico.FillPath([System.Drawing.SolidBrush]::new($corFundo), $caminho)

    # Abaixo de 32 px, 25 bolinhas viram borrão: usa 3 × 3.
    $grade = if ($tamanho -ge 32) { 5 } else { 3 }
    $marcadas = if ($grade -eq 5) { $marcadas5 } else { $marcadas3 }
    $margem = $tamanho * 0.16
    $passo = ($tamanho - 2 * $margem) / $grade
    $diametro = $passo * 0.72
    for ($linha = 0; $linha -lt $grade; $linha++) {
        for ($coluna = 0; $coluna -lt $grade; $coluna++) {
            $cor = if ($marcadas -contains "$linha,$coluna") { $corMarcada } else { $corLivre }
            $x = $margem + $coluna * $passo + ($passo - $diametro) / 2
            $y = $margem + $linha * $passo + ($passo - $diametro) / 2
            $grafico.FillEllipse([System.Drawing.SolidBrush]::new($cor), [single]$x, [single]$y, [single]$diametro, [single]$diametro)
        }
    }

    $grafico.Dispose()
    return $bitmap
}

function EmPng([System.Drawing.Bitmap] $bitmap) {
    $memoria = [System.IO.MemoryStream]::new()
    $bitmap.Save($memoria, [System.Drawing.Imaging.ImageFormat]::Png)
    return $memoria.ToArray()
}

# Arquivo .ico com uma imagem PNG por tamanho (formato aceito pelo Windows desde o Vista).
$tamanhos = @(16, 24, 32, 48, 64, 128, 256)
$imagens = [System.Collections.Generic.List[byte[]]]::new()
foreach ($tamanho in $tamanhos) { $imagens.Add((EmPng (Desenhar $tamanho))) }
$saida = [System.IO.MemoryStream]::new()
$escritor = [System.IO.BinaryWriter]::new($saida)
$escritor.Write([uint16]0)
$escritor.Write([uint16]1)
$escritor.Write([uint16]$tamanhos.Count)
$deslocamento = 6 + 16 * $tamanhos.Count
for ($i = 0; $i -lt $tamanhos.Count; $i++) {
    $lado = if ($tamanhos[$i] -eq 256) { 0 } else { $tamanhos[$i] }
    $escritor.Write([byte]$lado)
    $escritor.Write([byte]$lado)
    $escritor.Write([byte]0)
    $escritor.Write([byte]0)
    $escritor.Write([uint16]1)
    $escritor.Write([uint16]32)
    $escritor.Write([uint32]$imagens[$i].Length)
    $escritor.Write([uint32]$deslocamento)
    $deslocamento += $imagens[$i].Length
}
foreach ($imagem in $imagens) { $escritor.Write($imagem) }
$escritor.Flush()
[System.IO.File]::WriteAllBytes((Join-Path $pastaWeb 'lotofacil.ico'), $saida.ToArray())

[System.IO.File]::WriteAllBytes((Join-Path $pastaWeb 'wwwroot\favicon.png'), (EmPng (Desenhar 64)))
Write-Output "Ícone gerado em $pastaWeb"
