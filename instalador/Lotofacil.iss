; Instalador do Lotofácil para Windows 64 bits, só para o usuário atual (sem senha de administrador).
; Compilado por instalador/gerar-instalador.ps1, que informa a versão, a pasta publicada e a pasta de saída.

#ifndef Versao
  #error Informe a versão com /DVersao=x.y.z
#endif
#ifndef Publicacao
  #error Informe a pasta publicada com /DPublicacao=<pasta>
#endif
#ifndef Saida
  #error Informe a pasta de saída com /DSaida=<pasta>
#endif

[Setup]
AppId={{FA553D84-0F69-4DCA-821B-FCEDF6D71803}
AppName=Lotofácil
AppVersion={#Versao}
AppVerName=Lotofácil {#Versao}
AppPublisher=Fábio Nunes
VersionInfoVersion={#Versao}
DefaultDirName={localappdata}\Programs\Lotofacil
DisableDirPage=no
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#Saida}
OutputBaseFilename=Lotofacil-Setup-{#Versao}
SetupIconFile=..\src\Lotofacil.Web\lotofacil.ico
UninstallDisplayName=Lotofácil
UninstallDisplayIcon={app}\Lotofacil.Web.exe
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
; Mesmo nome do mutex do app (InstanciaUnica): com o app aberto, pede para fechá-lo antes de instalar ou desinstalar.
AppMutex=Lotofacil.Instancia

[Languages]
Name: "portugues"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "atalhoareadetrabalho"; Description: "Criar atalho na Área de trabalho"; GroupDescription: "Atalhos:"

[Files]
Source: "{#Publicacao}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Lotofácil"; Filename: "{app}\Lotofacil.Web.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Lotofácil"; Filename: "{app}\Lotofacil.Web.exe"; WorkingDir: "{app}"; Tasks: atalhoareadetrabalho

[Run]
Filename: "{app}\Lotofacil.Web.exe"; Description: "Abrir o Lotofácil"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

; A pasta dos dados é resolvida na instalação e gravada no registro da desinstalação.
; Só os arquivos do app são apagados; a pasta só sai se ficar vazia.
[UninstallDelete]
Type: files; Name: "{app}\appsettings.Production.json"
Type: files; Name: "{code:PastaDados}\lotofacil.db"
Type: files; Name: "{code:PastaDados}\lotofacil.db-wal"
Type: files; Name: "{code:PastaDados}\lotofacil.db-shm"
Type: files; Name: "{code:PastaDados}\lotofacil.db-journal"
Type: filesandordirs; Name: "{code:PastaDados}\log"
Type: dirifempty; Name: "{code:PastaDados}"

[Code]
var
  PaginaDados: TInputDirWizardPage;

{ Página da pasta dos dados, logo depois da pasta de instalação; na reinstalação, sugere a pasta escolhida antes. }
procedure InitializeWizard;
begin
  PaginaDados := CreateInputDirPage(wpSelectDir,
    'Pasta dos dados',
    'Onde o Lotofácil deve guardar os dados?',
    'Onde ficam seus jogos, os resultados baixados e o registro de erros.' + #13#10 +
    'Se a pasta já tiver uma base do Lotofácil, ela será usada.',
    False, '');
  PaginaDados.Add('');
  PaginaDados.Values[0] := GetPreviousData('PastaDados', ExpandConstant('{localappdata}\Lotofacil'));
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
  SetPreviousData(PreviousDataKey, 'PastaDados', PaginaDados.Values[0]);
end;

function PastaDados(Parametro: String): String;
begin
  Result := PaginaDados.Values[0];
end;

{ Resumo "Pronto pra instalar" com a pasta dos dados logo depois da pasta de instalação. }
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo,
  MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := MemoDirInfo + NewLine + NewLine +
    'Pasta dos dados:' + NewLine + Space + PaginaDados.Values[0];
  if MemoTasksInfo <> '' then
    Result := Result + NewLine + NewLine + MemoTasksInfo;
end;

function EscaparJson(Texto: String): String;
begin
  Result := Texto;
  StringChangeEx(Result, '\', '\\', True);
  StringChangeEx(Result, '"', '\"', True);
end;

{ Grava a configuração do app com a pasta dos dados escolhida. }
procedure CurStepChanged(CurStep: TSetupStep);
var
  Pasta: String;
  Linhas: TArrayOfString;
begin
  if CurStep = ssPostInstall then
  begin
    Pasta := EscaparJson(PaginaDados.Values[0]);
    SetArrayLength(Linhas, 4);
    Linhas[0] := '{';
    Linhas[1] := '  "BancoDados": { "Arquivo": "' + Pasta + '\\lotofacil.db" },';
    Linhas[2] := '  "Log": { "Pasta": "' + Pasta + '\\log" }';
    Linhas[3] := '}';
    SaveStringsToUTF8File(ExpandConstant('{app}\appsettings.Production.json'), Linhas, False);
  end;
end;
