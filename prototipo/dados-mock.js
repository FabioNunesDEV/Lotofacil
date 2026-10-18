// Dados fictícios usados apenas pelo protótipo visual.

const dadosMock = {
    totalConcursos: 3807,
    precoApostaMinima: 3.50,
    premiosFixos: { 11: 7.00, 12: 14.00, 13: 35.00 },

    proximoConcurso: { numero: 3808, data: 'seg, 19/10/2026', estimativa: 1800000.00 },

    concursosFuturos: [
        { numero: 3808, data: 'seg, 19/10/2026' },
        { numero: 3809, data: 'ter, 20/10/2026' },
        { numero: 3810, data: 'qua, 21/10/2026' },
        { numero: 3811, data: 'qui, 22/10/2026' },
    ],

    // ganhadores: [15, 14, 13, 12, 11]; premio15/premio14 vêm do rateio do concurso.
    concursos: [
        { numero: 3807, data: '17/10/2026', dezenas: [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25], ordem: [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4], ganhadores: [1, 287, 9874, 121045, 653211], premio15: 1712408.63, premio14: 1502.34, arrecadacao: 24873112.50 },
        { numero: 3806, data: '16/10/2026', dezenas: [1, 2, 4, 5, 6, 9, 10, 11, 13, 14, 15, 19, 20, 22, 25], ganhadores: [3, 412, 12530, 140877, 701432], premio15: 512330.17, premio14: 1206.88, arrecadacao: 23110450.00 },
        { numero: 3805, data: '15/10/2026', dezenas: [1, 2, 3, 5, 7, 10, 11, 13, 17, 18, 20, 21, 23, 24, 25], ganhadores: [0, 198, 8120, 108654, 612987], premio15: 0, premio14: 2240.51, arrecadacao: 21456310.50 },
        { numero: 3804, data: '14/10/2026', dezenas: [1, 2, 3, 4, 5, 9, 10, 11, 12, 13, 14, 15, 20, 22, 24], ganhadores: [2, 355, 11034, 131245, 688012], premio15: 801455.92, premio14: 1310.07, arrecadacao: 22340987.00 },
        { numero: 3803, data: '13/10/2026', dezenas: [2, 3, 4, 5, 7, 8, 11, 12, 13, 14, 15, 18, 20, 24, 25], ganhadores: [4, 520, 14211, 152300, 742118], premio15: 410220.35, premio14: 1012.44, arrecadacao: 22998102.50 },
        { numero: 3802, data: '10/10/2026', dezenas: [1, 3, 4, 5, 8, 9, 10, 11, 12, 14, 16, 19, 23, 24, 25], ganhadores: [1, 263, 9512, 118340, 640765], premio15: 1890211.40, premio14: 1588.90, arrecadacao: 25110220.00 },
        { numero: 3801, data: '09/10/2026', dezenas: [1, 4, 6, 8, 9, 11, 13, 15, 18, 20, 21, 22, 23, 24, 25], ganhadores: [0, 241, 8890, 112006, 631455], premio15: 0, premio14: 1720.12, arrecadacao: 21876543.50 },
        { numero: 3800, data: '08/10/2026', dezenas: [2, 3, 5, 7, 8, 9, 11, 13, 16, 17, 18, 19, 20, 21, 24], ganhadores: [5, 610, 15987, 160221, 760998], premio15: 386114.80, premio14: 903.55, arrecadacao: 23987650.00 },
        { numero: 3799, data: '07/10/2026', dezenas: [1, 2, 3, 5, 8, 9, 12, 13, 14, 16, 18, 19, 21, 23, 24], ganhadores: [2, 301, 10255, 125790, 670300], premio15: 905612.11, premio14: 1402.77, arrecadacao: 22678900.50 },
        { numero: 3798, data: '06/10/2026', dezenas: [1, 3, 4, 5, 6, 9, 11, 13, 14, 15, 16, 18, 20, 21, 24], ganhadores: [1, 276, 9901, 119998, 655432], premio15: 1650980.45, premio14: 1490.32, arrecadacao: 23045120.00 },
    ],

    // Índice 0 = dezena 01 ... índice 24 = dezena 25.
    frequencias: [2283, 2281, 2283, 2306, 2283, 2244, 2296, 2279, 2280, 2289, 2293, 2319, 2305, 2289, 2266, 2174, 2258, 2293, 2323, 2368, 2287, 2283, 2301, 2245, 2277],
    atrasos: [0, 0, 2, 0, 0, 1, 0, 4, 1, 0, 0, 3, 0, 0, 1, 5, 2, 0, 1, 0, 0, 0, 2, 0, 0],

    perfis: [
        {
            titulo: 'Pares e ímpares', descricao: '12 pares / 13 ímpares', rotulo: 'Pares',
            linhas: [
                { faixa: '5 pares', observado: 7.19, teorico: 6.93, atraso: 11 },
                { faixa: '6 pares', observado: 20.51, teorico: 20.21, atraso: 3 },
                { faixa: '7 pares', observado: 31.35, teorico: 31.18, atraso: 1 },
                { faixa: '8 pares', observado: 25.45, teorico: 25.99, atraso: 0 },
                { faixa: '9 pares', observado: 10.60, teorico: 11.55, atraso: 6 },
                { faixa: '10 pares', observado: 2.05, teorico: 2.60, atraso: 42 },
            ],
        },
        {
            titulo: 'Números primos', descricao: '02 03 05 07 11 13 17 19 23',
            linhas: [
                { faixa: '3', observado: 5.12, teorico: 4.68, atraso: 18 },
                { faixa: '4', observado: 17.27, teorico: 16.84, atraso: 2 },
                { faixa: '5', observado: 31.01, teorico: 30.87, atraso: 0 },
                { faixa: '6', observado: 29.14, teorico: 29.40, atraso: 1 },
                { faixa: '7', observado: 14.44, teorico: 14.17, atraso: 4 },
                { faixa: '8', observado: 3.27, teorico: 3.15, atraso: 27 },
            ],
        },
        {
            titulo: 'Múltiplos de 3', descricao: '03 06 09 12 15 18 21 24',
            linhas: [
                { faixa: '2', observado: 1.81, teorico: 2.04, atraso: 63 },
                { faixa: '3', observado: 10.16, teorico: 10.60, atraso: 0 },
                { faixa: '4', observado: 26.98, teorico: 26.50, atraso: 2 },
                { faixa: '5', observado: 33.82, teorico: 33.32, atraso: 1 },
                { faixa: '6', observado: 19.27, teorico: 20.82, atraso: 5 },
                { faixa: '7', observado: 5.72, teorico: 5.95, atraso: 9 },
            ],
        },
        {
            titulo: 'Fibonacci', descricao: '01 02 03 05 08 13 21',
            linhas: [
                { faixa: '2', observado: 4.96, teorico: 5.50, atraso: 14 },
                { faixa: '3', observado: 19.66, teorico: 19.88, atraso: 2 },
                { faixa: '4', observado: 34.30, teorico: 34.08, atraso: 1 },
                { faixa: '5', observado: 27.88, teorico: 28.11, atraso: 0 },
                { faixa: '6', observado: 10.72, teorico: 10.41, atraso: 7 },
                { faixa: '7', observado: 0.90, teorico: 1.34, atraso: 120 },
            ],
        },
        {
            titulo: 'Soma das dezenas', descricao: 'Faixas de 15 em 15',
            linhas: [
                { faixa: '150–164', observado: 4.52, teorico: 4.07, atraso: 21 },
                { faixa: '165–179', observado: 14.87, teorico: 15.30, atraso: 3 },
                { faixa: '180–194', observado: 30.03, teorico: 29.07, atraso: 1 },
                { faixa: '195–209', observado: 28.53, teorico: 29.67, atraso: 0 },
                { faixa: '210–224', observado: 15.90, teorico: 16.29, atraso: 4 },
                { faixa: '225–239', observado: 4.08, teorico: 4.55, atraso: 17 },
            ],
        },
        {
            titulo: 'Repetidas do anterior', descricao: 'Dezenas iguais ao concurso anterior',
            linhas: [
                { faixa: '7', observado: 9.07, teorico: 8.86, atraso: 8 },
                { faixa: '8', observado: 24.61, teorico: 23.62, atraso: 2 },
                { faixa: '9', observado: 32.38, teorico: 32.15, atraso: 1 },
                { faixa: '10', observado: 21.98, teorico: 23.15, atraso: 4 },
                { faixa: '11', observado: 8.02, teorico: 8.77, atraso: 0 },
                { faixa: '12', observado: 2.24, teorico: 1.67, atraso: 36 },
            ],
        },
    ],

    // Jogos salvos pelo usuário, agrupados por concurso alvo.
    jogosSalvos: [
        {
            concurso: 3808, data: 'seg, 19/10/2026',
            jogos: [
                [1, 2, 3, 4, 5, 7, 9, 10, 11, 12, 14, 18, 20, 23, 24],
                [1, 3, 5, 7, 8, 9, 10, 12, 16, 17, 18, 19, 20, 22, 23],
                [1, 4, 5, 6, 10, 11, 12, 13, 16, 17, 18, 19, 20, 22, 23],
                [1, 2, 3, 5, 7, 10, 11, 12, 14, 16, 19, 21, 22, 24, 25],
                [2, 3, 4, 5, 8, 9, 10, 12, 13, 14, 16, 18, 19, 23, 25],
                [3, 5, 6, 8, 9, 10, 11, 12, 14, 15, 20, 21, 22, 23, 24],
                [1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 17, 18, 19, 24],
                [3, 5, 6, 7, 9, 10, 11, 12, 13, 15, 17, 21, 22, 24, 25],
                [1, 2, 3, 4, 5, 6, 10, 12, 13, 15, 18, 20, 21, 23, 24],
                [5, 6, 8, 9, 10, 11, 12, 13, 14, 18, 20, 21, 23, 24, 25],
            ],
        },
        {
            concurso: 3807, data: 'sáb, 17/10/2026', prescricao: '15/01/2027',
            jogos: [
                [2, 3, 4, 5, 7, 8, 10, 11, 12, 15, 16, 17, 20, 21, 25],
                [1, 2, 3, 4, 5, 9, 10, 12, 13, 14, 15, 18, 20, 21, 24],
                [2, 4, 5, 7, 10, 12, 13, 14, 15, 17, 18, 20, 21, 24, 25],
                [5, 6, 7, 10, 11, 12, 13, 14, 17, 19, 20, 21, 22, 23, 25],
                [1, 2, 3, 4, 5, 7, 10, 11, 12, 13, 14, 20, 21, 22, 25],
            ],
        },
        {
            concurso: 3806, data: 'sex, 16/10/2026', prescricao: '14/01/2027',
            jogos: [
                [1, 2, 5, 6, 8, 9, 12, 13, 14, 16, 17, 19, 22, 23, 25],
                [2, 3, 5, 9, 10, 11, 12, 13, 14, 17, 18, 20, 21, 22, 24],
                [1, 2, 4, 9, 10, 11, 12, 13, 14, 15, 19, 21, 22, 23, 25],
            ],
        },
    ],
};
