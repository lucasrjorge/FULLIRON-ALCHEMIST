# PROTOTYPE PLAN — FULLIRON ALCHEMIST

## 1. Objetivo

Construir um **sandbox jogável** que valide as mecânicas base:

- pegar e carregar componentes físicos;
- processar componentes em tempo real nas estações;
- desenhar símbolo principal + sigilos de elementos à mão livre;
- posicionar componentes e conjurar com o gesto de palmas;
- resolver reações por componente exato e por tags;
- registrar descobertas e falhas no Grimório;
- lidar com uma anomalia de distorção da sala.

O protótipo não precisa de arte final, áudio definitivo, puzzles definitivos, reações auxiliares ou hand tracking.

## 2. Forma de trabalho

- O agente escreve scripts, dados de exemplo e um **guia de montagem** com hierarquia, componentes e campos do Inspector.
- O usuário monta cena, prefabs e assets no Unity, testa e reporta erros de Console ou comportamento inesperado.
- O agente não assume que consegue compilar ou rodar Unity no próprio ambiente.
- Cada fase tem um gate de aceite. Não avançar sem o teste da fase anterior.
- A cena base é uma duplicata de `BasicScene`; não alterar `SampleScene`.
- Sem gerador automático de cena nesta fase.

## 3. Fases

### Fase 0 — Laboratório e controles

**Objetivo:** ter um espaço navegável e um rig funcionando.

**Agente entrega:**

- Checklist de criação da cena `PrototypeLab` a partir de `BasicScene`.
- Guia de configuração de teleporte, movimento contínuo, snap turn e zona de conjuração.
- `PrototypeLog` e um cubo de teste grab.

**Usuário monta:**

- Duplicar `BasicScene` para `PrototypeLab` e adicionar ao Build Settings.
- Ajustar chão para ~20×20 m, paredes, luzes e um blockout das 4 mesas e 2 estantes.
- Configurar o XR Origin, locomoção e um cubo de teste.

**Teste de aceite:**

- Andar por teleporte e por movimento contínuo; alternar entre eles.
- Girar com snap turn.
- Pegar, soltar e arremessar o cubo.
- Console sem erros.

**Conceitos:** XR Origin, tracking space, conforto, escala, teleporte, snap turn.

### Fase 1 — Núcleo, itens, tags e estantes

**Objetivo:** componentes físicos com identidade, estado, tags e reposição.

**Agente entrega:**

- Enums, ScriptableObjects e `AlchemyDatabase`.
- `AlchemicalItem`, `ComponentVisuals`, `ShelfRow`, `ComponentTray`, `PrototypeLog`.
- Guia de criação de componentes-base, componentes processados demo e prefab de item.

**Usuário monta:**

- Componentes-base demo (ex.: Ferrum, Sulfur, Aqua, Sal).
- 3–5 componentes processados demo com tags e aparência distinta.
- `Item_Base.prefab` com `XRGrabInteractable`, Rigidbody, collider, `AlchemicalItem`, `ComponentVisuals` e label TMP.
- Duas estantes com fileiras e botões de reposição.
- Bandeja na Mesa do Símbolo.

**Teste de aceite:**

- Apertar o botão de uma fileira e ver os itens se materializarem.
- Pegar itens, colocar na bandeja e ver nome/estado/tags no label ou debug hud.
- Alterar o componente pelo menu de contexto e ver a aparência mudar.
- Console sem erros.

**Conceitos:** ScriptableObjects, prefabs, Rigidbody/Collider, `XRGrabInteractable`, separação dados/visual.

### Fase 2 — Símbolo livre, sigilos e mesa de reação

**Objetivo:** validar o núcleo do puzzle: desenhar, posicionar e conjurar.

**Agente entrega:**

- `SymbolDrawingSurface`, `StrokeCapture`, `SymbolCapture`.
- `SigilTemplate`, `SymbolTemplate`, `ISymbolValidator`, `GeometricSymbolValidator`.
- `CastGestureDetector`.
- `ReactionRequest`, `ReactionRequirement`, `ReactionRecipe`, `ReactionResolver`, `ReactionResult`.
- `SymbolTable`, `ReactionVfxPlayer`, `FailureProfile`.
- Guia de montagem da mesa e dos templates.

**Usuário monta:**

- Superfície de desenho na mesa, com collider e interactable.
- 4 âncoras físicas (Aqua, Ignis, Terra, Aer) com `ItemSocket`.
- Zona de conjuração, botão de limpar e ponto de spawn do resultado.
- 2 templates de símbolo principal e os 4 sigilos alquímicos clássicos.
- 2 receitas demo: uma por componente exato, uma por tags.
- Um perfil de falha demo e um VFX de fizzle.

**Teste de aceite:**

- Desenhar o símbolo principal e os 4 sigilos em uma sequência; conjurar com as palmas.
- Desenho válido: reação acontece.
- Desenho inválido: efeito de “magia não se concentra”, sem reação e sem consumir.
- Receita por componente exato funciona.
- Receita por tags funciona.
- Reação adversa aplica consumo/bloqueio/limpeza do perfil demo e registra no Grimório (stub).
- Console sem erros.

**Conceitos:** captura de traços, normalização, classificação de sigilos, tolerância, gesto com dois controles, lógica pura de reação.

> Esta é a fase mais delicada. Se necessário, dividir em 2a (captura + gesto) e 2b (validação + reação).

### Fase 3 — Dissolutio

**Objetivo:** primeira estação de processo ativo.

**Agente entrega:**

- `CrankWheel`, `DissolutionStation`, `ResidueSpawner`, `GaugeBar` (se necessário).
- Perfis de processo incompleto/ideal/superdissolvido.

**Usuário monta:**

- Tubo, pistão, manivela, bandeja de resíduo e VFX de líquido.
- Referências ao banco de dados e ao Grimório.

**Teste de aceite:**

- Item no tubo + girar pouco = incompleto.
- Girar na faixa ideal = dissolvido.
- Girar demais = superdissolvido.
- Resíduo Terra aparece.
- Remover o item pausa/interrompe.
- Console sem erros.

**Conceitos:** interação contínua, acumulação de rotação, máquina de estados, determinismo.

### Fase 4 — Calcinatio e Terra

**Objetivo:** estação de calor com estágios, selo e falha.

**Agente entrega:**

- `PumpHandle`, `CalcinationStation`, `GaugeBar`, `StationAlert`.
- `FailureMess`, `MessBin`.
- Perfis de estágio e overheat.

**Usuário monta:**

- Grelha, fole/bomba, botão de selar, fogo, medidor, resíduo e lixeira.
- Luz/som de alerta.

**Teste de aceite:**

- Bombear aumenta calor; com o fogo aceso o componente continua aquecendo; desligar pausa.
- Selar em cada faixa gera o componente correspondente.
- Ignorar e passar do ponto gera cinza/queimado.
- Overheat gera bagunça/fumaça e permite limpar.
- Resíduo Terra aparece.
- Console sem erros.

**Conceitos:** tempo real, curvas, alertas multi-sensoriais, falha controlada, limpeza.

### Fase 5 — Sublimatio

**Objetivo:** estação passiva com gás e cristalização.

**Agente entrega:**

- `SublimationStation`, suporte a 3 estados de gás, cristalização binária.
- VFX de vapor por cor e alertas.

**Usuário monta:**

- Aquecedor, câmara de gás, ventoinha, câmara de resfriamento e saídas.
- VFX de vapor/cristal.

**Teste de aceite:**

- Aquecer e ver os 3 estados de gás por cor.
- Usar o gás diretamente.
- Cristalizar e obter cristal diferente por estado.
- Ignorar e obter estado errado.
- Desligar/remover pausa.
- Console sem erros.

**Conceitos:** máquina de estados, timers, feedback por cor, escolha binária.

### Fase 6 — Grimório, perfis de falha e distorção

**Objetivo:** fechar o loop de descoberta e consequência.

**Agente entrega:**

- `GrimoireManager`, `GrimoireBoard`, `GrimoirePersistence`.
- `RoomDistortion`, `PlayerEffectController`, `LabReset`.
- Perfis de falha por receita.

**Usuário monta:**

- Púlpito/livro e painel world-space com TMP.
- Props que podem flutuar e área de distorção.
- Volume/overlay de vinheta/blur e color grading.
- Botão de reset.

**Teste de aceite:**

- Páginas de receita existem incompletas desde o início.
- Processar componentes e concluir reações preenche descobertas.
- Falhas são registradas.
- Distorção faz objetos flutuarem, permite pegá-los e some após um tempo sem bloquear.
- Efeito de vinheta/cor temporário funciona.
- Save/load do Grimório entre sessões.
- Reset limpa o laboratório.
- Console sem erros.

**Conceitos:** UI world-space, persistência, eventos de descoberta, perfis de falha, feedback de conforto.

### Fase 7 — Polimento e performance

**Objetivo:** deixar o sandbox confortável e apresentável.

**Tarefas:**

- Iluminação URP, materiais, partículas, áudio simples e haptics.
- Otimização para Quest: luzes, sombras, draw calls, partículas, textos.
- Teste de 72 Hz, leitura em VR e alcance.
- Checklist de conforto e playtest.
- Ajuste de tolerâncias do desenho e tempos dos processos.

## 4. Escopo e simplificações

- 4 âncoras fixas nos pontos cardeais no protótipo; âncoras móveis por template ficam para depois.
- Símbolo principal e sigilos são templates geométricos placeholders; a arte final vem depois.
- O validador geométrico é uma ponte; o LAYA entra depois sobre a imagem renderizada.
- O validador geométrico aceita variações dentro de famílias de templates; ele não aprende símbolos novos em tempo real. Cadastro/ensino de variantes fica para a fase LAYA.
- Duas receitas demo: uma exata e uma por tags.
- Componentes demo: 4 base + 5–8 processados.
- Uma anomalia: distorção da sala.
- Sem reações auxiliares, hand tracking, multiplayer, arte final ou conteúdo definitivo de puzzles.
- A bandeja começa estática na mesa; carregá-la com as duas mãos fica para depois.
- A distorção não bloqueia nada e termina sozinha.

## 5. Matriz de testes

| Teste | Editor (Simulator) | Quest via Link | Build Android |
|---|---|---|---|
| Locomoção e snap turn | Sim | Sim | Sim |
| Grab e arremesso | Sim | Sim | Sim |
| Reposição de estante | Sim | Sim | Sim |
| Bandeja | Sim | Sim | Sim |
| Desenho e sigilos | Sim (mouse/controle) | Sim | Sim |
| Gesto de palmas | Aproximação no simulador | Sim | Sim |
| Processos em tempo real | Sim | Sim | Sim |
| Grimório e persistência | Sim | Sim | Sim |
| Distorção e efeitos | Sim | Sim | Sim |
| Performance 72 Hz | Não | Parcial | Sim |

## 6. Definição de pronto do sandbox

- O jogador consegue pegar ingredientes, processar nas 3 estações, carregar componentes na bandeja, desenhar símbolo + sigilos, posicionar e conjurar.
- Pelo menos uma reação exata e uma por tags funcionam.
- Pelo menos um erro de processo gera componente novo.
- Pelo menos uma reação adversa aplica perfil de falha e é registrada.
- A distorção da sala acontece e termina.
- O Grimório registra descobertas e falhas e persiste.
- O laboratório pode ser resetado.
- O protótipo roda no Quest standalone sem erros de Console e com conforto aceitável.
