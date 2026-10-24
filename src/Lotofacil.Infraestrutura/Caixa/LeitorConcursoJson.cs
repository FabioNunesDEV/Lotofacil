// <copyright file="LeitorConcursoJson.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using System.Text;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;
using Newtonsoft.Json;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Converte as respostas JSON da API da Caixa em número do último concurso, em concurso validado ou só na ordem do
/// sorteio.
/// </summary>
internal static class LeitorConcursoJson
{
    /// <summary>
    /// Leitura com valores decimais lidos direto do texto, sem passar por <c>double</c>, e datas como texto.
    /// </summary>
    private static readonly JsonSerializerSettings Configuracao = new()
    {
        FloatParseHandling = FloatParseHandling.Decimal,
        DateParseHandling = DateParseHandling.None,
    };

    /// <summary>
    /// Lê o número da resposta do último concurso divulgado.
    /// </summary>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <returns>Número do último concurso.</returns>
    public static int LerNumero(byte[] conteudo)
    {
        const string Descricao = "o último concurso";
        ConcursoCaixaJson dados = Desserializar(conteudo, Descricao);

        return dados.Numero ?? throw FormatoInesperado(Descricao);
    }

    /// <summary>
    /// Lê e valida o concurso pedido; lança <see cref="FonteResultadosException"/> no primeiro problema.
    /// </summary>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <param name="numeroPedido">Número do concurso que foi pedido.</param>
    /// <returns>Concurso lido.</returns>
    public static Concurso LerConcurso(byte[] conteudo, int numeroPedido)
    {
        string descricao = $"o concurso {numeroPedido}";
        ConcursoCaixaJson dados = Desserializar(conteudo, descricao);
        if (!TemCamposObrigatorios(dados))
        {
            throw FormatoInesperado(descricao);
        }

        if (dados.Numero != numeroPedido)
        {
            throw new FonteResultadosException(
                $"A Caixa devolveu o concurso {dados.Numero} ao buscar o concurso {numeroPedido}.");
        }

        List<RateioCaixaJson> faixas = dados.ListaRateioPremio!;
        if (faixas.Count == 0 || faixas.All(faixa => faixa.NumeroDeGanhadores == 0))
        {
            throw new FonteResultadosException(
                $"O concurso {numeroPedido} já foi sorteado, mas a Caixa ainda não divulgou o rateio. Tente atualizar mais tarde.");
        }

        bool dataValida = DateOnly.TryParseExact(
            dados.DataApuracao,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly data);
        List<int>? dezenas = LerDezenas(dados.ListaDezenas!);
        List<string> textosOrdem = dados.DezenasOrdemSorteio ?? [];
        List<int>? ordem = LerDezenas(textosOrdem);
        if (!dataValida || dezenas is null || ordem is null || !FaixasComDescricaoEsperada(faixas))
        {
            throw FormatoInesperado(descricao);
        }

        try
        {
            List<Rateio> rateios = faixas
                .Select(faixa => new Rateio(Acertos(faixa.Faixa!.Value), faixa.NumeroDeGanhadores!.Value, faixa.ValorPremio!.Value))
                .ToList();

            return new Concurso(
                numeroPedido,
                data,
                dezenas,
                textosOrdem.Count > 0 ? ordem : null,
                rateios,
                dados.ValorArrecadado!.Value,
                dados.ValorAcumuladoProximoConcurso!.Value,
                dados.ValorEstimadoProximoConcurso!.Value,
                dados.ValorAcumuladoConcursoEspecial!.Value);
        }
        catch (ArgumentException erro)
        {
            throw new FonteResultadosException($"Concurso {numeroPedido}: {MensagemValidacao.SemParametro(erro)}", erro);
        }
    }

    /// <summary>
    /// Lê só a ordem do sorteio do concurso pedido, sem exigir os demais campos; lança
    /// <see cref="FonteResultadosException"/> se a resposta for de outro concurso ou estiver fora do formato.
    /// </summary>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <param name="numeroPedido">Número do concurso que foi pedido.</param>
    /// <returns>As dezenas na ordem do sorteio, ou <c>null</c> se a resposta não trouxer a ordem.</returns>
    public static IReadOnlyList<int>? LerOrdemSorteio(byte[] conteudo, int numeroPedido)
    {
        string descricao = $"o concurso {numeroPedido}";
        ConcursoCaixaJson dados = Desserializar(conteudo, descricao);
        if (dados.Numero is null)
        {
            throw FormatoInesperado(descricao);
        }

        if (dados.Numero != numeroPedido)
        {
            throw new FonteResultadosException(
                $"A Caixa devolveu o concurso {dados.Numero} ao buscar o concurso {numeroPedido}.");
        }

        List<string> textosOrdem = dados.DezenasOrdemSorteio ?? [];
        if (textosOrdem.Count == 0)
        {
            return null;
        }

        return LerDezenas(textosOrdem) ?? throw FormatoInesperado(descricao);
    }

    /// <summary>
    /// Converte o JSON nos campos usados.
    /// </summary>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <param name="descricao">Descrição do que foi pedido, usada na mensagem de erro.</param>
    /// <returns>Campos lidos.</returns>
    private static ConcursoCaixaJson Desserializar(byte[] conteudo, string descricao)
    {
        try
        {
            string texto = Encoding.UTF8.GetString(conteudo);

            return JsonConvert.DeserializeObject<ConcursoCaixaJson>(texto, Configuracao)
                ?? throw FormatoInesperado(descricao);
        }
        catch (JsonException erro)
        {
            throw new FonteResultadosException(MensagemFormatoInesperado(descricao), erro);
        }
    }

    /// <summary>
    /// Confere se a resposta tem todos os campos usados na conversão.
    /// </summary>
    /// <param name="dados">Campos lidos.</param>
    /// <returns><c>true</c> se nenhum campo obrigatório estiver ausente.</returns>
    private static bool TemCamposObrigatorios(ConcursoCaixaJson dados)
    {
        bool rateioCompleto = dados.ListaRateioPremio?.All(faixa =>
            faixa.Faixa is not null
            && faixa.DescricaoFaixa is not null
            && faixa.NumeroDeGanhadores is not null
            && faixa.ValorPremio is not null) ?? false;

        return dados.Numero is not null
            && dados.DataApuracao is not null
            && dados.ListaDezenas is not null
            && rateioCompleto
            && dados.ValorArrecadado is not null
            && dados.ValorAcumuladoProximoConcurso is not null
            && dados.ValorEstimadoProximoConcurso is not null
            && dados.ValorAcumuladoConcursoEspecial is not null;
    }

    /// <summary>
    /// Confere se cada faixa tem a descrição correspondente à sua posição (faixa 1 = "15 acertos").
    /// </summary>
    /// <param name="faixas">Faixas do rateio.</param>
    /// <returns><c>true</c> se todas as descrições conferem.</returns>
    private static bool FaixasComDescricaoEsperada(List<RateioCaixaJson> faixas)
    {
        return faixas.All(faixa => faixa.DescricaoFaixa == $"{Acertos(faixa.Faixa!.Value)} acertos");
    }

    /// <summary>
    /// Converte a posição da faixa na API na quantidade de acertos: 1 → 15, ..., 5 → 11.
    /// </summary>
    /// <param name="faixa">Posição da faixa na API.</param>
    /// <returns>Quantidade de acertos.</returns>
    private static int Acertos(int faixa)
    {
        return RegrasLotofacil.MaiorFaixaPremiada + 1 - faixa;
    }

    /// <summary>
    /// Converte as dezenas em texto (<c>"01"</c>) em números.
    /// </summary>
    /// <param name="textos">Dezenas em texto.</param>
    /// <returns>Dezenas, ou <c>null</c> se alguma não for um número.</returns>
    private static List<int>? LerDezenas(List<string> textos)
    {
        List<int> dezenas = [];
        foreach (string texto in textos)
        {
            if (!int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out int dezena))
            {
                return null;
            }

            dezenas.Add(dezena);
        }

        return dezenas;
    }

    /// <summary>
    /// Cria o erro de resposta fora do formato esperado.
    /// </summary>
    /// <param name="descricao">Descrição do que foi pedido (ex.: "o concurso 3809").</param>
    /// <returns>Exceção com a mensagem para o usuário.</returns>
    private static FonteResultadosException FormatoInesperado(string descricao)
    {
        return new FonteResultadosException(MensagemFormatoInesperado(descricao));
    }

    /// <summary>
    /// Monta a mensagem de resposta fora do formato esperado.
    /// </summary>
    /// <param name="descricao">Descrição do que foi pedido.</param>
    /// <returns>Mensagem para o usuário.</returns>
    private static string MensagemFormatoInesperado(string descricao)
    {
        return $"A resposta da Caixa para {descricao} não está no formato esperado.";
    }
}
