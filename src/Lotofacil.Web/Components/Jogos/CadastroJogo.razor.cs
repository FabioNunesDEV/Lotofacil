// <copyright file="CadastroJogo.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Diálogo de cadastro de um jogo: concurso e 15 dezenas marcadas no volante.
/// </summary>
public partial class CadastroJogo
{
    /// <summary>
    /// Dezenas marcadas no volante.
    /// </summary>
    private IReadOnlyCollection<int> marcadas = [];

    /// <summary>
    /// Concurso informado.
    /// </summary>
    private int? numeroConcurso;

    /// <summary>
    /// Concurso sugerido ao abrir, ou <c>null</c> com a base vazia.
    /// </summary>
    [Parameter]
    public int? ConcursoSugerido { get; set; }

    /// <summary>
    /// Instância do diálogo, usada para fechar.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance Dialogo { get; set; } = null!;

    /// <summary>
    /// Serviço de jogos.
    /// </summary>
    [Inject]
    private JogosServico Servico { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Indica se o jogo está completo para ser salvo.
    /// </summary>
    private bool PodeSalvar => this.numeroConcurso is not null && this.marcadas.Count == RegrasLotofacil.DezenasPorJogo;

    /// <summary>
    /// Preenche o concurso sugerido.
    /// </summary>
    protected override void OnInitialized()
    {
        this.numeroConcurso = this.ConcursoSugerido;
    }

    /// <summary>
    /// Guarda as dezenas marcadas no volante.
    /// </summary>
    /// <param name="novas">Dezenas marcadas.</param>
    private void TrocarMarcadas(IReadOnlyCollection<int> novas)
    {
        this.marcadas = novas;
    }

    /// <summary>
    /// Fecha o diálogo sem salvar.
    /// </summary>
    private void Cancelar()
    {
        this.Dialogo.Cancel();
    }

    /// <summary>
    /// Salva o jogo; fecha o diálogo no sucesso ou mantém aberto mostrando o motivo da recusa.
    /// </summary>
    /// <returns>Tarefa do cadastro.</returns>
    private async Task SalvarAsync()
    {
        if (this.numeroConcurso is not int concurso)
        {
            return;
        }

        ResultadoCadastro resultado = await this.Servico.CadastrarAsync(concurso, this.marcadas);
        if (!resultado.Sucesso)
        {
            this.Mensagens.Add(resultado.Mensagem, Severity.Warning);
            return;
        }

        this.Mensagens.Add(resultado.Mensagem, Severity.Success);
        this.Dialogo.Close(DialogResult.Ok(true));
    }
}
