// <copyright file="Program.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/nao-encontrada", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
