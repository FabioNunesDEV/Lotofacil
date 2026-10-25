## 1. Publicação com o script do Blazor

- [x] 1.1 No `Lotofacil.Web.csproj`, grupo da `Distribuicao` sem `OutputType` `WinExe` e com
  `_UseWindowsGraphicalUserInterface` `true`, com comentário curto do porquê; verificar publicando como o script de
  geração faz (`-p:Distribuicao=true`, `win-x64`, .NET incluído) numa pasta temporária: existe
  `wwwroot/_framework/blazor.web.js` e o `Lotofacil.Web.exe` tem subsistema GUI (2)
- [x] 1.2 `instalador/gerar-instalador.ps1` confere `wwwroot/_framework/blazor.web.js` na publicação antes do Inno Setup
  e lança erro com o caminho que faltou; verificar o caminho de erro apontando a conferência para um nome de arquivo
  inexistente (o script para sem chamar o Inno Setup) e desfazer
- [x] 1.3 `Directory.Build.props` com a versão 1.0.1; verificar gerando o instalador com
  `pwsh instalador/gerar-instalador.ps1`: sai `artefatos/Lotofacil-Setup-1.0.1.exe`, e o `git status` não mostra os
  arquivos gerados

## 2. Verificação final

- [x] 2.1 App publicado rodando em `Production` numa cópia com base vazia e porta livre: sem janela de console,
  `/_framework/blazor.web.js` responde 200, sem erro no console do navegador, e "Importar histórico" mostra o andamento
  e termina com "Importação concluída"
- [x] 2.2 Instalar o `Lotofacil-Setup-1.0.1.exe` por cima da 1.0.0 nesta máquina: pastas sugeridas e dados mantidos,
  versão 1.0.1 no "Adicionar ou remover programas", e os botões das páginas respondem
- [x] 2.3 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
