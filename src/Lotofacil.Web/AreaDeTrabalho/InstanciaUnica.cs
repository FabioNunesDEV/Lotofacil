// <copyright file="InstanciaUnica.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web.AreaDeTrabalho;

/// <summary>
/// Garante uma só execução do app por usuário, por um mutex nomeado na sessão do Windows.
/// </summary>
public sealed class InstanciaUnica : IDisposable
{
    /// <summary>
    /// Nome do mutex; o instalador usa o mesmo nome para pedir que o app seja fechado.
    /// </summary>
    public const string NomeMutex = "Lotofacil.Instancia";

    /// <summary>
    /// Mutex mantido aberto enquanto o app roda.
    /// </summary>
    private readonly Mutex mutex;

    /// <summary>
    /// Abre ou cria o mutex com o nome informado.
    /// </summary>
    /// <param name="nome">Nome do mutex.</param>
    public InstanciaUnica(string nome = NomeMutex)
    {
        this.mutex = new Mutex(initiallyOwned: false, nome, out bool criado);
        this.Primeira = criado;
    }

    /// <summary>
    /// Indica que nenhuma outra execução estava aberta.
    /// </summary>
    public bool Primeira { get; }

    /// <summary>
    /// Fecha o mutex; com o último aberto, o nome deixa de existir.
    /// </summary>
    public void Dispose()
    {
        this.mutex.Dispose();
    }
}
