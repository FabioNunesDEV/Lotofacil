// <copyright file="RespostaConcursoCaixa.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Text;
using Newtonsoft.Json.Linq;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Monta respostas JSON da API da Caixa a partir da resposta real do concurso 3808, alterando só o necessário.
/// </summary>
internal static class RespostaConcursoCaixa
{
    /// <summary>
    /// Caminho da resposta real do concurso 3808.
    /// </summary>
    private static readonly string CaminhoAmostra = Path.Combine(AppContext.BaseDirectory, "Arquivos", "lotofacil-3808.json");

    /// <summary>
    /// Lê a resposta real do concurso 3808 como objeto editável.
    /// </summary>
    /// <returns>JSON da resposta.</returns>
    public static JObject LerAmostra()
    {
        return JObject.Parse(File.ReadAllText(CaminhoAmostra));
    }

    /// <summary>
    /// Cria a resposta de um concurso com o número informado e o restante igual ao 3808.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <returns>Corpo da resposta.</returns>
    public static byte[] ComNumero(int numero)
    {
        JObject resposta = LerAmostra();
        resposta["numero"] = numero;

        return EmBytes(resposta);
    }

    /// <summary>
    /// Converte o JSON no corpo da resposta.
    /// </summary>
    /// <param name="resposta">JSON da resposta.</param>
    /// <returns>Corpo da resposta em UTF-8.</returns>
    public static byte[] EmBytes(JObject resposta)
    {
        return Encoding.UTF8.GetBytes(resposta.ToString());
    }
}
