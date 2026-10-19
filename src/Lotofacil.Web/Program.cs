// <copyright file="Program.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Infraestrutura;
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
builder.Services.AddInfraestrutura(caminhoBanco);
builder.Services.AddSingleton<ConsultaConcursosServico>();

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

app.Run();
