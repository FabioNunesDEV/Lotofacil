// <copyright file="CampoTeimosinha.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Teimosinhas;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Teimosinhas;

/// <summary>
/// Switch que marca os jogos como Teimosinha e o nome dela, com os nomes das ativas como sugestão.
/// </summary>
public partial class CampoTeimosinha
{
    /// <summary>
    /// Identificador da lista de sugestões, único por campo para não colidir com outro na página.
    /// </summary>
    private readonly string idSugestoes = $"teimosinhas-{Guid.NewGuid():N}";

    /// <summary>
    /// Atributos do campo do nome, que o ligam à lista de sugestões.
    /// </summary>
    private readonly Dictionary<string, object?> atributosNome;

    /// <summary>
    /// Nomes das Teimosinhas ativas.
    /// </summary>
    private IReadOnlyList<string> sugestoes = [];

    /// <summary>
    /// Cria o campo ligado à própria lista de sugestões.
    /// </summary>
    public CampoTeimosinha()
    {
        this.atributosNome = new Dictionary<string, object?> { ["list"] = this.idSugestoes };
    }

    /// <summary>
    /// Indica se os jogos serão salvos como Teimosinha.
    /// </summary>
    [Parameter]
    public bool Ligada { get; set; }

    /// <summary>
    /// Avisa a troca do switch.
    /// </summary>
    [Parameter]
    public EventCallback<bool> LigadaChanged { get; set; }

    /// <summary>
    /// Nome da Teimosinha digitado.
    /// </summary>
    [Parameter]
    public string? Nome { get; set; }

    /// <summary>
    /// Avisa a troca do nome, a cada letra.
    /// </summary>
    [Parameter]
    public EventCallback<string?> NomeChanged { get; set; }

    /// <summary>
    /// Serviço das Teimosinhas, que fornece os nomes das ativas.
    /// </summary>
    [Inject]
    private TeimosinhaServico Servico { get; set; } = null!;

    /// <summary>
    /// Indica se o salvamento pode seguir: switch desligado, ou ligado com nome.
    /// </summary>
    /// <param name="ligada">Estado do switch.</param>
    /// <param name="nome">Nome digitado.</param>
    /// <returns><c>true</c> se o nome não falta.</returns>
    public static bool Completo(bool ligada, string? nome)
    {
        return !ligada || !string.IsNullOrWhiteSpace(nome);
    }

    /// <summary>
    /// Carrega os nomes das Teimosinhas ativas.
    /// </summary>
    /// <returns>Tarefa da carga.</returns>
    protected override async Task OnInitializedAsync()
    {
        IReadOnlyList<Teimosinha> ativas = await this.Servico.ListarAtivasAsync();
        this.sugestoes = ativas.Select(teimosinha => teimosinha.Nome).ToList();
    }

    /// <summary>
    /// Repassa a troca do switch.
    /// </summary>
    /// <param name="ligada">Novo estado.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task TrocarLigadaAsync(bool ligada)
    {
        return this.LigadaChanged.InvokeAsync(ligada);
    }

    /// <summary>
    /// Repassa a troca do nome.
    /// </summary>
    /// <param name="nome">Nome digitado.</param>
    /// <returns>Tarefa do aviso.</returns>
    private Task TrocarNomeAsync(string? nome)
    {
        return this.NomeChanged.InvokeAsync(nome);
    }
}
