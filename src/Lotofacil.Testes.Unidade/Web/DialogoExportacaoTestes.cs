// <copyright file="DialogoExportacaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using AngleSharp.Dom;
using Bunit;
using Lotofacil.Web.Components.Jogos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do diálogo de exportação dos jogos.
/// </summary>
public class DialogoExportacaoTestes : ComponenteTesteBase
{
    /// <summary>
    /// Texto exportado nos testes.
    /// </summary>
    private const string TextoExportado = "Lotofácil · concurso 3813\r\n1 jogo · R$ 3,50\r\n\r\n01) 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15";

    /// <summary>
    /// Provedor dos diálogos.
    /// </summary>
    private readonly IRenderedComponent<MudDialogProvider> dialogos;

    /// <summary>
    /// Provedor das mensagens rápidas.
    /// </summary>
    private readonly IRenderedComponent<MudSnackbarProvider> mensagens;

    /// <summary>
    /// Renderiza os provedores.
    /// </summary>
    public DialogoExportacaoTestes()
    {
        this.dialogos = this.Render<MudDialogProvider>();
        this.mensagens = this.Render<MudSnackbarProvider>();
    }

    /// <summary>
    /// O diálogo mostra o texto e o link de download com o nome e o endereço do arquivo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MostraOTextoEOLinkDeDownload()
    {
        await this.AbrirAsync();

        Assert.Equal("Exportar · concurso 3813", Texto(this.dialogos.Find(".mud-dialog-title")));

        // O HTML normaliza as quebras de linha da área de texto para "\n"; a cópia e o arquivo usam o texto original.
        Assert.Equal(
            TextoExportado.Replace("\r\n", "\n", StringComparison.Ordinal),
            this.dialogos.Find(".texto-exportacao textarea").GetAttribute("value"));
        IElement baixar = this.dialogos.Find("a.botao-baixar");
        Assert.Equal("concurso-3813.txt", baixar.GetAttribute("download"));
        Assert.Equal(FormatacaoExportacao.EnderecoDownload(TextoExportado), baixar.GetAttribute("href"));
    }

    /// <summary>
    /// "Copiar" grava o texto na área de transferência e avisa.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CopiaOTexto()
    {
        await this.AbrirAsync();

        this.dialogos.Find(".botao-copiar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains("Jogos copiados.", this.MensagensExibidas()));
        JSRuntimeInvocation chamada = Assert.Single(this.JSInterop.Invocations, invocacao => invocacao.Identifier == "navigator.clipboard.writeText");
        Assert.Equal([TextoExportado], chamada.Arguments);
    }

    /// <summary>
    /// Cópia recusada pelo navegador: mensagem de falha e o diálogo continua aberto.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CopiaRecusada()
    {
        this.JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true).SetException(new JSException("NotAllowedError"));
        await this.AbrirAsync();

        this.dialogos.Find(".botao-copiar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Não foi possível copiar. Selecione o texto e copie manualmente.",
            this.MensagensExibidas()));
        Assert.NotEmpty(this.dialogos.FindAll(".texto-exportacao"));
    }

    /// <summary>
    /// "Fechar" fecha o diálogo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Fecha()
    {
        await this.AbrirAsync();

        this.dialogos.Find(".botao-fechar").Click();

        this.dialogos.WaitForAssertion(() => Assert.Empty(this.dialogos.FindAll(".mud-dialog")));
    }

    /// <summary>
    /// Texto de um elemento, com espaços não separáveis trocados por espaços comuns.
    /// </summary>
    /// <param name="elemento">Elemento da página.</param>
    /// <returns>Texto sem espaços nas pontas.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Mensagens rápidas exibidas.
    /// </summary>
    /// <returns>Textos das mensagens.</returns>
    private List<string> MensagensExibidas()
    {
        return this.mensagens.FindAll(".mud-snackbar-content-message").Select(Texto).ToList();
    }

    /// <summary>
    /// Abre o diálogo do concurso 3813 e espera o texto aparecer.
    /// </summary>
    /// <returns>Tarefa da abertura.</returns>
    private async Task AbrirAsync()
    {
        IDialogService servico = this.Services.GetRequiredService<IDialogService>();
        var parametros = new DialogParameters<DialogoExportacao>
        {
            { dialogo => dialogo.Texto, TextoExportado },
            { dialogo => dialogo.NomeArquivo, "concurso-3813.txt" },
        };

        await this.dialogos.InvokeAsync(() => servico.ShowAsync<DialogoExportacao>("Exportar · concurso 3813", parametros));
        this.dialogos.WaitForAssertion(() => Assert.NotEmpty(this.dialogos.FindAll(".texto-exportacao")));
    }
}
