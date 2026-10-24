// <copyright file="DialogoExportacao.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Diálogo com o texto dos jogos de um concurso, para copiar ou baixar como arquivo.
/// </summary>
public partial class DialogoExportacao
{
    /// <summary>
    /// Aviso de cópia feita.
    /// </summary>
    public const string Copiado = "Jogos copiados.";

    /// <summary>
    /// Aviso de cópia recusada pelo navegador.
    /// </summary>
    public const string FalhaCopia = "Não foi possível copiar. Selecione o texto e copie manualmente.";

    /// <summary>
    /// Função do navegador que grava texto na área de transferência.
    /// </summary>
    private const string FuncaoCopiar = "navigator.clipboard.writeText";

    /// <summary>
    /// Texto da exportação.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string Texto { get; set; } = string.Empty;

    /// <summary>
    /// Nome do arquivo baixado.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>
    /// Instância do diálogo, usada para fechar.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance Dialogo { get; set; } = null!;

    /// <summary>
    /// Acesso ao JavaScript do navegador, para a área de transferência.
    /// </summary>
    [Inject]
    private IJSRuntime Navegador { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Copia o texto para a área de transferência e avisa o resultado.
    /// </summary>
    /// <returns>Tarefa da cópia.</returns>
    private async Task CopiarAsync()
    {
        try
        {
            await this.Navegador.InvokeVoidAsync(FuncaoCopiar, this.Texto);
            this.Mensagens.Add(Copiado, Severity.Success);
        }
        catch (Exception erro) when (erro is JSException or InvalidOperationException)
        {
            this.Mensagens.Add(FalhaCopia, Severity.Warning);
        }
    }

    /// <summary>
    /// Fecha o diálogo.
    /// </summary>
    private void Fechar()
    {
        this.Dialogo.Close();
    }
}
