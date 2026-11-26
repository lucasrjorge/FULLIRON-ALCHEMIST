# GAME DESIGN — FULLIRON ALCHEMIST

## Resumo

FULLIRON ALCHEMIST é um jogo VR de alquimia em que o jogador descobre receitas por tentativa e erro guiada por observação. O laboratório é físico: ingredientes, componentes processados, resíduos e resultados são objetos únicos que podem ser pegos, carregados, colocados em estações e usados em símbolos. O Grimório começa incompleto e vai sendo preenchido conforme o jogador experimenta.

O jogo não é sobre apertar a combinação certa de um menu. É sobre entender o que cada componente — em cada estado, com cada tag — faz em uma reação, e montar um quebra-cabeça de efeitos para chegar ao resultado mostrado no Grimório.

## Fantasia e tom

- **Fantasia:** alquimista cientista em um laboratório vivo, metódico, que aprende com o próprio erro.
- **Tom:** base científica de descoberta, observação e experimentação, com momentos de caos mágico exagerado.
- **Erros:** são pistas e descobertas, mas também podem criar bagunça, perigo ocasional e anomalias. O jogador não é punido com morte; ele lida com as consequências no laboratório.
- **Humor/estética:** consequências mágicas exageradas, mas legíveis e confortáveis em VR. Não é um jogo sombrio nem puramente cômico.

## Pilares de design

1. **Tudo é físico.** Não existe inventário abstrato. Componentes são objetos únicos; a bandeja da Mesa do Símbolo é o inventário físico.
2. **Erro é conteúdo.** Processar errado transforma o componente em outro componente permanente, que pode ser útil em outra receita.
3. **Observação vence chute.** O jogador vê cor, vapor, estágios, som e movimento; o alvo exato nem sempre é mostrado.
4. **Tempo real com pressão.** Processos continuam enquanto o jogador faz outra coisa. Ignorar gera um componente diferente do pedido.
5. **Desenho é intenção.** O símbolo principal é expressivo e tolerante; os sigilos dos elementos e suas posições são restritos.
6. **Consequência específica.** Cada receita define seu próprio perfil de falha: consumo, bloqueio, limpeza, anomalia e efeito no jogador.

## Loop de gameplay

```text
Olhar o Grimório
  -> Pegar ingredientes nas estantes
  -> Modificar os componentes nas estações
  -> Desenhar o símbolo principal + sigilos dos elementos
  -> Posicionar os componentes nas âncoras
  -> Juntar as palmas/controles para conjurar
  -> Observar o resultado
  -> Ajustar processos/componentes
  -> Completar a reação e registrar no Grimório
  -> Se der errado, cuidar do laboratório
```

O jogador pode andar livremente pelo laboratório, alternar entre teleporte e movimento contínuo e usar snap turn. Alguns processos são ativos, outros são passivos e continuam sozinhos.

## Laboratório e estações

O laboratório do protótipo é um bloqueio (blockout) com primitivas, materiais URP e partículas simples.

| Área | Função |
|---|---|
| Mesa do Símbolo | Desenhar símbolo e sigilos, posicionar componentes nas âncoras, usar a bandeja e conjurar. |
| Mesa da Dissolução | Dissolutio: tubo, pistão, manivela e bandeja de resíduo. |
| Mesa da Purificação | Calcinatio e Terra: grelha, fole/bomba de ar, botão de selar, fogo, medidor e resíduo. |
| Mesa da Gaseificação | Sublimatio: aquecedor, câmara de gás, ventoinha, câmara de resfriamento e saídas. |
| Estante 1 | Ingredientes-base com botão de reposição por fileira. |
| Estante 2 | Outras fileiras de ingredientes-base e espaço de apoio para componentes processados. Reposição apenas das fileiras-base. |
| Púlpito do Grimório | Livro/painel world-space com páginas incompletas e descobertas. |
| Área de limpeza | Ferramentas físicas e destino para resíduos/anomalias. |

O layout deve permitir que o jogador veja e ouça as estações passivas enquanto trabalha em outra. Os alertas de processo usam som + luz + movimento.

## Componentes e recursos

- **Ingredientes-base:** ficam em fileiras de estante. Cada fileira tem um botão que materializa o estoque daquela fileira. O jogador pega o item físico.
- **Elementos como componentes:** os próprios elementos (Aqua, Ignis, Terra, Aer) também podem existir como componentes físicos e ser usados em reações.
- **Componentes processados:** são objetos físicos novos, com aparência, estado e tags próprias. Não voltam ao estado anterior.
- **Bandeja da Mesa do Símbolo:** inventário físico com slots para carregar vários componentes de uma vez. Não é menu.
- **Resíduos (Terra):** resultado de Calcinatio e Dissolutio; variam conforme o componente de entrada.
- **Capacidade e reposição:** valores de protótipo, ajustáveis. A reposição é manual por fileira, não automática por tempo.
- **Sem hotbar, sem menu de itens, sem inventário abstrato.**

## Processos alquímicos

Todos os processos funcionam em **tempo real + pressão**. Alguns são ativos (o jogador opera), outros passivos (continuam sozinhos). Se o jogador ignorar um processo, ele chega a um resultado que não era o pedido. Remover o item ou desligar a fonte pausa/interrompe.

### Dissolutio (Água)

- O jogador coloca o componente no tubo e gira a manivela; um pistão empurra o componente para baixo e o filtra.
- **Menos giro = incompleto.** **Mais giro = superdissolvido.** Ambos são componentes novos, com aparência e tags próprias.
- O resíduo Terra depende do componente de entrada.
- Matéria-prima: componente físico + operação da manivela.
- Feedback: pistão descendo, líquido, som, cor e mudança de estado.

### Calcinatio (Fogo)

- O jogador bombeia ar para aumentar o calor; o componente fica sobre uma grelha.
- Estágios visíveis: **cru -> calcinado -> supercalcinado -> cinza**. Cada estágio é um componente.
- Para não passar do ponto, o jogador sela a chama no momento certo. Enquanto o fogo estiver aceso, o componente continua aquecendo; se ignorar, ele passa do ponto.
- O resíduo Terra depende do componente de entrada.
- Feedback: intensidade do fogo, cor do componente, fumaça, som, medidor de calor.

### Sublimatio (Ar)

- O jogador coloca o componente em um aquecedor potente (ou pelicano) para transformá-lo em gás.
- O tempo de aquecimento importa: o gás pode sair em **3 estados**, lidos pela cor do vapor.
- Depois, o jogador escolhe entre **usar o gás diretamente** ou **cristalizar**. O gás extraído é um item físico; a cristalização é binária e gera outro item físico, e estados diferentes de gás geram cristais diferentes.
- Se ignorar, o gás passa do ponto e vira um estado que não era o pedido.
- Feedback: cor do vapor, som, vibração e mudança visual do componente.

### Terra (resíduos)

- Terra não é uma estação separada; é o resíduo de Calcinatio e Dissolutio.
- O resíduo é um componente físico que pode ser usado em outras receitas ou descartado/limpo.
- O resíduo de um processo errado também é um componente novo, não um “erro” invisível.

## Tags e estados

Cada componente possui tags de efeito. As tags podem ser atribuídas/alteradas conforme o estado e o processo:

- Exemplo: um componente que passa pouco tempo em Calcinatio vira **outro componente**, com aparência própria e uma tag específica daquele estágio.
- As tags não são apenas metadados: receitas podem exigir tags, componentes exatos, ou os dois.
- O jogador descobre a relação entre processo -> estado -> tags por observação e experimentação.
- O conjunto inicial de tags do protótipo é pequeno e editável (ex.: Calor, Frio, Úmido, Seco, Volátil, Puro, Corrosivo).

## Símbolo e conjuração

- O jogador desenha **tudo à mão livre, em uma sequência**: o símbolo principal e os sigilos dos elementos.
- Sigilos dos elementos: símbolos alquímicos clássicos (Aqua, Ignis, Terra, Aer).
- O **símbolo principal** tem alta tolerância e pode ser expresso com estilo próprio, mas precisa conectar os sigilos de forma congruente com o template.
- A **posição dos sigilos** é restrita às âncoras do template.
- O jogador também pode criar variações intencionais aceitas; o validador geométrico do protótipo usa famílias de templates e tolerância. O LAYA entra depois para validar a imagem renderizada como modelo de decisão.
- A validação acontece no **gesto de conjurar**: juntar as palmas/controles como em uma prece, com animação contextual das mãos/palmas.
- Se o desenho for inválido, toca um efeito sonoro/visual de “magia não se concentra”; a reação não acontece e os componentes permanecem na mesa.
- Não há consumo de componentes por desenho inválido.

### Mesa do Símbolo

- Superfície de desenho com âncoras físicas para os componentes (no protótipo, 4 âncoras nos pontos cardeais).
- Bandeja física para apoiar vários componentes.
- Botão/área de limpar o desenho.
- Zona de conjuração: o gesto de palmas só dispara quando o jogador está na área da mesa.

## Reações

- A reação valida **componente exato e/ou tags** por âncora.
- Cada receita referencia um template de símbolo. Se várias receitas podem compartilhar o mesmo template é uma decisão ainda em aberto; o modelo de dados suporta os dois casos.
- **Sucesso:** a reação produz um resultado, registra no Grimório (resultado, componentes e símbolo descoberto) e pode gerar um componente sólido ou efeito. No protótipo, o foco é o resultado como componente físico; efeitos puros ficam para depois.
- **Reação adversa:** usa o perfil de falha da receita. O perfil define consumo, bloqueio, tipo de limpeza, anomalia, efeito no jogador e duração.
- **Componente de ordem errada:** é um componente novo e permanente. Pode ser útil em outra receita. Se for usado na reação que esperava o componente “certo”, dispara um evento adverso que exige cuidado com o laboratório.
- O jogador pode usar reações auxiliares no futuro; elas não entram no primeiro protótipo.

## Grimório

- O Grimório começa com páginas de **uso básico das ferramentas/estações** e o **objetivo final de cada reação**.
- O Grimório já contém **todas as páginas de receitas**, mas incompletas: mostra o resultado final e alguns componentes.
- Processos, ordem, tempo/temperatura, símbolo e sigilos são descobertos pelo jogador.
- Conforme o jogador descobre, as páginas são completadas e o novo componente é adicionado às páginas das reações que o utilizam.
- As últimas páginas são reservadas aos componentes: o que o jogador sabe confeccionar.
- Falhas e resultados inesperados também são registrados como descobertas/notas.
- Persistência entre sessões no protótipo.

## Erros, consequências e limpeza

- **Processo errado:** gera um componente diferente e permanente; não há “consertar” o componente.
- **Processo no tempo/calor errado:** depende da estação (incompleto, superdissolvido, supercalcinado, gás errado etc.).
- **Ordem errada:** gera um componente novo; se usado na reação que esperava outro, dispara evento adverso.
- **Reação adversa:** perfil por receita; pode consumir componentes, bloquear a mesa/área, criar bagunça, gerar anomalia e afetar o jogador.
- **Anomalias do protótipo:** apenas distorção da sala. Objetos flutuam; o jogador pode pegá-los e colocá-los onde quiser; o efeito some após um tempo e **não bloqueia nada**. Criatura e sigilo instável ficam para depois.
- **Limpeza:** ferramentas físicas (pano, vassoura, extintor, giz) quando aplicável. O bloqueio e a forma de limpeza são definidos por receita.
- **Efeitos no jogador:** vinheta/blur e alteração de cor/alucinação temporárias. Sem dano, morte ou respawn.
- **Toda falha é registrada no Grimório**, mesmo que o resultado seja feio.

## Conforto e acessibilidade

- O jogador pode ficar sentado ou em pé.
- Olhar em 360 graus; snap turn disponível para quem está sentado.
- Teleporte e movimento contínuo alternáveis.
- Textos grandes, contrastados e a uma distância confortável.
- Evitar efeitos que invertam controles ou causem desorientação forte. Efeitos permitidos: vinheta/blur e cor/alucinação.
- Alertas de processo por som + luz + movimento para o jogador perceber o que está acontecendo à distância.

## Progressão

- O jogo é uma escada: resultados e componentes de reações anteriores servem para as próximas.
- O Grimório cresce em duas dimensões: reações descobertas e componentes que o jogador sabe confeccionar.
- O jogador pode usar componentes “errados” como parte de outras receitas; o erro não é lixo automático.
- Reações auxiliares (futuro) podem gerar efeitos ou componentes usados na reação principal.

## Escopo do protótipo

Entra:

- Mesa do Símbolo com desenho livre, sigilos, âncoras, bandeja e gesto de conjurar.
- Dissolutio, Calcinatio/Terra e Sublimatio em tempo real.
- Estantes com reposição por fileira.
- Grimório simplificado com descobertas e falhas.
- Distorção da sala.
- Receitas demo: uma por componente exato e uma por tags.

Não entra:

- Reações auxiliares.
- Outras anomalias (criatura, sigilo instável).
- Hand tracking.
- Multiplayer.
- Arte final, animações completas e áudio definitivo.
- Integração LAYA (interface fica pronta).
- Conteúdo definitivo de puzzles.
