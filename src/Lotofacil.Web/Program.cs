// <copyright file="Program.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Geracao;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Painel;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Infraestrutura;
using Lotofacil.Infraestrutura.Caixa;
using Lotofacil.Web;
using Lotofacil.Web.Components;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string pastaLog = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Log:Pasta"] ?? "log");
builder.Logging.AddSerilog(ConfiguracaoLog.Criar(pastaLog).CreateLogger(), dispose: true);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

string caminhoBanco = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["BancoDados:Arquivo"] ?? "lotofacil.db");
var opcoesFonte = new OpcoesFonteResultados(
    builder.Configuration["FonteResultados:UrlBase"] ?? "https://servicebus3.caixa.gov.br/portaldeloterias",
    builder.Configuration.GetValue("FonteResultados:TempoLimiteSegundos", 60),
    builder.Configuration.GetValue("FonteResultados:PausaEntreRequisicoesMs", 500));
builder.Services.AddInfraestrutura(caminhoBanco, opcoesFonte);
builder.Services.AddSingleton<ConsultaConcursosServico>();
builder.Services.AddSingleton<ImportacaoHistoricoServico>();
builder.Services.AddSingleton<AtualizacaoConcursosServico>();
builder.Services.AddSingleton<OrdemSorteioServico>();
builder.Services.AddSingleton<EstatisticasServico>();
builder.Services.AddSingleton<JogosServico>();
builder.Services.AddSingleton<PainelServico>();
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddSingleton<GeracaoJogosServico>();
decimal precoPadrao = builder.Configuration.GetValue("Lotofacil:PrecoApostaMinima", 3.50m);
builder.Services.AddSingleton(provedor =>
    new ConfiguracoesServico(provedor.GetRequiredService<IConfiguracaoApostasRepositorio>(), precoPadrao));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro", createScopeForErrors: true);
}

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR")
    .AddSupportedCultures("pt-BR")
    .AddSupportedUICultures("pt-BR"));
app.UseStatusCodePagesWithReExecute("/nao-encontrada", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.InicializarBancoDadosAsync();
await app.Services.GetRequiredService<ConfiguracoesServico>().CarregarAsync();

app.Run();
