## Why

O app instalado pelo `Lotofacil-Setup-1.0.0.exe` abre as páginas, mas nenhum botão funciona: em Resultados, "Importar
histórico" e "Atualizar resultados" não fazem nada, sem andamento, sem mensagem e sem registro no log. A causa é a
publicação do instalador: com `-p:Distribuicao=true` o projeto vira `WinExe` (para abrir sem console), e o pacote do
.NET que acrescenta o `_framework/blazor.web.js` à publicação só faz isso quando o tipo do projeto é `Exe`. Sem esse
script o circuito do Blazor não conecta, e a página fica só com o HTML pré-renderizado. O defeito afeta todas as
páginas interativas do app instalado, não só Resultados; o modo de desenvolvimento não é afetado.

## What Changes

- A publicação do instalador mantém o projeto como `Exe` e marca só o executável como aplicativo gráfico, para
  continuar abrindo sem janela de console e passar a incluir o `blazor.web.js`.
- O script de geração do instalador confere se o `blazor.web.js` está na publicação e para com erro, sem gerar o
  instalador, quando ele faltar.
- A versão do app passa para 1.0.1, para o instalador corrigido ser distinguível do 1.0.0 no "Adicionar ou remover
  programas". Instalar por cima mantém as pastas e os dados, como já previsto.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
- `instalacao-windows`: "Execução em segundo plano com ícone na bandeja" passa a exigir que as páginas do app instalado
  sejam interativas como no desenvolvimento; "Geração do instalador" passa a recusar uma publicação sem o script do
  Blazor.

## Impact

- `src/Lotofacil.Web/Lotofacil.Web.csproj`: grupo da `Distribuicao` sem `OutputType` `WinExe`, com o executável
  marcado como gráfico.
- `instalador/gerar-instalador.ps1`: conferência do `wwwroot/_framework/blazor.web.js` antes de compilar o instalador.
- `Directory.Build.props`: versão 1.0.1.
- Sem mudança de código C#, de banco ou de dependências. Quem já instalou a 1.0.0 precisa reinstalar com o instalador
  novo.
