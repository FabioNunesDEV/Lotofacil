// <copyright file="OpcoesFonteResultados.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Configuração do acesso à API de loterias da Caixa.
/// </summary>
/// <param name="UrlBase">Endereço base da API (ex.: <c>https://servicebus3.caixa.gov.br/portaldeloterias</c>).</param>
/// <param name="TempoLimiteSegundos">Tempo máximo de espera por uma resposta, em segundos.</param>
public sealed record OpcoesFonteResultados(string UrlBase, int TempoLimiteSegundos);
