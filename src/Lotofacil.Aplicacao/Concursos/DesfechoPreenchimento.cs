// <copyright file="DesfechoPreenchimento.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Como o preenchimento da ordem do sorteio terminou.
/// </summary>
public enum DesfechoPreenchimento
{
    /// <summary>
    /// Todos os concursos que faltavam foram buscados.
    /// </summary>
    Concluido,

    /// <summary>
    /// O usuário interrompeu, ou saiu da página.
    /// </summary>
    Interrompido,

    /// <summary>
    /// Parou por falhas seguidas.
    /// </summary>
    EncerradoPorFalhas,

    /// <summary>
    /// Parou porque a Caixa continuou limitando as consultas depois de todas as esperas.
    /// </summary>
    EncerradoPorLimite,
}
