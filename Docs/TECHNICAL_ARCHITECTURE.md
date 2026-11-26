# TECHNICAL ARCHITECTURE — FULLIRON ALCHEMIST

## 1. Princípios

- **Dados separados de comportamento:** componentes, perfis de processo, receitas e símbolos são ScriptableObjects; MonoBehaviours orquestram.
- **Itens físicos:** cada componente é uma instância de prefab com `XRGrabInteractable`, collider, Rigidbody e `AlchemicalItem`.
- **Regras determinísticas:** a resolução de processo e de reação é lógica pura, sem depender de física complexa.
- **Referências explícitas:** preferir campos no Inspector a singletons escondidos. Um `PrototypeLog` e um `GrimoireManager` podem ser singletons de protótipo se isso reduzir wiring.
- **Fatias pequenas:** cada entrega deve ser compilável e testável isoladamente.
- **Sem gerador de cena:** o usuário monta a cena, prefabs e assets no Editor para aprender. O agente entrega scripts e guias.
- **Sem asmdef nesta fase:** o código novo fica em `Assembly-CSharp` para referenciar XRI, TMP e scripts do template sem configuração extra.
- **Namespace:** `Fulliron.Alchemy`.

## 2. Dependências e APIs

- `UnityEngine.XR.Interaction.Toolkit`
- `UnityEngine.XR.Interaction.Toolkit.Interactables`
- `UnityEngine.XR.Interaction.Toolkit.Interactors`
- `TMPro`
- `UnityEngine.InputSystem` (quando necessário)
- URP (`Universal Render Pipeline/Lit`, partículas, post-processing)

O arquivo `Assets/VRTemplateAssets/Scripts/XRKnob.cs` serve de referência para criar interactables customizados. Para o protótipo, a manivela deve ser implementada como `CrankWheel` próprio, para não depender de scripts do template e permitir acumulação de rotação.

## 3. Estrutura de pastas

```text
Assets/FullironAlchemist/
  Scripts/
    Core/
      AlchemicalElement.cs
      ComponentState.cs
      ProcessType.cs
      EffectTag.cs
      AlchemicalComponent.cs
      AlchemicalItem.cs
      ComponentVisuals.cs
      ItemLabel.cs
      ProcessHistory.cs
      ProcessingProfile.cs
      ProcessingOutcome.cs
      AlchemyDatabase.cs
      AlchemyEvents.cs
      PrototypeLog.cs
    Interaction/
      ShelfRow.cs
      ComponentTray.cs
      ItemSocket.cs
      SimpleButton.cs
      ToggleButton.cs
      CrankWheel.cs
      PumpHandle.cs
      GaugeBar.cs
      HapticFeedback.cs
      LocomotionSettings.cs
    Stations/
      AlchemyStation.cs
      DissolutionStation.cs
      CalcinationStation.cs
      SublimationStation.cs
      StationAlert.cs
      ResidueSpawner.cs
      FailureMess.cs
      MessBin.cs
    Symbols/
      SymbolDrawingSurface.cs
      StrokeCapture.cs
      SymbolCapture.cs
      SigilTemplate.cs
      SymbolTemplate.cs
      ISymbolValidator.cs
      GeometricSymbolValidator.cs
      SymbolValidationResult.cs
      CastGestureDetector.cs
    Reactions/
      ReactionRequest.cs
      Placement.cs
      ReactionRequirement.cs
      ReactionRecipe.cs
      FailureProfile.cs
      ReactionResult.cs
      ReactionResolver.cs
      ReactionVfxPlayer.cs
      SymbolTable.cs
    Grimoire/
      GrimoireManager.cs
      GrimoireBoard.cs
      GrimoireDatabase.cs
      GrimoireState.cs
      GrimoirePersistence.cs
    Debug/
      AlchemyDebugHud.cs
      LabReset.cs
      RoomDistortion.cs
      PlayerEffectController.cs
  Data/
    Components/
    Processing/
    Reactions/
    Symbols/
    FailureProfiles/
    AlchemyDatabase.asset
  Prefabs/
    Item_Base.prefab
    Station_*.prefab
    VFX_*.prefab
  Materials/
  Scenes/
    PrototypeLab.unity
```

Os assets de dados podem ficar em `Data/` com subpastas por tipo. O banco central (`AlchemyDatabase`) referencia todos.

## 4. Modelo de dados

### 4.1 Enums

```csharp
public enum AlchemicalElement { None, Aqua, Ignis, Terra, Aer }

public enum ComponentState
{
    Raw, Incomplete, Dissolved, Superdissolved,
    Calcined, Supercalcined, Ash,
    GasWeak, GasStable, GasUnstable,
    Crystallized, Residue, Burnt, Corrupted
}

public enum ProcessType { None, Dissolutio, Calcinatio, Sublimatio }

[Flags]
public enum EffectTag
{
    None = 0,
    Heat = 1 << 0,
    Cold = 1 << 1,
    Moist = 1 << 2,
    Dry = 1 << 3,
    Volatile = 1 << 4,
    Pure = 1 << 5,
    Corrosive = 1 << 6
}
```

### 4.2 ScriptableObjects

```text
AlchemicalComponent
  string id
  string displayName
  string description
  AlchemicalElement element
  ComponentState defaultState
  Color color
  EffectTag tags
  AlchemicalComponent residue
  Material optionalMaterial

ProcessingProfile
  AlchemicalComponent input
  ComponentState inputState (optional)
  ProcessType process
  ProcessingOutcome[] outcomes
  ProcessingOutcome interruptOutcome (optional)

ProcessingOutcome
  float minProgress
  float maxProgress
  AlchemicalComponent output
  ComponentState outputState
  AlchemicalComponent residue
  string message

ReactionRecipe
  string id
  string displayName
  SymbolTemplate symbol
  ReactionRequirement[] requirements
  AlchemicalComponent result
  ComponentState resultState
  bool consumeComponents
  FailureProfile failureProfile
  int priority

SymbolTemplate
  string id
  string displayName
  Vector2[] anchors
  AlchemicalElement[] anchorElements
  MainSymbolReference mainSymbol
  SigilTemplate[] acceptedSigils
  float mainTolerance
  float sigilPositionTolerance

FailureProfile
  bool consumeComponents
  BlockTarget blockTarget
  float blockDuration
  CleanupType cleanupType
  AnomalyType anomalyType
  PlayerEffectFlags playerEffect
  float playerEffectDuration
  bool recordInGrimoire
```

## 5. Runtime

### 5.1 Item

`AlchemicalItem`:

- Referencia `AlchemicalComponent`, `ComponentState`, `EffectTag` e `ProcessHistory`.
- Métodos:
  - `SetData(AlchemicalComponent, ComponentState)`.
  - `ApplyOutcome(ProcessingOutcome)`.
  - `Consume()` (dispara evento e destrói).
  - `ResetToDefault()`.
- `ComponentVisuals` observa o item e atualiza:
  - cor/emissão via `MaterialPropertyBlock` (URP `_BaseColor`, `_EmissionColor`);
  - escala/partículas para gás, cinza, resíduo;
  - label TMP com nome e estado.

### 5.2 Estantes e bandeja

`ShelfRow`:

- Campos: componente-base, prefab de item, slots, botão de reposição, VFX de materialização.
- `Restock()` instancia itens nos slots vazios.
- Sem reposição automática por tempo nesta fase.

`ComponentTray`:

- Slots com `XRSocketInteractor` ou triggers.
- Capacidade inicial 6.
- Pode ser estática na mesa ou grabbable; começar estática para reduzir escopo.

### 5.3 Estações

`AlchemyStation` (base):

- Referência ao `AlchemyDatabase`, `PrototypeLog` e `GrimoireManager`.
- Estado: `Idle`, `Running`, `Waiting`, `Complete`, `Blocked`.
- Métodos: `TryStart()`, `Tick(float deltaTime)`, `Interrupt()`, `ApplyResult()`.
- Atualiza em `Update` mesmo com o jogador longe.

`DissolutionStation`:

- `CrankWheel` alimenta progresso.
- Faixas do `ProcessingProfile` escolhem o resultado.
- `ResidueSpawner` instancia o resíduo.

`CalcinationStation`:

- `PumpHandle` adiciona calor; enquanto o fogo estiver aceso, o componente continua aquecendo.
- `SimpleButton` de selar captura o estágio atual e para o processo.
- Desligar o fogo ou remover o item pausa; o estágio atingido não regride.
- `GaugeBar` mostra calor.
- Overheat dispara `FailureMess`/VFX e limpeza.

`SublimationStation`:

- `ToggleButton` liga/desliga aquecedor.
- Timer define `GasWeak/GasStable/GasUnstable`.
- `ToggleButton` da ventoinha cristaliza; o resultado depende do estado do gás.
- VFX de vapor por cor.

`StationAlert`:

- Toca som, muda luz/emissão e move o item quando o processo passivo está perto do limite.
- Deve ser perceptível a distância.

### 5.4 Símbolo

`SymbolDrawingSurface`:

- Implementa a captura de traços com o gatilho do controle.
- Recebe `IXRSelectInteractor` do ray/direct interactor.
- Salva `StrokeCapture` com pontos no espaço local da superfície.
- Renderiza com `LineRenderer` ou mesh.
- Fornece `Clear()`.

`StrokeCapture`:

- Lista de pontos normalizados.
- Bounding box, comprimento, número de pontos.
- Pontos brutos para debug.

`SymbolCapture`:

- Todos os traços.
- `Texture2D` renderizada (para LAYA futuro).
- `SymbolValidationResult` após validação.

`ISymbolValidator`:

```csharp
public interface ISymbolValidator
{
    SymbolValidationResult Validate(SymbolCapture capture, IReadOnlyList<SymbolTemplate> templates);
}
```

`GeometricSymbolValidator`:

- Classifica traços pequenos como candidatos a sigilo.
- Associa candidatos às âncoras mais próximas.
- Verifica elemento esperado por âncora e distância.
- Remove sigilos dos traços e compara o restante com o símbolo principal do template.
- Usa tolerância alta no principal e baixa nos sigilos.
- Retorna motivo da falha para o feedback.

`CastGestureDetector`:

- Referências aos transforms dos controles esquerdo/direito.
- Mede distância e alinhamento.
- Exige permanência e cooldown.
- Só dispara se o jogador estiver na `CastZone` da mesa.
- Toca VFX/som/haptics e chama `SymbolTable.Cast()`.
- Animação contextual: no protótipo, usar VFX de palmas se encontrando + haptics; modelos de mão dedicados ficam para o polimento.

`SymbolTable`:

- Referencia superfície de desenho, âncoras, bandeja, `GrimoireManager`, `AlchemyDatabase`, `ReactionVfxPlayer`.
- No cast:
  1. valida o desenho;
  2. se inválido, fizzle;
  3. coleta componentes das âncoras;
  4. monta `ReactionRequest`;
  5. chama `ReactionResolver`;
  6. aplica sucesso ou perfil de falha;
  7. registra no Grimório.

### 5.5 Reação

`ReactionResolver` é lógica pura:

```csharp
public static ReactionResult Resolve(
    AlchemyDatabase database,
    ReactionRequest request);
```

- Não acessa cena nem `MonoBehaviour`.
- Retorna `ReactionResult` com sucesso, receita, mensagem, motivo de falha e perfil aplicado.
- `SymbolTable` executa os efeitos de cena.

### 5.6 Grimório

`GrimoireManager`:

- Carrega/salva `GrimoireState`.
- Expõe `DiscoverReaction`, `DiscoverComponent`, `RecordFailure`.
- Atualiza `GrimoireBoard` e persiste via `PlayerPrefs`/JSON.
- Chave sugerida: `fulliron.grimoire.v1`.

`GrimoireBoard`:

- Painel world-space com 2–3 textos TMP: reações, componentes e detalhes.
- Sem UI Canvas complexa nesta fase.
- Atualiza ao receber eventos do manager.

### 5.7 Debug e falhas

- `PrototypeLog`: log no Console e em um TMP world-space opcional.
- `AlchemyDebugHud`: estado do item, progresso da estação, símbolo reconhecido, contagens do Grimório.
- `RoomDistortion`: escolhe props, desliga gravidade/rigidbody ou aplica flutuação, permite grab, encerra após duração.
- `PlayerEffectController`: vinheta/blur e color grading temporários.
- `LabReset`: limpa itens, sockets, desenho, estações e opcionalmente o save.

## 6. Hierarquia sugerida da cena `PrototypeLab`

```text
PrototypeLab
  Directional Light
  XR Origin (XR Rig)                // duplicado de BasicScene
  XR Interaction Manager
  EventSystem
  Floor + TeleportationArea
  Room
    Walls
    Ceiling
    Lights
  Stations
    SymbolTable
      DrawingSurface
      Anchor_Aqua / Anchor_Ignis / Anchor_Terra / Anchor_Aer
      Tray
      CastZone
      ClearButton
      ResultSpawn
    DissolutionTable
      Tube / Piston / Crank / ResidueTray
    PurificationTable
      Grate / Bellows / SealButton / FireVFX / HeatGauge / ResidueTray
    SublimationTable
      Heater / GasChamber / Fan / CoolingCoil / GasOutput / CrystalOutput
  Shelves
    ShelfRow_1..N
  Grimoire
    Lectern
    Book
    Board
  Cleanup
    MessBin
    Tools
  Debug
    PrototypeLog
    AlchemyDebugHud
```

## 7. Wiring de interação

- **Itens:** `XRGrabInteractable` + `Rigidbody` + collider. Ajustar `movementType` para `Instantaneous` ou `VelocityTracking` conforme conforto.
- **Sockets:** `XRSocketInteractor` com collider trigger e attach transform; usar `ItemSocket` para expor o item.
- **Botões:** `XRSimpleInteractable` + collider + `SimpleButton` com evento UnityEvent.
- **Toggles:** `XRSimpleInteractable` + `ToggleButton` com estado ligado/desligado.
- **Manivela:** `XRBaseInteractable` próprio com cálculo de ângulo local e evento de progresso.
- **Fole:** `PumpHandle` com animação de descida/retorno e evento de bombeada.
- **Gesto:** transforms dos controles do XR Origin; não requer hand tracking.

## 8. Fluxo de dados de uma reação

```text
DrawingSurface
  -> SymbolCapture
  -> GeometricSymbolValidator / LayaSymbolValidator
  -> SymbolValidationResult

SymbolTable
  -> coleta ItemSocket[] + símbolo
  -> ReactionRequest
  -> ReactionResolver (puro)
  -> ReactionResult
  -> ReactionVfxPlayer + GrimoireManager + FailureProfile
```

## 9. Falhas e limpeza

- O perfil de falha é aplicado pelo `SymbolTable` após o resolvedor.
- `RoomDistortion` implementa a anomalia do protótipo; não bloqueia nada.
- `FailureMess`/`MessBin` implementam bagunça física e limpeza.
- `PlayerEffectController` aplica efeitos no jogador.
- O bloqueio de mesa/área deve ser explícito: um `Blockable`/`StationBlock` com duração, para o jogador saber o que está impedido.

## 10. Persistência

- Salvar apenas IDs e flags, não referências de cena.
- `GrimoireState` serializável com listas de IDs.
- Carregar no `Awake` do `GrimoireManager`.
- Reset de save via `ContextMenu` e botão de debug.
- Dados de conteúdo (receitas/componentes) ficam nos ScriptableObjects e não são salvos.

## 11. Performance para Quest

- URP com poucas luzes em tempo real; preferir luzes baked/ambiente no laboratório.
- Evitar sombras em tempo real em muitos objetos.
- Partículas simples e com orçamento.
- Textos TMP legíveis, sem canvas gigante.
- Evitar `GetComponent` em `Update`; cachear referências.
- Testar 72 Hz no Quest; medir com Profiler.
- ASTC para texturas; não adicionar texturas grandes no protótipo.
- Não usar post-processing pesado; vinheta/blur de efeito no jogador deve ser simples.

## 12. Estratégia de testes

- Editor: XR Device Simulator para locomoção, grab e botões.
- Quest via Link para testar gesto, alcance e leitura.
- Build Android para validar IL2CPP/ARM64.
- Testes manuais por fase, com checklist em `Docs/PROTOTYPE_PLAN.md`.
- Testes automatizados de lógica pura só depois de criar asmdefs; por enquanto, validar via debug hud e Console.
