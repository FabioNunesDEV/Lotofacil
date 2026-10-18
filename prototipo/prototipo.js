// Protótipo visual: navegação e montagem das telas a partir de dados fictícios.
// Nenhuma requisição é feita e nada é gravado.

const icones = {
    menu: 'M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z',
    painel: 'M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z',
    historico: 'M13 3a9 9 0 0 0-9 9H1l3.89 3.89.07.14L9 12H6c0-3.87 3.13-7 7-7s7 3.13 7 7-3.13 7-7 7c-1.93 0-3.68-.79-4.94-2.06l-1.42 1.42A8.954 8.954 0 0 0 13 21a9 9 0 0 0 0-18zm-1 5v5l4.28 2.54.72-1.21-3.5-2.08V8H12z',
    grafico: 'M5 9.2h3V19H5zM10.6 5h2.8v14h-2.8zm5.6 8H19v6h-2.8z',
    dado: 'M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM7.5 18c-.83 0-1.5-.67-1.5-1.5S6.67 15 7.5 15s1.5.67 1.5 1.5S8.33 18 7.5 18zm0-9C6.67 9 6 8.33 6 7.5S6.67 6 7.5 6 9 6.67 9 7.5 8.33 9 7.5 9zm4.5 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm4.5 4.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm0-9c-.83 0-1.5-.67-1.5-1.5S15.67 6 16.5 6s1.5.67 1.5 1.5S17.33 9 16.5 9z',
    conferir: 'M20 3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM10 17H5v-2h5v2zm0-4H5v-2h5v2zm0-4H5V7h5v2zm4.82 6L12 12.16l1.41-1.41 1.41 1.42L17.99 9l1.42 1.42L14.82 15z',
    engrenagem: 'M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58a.49.49 0 0 0 .12-.61l-1.92-3.32a.488.488 0 0 0-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54a.484.484 0 0 0-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58a.49.49 0 0 0-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z',
    atualizar: 'M17.65 6.35A7.958 7.958 0 0 0 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08A5.99 5.99 0 0 1 12 18c-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z',
    banco: 'M12 3C7.58 3 4 4.79 4 7v10c0 2.21 3.59 4 8 4s8-1.79 8-4V7c0-2.21-3.58-4-8-4zm6 14c0 .5-2.13 2-6 2s-6-1.5-6-2v-2.23c1.61.78 3.72 1.23 6 1.23s4.39-.45 6-1.23V17zm0-4.55c-1.3.95-3.58 1.55-6 1.55s-4.7-.6-6-1.55V9.64c1.47.83 3.61 1.36 6 1.36s4.53-.53 6-1.36v2.81zM12 9C8.13 9 6 7.5 6 7s2.13-2 6-2 6 1.5 6 2-2.13 2-6 2z',
    calendario: 'M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zM9 14H7v-2h2v2zm4 0h-2v-2h2v2zm4 0h-2v-2h2v2zm-8 4H7v-2h2v2zm4 0h-2v-2h2v2zm4 0h-2v-2h2v2z',
    relogio: 'M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z',
    trofeu: 'M19 5h-2V3H7v2H5c-1.1 0-2 .9-2 2v1c0 2.55 1.92 4.63 4.39 4.94A5.01 5.01 0 0 0 11 15.9V19H7v2h10v-2h-4v-3.1a5.01 5.01 0 0 0 3.61-2.96C19.08 12.63 21 10.55 21 8V7c0-1.1-.9-2-2-2zM5 8V7h2v3.82C5.84 10.4 5 9.3 5 8zm14 0c0 1.3-.84 2.4-2 2.82V7h2v1z',
    busca: 'M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.1-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z',
    anterior: 'M15.41 7.41 14 6l-6 6 6 6 1.41-1.41L10.83 12z',
    proximo: 'M10 6 8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z',
    expandir: 'M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z',
    fechar: 'M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z',
    info: 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z',
    salvar: 'M17 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V7l-4-4zm-5 16c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3zm3-10H5V5h10v4z',
    dinheiro: 'M11.8 10.9c-2.27-.59-3-1.2-3-2.15 0-1.09 1.01-1.85 2.7-1.85 1.78 0 2.44.85 2.5 2.1h2.21c-.07-1.72-1.12-3.3-3.21-3.81V3h-3v2.16c-1.94.42-3.5 1.68-3.5 3.61 0 2.31 1.91 3.46 4.7 4.13 2.5.6 3 1.48 3 2.41 0 .69-.49 1.79-2.7 1.79-2.06 0-2.87-.92-2.98-2.1h-2.2c.12 2.19 1.76 3.42 3.68 3.83V21h3v-2.15c1.95-.37 3.5-1.5 3.5-3.55 0-2.84-2.43-3.81-4.7-4.4z',
    importar: 'M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z',
    lixeira: 'M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z',
};

const formatoMoeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
const formatoNumero = new Intl.NumberFormat('pt-BR');
const formatoDecimal = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
const formatoPercentual = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const ultimoConcurso = dadosMock.concursos[0];
const frequenciaEsperada = dadosMock.totalConcursos * 0.6;
const desvioPadraoEsperado = Math.sqrt(dadosMock.totalConcursos * 0.6 * 0.4);

// ---------- Utilitários de exibição ----------

function svgIcone(nome) {
    return `<svg viewBox="0 0 24 24" aria-hidden="true"><path d="${icones[nome]}"/></svg>`;
}

function aplicarIcones(raiz = document) {
    raiz.querySelectorAll('[data-icone]').forEach(elemento => {
        if (!elemento.querySelector(':scope > svg')) {
            elemento.insertAdjacentHTML('afterbegin', svgIcone(elemento.dataset.icone));
        }
    });
}

function formatarComSinal(valor) {
    const texto = formatoDecimal.format(valor);
    if (texto === '0,0' || texto === '-0,0') return '0,0';
    return valor > 0 ? `+${texto}` : texto;
}

function formatarDezena(dezena) {
    return String(dezena).padStart(2, '0');
}

function montarBolas(dezenas, classeDaDezena = () => '') {
    return dezenas.map(dezena => `<span class="bola ${classeDaDezena(dezena)}">${formatarDezena(dezena)}</span>`).join('');
}

function mostrarAviso(mensagem) {
    const snackbar = document.getElementById('snackbar');
    snackbar.textContent = mensagem;
    snackbar.classList.add('visivel');
    clearTimeout(mostrarAviso.temporizador);
    mostrarAviso.temporizador = setTimeout(() => snackbar.classList.remove('visivel'), 3500);
}

function contarAcertos(jogo, dezenasSorteadas) {
    return jogo.filter(dezena => dezenasSorteadas.includes(dezena)).length;
}

function calcularPremio(acertos, concurso) {
    if (acertos === 15) return concurso.premio15;
    if (acertos === 14) return concurso.premio14;
    return dadosMock.premiosFixos[acertos] ?? 0;
}

function buscarConcurso(numero) {
    return dadosMock.concursos.find(concurso => concurso.numero === numero);
}

function combinacoes(n, k) {
    let resultado = 1;
    for (let i = 1; i <= k; i++) resultado = resultado * (n - k + i) / i;
    return Math.round(resultado);
}

function linhasRateio(concurso) {
    const faixas = [15, 14, 13, 12, 11];
    return faixas.map((faixa, indice) => {
        const ganhadores = concurso.ganhadores[indice];
        const premio = ganhadores === 0 ? '<span class="chip aviso">Acumulou</span>' : formatoMoeda.format(calcularPremio(faixa, concurso));
        return `<tr><td>${faixa} acertos</td><td class="numero">${formatoNumero.format(ganhadores)}</td><td class="numero">${premio}</td></tr>`;
    }).join('');
}

function itemRanking(dezena, texto, classe = 'sorteada') {
    return `<li><span class="bola ${classe}">${formatarDezena(dezena)}</span><span class="valor">${texto}</span></li>`;
}

function dezenasOrdenadasPor(valores, decrescente) {
    const dezenas = valores.map((valor, indice) => ({ dezena: indice + 1, valor }));
    return dezenas.sort((a, b) => decrescente ? b.valor - a.valor : a.valor - b.valor);
}

// ---------- Navegação ----------

function navegar() {
    const paginaAtual = location.hash.slice(1) || 'painel';
    document.querySelectorAll('.pagina').forEach(pagina => pagina.classList.toggle('visivel', pagina.id === `pagina-${paginaAtual}`));
    document.querySelectorAll('.menu-lateral a').forEach(link => link.classList.toggle('ativo', link.getAttribute('href') === `#${paginaAtual}`));
    document.body.classList.remove('menu-aberto');
    document.getElementById('dialogo-conferencia').classList.remove('visivel');
    window.scrollTo(0, 0);
}

function configurarMenu() {
    document.getElementById('botao-menu').addEventListener('click', () => {
        const telaPequena = window.matchMedia('(max-width: 760px)').matches;
        document.body.classList.toggle(telaPequena ? 'menu-aberto' : 'menu-fechado');
    });
    window.addEventListener('hashchange', navegar);
}

function configurarComponentesGenericos() {
    document.querySelectorAll('.filtro-chips').forEach(grupo => {
        grupo.addEventListener('click', evento => {
            const botao = evento.target.closest('button');
            if (!botao) return;
            grupo.querySelectorAll('button').forEach(b => b.classList.toggle('selecionado', b === botao));
        });
    });

    document.querySelectorAll('[data-grupo-abas]').forEach(grupo => {
        grupo.addEventListener('click', evento => {
            const botao = evento.target.closest('button');
            if (!botao) return;
            grupo.querySelectorAll('button').forEach(b => {
                b.classList.toggle('selecionada', b === botao);
                document.getElementById(b.dataset.aba).classList.toggle('visivel', b === botao);
            });
        });
    });

    document.querySelectorAll('[data-acao-prototipo]').forEach(botao => {
        botao.addEventListener('click', () => mostrarAviso(`${botao.dataset.acaoPrototipo} (protótipo: nada foi executado)`));
    });

    document.getElementById('filtro-periodo').addEventListener('click', () =>
        mostrarAviso('Protótipo: os números não mudam com o período.'));
}

// ---------- Painel ----------

function renderizarPainel() {
    document.getElementById('painel-ultimo-resultado').innerHTML = montarBolas(ultimoConcurso.dezenas, () => 'sorteada');
    document.getElementById('painel-ordem-sorteio').textContent = ultimoConcurso.ordem.map(formatarDezena).join(' · ');
    document.getElementById('painel-rateio').innerHTML = linhasRateio(ultimoConcurso);

    const grupo = dadosMock.jogosSalvos.find(g => g.concurso === ultimoConcurso.numero);
    document.getElementById('painel-jogos').innerHTML = grupo.jogos.map((jogo, indice) =>
        montarLinhaJogo(jogo, indice, ultimoConcurso, false)).join('');

    document.getElementById('painel-total-premios').textContent = formatoMoeda.format(calcularResumoPremios().total);

    const maisSorteadas = dezenasOrdenadasPor(dadosMock.frequencias, true).slice(0, 5);
    document.getElementById('painel-mais-sorteadas').innerHTML = maisSorteadas
        .map(item => itemRanking(item.dezena, `${formatoNumero.format(item.valor)} vezes`)).join('');

    const maisAtrasadas = dezenasOrdenadasPor(dadosMock.atrasos, true).slice(0, 5);
    document.getElementById('painel-mais-atrasadas').innerHTML = maisAtrasadas
        .map(item => itemRanking(item.dezena, `${item.valor} concursos`, '')).join('');

    document.getElementById('botao-atualizar').addEventListener('click', simularAtualizacao);
}

function simularAtualizacao() {
    const etapas = ['Consultando último concurso…', 'Comparando com a base local…', 'Conferindo jogos salvos…'];
    const cartao = document.getElementById('progresso-atualizacao');
    const texto = document.getElementById('progresso-texto');
    const barra = document.getElementById('barra-progresso-valor');
    const botao = document.getElementById('botao-atualizar');

    cartao.classList.remove('oculto');
    botao.disabled = true;
    let etapa = 0;
    const avancar = () => {
        if (etapa < etapas.length) {
            texto.textContent = etapas[etapa];
            barra.style.width = `${((etapa + 1) / etapas.length) * 100}%`;
            etapa++;
            setTimeout(avancar, 700);
            return;
        }
        cartao.classList.add('oculto');
        barra.style.width = '0';
        botao.disabled = false;
        mostrarAviso('Base já atualizada: último concurso 3807. Próximo sorteio seg, 19/10/2026.');
    };
    avancar();
}

// ---------- Resultados ----------

function renderizarResultados() {
    const corpo = document.getElementById('tabela-resultados');
    corpo.innerHTML = dadosMock.concursos.map(concurso => {
        const ganhadores15 = concurso.ganhadores[0];
        const situacao = ganhadores15 === 0
            ? '<span class="chip aviso">Acumulou</span>'
            : '<span class="chip sucesso">Saiu</span>';
        const ordem = concurso.ordem
            ? `<p class="texto-secundario">Ordem do sorteio: ${concurso.ordem.map(formatarDezena).join(' · ')}</p>`
            : '';
        return `
            <tr class="clicavel" data-concurso="${concurso.numero}">
                <td><b>${concurso.numero}</b></td>
                <td>${concurso.data}</td>
                <td><div class="bolas">${montarBolas(concurso.dezenas, () => 'sorteada')}</div></td>
                <td class="numero">${ganhadores15}</td>
                <td class="numero">${ganhadores15 === 0 ? '—' : formatoMoeda.format(concurso.premio15)}</td>
                <td>${situacao}</td>
            </tr>
            <tr class="detalhe oculto" id="detalhe-${concurso.numero}">
                <td colspan="6">
                    <div class="grade-2">
                        <table class="tabela tabela-compacta">
                            <thead><tr><th>Faixa</th><th class="numero">Ganhadores</th><th class="numero">Prêmio</th></tr></thead>
                            <tbody>${linhasRateio(concurso)}</tbody>
                        </table>
                        <div>
                            <p>Arrecadação: <b>${formatoMoeda.format(concurso.arrecadacao)}</b></p>
                            ${ordem}
                        </div>
                    </div>
                </td>
            </tr>`;
    }).join('');

    corpo.addEventListener('click', evento => {
        const linha = evento.target.closest('tr.clicavel');
        if (linha) document.getElementById(`detalhe-${linha.dataset.concurso}`).classList.toggle('oculto');
    });
}

// ---------- Estatísticas ----------

const metricasMapa = {
    frequencia: {
        valor: dezena => dadosMock.frequencias[dezena - 1],
        texto: valor => formatoNumero.format(valor),
        legenda: 'Quantidade de concursos em que a dezena saiu. Mais escuro = mais sorteada.',
    },
    atraso: {
        valor: dezena => dadosMock.atrasos[dezena - 1],
        texto: valor => `${valor} conc.`,
        legenda: 'Concursos desde a última vez que a dezena saiu. Mais escuro = mais atrasada.',
    },
    desvio: {
        valor: dezena => (dadosMock.frequencias[dezena - 1] - frequenciaEsperada) / desvioPadraoEsperado,
        texto: valor => `${formatarComSinal(valor)}σ`,
        legenda: 'Distância da frequência esperada em desvios-padrão. Verde acima, vermelho abaixo do esperado.',
    },
};

function renderizarMapaCalor(nomeMetrica) {
    const metrica = metricasMapa[nomeMetrica];
    const dezenas = Array.from({ length: 25 }, (_, i) => i + 1);
    const valores = dezenas.map(metrica.valor);
    const minimo = Math.min(...valores);
    const maximo = Math.max(...valores);
    const maiorAbsoluto = Math.max(...valores.map(Math.abs));

    document.getElementById('mapa-calor').innerHTML = dezenas.map((dezena, indice) => {
        const valor = valores[indice];
        let cor;
        let intensidade;
        if (nomeMetrica === 'desvio') {
            intensidade = Math.round((Math.abs(valor) / maiorAbsoluto) * 85);
            cor = valor >= 0 ? 'var(--sucesso)' : 'var(--erro)';
        } else {
            intensidade = Math.round(((valor - minimo) / (maximo - minimo || 1)) * 85) + 5;
            cor = 'var(--primaria)';
        }
        const corTexto = intensidade > 50 ? '#fff' : 'var(--texto)';
        return `<div class="celula-calor" style="background: color-mix(in srgb, ${cor} ${intensidade}%, white); color: ${corTexto}">
                    <strong>${formatarDezena(dezena)}</strong><span>${metrica.texto(valor)}</span>
                </div>`;
    }).join('');
    document.getElementById('legenda-mapa').textContent = metrica.legenda;
}

function renderizarRankings() {
    const porFrequencia = dezenasOrdenadasPor(dadosMock.frequencias, true);
    const porAtraso = dezenasOrdenadasPor(dadosMock.atrasos, true);

    document.getElementById('ranking-mais-sorteadas').innerHTML = porFrequencia.slice(0, 5)
        .map(item => itemRanking(item.dezena, formatoNumero.format(item.valor))).join('');
    document.getElementById('ranking-menos-sorteadas').innerHTML = porFrequencia.slice(-5).reverse()
        .map(item => itemRanking(item.dezena, formatoNumero.format(item.valor), '')).join('');
    document.getElementById('ranking-mais-atrasadas').innerHTML = porAtraso.slice(0, 5)
        .map(item => itemRanking(item.dezena, `${item.valor} conc.`, '')).join('');

    const semAtraso = porAtraso.filter(item => item.valor === 0);
    document.getElementById('ranking-menos-atrasadas').innerHTML = semAtraso.slice(0, 4)
        .map(item => itemRanking(item.dezena, 'último concurso')).join('')
        + `<li class="texto-secundario">+${semAtraso.length - 4} empatadas com atraso 0</li>`;
}

function renderizarTabelaDezenas() {
    document.getElementById('tabela-dezenas').innerHTML = dadosMock.frequencias.map((frequencia, indice) => {
        const diferenca = frequencia - frequenciaEsperada;
        const desvio = diferenca / desvioPadraoEsperado;
        const classe = diferenca >= 0 ? 'positivo' : 'negativo';
        const atraso = dadosMock.atrasos[indice];
        return `<tr>
            <td><span class="bola">${formatarDezena(indice + 1)}</span></td>
            <td class="numero">${formatoNumero.format(frequencia)}</td>
            <td class="numero">${formatoPercentual.format((frequencia / dadosMock.totalConcursos) * 100)}%</td>
            <td class="numero ${classe}">${formatarComSinal(diferenca)}</td>
            <td class="numero ${classe}">${formatarComSinal(desvio)}</td>
            <td class="numero">${atraso}</td>
            <td class="numero">${ultimoConcurso.numero - atraso}</td>
        </tr>`;
    }).join('');
}

function renderizarPerfis() {
    document.getElementById('grade-perfis').innerHTML = dadosMock.perfis.map(perfil => {
        const escala = Math.max(...perfil.linhas.map(l => Math.max(l.observado, l.teorico))) * 1.1;
        const linhas = perfil.linhas.map(linha => {
            const atraso = linha.atraso === 0
                ? '<span class="chip primario">último</span>'
                : `${linha.atraso}`;
            return `<tr>
                <td>${linha.faixa}</td>
                <td class="coluna-barra"><div class="barra-perfil">
                    <span class="observado" style="width:${(linha.observado / escala) * 100}%"></span>
                    <span class="teorico" style="left:${(linha.teorico / escala) * 100}%"></span>
                </div></td>
                <td class="numero">${formatoPercentual.format(linha.observado)}%</td>
                <td class="numero texto-secundario">${formatoPercentual.format(linha.teorico)}%</td>
                <td class="numero">${atraso}</td>
            </tr>`;
        }).join('');
        return `<div class="card">
            <div class="card-cabecalho"><div><h2>${perfil.titulo}</h2><p class="subtitulo">${perfil.descricao}</p></div></div>
            <div class="tabela-rolagem"><table class="tabela tabela-compacta">
                <thead><tr><th>Faixa</th><th></th><th class="numero">Obs.</th><th class="numero">Teór.</th><th class="numero">Atraso</th></tr></thead>
                <tbody>${linhas}</tbody>
            </table></div>
        </div>`;
    }).join('');
}

function renderizarEstatisticas() {
    renderizarMapaCalor('frequencia');
    renderizarRankings();
    renderizarTabelaDezenas();
    renderizarPerfis();

    const seletor = document.getElementById('seletor-mapa');
    seletor.addEventListener('click', evento => {
        const botao = evento.target.closest('button');
        if (!botao) return;
        seletor.querySelectorAll('button').forEach(b => b.classList.toggle('selecionado', b === botao));
        renderizarMapaCalor(botao.dataset.metrica);
    });
}

// ---------- Gerar jogos ----------

const estadoVolante = {};
[5, 10, 12].forEach(dezena => { estadoVolante[dezena] = 'fixa'; });
let jogosGerados = [];

function renderizarVolanteGerador() {
    const dezenas = Array.from({ length: 25 }, (_, i) => i + 1);
    document.getElementById('volante-gerador').innerHTML = dezenas
        .map(dezena => `<span class="bola ${estadoVolante[dezena] ?? ''}" data-dezena="${dezena}">${formatarDezena(dezena)}</span>`)
        .join('');
    const estados = Object.values(estadoVolante);
    document.getElementById('contador-fixas').textContent = estados.filter(e => e === 'fixa').length;
    document.getElementById('contador-excluidas').textContent = estados.filter(e => e === 'excluida').length;
}

function alternarDezena(dezena) {
    const proximoEstado = { undefined: 'fixa', fixa: 'excluida', excluida: undefined };
    const novoEstado = proximoEstado[estadoVolante[dezena]];
    if (novoEstado) estadoVolante[dezena] = novoEstado;
    else delete estadoVolante[dezena];
    renderizarVolanteGerador();
}

function atualizarResumoCusto() {
    const quantidade = Number(document.getElementById('campo-quantidade').value) || 0;
    const dezenasPorJogo = Number(document.getElementById('campo-dezenas-por-jogo').value);
    const valorJogo = combinacoes(dezenasPorJogo, 15) * dadosMock.precoApostaMinima;
    document.getElementById('resumo-custo-texto').textContent = `${quantidade} jogos × ${formatoMoeda.format(valorJogo)}`;
    document.getElementById('resumo-custo-valor').textContent = formatoMoeda.format(quantidade * valorJogo);
}

// Sorteio simples só para preencher a tela; a estratégia real ainda será definida.
function sortearJogoFicticio(dezenasPorJogo) {
    const fixas = Object.keys(estadoVolante).filter(d => estadoVolante[d] === 'fixa').map(Number);
    const livres = Array.from({ length: 25 }, (_, i) => i + 1).filter(d => !estadoVolante[d]);
    const embaralhadas = livres.sort(() => Math.random() - 0.5);
    return [...fixas, ...embaralhadas.slice(0, dezenasPorJogo - fixas.length)].sort((a, b) => a - b);
}

function descreverPerfil(jogo) {
    const pares = jogo.filter(d => d % 2 === 0).length;
    const soma = jogo.reduce((total, d) => total + d, 0);
    const repetidas = contarAcertos(jogo, ultimoConcurso.dezenas);
    return `<span class="chip">${pares}P / ${jogo.length - pares}I</span>
            <span class="chip">Soma ${soma}</span>
            <span class="chip">${repetidas} rep.</span>`;
}

function renderizarJogosGerados() {
    const lista = document.getElementById('lista-gerados');
    const alvo = document.getElementById('campo-concurso-alvo');
    const dezenasPorJogo = Number(document.getElementById('campo-dezenas-por-jogo').value);
    const valorTotal = jogosGerados.length * combinacoes(dezenasPorJogo, 15) * dadosMock.precoApostaMinima;

    lista.innerHTML = jogosGerados.map((jogo, indice) => `
        <div class="linha-jogo">
            <span class="indice">${String(indice + 1).padStart(2, '0')}</span>
            <div>
                <div class="bolas">${montarBolas(jogo, d => estadoVolante[d] === 'fixa' ? 'fixa' : '')}</div>
                <div class="chips">${descreverPerfil(jogo)}</div>
            </div>
            <div class="resultado-jogo">
                <button class="botao-icone" data-remover="${indice}" data-icone="lixeira" aria-label="Remover jogo"></button>
            </div>
        </div>`).join('');
    aplicarIcones(lista);

    document.getElementById('subtitulo-gerados').textContent =
        `${jogosGerados.length} jogos para o concurso ${alvo.selectedOptions[0].text} · ${formatoMoeda.format(valorTotal)}`;
    document.getElementById('acoes-gerados').classList.toggle('oculto', jogosGerados.length === 0);
}

function gerarJogos() {
    const quantidade = Number(document.getElementById('campo-quantidade').value) || 0;
    const dezenasPorJogo = Number(document.getElementById('campo-dezenas-por-jogo').value);
    const quantidadeFixas = Object.values(estadoVolante).filter(e => e === 'fixa').length;
    const quantidadeLivres = 25 - Object.keys(estadoVolante).length;

    if (quantidadeFixas > dezenasPorJogo || quantidadeFixas + quantidadeLivres < dezenasPorJogo) {
        mostrarAviso('Combinação de fixas e excluídas não permite montar o jogo.');
        return;
    }
    jogosGerados = Array.from({ length: quantidade }, () => sortearJogoFicticio(dezenasPorJogo));
    renderizarJogosGerados();
}

function renderizarGerador() {
    document.getElementById('campo-concurso-alvo').innerHTML = dadosMock.concursosFuturos
        .map(c => `<option value="${c.numero}">${c.numero} — ${c.data}</option>`).join('');
    document.getElementById('campo-dezenas-por-jogo').innerHTML = [15, 16, 17, 18, 19, 20]
        .map(n => `<option value="${n}">${n} dezenas</option>`).join('');

    renderizarVolanteGerador();
    atualizarResumoCusto();

    document.getElementById('volante-gerador').addEventListener('click', evento => {
        const bola = evento.target.closest('[data-dezena]');
        if (bola) alternarDezena(Number(bola.dataset.dezena));
    });
    document.getElementById('campo-quantidade').addEventListener('input', atualizarResumoCusto);
    document.getElementById('campo-dezenas-por-jogo').addEventListener('change', atualizarResumoCusto);
    document.getElementById('botao-gerar').addEventListener('click', gerarJogos);
    document.getElementById('botao-gerar-novamente').addEventListener('click', gerarJogos);
    document.getElementById('botao-salvar-jogos').addEventListener('click', () => {
        const alvo = document.getElementById('campo-concurso-alvo').value;
        mostrarAviso(`${jogosGerados.length} jogos salvos para o concurso ${alvo} (protótipo: nada foi gravado).`);
    });
    document.getElementById('lista-gerados').addEventListener('click', evento => {
        const botao = evento.target.closest('[data-remover]');
        if (!botao) return;
        jogosGerados.splice(Number(botao.dataset.remover), 1);
        renderizarJogosGerados();
    });
}

// ---------- Meus jogos ----------

function calcularResumoPremios() {
    let total = 0;
    let premiados = 0;
    dadosMock.jogosSalvos.forEach(grupo => {
        const concurso = buscarConcurso(grupo.concurso);
        if (!concurso) return;
        grupo.jogos.forEach(jogo => {
            const premio = calcularPremio(contarAcertos(jogo, concurso.dezenas), concurso);
            total += premio;
            if (premio > 0) premiados++;
        });
    });
    return { total, premiados };
}

function montarLinhaJogo(jogo, indice, concurso, clicavel) {
    const numero = String(indice + 1).padStart(2, '0');
    if (!concurso) {
        return `<div class="linha-jogo">
            <span class="indice">${numero}</span>
            <div class="bolas">${montarBolas(jogo)}</div>
            <div class="resultado-jogo"><span class="chip aviso">Aguardando</span></div>
        </div>`;
    }
    const acertos = contarAcertos(jogo, concurso.dezenas);
    const premio = calcularPremio(acertos, concurso);
    const classeChip = premio > 0 ? 'sucesso' : '';
    const atributos = clicavel ? `class="linha-jogo clicavel" data-concurso="${concurso.numero}" data-jogo="${indice}"` : 'class="linha-jogo"';
    return `<div ${atributos}>
        <span class="indice">${numero}</span>
        <div class="bolas">${montarBolas(jogo, d => concurso.dezenas.includes(d) ? 'acerto' : 'erro')}</div>
        <div class="resultado-jogo">
            <span class="chip ${classeChip}">${acertos} acertos</span>
            <span class="premio ${premio > 0 ? 'positivo' : 'texto-secundario'}">${premio > 0 ? formatoMoeda.format(premio) : '—'}</span>
        </div>
    </div>`;
}

function montarGrupoJogos(grupo) {
    const concurso = buscarConcurso(grupo.concurso);
    const situacao = concurso ? 'conferido' : 'aguardando';
    const totalGrupo = concurso
        ? grupo.jogos.reduce((total, jogo) => total + calcularPremio(contarAcertos(jogo, concurso.dezenas), concurso), 0)
        : 0;
    const chipSituacao = concurso
        ? '<span class="chip info">Conferido</span>'
        : '<span class="chip aviso">Aguardando sorteio</span>';
    const chipPremio = concurso
        ? `<span class="chip ${totalGrupo > 0 ? 'sucesso' : ''}">${totalGrupo > 0 ? `Prêmio ${formatoMoeda.format(totalGrupo)}` : 'Sem prêmio'}</span>`
        : `<span class="chip">Custo ${formatoMoeda.format(grupo.jogos.length * dadosMock.precoApostaMinima)}</span>`;
    const resultado = concurso
        ? `<div class="resultado-oficial">
               <b>Resultado</b>
               <div class="bolas">${montarBolas(concurso.dezenas, () => 'sorteada')}</div>
               <span class="texto-secundario">Prescreve em ${grupo.prescricao}</span>
           </div>`
        : '';

    return `<div class="expansao aberta" data-situacao="${situacao}" data-premiado="${totalGrupo > 0}">
        <div class="expansao-cabecalho">
            <h3>Concurso ${grupo.concurso} <span class="texto-secundario">· ${grupo.data}</span></h3>
            ${chipSituacao}
            <span class="chip">${grupo.jogos.length} jogos</span>
            ${chipPremio}
            <span class="seta" data-icone="expandir"></span>
        </div>
        <div class="expansao-corpo">
            ${resultado}
            ${grupo.jogos.map((jogo, indice) => montarLinhaJogo(jogo, indice, concurso, true)).join('')}
        </div>
    </div>`;
}

function abrirConferencia(numeroConcurso, indiceJogo) {
    const concurso = buscarConcurso(numeroConcurso);
    const grupo = dadosMock.jogosSalvos.find(g => g.concurso === numeroConcurso);
    const jogo = grupo.jogos[indiceJogo];
    const acertos = contarAcertos(jogo, concurso.dezenas);
    const premio = calcularPremio(acertos, concurso);

    const classeDaDezena = dezena => {
        const marcada = jogo.includes(dezena);
        const sorteada = concurso.dezenas.includes(dezena);
        if (marcada && sorteada) return 'acerto';
        if (marcada) return 'erro';
        if (sorteada) return 'nao-marcada';
        return '';
    };
    document.getElementById('dialogo-titulo').textContent = `Jogo ${String(indiceJogo + 1).padStart(2, '0')} · concurso ${numeroConcurso}`;
    document.getElementById('dialogo-volante').innerHTML = montarBolas(Array.from({ length: 25 }, (_, i) => i + 1), classeDaDezena);
    document.getElementById('dialogo-detalhes').innerHTML = `
        <dt>Sorteio</dt><dd>${concurso.data}</dd>
        <dt>Acertos</dt><dd>${acertos} de 15</dd>
        <dt>Faixa</dt><dd>${premio > 0 ? `${acertos} acertos` : 'Não premiado'}</dd>
        <dt>Prêmio</dt><dd class="${premio > 0 ? 'positivo' : ''}">${premio > 0 ? formatoMoeda.format(premio) : '—'}</dd>
        <dt>Prescreve em</dt><dd>${grupo.prescricao}</dd>
        <dt>Receber em</dt><dd>${premio > 0 ? 'Casa lotérica' : '—'}</dd>`;
    document.getElementById('dialogo-conferencia').classList.add('visivel');
}

function filtrarGrupos(filtro) {
    document.querySelectorAll('#grupos-jogos .expansao').forEach(grupo => {
        const visivel = filtro === 'Todos'
            || (filtro === 'Aguardando sorteio' && grupo.dataset.situacao === 'aguardando')
            || (filtro === 'Conferidos' && grupo.dataset.situacao === 'conferido')
            || (filtro === 'Premiados' && grupo.dataset.premiado === 'true');
        grupo.classList.toggle('oculto', !visivel);
    });
}

function renderizarMeusJogos() {
    const container = document.getElementById('grupos-jogos');
    container.innerHTML = dadosMock.jogosSalvos.map(montarGrupoJogos).join('');

    const resumo = calcularResumoPremios();
    document.getElementById('meus-jogos-premiados').textContent = resumo.premiados;
    document.getElementById('meus-jogos-total').textContent = formatoMoeda.format(resumo.total);

    container.addEventListener('click', evento => {
        const linha = evento.target.closest('.linha-jogo.clicavel');
        if (linha) {
            abrirConferencia(Number(linha.dataset.concurso), Number(linha.dataset.jogo));
            return;
        }
        const cabecalho = evento.target.closest('.expansao-cabecalho');
        if (cabecalho) cabecalho.parentElement.classList.toggle('aberta');
    });

    document.getElementById('filtro-situacao').addEventListener('click', evento => {
        const botao = evento.target.closest('button');
        if (botao) filtrarGrupos(botao.textContent.trim());
    });

    const dialogo = document.getElementById('dialogo-conferencia');
    dialogo.addEventListener('click', evento => {
        if (evento.target === dialogo || evento.target.closest('[data-fechar-dialogo]')) {
            dialogo.classList.remove('visivel');
        }
    });
}

// ---------- Configurações ----------

function renderizarConfiguracoes() {
    document.getElementById('tabela-precos').innerHTML = [15, 16, 17, 18, 19, 20].map(dezenas => {
        const apostas = combinacoes(dezenas, 15);
        return `<tr><td>${dezenas}</td><td class="numero">${formatoNumero.format(apostas)}</td>
                <td class="numero">${formatoMoeda.format(apostas * dadosMock.precoApostaMinima)}</td></tr>`;
    }).join('');
}

// ---------- Inicialização ----------

renderizarPainel();
renderizarResultados();
renderizarEstatisticas();
renderizarGerador();
renderizarMeusJogos();
renderizarConfiguracoes();
configurarMenu();
configurarComponentesGenericos();
aplicarIcones();
navegar();
