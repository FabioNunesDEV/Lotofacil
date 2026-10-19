## Why

O repositório ainda não tem código: existem apenas as regras (`CLAUDE.md`), as referências em `Docs/` e o
protótipo visual em HTML. Antes de implementar qualquer funcionalidade é preciso ter a solução .NET montada,
compilando sem avisos e com os projetos de teste rodando, para que cada fatia seguinte (concursos, importação,
atualização, estatísticas, jogos) seja entregue de forma incremental e testada.

## What Changes

- Criar a pasta `src/` com a solução `Lotofacil.slnx` e os seis projetos definidos no `CLAUDE.md`
  (`Dominio`, `Aplicacao`, `Infraestrutura`, `Web`, `Testes.Unidade`, `Testes.Integracao`), com as referências
  entre eles respeitando a arquitetura.
- Criar na raiz do repositório as configurações compartilhadas: `global.json` (fixa o SDK 10),
  `Directory.Build.props` (alvo `net10.0`, StyleCop e avisos tratados como erro em todos os projetos),
  `Directory.Packages.props` (versões dos pacotes em um só lugar), `stylecop.json` e `.editorconfig`.
- Criar a aplicação Blazor Server com MudBlazor, sem Bootstrap e sem jQuery, acessível apenas em `localhost`,
  com o layout principal (barra superior e menu lateral) e a página inicial "Painel" vazia.
- Registrar em arquivo, na pasta `log`, apenas as mensagens de erro da aplicação, usando Serilog.
- Criar os primeiros testes: renderização do layout com bUnit, verificação automática das dependências
  entre as camadas e gravação do log de erros.
- Atualizar a lista de dependências autorizadas do `CLAUDE.md` com os pacotes aprovados nesta change.

## Capabilities

### New Capabilities
- `aplicacao-web`: execução local da aplicação, layout principal com navegação, página inicial e registro de
  erros em arquivo; serve de base para as telas das próximas changes.

### Modified Capabilities
<!-- Nenhuma: não há specs existentes. -->

## Impact

- Novos diretórios e arquivos: `src/` e as configurações na raiz (`global.json`, `Directory.Build.props`,
  `Directory.Packages.props`, `stylecop.json`, `.editorconfig`).
- Dependências já autorizadas usadas: `MudBlazor`, `StyleCop.Analyzers`, `xunit`, `NSubstitute`, `bunit`.
- Dependências autorizadas pelo usuário nesta change (entram na lista do `CLAUDE.md`):
  `Serilog.Extensions.Logging` e `Serilog.Sinks.File`, para o log de erros em arquivo.
- Os testes rodam pela Microsoft.Testing.Platform (configurada no `global.json`), que já vem com o `xunit.v3`;
  `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio` chegaram a ser autorizados, mas não são necessários.
- Nova pasta `log` ao lado da aplicação em execução; os arquivos `*.log` já são ignorados pelo `.gitignore`.
- Sem banco de dados, sem acesso à API da Caixa e sem regra de negócio nesta change.
