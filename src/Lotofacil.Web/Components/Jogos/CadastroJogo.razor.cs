// <copyright file="CadastroJogo.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Diálogo de cadastro de um jogo: concurso, quantidade de dezenas e as dezenas marcadas no volante.
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
    /// Quantidade de dezenas escolhida para o jogo.
    /// </summary>
    private int dezenasPorJogo = RegrasLotofacil.MinimoDezenasPorJogo;

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
    /// Configurações, com o preço da aposta usado no custo exibido.
    /// </summary>
    [Inject]
    private ConfiguracoesServico Configuracoes { get; set; } = null!;

    /// <summary>
    /// Indica se o jogo está completo para ser salvo.
    /// </summary>
    private bool PodeSalvar => this.numeroConcurso is not null && this.marcadas.Count == this.dezenasPorJogo;

    /// <summary>
    /// Custo da aposta com a quantidade de dezenas escolhida.
    /// </summary>
    private decimal Custo => Jogo.CalcularQuantidadeApostas(this.dezenasPorJogo) * this.Configuracoes.PrecoApostaMinima;

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
