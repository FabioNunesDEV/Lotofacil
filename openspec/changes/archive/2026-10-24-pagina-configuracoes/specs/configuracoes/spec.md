## Purpose

Permite ao usuário ajustar pela interface o preço da aposta mínima usado em todos os custos, gravado na base local,
e ver quanto custam as apostas de 15 a 20 dezenas com esse preço.

## ADDED Requirements

### Requirement: Acesso à página Configurações
A aplicação SHALL oferecer a página "Configurações" na rota `/configuracoes`, acessível por um item "Configurações"
no menu lateral, o último da lista, logo abaixo de "Meus jogos". A página MUST ter o título "Configurações" e o
subtítulo "Valores usados nos cálculos".

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Configurações" do menu lateral
- **THEN** a página "Configurações" é exibida com o título "Configurações"
- **AND** o item "Configurações" do menu aparece como selecionado

### Requirement: Preço da aposta mínima
A página SHALL ter o campo "Aposta mínima (15 dezenas)", em reais, preenchido ao abrir com o preço atual, e o botão
"Salvar". O preço atual MUST ser o gravado na base pela página; enquanto nenhum preço tiver sido gravado, MUST ser o
preço da configuração da aplicação (R$ 3,50 por padrão).

O preço MUST estar entre R$ 0,01 e R$ 100,00, com no máximo dois decimais. Com um valor fora disso, ou com o campo
vazio, o campo MUST exibir "Informe um preço entre R$ 0,01 e R$ 100,00, com até dois decimais." e "Salvar" MUST NOT
gravar nada. Com um valor válido, "Salvar" MUST gravar o preço na base e exibir "Configurações salvas.".

O preço é único, sem vigência: depois de salvo, MUST valer imediatamente em todos os custos da aplicação (cadastro de
jogo, Gerar jogos, Meus jogos e exportação), inclusive para os jogos já cadastrados, e continuar valendo depois que a
aplicação for reiniciada. Um valor digitado e não salvo MUST NOT ser usado em nenhum cálculo fora da própria página.

#### Scenario: Base sem preço gravado
- **WHEN** nenhum preço foi salvo, a configuração da aplicação tem R$ 3,50 e o usuário abre a página
- **THEN** o campo "Aposta mínima (15 dezenas)" mostra 3,50

#### Scenario: Salvar um preço novo
- **WHEN** o usuário digita 4,00 e aciona "Salvar"
- **THEN** a página exibe "Configurações salvas."
- **AND** ao reabrir a página, mesmo depois de reiniciar a aplicação, o campo mostra 4,00

#### Scenario: Custos com o preço novo
- **WHEN** o preço salvo é R$ 4,00 e há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas cadastrados
- **THEN** o indicador "Investido" de Meus jogos mostra "R$ 72,00"
- **AND** o cadastro de um jogo de 15 dezenas mostra o custo de R$ 4,00

#### Scenario: Preço inválido
- **WHEN** o usuário digita 0 (ou 100,01, ou 3,555, ou deixa o campo vazio) e aciona "Salvar"
- **THEN** o campo exibe "Informe um preço entre R$ 0,01 e R$ 100,00, com até dois decimais."
- **AND** o preço gravado continua o mesmo

#### Scenario: Sair sem salvar
- **WHEN** o usuário digita 5,00, não aciona "Salvar" e abre Meus jogos
- **THEN** os custos continuam calculados com o preço gravado antes

### Requirement: Tabela de preços por quantidade de dezenas
A página SHALL mostrar, só para leitura, uma tabela com uma linha para cada quantidade de dezenas de 15 a 20 e as
colunas "Dezenas", "Apostas equivalentes" (C(n, 15)) e "Valor" (apostas equivalentes × o preço do campo). A coluna
"Valor" MUST acompanhar o valor digitado no campo, antes de salvar; com um valor inválido no campo, MUST mostrar "—".

#### Scenario: Tabela com o preço de R$ 3,50
- **WHEN** o campo tem 3,50
- **THEN** a tabela mostra:
  | Dezenas | Apostas equivalentes | Valor |
  |---|---|---|
  | 15 | 1 | R$ 3,50 |
  | 16 | 16 | R$ 56,00 |
  | 17 | 136 | R$ 476,00 |
  | 18 | 816 | R$ 2.856,00 |
  | 19 | 3.876 | R$ 13.566,00 |
  | 20 | 15.504 | R$ 54.264,00 |

#### Scenario: Tabela acompanha o valor digitado
- **WHEN** o usuário troca o campo para 4,00, sem salvar
- **THEN** a linha de 16 dezenas mostra "R$ 64,00" e a de 20 dezenas mostra "R$ 62.016,00"

#### Scenario: Valor inválido na tabela
- **WHEN** o campo está vazio
- **THEN** a coluna "Valor" mostra "—" em todas as linhas
