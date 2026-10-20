// <copyright file="Resultados.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Lista os concursos gravados, com paginação, busca por número e detalhe do rateio.
/// </summary>
public partial class Resultados
{
    /// <summary>
    /// Mensagem exibida quando a base não tem concursos.
    /// </summary>
    private const string MensagemBaseVazia =
        "Nenhum concurso gravado na base local. Use “Importar histórico” para carregar os concursos da Caixa.";

    /// <summary>
    /// Início da mensagem de falha da importação.
    /// </summary>
    private const string PrefixoFalhaImportacao = "A importação falhou e nada foi gravado.";

    /// <summary>
    /// Quantidades de concursos por página oferecidas ao usuário.
    /// </summary>
    private static readonly int[] OpcoesTamanhoPagina = [10, 25, 50];

    /// <summary>
    /// Números dos concursos com o detalhe aberto.
    /// </summary>
    private readonly HashSet<int> expandidos = [];

    /// <summary>
    /// Tabela de concursos, usada para recarregar os dados após a busca.
    /// </summary>
    private MudTable<Concurso> tabela = null!;

    /// <summary>
    /// Número buscado, ou <c>null</c> para listar todos.
    /// </summary>
    private int? numeroBusca;

    /// <summary>
    /// Mensagem exibida quando a tabela fica sem linhas.
    /// </summary>
    private string mensagemSemRegistros = MensagemBaseVazia;

    /// <summary>
    /// Indica se a importação do histórico está em andamento.
    /// </summary>
    private bool importando;

    /// <summary>
    /// Serviço de consulta dos concursos.
    /// </summary>
    [Inject]
    private ConsultaConcursosServico Servico { get; set; } = null!;

    /// <summary>
    /// Serviço de importação do histórico da Caixa.
    /// </summary>
    [Inject]
    private ImportacaoHistoricoServico Importacao { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Log de erros da aplicação.
    /// </summary>
    [Inject]
    private ILogger<Resultados> Log { get; set; } = null!;

    /// <summary>
    /// Devolve o rateio da faixa de 15 acertos.
    /// </summary>
    /// <param name="concurso">Concurso da linha.</param>
    /// <returns>Rateio de 15 acertos.</returns>
    private static Rateio Rateio15(Concurso concurso)
    {
        return concurso.ObterRateio(RegrasLotofacil.MaiorFaixaPremiada);
    }

    /// <summary>
    /// Carrega a página pedida pela tabela, ou somente o concurso buscado.
    /// </summary>
    /// <param name="estado">Página e tamanho de página pedidos pela tabela.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Concursos a exibir e total para o paginador.</returns>
    private async Task<TableData<Concurso>> CarregarAsync(TableState estado, CancellationToken cancelamento)
    {
        if (this.numeroBusca is int numero)
        {
            this.mensagemSemRegistros = $"Concurso {numero} não encontrado na base local";
            Concurso? concurso = await this.Servico.ObterAsync(numero, cancelamento);
            List<Concurso> encontrados = concurso is null ? [] : [concurso];

            return new TableData<Concurso> { Items = encontrados, TotalItems = encontrados.Count };
        }

        this.mensagemSemRegistros = MensagemBaseVazia;
        PaginaConcursos pagina = await this.Servico.ListarPaginaAsync(estado.Page, estado.PageSize, cancelamento);

        return new TableData<Concurso> { Items = pagina.Itens, TotalItems = pagina.Total };
    }

    /// <summary>
    /// Aplica ou limpa a busca e volta a tabela para a primeira página.
    /// </summary>
    /// <param name="numero">Número buscado, ou <c>null</c> quando o campo é limpo.</param>
    /// <returns>Tarefa da recarga.</returns>
    private async Task BuscarAsync(int? numero)
    {
        this.numeroBusca = numero;
        this.expandidos.Clear();

        // Trocar de página já recarrega os dados; na primeira página é preciso pedir a recarga.
        if (this.tabela.CurrentPage > 0)
        {
            this.tabela.NavigateTo(0);
        }
        else
        {
            await this.tabela.ReloadServerData();
        }
    }

    /// <summary>
    /// Importa o histórico da Caixa, informa o resultado e, se houver concursos novos, volta ao mais recente.
    /// </summary>
    /// <returns>Tarefa da importação.</returns>
    private async Task ImportarHistoricoAsync()
    {
        this.importando = true;
        try
        {
            ResultadoImportacao resultado = await this.Importacao.ImportarAsync();
            if (resultado.ConcursosGravados == 0)
            {
                this.Mensagens.Add(
                    $"A base local já tem todos os {resultado.ConcursosNoArquivo:N0} concursos do arquivo; nada foi gravado.",
                    Severity.Info);
            }
            else
            {
                this.Mensagens.Add(
                    $"Importação concluída: {resultado.ConcursosNoArquivo:N0} concursos no arquivo, "
                        + $"{resultado.ConcursosGravados:N0} gravados na base local.",
                    Severity.Success);
                await this.BuscarAsync(null);
            }
        }
        catch (FonteResultadosException erro)
        {
            this.Log.LogError(erro, "Falha na importação do histórico: {Mensagem}", erro.Message);
            this.MostrarFalhaImportacao(erro.Message);
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            this.Log.LogError(erro, "Falha inesperada na importação do histórico.");
            this.MostrarFalhaImportacao("Erro inesperado; os detalhes foram registrados no log.");
        }
        finally
        {
            this.importando = false;
        }
    }

    /// <summary>
    /// Mostra a falha da importação numa mensagem que só some quando o usuário fecha.
    /// </summary>
    /// <param name="motivo">Motivo da falha.</param>
    private void MostrarFalhaImportacao(string motivo)
    {
        this.Mensagens.Add(
            $"{PrefixoFalhaImportacao} {motivo}",
            Severity.Error,
            opcoes => opcoes.RequireInteraction = true);
    }

    /// <summary>
    /// Abre o detalhe do concurso clicado, ou fecha se já estiver aberto.
    /// </summary>
    /// <param name="clique">Dados do clique na linha.</param>
    private void AlternarDetalhe(TableRowClickEventArgs<Concurso> clique)
    {
        if (clique.Item is null)
        {
            return;
        }

        if (!this.expandidos.Remove(clique.Item.Numero))
        {
            this.expandidos.Add(clique.Item.Numero);
        }
    }
}
