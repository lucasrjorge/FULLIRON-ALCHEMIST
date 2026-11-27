# MECHANICS — FULLIRON ALCHEMIST

Este documento detalha as regras que devem ser implementadas. Os números citados são **placeholders configuráveis**; a lógica é o que importa.

## 1. Termos e unidades

- **Componente:** identidade alquímica de um item (`AlchemicalComponent`).
- **Estado:** categoria ampla do componente (`ComponentState`).
- **Tag:** efeito abstrato do componente (`EffectTag`), usado em receitas.
- **Processo:** Dissolutio, Calcinatio, Sublimatio.
- **Progresso/intensidade:** valor normalizado de 0 a 1 usado pelas estações para escolher o resultado.
- **Âncora:** ponto do template de símbolo onde o jogador desenha o sigilo e posiciona o componente.
- **Perfil de falha:** dados por receita que definem o que uma reação adversa consome, bloqueia, deixa e causa no jogador.

## 2. Identidade do componente

Cada item físico tem:

```text
AlchemicalItem
  AlchemicalComponent component
  ComponentState state
  EffectTag tags              // tags ativas no item
  ProcessHistory history      // processos, intensidades e ordem
```

Cada componente tem:

```text
AlchemicalComponent
  bool primary
  AlchemicalComponent dissolutionResidue
  AlchemicalComponent calcinationResidueWeak
  AlchemicalComponent calcinationResidueStrong
  string description          // lore/significado
```

Regras:

- Processar um componente **não edita** o componente original: produz um novo `AlchemicalComponent`/estado/tags e atualiza a aparência do item.
- O novo componente é permanente. Não existe desfazer ou “consertar” o componente errado.
- O mesmo componente pode ser usado em várias receitas; a identidade exata e as tags são consultadas pelo resolvedor.
- Um componente é uma substância física. `Aqua`, `Ignis`, `Terra` e `Aer` são processos/símbolos, não componentes.
- O histórico é registrado para depuração e para futuras receitas que exijam ordem/etapas. No protótipo, a validação principal usa componente + estado + tags.

### Estados iniciais sugeridos

`Raw`, `Incomplete`, `Dissolved`, `Superdissolved`, `Calcined`, `Supercalcined`, `Ash`, `GasWeak`, `GasStable`, `GasUnstable`, `Crystallized`, `Residue`, `Burnt`, `Corrupted`.

O estado é uma categoria; o nome e a aparência exatos vêm do `AlchemicalComponent`.

## 3. Modelo de processo

Cada combinação de componente + processo tem um **perfil** com faixas de resultado:

```text
ProcessingProfile
  AlchemicalComponent input
  ComponentState inputState       // opcional; Any se vazio
  ProcessType process
  ProcessingOutcome[] outcomes
  AlchemicalComponent interruptOutcome   // opcional

ProcessingOutcome
  float minProgress
  float maxProgress
  AlchemicalComponent output
  ComponentState outputState
  int residueAmount
  string message
```

O tipo de resíduo vem do próprio componente:
- `dissolutionResidue`: 1 tipo por componente; `residueAmount` muda por estado.
- `calcinationResidueWeak` / `calcinationResidueStrong`: 2 variações por componente.

Regras:

- A estação calcula um progresso/intensidade em tempo real e consulta a faixa correspondente.
- Se o processo for interrompido antes de uma faixa válida, usa `interruptOutcome` (ou mantém o item, conforme a estação).
- Se o jogador ignorar um processo passivo, o progresso continua até a faixa final/errada.
- O resultado é determinístico: mesmos dados + mesmo progresso = mesmo componente.
- Remover o item ou desligar a fonte pausa/interrompe o processo.
- O processo não depende de física complexa para decidir o resultado.

## 4. Estações e regras específicas

### 4.1 Dissolutio

- **Entrada:** item no tubo.
- **Controle:** a manivela controla a descida do componente e o grau de filtragem.
- **Estados:** `Incomplete`, `Dissolved`, `Superdissolved` (nomes mantidos).
- **Conclusão:** o jogador para no estado desejado e aguarda alguns instantes; o processo conclui sozinho e o componente é extraído automaticamente para a bancada. Não é preciso segurar a manivela durante a espera.
- **Resíduo:** `component.dissolutionResidue`; 1 tipo por componente. O `residueAmount` do outcome muda conforme o estado.
- **Pausa:** parar de girar mantém a profundidade; remover o item interrompe.
- **Feedback:** pistão desce, líquido, mudança de cor, som.

### 4.2 Calcinatio

- **Entrada:** item sobre a grelha.
- **Controle:** bombear ar aumenta a potência da chama. A chama **decai continuamente**; o jogador deve manter a potência desejada com o fole.
- **Estágios:** `Raw` (cru, o próprio primário) -> `Calcined` -> `Supercalcined` -> `Ash` (cinza).
- **Progressão:** o componente avança conforme o tempo exposto à chama; chama mais forte acelera. Ao chegar ao estágio desejado, o jogador sela a chama.
- **Resíduo:** 2 variações por componente:
  - `calcinationResidueWeak` para chama fraca;
  - `calcinationResidueStrong` para chama forte.
- **Pausa:** desligar o fogo ou remover o item interrompe; o estágio atingido não regride.
- **Feedback:** intensidade do fogo, cor, fumaça, som e medidor.

### 4.3 Sublimatio

- **Entrada:** item no aquecedor.
- **Controle:** ligar/desligar o aquecedor; o tempo de aquecimento determina o estado do gás.
- **Estados de gás:** `GasWeak`, `GasStable`, `GasUnstable`, lidos pela cor do vapor.
- **Gás físico:** o gás extraído é um componente único por **origem + estado** (ex.: gás de Enxofre no estado estável).
- **Escolha:** usar o gás diretamente ou cristalizar. Cristalizar é binário.
- **Cristal:** depende da **origem + estado do gás**, gerando um componente diferente por combinação.
- **Ignorar:** o gás passa do ponto e vira um estado errado; não é o pedido.
- **Pausa:** desligar o aquecedor ou remover o item interrompe.
- **Feedback:** cor do vapor, som, vibração e mudança visual do componente.

### 4.4 Terra

- Terra é resíduo, não uma estação.
- Dissolutio: 1 tipo de resíduo por componente; a quantidade muda com o estado.
- Calcinatio: 2 variações por componente (chama fraca/forte).
- Os resíduos ficam em extensões das bancadas de Dissolutio e Calcinatio, com o símbolo da Terra na própria extensão.
- O resíduo é um item físico com componente próprio, tags próprias e uso potencial em outras receitas.
- O jogador pode descartar/limpar resíduos que não quer manter.

## 5. Tags e efeitos

Os significados simbólicos dos primários (alma, espírito, corpo, emoções, força, crescimento, tempo etc.) são **lore/descrição** por enquanto. Eles não são tags mecânicas; servem para o jogador inferir a receita a partir da descrição do resultado.

- `EffectTag` é um conjunto de flags (ex.: `Heat`, `Cold`, `Moist`, `Dry`, `Volatile`, `Pure`, `Corrosive`).
- Cada `AlchemicalComponent` define suas tags. O estado/processo pode adicionar, remover ou alterar tags por meio do componente de saída.
- Para o jogador, o material parece o mesmo transformado; para os dados, cada resultado é um novo `AlchemicalComponent` (ex.: `EnxofreCalcinado`) com tags próprias.
- Exemplo de design: um componente que passa pouco tempo em Calcinatio vira outro componente, com aparência própria e tags específicas daquele estágio.
- Receitas podem exigir:
  - componente exato;
  - componente exato + estado;
  - conjunto de tags (todas ou qualquer uma);
  - componente exato + tags.
- O resolvedor deve deixar claro no resultado por que falhou (componente errado, estado errado, tag faltando).

## 6. Desenho, sigilos e validação

### 6.1 Captura

- O jogador desenha **tudo à mão livre, em uma sequência**, segurando o gatilho para cada traço.
- Soltar o gatilho encerra o traço; o próximo traço continua a mesma captura.
- A captura guarda todos os traços, pontos normalizados, caixas delimitadoras e uma textura renderizada.
- O jogador pode limpar e recomeçar; a validação só acontece no gesto de conjurar.

### 6.2 Sigilos dos elementos

- Os sigilos são os símbolos alquímicos clássicos:

| Elemento | Sigilo |
|---|---|
| Aqua | Triângulo apontando para baixo. |
| Ignis | Triângulo apontando para cima. |
| Terra | Triângulo para baixo com uma linha horizontal. |
| Aer | Triângulo para cima com uma linha horizontal. |

- Cada traço é comparado com os templates de sigilo.
- Um traço é candidato a sigilo se:
  - a forma bate com um dos quatro templates;
  - o tamanho é pequeno em relação ao desenho total;
  - está próximo de uma âncora do template.
- Cada âncora deve ter exatamente um sigilo, do elemento exigido pelo template.
- Posição dos sigilos: tolerância mais restrita que o símbolo principal.

### 6.3 Símbolo principal

- Os traços que não forem sigilos formam o símbolo principal.
- O símbolo principal é comparado ao template com **alta tolerância**: o jogador pode variar o estilo, desde que a forma geral seja congruente.
- A congruência inclui conectar/passar perto de todas as âncoras e manter a topologia geral do template.
- O validador geométrico do protótipo pode usar distância de contorno, IoU rasterizado ou features de traço. O importante é aceitar variações expressivas sem aceitar qualquer rabisco.
- O LAYA entrará depois como um validador de decisão sobre a textura renderizada, não como um reconhecedor de traços dedicado.

### 6.4 Desenho inválido

- Efeito sonoro/visual de “magia não se concentra”.
- Nenhuma reação acontece.
- Nenhum componente é consumido.
- O jogador pode limpar e redesenhar.

### 6.5 Gesto de conjurar

- Detectado pela proximidade e orientação dos dois controles, como palmas se encontrando em uma prece.
- Parâmetros iniciais: distância < 0,15 m, tempo de sustentação ~0,3 s, cooldown ~1 s.
- Só dispara quando o jogador está na zona da Mesa do Símbolo.
- Toca animação/VFX contextual das palmas, som e haptics.
- No gesto, o jogo valida o desenho e, se válido, resolve a reação com os componentes posicionados.

## 7. Reação

### 7.1 Requisição

```text
ReactionRequest
  SymbolTemplate symbol
  Placement[] placements
  bool allowExtraPlacements

Placement
  int anchorIndex
  AlchemicalElement anchorElement
  AlchemicalItem item
```

### 7.2 Requisito

```text
ReactionRequirement
  int anchorIndex
  AlchemicalElement anchorElement
  AlchemicalComponent exactComponent    // opcional
  ComponentState requiredState          // opcional
  EffectTag requiredTags                // opcional
  TagMatchMode tagMatchMode             // All / Any
```

### 7.3 Resolução

1. Se o desenho for inválido: fizzle, não consome, não registra falha alquímica.
2. Filtrar receitas pelo template de símbolo reconhecido.
3. Para cada receita, verificar os requisitos por âncora:
   - componente exato, se exigido;
   - estado, se exigido;
   - tags, se exigidas;
   - posição/elemento da âncora.
4. Se houver mais de uma receita compatível, usar prioridade ou a mais específica.
5. Se nenhuma receita casar: reação adversa usando o perfil de falha padrão do banco de dados.
6. Se casar: sucesso, consumir componentes conforme a receita, gerar resultado e registrar no Grimório.

### 7.4 Perfil de falha (por receita)

```text
FailureProfile
  bool consumeComponents
  BlockTarget blockTarget          // None, Table, Area, Lab
  float blockDuration
  CleanupType cleanupType          // None, PhysicalMess, Extinguish, BanishSigil, StabilizeDistortion
  AnomalyType anomalyType          // None, RoomDistortion, Creature, UnstableSigil
  PlayerEffectFlags playerEffect   // None, VignetteBlur, ColorHallucination
  float playerEffectDuration
  bool recordInGrimoire
```

- O perfil é específico por receita; a falha é sempre registrada no Grimório quando `recordInGrimoire` for verdadeiro.
- No protótipo, a única anomalia é `RoomDistortion`: objetos flutuam, podem ser movidos, o efeito some após um tempo e não bloqueia nada.
- Outros erros podem bloquear a mesa/área; o bloqueio é definido pelo perfil.
- O consumo de componentes também é definido pelo perfil; não é uma regra global.

## 8. Grimório

- `GrimoireDatabase` contém todas as receitas e componentes.
- O Grimório também contém instruções básicas de uso das ferramentas/estações; essas instruções não são receitas e não são “descobertas”.
- `GrimoireState` guarda:
  - receitas descobertas;
  - componentes descobertos/confeccionáveis;
  - tentativas falhas e notas.
- Páginas de receita existem desde o início no estado **incompleto**:
  - resultado final visível;
  - alguns componentes visíveis;
  - processos, tempos, símbolo e sigilos ocultos.
- Sucesso completa a página com resultado, componentes e símbolo descoberto.
- Falha registra a tentativa, os componentes usados e o resultado adverso.
- Páginas de componente mostram como confeccionar aquele componente (processo + faixa) quando descoberto.
- Persistência do protótipo: JSON em `PlayerPrefs` com chave versionada.

## 9. Recursos e inventário físico

- `ShelfRow`: fileira de estante com componente-base, slots e botão de reposição.
- O botão materializa o estoque da fileira com VFX; não é automático por tempo.
- `ComponentTray`: bandeja física na Mesa do Símbolo, com slots para vários componentes.
- Componentes processados são objetos físicos; podem ser guardados em uma área/estante de produtos ou carregados na bandeja.
- Sem hotbar, sem menu de itens, sem inventário abstrato.

### 9.1 Nomes, hover e sockets

- O nome do componente aparece apenas no hover/seleção, não o tempo todo.
- O label 3D fica desabilitado até o item ser hover/selecionado.
- Um **socket** é o slot físico de encaixe (por exemplo, os 6 sockets da bandeja). Quando o item está em um socket, o nome some.
- Os labels persistentes das fileiras de estante e os textos das estações foram removidos.
- As estações são identificadas por símbolos alquímicos nas paredes; a Terra fica em extensões das bancadas com o símbolo da Terra na própria extensão.

## 10. Efeitos no jogador

- `PlayerEffectController` aplica efeitos temporários:
  - `VignetteBlur`: vinheta/escurecimento e blur;
  - `ColorHallucination`: alteração de cor/visão.
- Não usar inversão de controles ou efeitos que causem desorientação forte.
- Sem dano, morte ou respawn.
- Efeitos podem ser aplicados por perfis de falha de reação ou por eventos de estação.

## 11. Números iniciais sugeridos (placeholders)

| Parâmetro | Valor inicial |
|---|---|
| Dissolutio: giro total para progresso 1 | 360 graus |
| Dissolutio: faixa ideal | 0,60–0,85 |
| Dissolutio: tempo de permanência no estado | 1,5 s (placeholder) |
| Dissolutio: extração | automática após o dwell |
| Calcinatio: calor máximo | 100 |
| Calcinatio: ganho por bombeada | 15 |
| Calcinatio: decaimento contínuo da chama | 8 por segundo |
| Calcinatio: limite entre resíduo fraco/forte | 50% da potência |
| Calcinatio: estágios incluem cru | sim |
| Calcinatio: faixas de estágio | 0–30 cru, 30–55 calcinado, 55–80 supercalcinado, 80–100 cinza |
| Sublimatio: tempo total de aquecimento | 20 s |
| Sublimatio: faixas de gás | 0–5 fraco, 5–12 estável, 12–20 instável |
| Sublimatio: cristal | origem + estado do gás |
| Sublimatio: resfriamento | 8 s |
| Tolerância de posição do sigilo | ~8 cm no espaço da mesa |
| Confiança mínima do sigilo | 0,75 |
| Distância do gesto de conjurar | 0,15 m |
| Tempo do gesto | 0,30 s |
| Capacidade da bandeja | 6 slots |
| Reposição de fileira | instantânea com VFX |

Todos esses valores devem ficar em campos do Inspector ou em ScriptableObjects, não espalhados pelo código. Os números de dwell, decaimento e potência são placeholders até as duas cadeias de reações serem definidas.

## 12. Critérios de determinismo e teste

- Dado o mesmo componente, estado, tags, progresso e símbolo, o resolvedor deve retornar sempre o mesmo resultado.
- Testar cada faixa de processo de forma isolada.
- Testar reação por componente exato e por tags.
- Testar desenho válido, desenho inválido, sigilo faltando, sigilo na âncora errada e símbolo principal incoerente.
- Testar perfis de falha: consumo, bloqueio, limpeza, distorção e efeito no jogador.
- Testar pausa/interrupção removendo o item ou desligando a fonte.
- Testar o dwell da Dissolutio: parar no estado, aguardar, extração automática e quantidade de resíduo.
- Testar os 2 resíduos da Calcinatio (chama fraca/forte) e a manutenção/decaimento da chama.
- Testar a matriz da Sublimatio: origem + estado do gás → gás físico e cristal.
- Testar o label no hover/seleção e o sumiço quando o item entra em um socket.
- Testar persistência do Grimório entre sessões.
