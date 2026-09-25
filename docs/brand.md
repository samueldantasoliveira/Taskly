# Identidade visual da Rivulus

## Essência

**Rivulus** vem do latim para pequeno curso d'água. A marca traduz a ideia de
trabalho que encontra um caminho claro e segue continuamente da intenção à
entrega.

**Assinatura:** Projetos e equipes em movimento.

**Promessa:** transformar planos dispersos em um fluxo de trabalho visível,
colaborativo e confiável.

## Personalidade

- Clara: reduz ruído e mostra o próximo passo.
- Fluida: acompanha o trabalho sem criar burocracia.
- Confiável: comunica segurança, continuidade e controle.
- Humana: fala de forma direta, próxima e útil.

## Símbolo

O símbolo é um **rio sinuoso visto de cima**, em turquesa e azul, sobre um
emblema azul-petróleo. O curso se alarga em direção à base, traduzindo o
movimento da intenção à entrega. Margens discretas, reflexos e uma linha de
corrente dão profundidade à versão completa.

O arquivo principal está em
`Rivulus.Web/public/brand/rivulus-mark.svg`. Não distorça, gire ou altere as
cores do símbolo. Preserve ao redor dele um espaço livre equivalente a um
quarto de sua largura.

- `rivulus-mark.svg`: emblema completo, também utilizado pelo componente `Logo`
  na navegação e nas telas de autenticação.
- `rivulus-banner.svg`: capa do README, com assinatura e correntes ao fundo.
  É autocontida para renderizar como imagem no GitHub.
- `../favicon.svg`: versão reduzida, sem detalhes finos, para a aba do navegador.

Os três arquivos são vetoriais, sem dependências externas ou animações. A base
escura do emblema preserva sua aparência nos temas claro e escuro. Ao alterar o
símbolo, mantenha a silhueta do rio consistente nas três versões. No favicon,
preserve apenas a água e o fundo: margens e reflexos não são legíveis em 16 px.

## Cores

| Papel | Cor | Hexadecimal |
| --- | --- | --- |
| Profundidade | Azul-petróleo | `#092630` |
| Movimento | Turquesa | `#45D6C5` |
| Corrente clara | Turquesa-claro | `#52E3D2` |
| Conexão | Azul | `#2AA7D6` |
| Fundo escuro | Azul profundo | `#071B24` |
| Fundo claro | Névoa | `#F3F8F8` |

O turquesa identifica ações principais, foco e progresso. O azul-petróleo cria
a base de confiança. Cores de erro, alerta e status continuam funcionais e não
devem ser substituídas apenas por razões estéticas.

## Aplicação na interface

As correntes de `Rivulus.Web/public/brand/rivulus-flow.svg` aparecem nas telas de
autenticação, nos cabeçalhos de boas-vindas e nas capas das equipes. São fundos
decorativos: não recebem foco, não interceptam cliques e não comunicam estado.
As áreas de leitura usam superfícies discretas, bordas e relevo suave.

Botões principais usam gradientes de turquesa; os estados do Kanban mantêm
rótulos e ícones junto das cores. Prioridade e prazo usam cores semânticas
adaptadas aos temas claro e escuro. No celular, a marca permanece visível no
login, mesmo sem o painel ilustrado. A interface respeita movimento reduzido.

## Tipografia da interface

A interface usa `Inter`, com fallback para fontes nativas do sistema. Títulos
usam peso forte e espaçamento compacto; textos de apoio priorizam legibilidade.
O nome deve ser escrito como **Rivulus**, nunca `RIVULUS` em textos corridos.

## Voz

Prefira frases curtas, verbos de ação e orientações específicas. A marca ajuda
o usuário a avançar; não usa linguagem excessivamente técnica ou promocional.

Exemplos:

- “Crie o primeiro projeto da equipe.”
- “Esta tarefa está pronta para avançar.”
- “Não foi possível salvar. Tente novamente.”

## Acessibilidade

- Não use cor como único indicador de estado.
- Preserve o foco visível e os nomes acessíveis dos controles.
- Mantenha contraste mínimo WCAG AA em texto e elementos interativos.
- Use o símbolo sem texto apenas quando o contexto já identificar a Rivulus.
