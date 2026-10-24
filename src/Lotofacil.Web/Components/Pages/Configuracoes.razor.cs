// <copyright file="Configuracoes.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web.Components.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Página Configurações: preço da aposta mínima e a tabela de preços de 15 a 20 dezenas.
/// </summary>
public partial class Configuracoes
{
    /// <summary>
    /// Aviso exibido no campo quando o preço é inválido.
    /// </summary>
    public const string MensagemPrecoInvalido = "Informe um preço entre R$ 0,01 e R$ 100,00, com até dois decimais.";

    /// <summary>
    /// Mensagem exibida depois de salvar.
    /// </summary>
    public const string MensagemSalvo = "Configurações salvas.";

    /// <summary>
    /// Preço digitado no campo; <c>null</c> com o campo vazio.
    /// </summary>
    private decimal? preco;

    /// <summary>
    /// Campo do preço, validado de novo ao salvar.
    /// </summary>
    private MudNumericField<decimal?> campoPreco = null!;

    /// <summary>
    /// Serviço das configurações, com o preço em uso.
    /// </summary>
    [Inject]
    private ConfiguracoesServico Servico { get; set; } = null!;

    /// <summary>
    /// Mensagens rápidas exibidas ao usuário.
    /// </summary>
    [Inject]
    private ISnackbar Mensagens { get; set; } = null!;

    /// <summary>
    /// Indica se o campo tem um preço aceito.
    /// </summary>
    private bool PrecoValido => this.preco is decimal valor && ConfiguracaoApostas.PrecoValido(valor);

    /// <summary>
    /// Preenche o campo com o preço em uso.
    /// </summary>
    protected override void OnInitialized()
    {
        this.preco = this.Servico.PrecoApostaMinima;
    }

    /// <summary>
    /// Validação do campo do preço.
    /// </summary>
    /// <param name="valor">Preço digitado; <c>null</c> com o campo vazio.</param>
    /// <returns>O aviso, ou <c>null</c> com um preço aceito.</returns>
    private static string? ValidarPreco(decimal? valor)
    {
        return valor is decimal preco && ConfiguracaoApostas.PrecoValido(preco) ? null : MensagemPrecoInvalido;
    }

    /// <summary>
    /// Valor de uma aposta com a quantidade de dezenas, pelo preço digitado.
    /// </summary>
    /// <param name="dezenas">Quantidade de dezenas da aposta.</param>
    /// <returns>Valor em reais, ou "—" com o preço inválido.</returns>
    private string Valor(int dezenas)
    {
        return this.preco is decimal valor && this.PrecoValido
            ? FormatacaoJogo.Reais(Jogo.CalcularQuantidadeApostas(dezenas) * valor)
            : "—";
    }

    /// <summary>
    /// Grava o preço digitado; com preço inválido, só mostra o aviso no campo.
    /// </summary>
    /// <returns>Tarefa da gravação.</returns>
    private async Task SalvarAsync()
    {
        await this.campoPreco.ValidateAsync();
        if (!this.PrecoValido || this.preco is not decimal valor)
        {
            return;
        }

        await this.Servico.SalvarPrecoApostaMinimaAsync(valor);
        this.Mensagens.Add(MensagemSalvo, Severity.Success);
    }
}
