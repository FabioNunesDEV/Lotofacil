// <copyright file="Teimosinhas.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Web.Components.Teimosinhas;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Lista as Teimosinhas ativas, com os jogos e o custo por concurso, e permite removê-las.
/// </summary>
public partial class Teimosinhas
{
    /// <summary>
    /// Teimosinhas ativas, em ordem de nome.
    /// </summary>
    private IReadOnlyList<Teimosinha> ativas = [];

    /// <summary>
    /// Indica que a primeira carga terminou.
    /// </summary>
    private bool carregado;

    /// <summary>
    /// Serviço das Teimosinhas.
    /// </summary>
    [Inject]
    private TeimosinhaServico Servico { get; set; } = null!;

    /// <summary>
    /// Configurações, com o preço da aposta usado no custo.
    /// </summary>
    [Inject]
    private ConfiguracoesServico Configuracoes { get; set; } = null!;

    /// <summary>
    /// Serviço de diálogos do MudBlazor, usado na confirmação.
    /// </summary>
    [Inject]
    private IDialogService Dialogos { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Carrega as Teimosinhas ao abrir a página.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    protected override async Task OnInitializedAsync()
    {
        this.ativas = await this.Servico.ListarAtivasAsync();
        this.carregado = true;
    }

    /// <summary>
    /// Custo dos jogos da Teimosinha em um concurso.
    /// </summary>
    /// <param name="teimosinha">Teimosinha.</param>
    /// <returns>Soma das apostas de 15 de cada jogo × preço da aposta mínima.</returns>
    private decimal Custo(Teimosinha teimosinha)
    {
        int apostas = teimosinha.Jogos.Sum(jogo => Jogo.CalcularQuantidadeApostas(jogo.Dezenas.Count));

        return apostas * this.Configuracoes.PrecoApostaMinima;
    }

    /// <summary>
    /// Pede confirmação e remove a Teimosinha.
    /// </summary>
    /// <param name="teimosinha">Teimosinha a remover.</param>
    /// <returns>Tarefa da remoção.</returns>
    private async Task RemoverAsync(Teimosinha teimosinha)
    {
        bool? confirmado = await this.Dialogos.ShowMessageBoxAsync(
            "Remover Teimosinha",
            $"Remover a Teimosinha {teimosinha.Nome}? Os jogos já criados continuam em Meus jogos.",
            yesText: "Remover",
            cancelText: "Cancelar");
        if (confirmado != true)
        {
            return;
        }

        await this.Servico.RemoverAsync(teimosinha.Id);
        this.Mensagens.Add(FormatacaoTeimosinha.Removida, Severity.Success);
        this.ativas = await this.Servico.ListarAtivasAsync();
    }
}
