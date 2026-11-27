# GUIA DE MONTAGEM — FASE 0 + FASE 1

Este guia acompanha os scripts da Fase 0 e Fase 1. Você pode montar a cena, os prefabs e os assets manualmente seguindo o passo a passo, ou usar o construtor automático de protótipo.

> **Versão repaginada:** o construtor atual usa o subconjunto **Enxofre, Mercúrio, Sal, Orvalho e Ferro**; os nomes aparecem só no hover; as estações têm símbolos alquímicos nas paredes; a Terra fica em extensões das bancadas; a iluminação foi aumentada. As tabelas manuais abaixo são a referência de wiring da primeira versão; para a cena atual, a fonte de verdade é o construtor (`Fulliron > Prototype > Construir Fase 0+1`) e o código em `Assets/FullironAlchemist/Editor/PrototypeBuilder.cs`.

## Atalho: construtor automático

O projeto tem um construtor de Editor em `Assets/FullironAlchemist/Editor/PrototypeBuilder.cs`.

1. Abra o Unity e espere a compilação terminar.
2. Use o menu **Fulliron > Prototype > Construir Fase 0+1**.
3. O construtor cria ou atualiza:
   - componentes demo em `Assets/FullironAlchemist/Data/Components`;
   - banco `AlchemyDatabase.asset`;
   - materiais em `Assets/FullironAlchemist/Materials`;
   - prefab `Assets/FullironAlchemist/Prefabs/Item_Base.prefab`;
   - cena `Laboratorium` com sala, mesas, estantes, bandeja, botões, log e item de teste;
   - a entrada da cena nas Build Settings.
4. Entre em Play Mode e teste a Fase 1.

O construtor preserva os objetos existentes da cena e recria apenas o root `FULLIRON_PROTOTYPE`. Se a cena `Laboratorium` não existir, ele duplica `BasicScene`.

Também é possível rodar em batch mode, com o Unity fechado:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -batchmode -quit -projectPath "D:\VR-Game\FULLIRON-ALCHEMIST" -executeMethod Fulliron.Alchemy.Editor.PrototypeBuilder.BuildPhase01 -logFile "Logs/PrototypeBuilderBatch.log"
```

## Fatia atual: Calcinatio + Mesa do Símbolo + Grimório

O protótipo atual é uma fatia vertical com apenas a Calcinatio, a reação na Mesa do Símbolo e o Grimório. Dissolutio e Sublimatio ficam para depois.

Para testar sem procurar as estantes, o construtor deixa itens de teste nas próprias bancadas:
- **Calcinatio:** um Enxofre cru na mesa.
- **Mesa do Símbolo:** Enxofre, Sal e Orvalho crus à esquerda.

Os botões têm rótulos no mundo: **BOMBEAR**, **SELAR**, **CONJURAR** e **REPOR**.

### Calcinatio

1. Pegue **Enxofre** na estante.
2. Coloque o Enxofre no socket do **cadinho** na bancada da Calcinatio.
3. Pressione o **fole** (proxy: balde + botão) para subir a chama.
4. A chama decai com o tempo; continue bombeando para manter a potência no medidor.
5. O texto mostra o estágio: Cru, Calcinado, Supercalcinado ou Cinza.
6. Quando chegar no estágio desejado, pressione o botão de **selar**.
7. O Enxofre vira o componente daquele estágio. Retire-o do cadinho.
8. O resíduo Terra aparece na extensão da bancada. Ele é **fraco** ou **forte** conforme a potência da chama no momento do selo.

A Calcinatio tem perfil de processo configurado para o Enxofre. Outros primários mostram “sem perfil” nesta fatia.

### Mesa do Símbolo

A mesa está em modo demo com o símbolo fixo **Ouro**. O desenho livre/LAYA fica para a próxima etapa.

1. Posicione os componentes nos sockets com os símbolos:
   - **Ignis**: Enxofre Calcinado;
   - **Terra**: Sal;
   - **Aqua**: Orvalho.
2. Pressione o **botão de conjurar** na frente da mesa.
3. A mesa consome os componentes e gera **Ouro**.
4. O Grimório registra a reação e o componente Ouro.

Se os componentes estiverem errados, a reação não acontece e a falha é registrada no Grimório.

### Grimório

- Passe o cursor sobre um item para descobrir o componente.
- O Grimório mostra a página da reação “Transmutação de Ouro” com resultado visível e componentes ocultos até a descoberta.
- Selar uma Calcinatio ou concluir a reação preenche as descobertas.
- O save fica em `PlayerPrefs`; use o menu de contexto `Reset Save` no `GrimoireManager` para limpar.

### Nomes e sockets

- O nome do componente aparece apenas no hover/seleção.
- Em um socket (slots da bandeja, âncoras da mesa ou cadinho), o label some.

## Testar sem headset

O jeito recomendado é usar o **XR Interaction Simulator** do XR Interaction Toolkit. Ele simula headset e controles no Editor com teclado e mouse.

1. Abra `Window > Package Manager`.
2. Selecione o pacote **XR Interaction Toolkit**.
3. Abra a aba **Samples**.
4. Importe **XR Interaction Simulator** (recomendado) ou **XR Device Simulator** (clássico/deprecado).
5. No Project, abra `Assets/Samples/XR Interaction Toolkit/3.5.1/XR Interaction Simulator`.
6. Arraste `XR Interaction Simulator.prefab` para a cena `Laboratorium` (como root).
7. Salve a cena e entre em Play Mode com o Quest desconectado.
8. O HUD do simulador aparece no Game view. Use o HUD para clicar nos controles virtuais e ver os atalhos atuais.

Atalhos iniciais do **XR Interaction Simulator**:

| Tecla | Ação |
|---|---|
| `Tab` | Alterna o dispositivo manipulado (headset, controle esquerdo, direito). |
| `WASD` | Move o dispositivo para frente/trás/lados. |
| `Q` / `E` | Move para baixo/cima. |
| Setas | Rotaciona o dispositivo. |
| `[` / `]` | Manipula o controle esquerdo/direito. |
| `H` | Manipula o headset. |
| `` ` `` (backquote) | Alterna o “quick action” atual. |
| `Espaço` | Executa o quick action (útil para trigger/grip). |
| `R` | Reseta o simulador. |
| Botão direito do mouse | Alterna o modo de rotação com o mouse. |
| `X` | Abre/fecha o menu de ações do HUD. |

Dicas:

- Use o HUD para selecionar o controle e clicar nos botões virtuais quando não lembrar o atalho.
- Para pegar um componente, selecione o controle direito/esquerdo, escolha o quick action de trigger/grip e execute com `Espaço`; o `XRGrabInteractable` responde como no VR.
- O simulador é uma ferramenta de **Editor**. Antes de buildar para o Quest, remova ou desative o prefab do simulador.
- Se o HUD não aparecer, confira se o prefab principal do sample foi instanciado e se o Game view não está com resolução muito pequena. Se necessário, arraste também `UI/XR Interaction Simulator UI.prefab` para a cena.

O passo a passo manual abaixo continua válido para aprender o wiring e para ajustar o que o construtor criou.

## O que foi implementado

```text
Assets/FullironAlchemist/Scripts/Core/
  AlchemicalElement.cs
  ComponentState.cs
  ProcessType.cs
  EffectTag.cs
  AlchemicalComponent.cs
  ProcessHistory.cs
  ProcessingOutcome.cs
  ProcessingProfile.cs
  AlchemyDatabase.cs
  AlchemicalItem.cs
  ComponentVisuals.cs
  ItemLabel.cs

Assets/FullironAlchemist/Scripts/Interaction/
  SimpleButton.cs
  ItemSocket.cs
  ComponentTray.cs
  ShelfRow.cs

Assets/FullironAlchemist/Scripts/Debug/
  PrototypeLog.cs
```

Não foram criadas cenas, prefabs ou assets de dados automaticamente. Esta fase é intencionalmente montada por você para aprender o wiring.

## Antes de começar

1. Abra o projeto no Unity **6000.3.8f1** e espere a importação terminar.
2. Abra o Console (`Window > General > Console`) e confirme que não há erros de compilação.
3. Se aparecerem erros, pare aqui e envie a mensagem completa do Console antes de montar a cena.
4. Não altere `Assets/Scenes/SampleScene.unity`, `Assets/Samples` ou `Assets/VRTemplateAssets`.

---

# FASE 0 — Laboratório e controles

## 1. Criar a cena PrototypeLab

1. No Project, abra `Assets/Scenes`.
2. Clique com o botão direito em `BasicScene` e escolha **Duplicate**.
3. Renomeie a cópia para `PrototypeLab` e abra.
   - Se você já criou `Assets/Scenes/Laboratorium.unity`, use essa cena no lugar de criar outra; `PrototypeLab` é apenas um nome sugerido.
4. Adicione a cena (`PrototypeLab` ou `Laboratorium`) às cenas de build:
   - Unity 6: `File > Build Profiles` (ou `Build Settings`) > `Add Open Scenes`.
   - Confirme que o build target é **Android**.
5. Salve a cena (`Ctrl+S`).

A `BasicScene` já contém:

- XR Origin (XR Rig) do Starter Assets;
- XR Interaction Manager;
- EventSystem;
- chão com TeleportationArea;
- Directional Light.

### Teste do rig antes de montar o resto

1. Se estiver no Editor sem headset, ative o **XR Device Simulator** (`Window > XR > XR Device Simulator`) ou use o Quest via Link.
2. Entre em Play Mode.
3. Teste:
   - olhar em 360 graus;
   - virar com snap turn;
   - andar com movimento contínuo (analógico);
   - teleportar;
   - pegar/soltar um cubo de teste (você vai criar um abaixo).
4. Se algum modo de locomoção não funcionar, verifique o GameObject de locomoção dentro do XR Origin e me avise. Nesta fase não vamos criar o botão de alternância; vamos validar os dois modos manualmente.

## 2. Ajustar o chão e as paredes

1. Selecione `Plane`.
2. Ajuste o Scale para `(2, 1, 2)` para um chão de aproximadamente 20×20 m.
3. Crie uma pasta `Room` (Empty) para organizar.
4. Crie as paredes com cubos:
   - `Wall_North`: Position `(0, 1.5, 4)`, Scale `(8, 3, 0.2)`.
   - `Wall_South`: Position `(0, 1.5, -4)`, Scale `(8, 3, 0.2)`.
   - `Wall_East`: Position `(4, 1.5, 0)`, Scale `(0.2, 3, 8)`.
   - `Wall_West`: Position `(-4, 1.5, 0)`, Scale `(0.2, 3, 8)`.
5. Use um material URP/Lit simples para as paredes (cinza ou pedra).
6. As paredes devem manter o BoxCollider padrão do cubo.

## 3. Criar as mesas do laboratório

Use cubos com BoxCollider. O topo deve ficar a ~0,90 m do chão.

| Mesa | Position do topo | Scale |
|---|---|---|
| Mesa do Símbolo | `(0, 0.86, 2.5)` | `(1.6, 0.08, 0.9)` |
| Mesa da Dissolução | `(-2.8, 0.86, 0)` | `(1.2, 0.08, 0.8)` |
| Mesa da Calcinatio | `(2.8, 0.86, 0)` | `(1.2, 0.08, 0.8)` |
| Mesa da Gaseificação | `(0, 0.86, -2.5)` | `(1.2, 0.08, 0.8)` |

As bancadas de Dissolução e Calcinatio têm extensões laterais com o símbolo da Terra na própria extensão. As paredes atrás de Dissolução, Calcinatio e Gaseificação recebem os símbolos de Água, Fogo e Ar.

Dicas:

- O topo do cubo fica em `y = 0.86 + 0.04 = 0.90`.
- Use `M_Table` (URP/Lit, cor madeira escura) ou o material do template.
- As mesas precisam de collider para os itens não caírem.
- Pernas são opcionais nesta fase; você pode adicionar depois.

## 4. Criar as estantes

Para a Fase 1, crie pelo menos uma fileira funcional; depois duplique.

1. Crie um cubo `Shelf_West` na parede oeste:
   - Position `(-3.8, 1.2, 1.5)`;
   - Scale `(0.3, 0.05, 2.0)`;
   - Material `M_Shelf`.
2. Crie um cubo `Shelf_East` na parede leste:
   - Position `(3.8, 1.2, 1.5)`;
   - Scale `(0.3, 0.05, 2.0)`.
3. As prateleiras precisam de BoxCollider (o cubo já tem).
4. Mais tarde, as fileiras `ShelfRow` serão posicionadas em cima dessas prateleiras.

## 5. Iluminação básica

1. Mantenha a Directional Light existente.
2. Adicione 2–3 luzes pontuais sem sombras:
   - `Light_Center`: Position `(0, 2.8, 0)`, Range 6, Intensity 1.5;
   - `Light_West`: Position `(-2.5, 2.8, 1.5)`, Range 5, Intensity 1.2;
   - `Light_East`: Position `(2.5, 2.8, 1.5)`, Range 5, Intensity 1.2.
3. Para o Quest, evite muitas luzes em tempo real e sombras pesadas. Nesta fase, o visual pode ser simples.

## 6. Criar o cubo de teste de grab

1. Crie um Cube `TestCube`.
2. Position `(0.3, 1.0, 2.5)`, Scale `(0.15, 0.15, 0.15)`.
3. Adicione:
   - `XRGrabInteractable`;
   - `Rigidbody` (Mass 1, Use Gravity ligado);
   - o BoxCollider do cubo.
4. No `XRGrabInteractable`, deixe `Interaction Manager` vazio; deixe `Colliders` vazio para ele encontrar o BoxCollider. Movement Type `Instantaneous` é suficiente para o teste.
5. Entre em Play e confirme que consegue pegar, soltar e arremessar.

## 7. Checklist da Fase 0

- [ ] `PrototypeLab` existe e está no build.
- [ ] Chão, paredes e 4 mesas têm collider.
- [ ] Estantes têm collider.
- [ ] O rig anda, vira e teleporta.
- [ ] O TestCube é pegável.
- [ ] Console sem erros.

Só avance para a Fase 1 depois desse checklist.

---

# FASE 1 — Núcleo, itens, tags e estantes

## 1. Criar as pastas de dados e assets

Em `Assets/FullironAlchemist`, crie:

```text
Data/
  Components/
  Processing/
Prefabs/
Materials/
```

## 2. Criar os componentes ScriptableObject

No Project, clique com o botão direito em `Data/Components` e escolha:

`Create > Fulliron > Alchemical Component`

### 2.1 Assets atuais gerados pelo construtor

O construtor cria automaticamente:

- **5 primários:** Enxofre, Mercúrio, Sal, Orvalho e Ferro.
- **15 resíduos:** 1 de Dissolutio + 2 de Calcinatio (chama fraca/forte) por primário.
- **12 processados demo de Enxofre:** Incompleto/Dissolvido/Superdissolvido; Calcinado/Supercalcinado/Cinza; 3 gases; 3 cristais.
- **`AlchemyDatabase.asset`** com todos os componentes.

Não existem mais os assets antigos `Ferrum`, `Sulfur`, `Aqua`, `TerraFerrosa`, etc.; eles são removidos pelo construtor. Para criar/alterar os dados manualmente, replique os campos do método `CreateData()` em `Assets/FullironAlchemist/Editor/PrototypeBuilder.cs`.

Cada primário tem:

- `Primary`: ligado.
- `Dissolution Residue`: 1 resíduo.
- `Calcination Residue Weak` / `Strong`: 2 resíduos.

### 2.2 Processados demo

Os processados de Enxofre existem para testar aparência/estado. O número e os nomes dos processados dos outros primários entram quando as duas cadeias de reações forem definidas.

### 2.4 Banco de dados (recomendado)

1. Em `Data`, crie `Create > Fulliron > Alchemy Database` e nomeie `AlchemyDatabase`.
2. Adicione todos os componentes criados à lista `Components`.
3. Deixe `Processing Profiles` vazio por enquanto; os perfis entram nas fases das estações.
4. No menu de contexto do componente (três pontinhos no Inspector), clique em **Validar banco** e confira o Console.

## 3. Criar os materiais

Em `Assets/FullironAlchemist/Materials`, crie materiais URP/Lit:

| Material | Uso | Cor |
|---|---|---|
| `M_Item_Base` | Visual dos itens | Branco |
| `M_Shelf` | Prateleiras | Madeira escura |
| `M_Table` | Mesas | Madeira/metal |
| `M_Button` | Botão de reposição | Latão/azul |
| `M_Tray` | Bandeja | Metal escuro |

Configuração:

- Shader: `Universal Render Pipeline/Lit`.
- `M_Item_Base` deve ter Base Map branco para o `ComponentVisuals` tingir com a cor do componente.
- Deixe Emission desligado por enquanto. Se quiser brilho, habilite Emission no material e preencha `Emission Color`/`Emission Intensity` no componente.

## 4. Criar o prefab `Item_Base`

Monte na cena, em uma posição livre, e depois arraste para `Assets/FullironAlchemist/Prefabs`.

### Hierarquia

```text
Item_Base
  AlchemicalItem
  ComponentVisuals
  XRGrabInteractable
  Rigidbody
  SphereCollider
  Visual (filho)
    MeshRenderer (Sphere, M_Item_Base)
  Label (filho)
    TextMeshPro (3D)
    ItemLabel
  GasVFX (filho, opcional)
```

### 4.1 Root `Item_Base`

Adicione:

- `AlchemicalItem`
  - `Initial Component`: vazio
  - `Initial State`: Raw
  - `Apply Initial On Awake`: desligado (a estante define o componente)
  - `History`: deixe como está

- `ComponentVisuals`
  - `Item`: arraste o próprio `AlchemicalItem`
  - `Visual Root`: arraste o filho `Visual`
  - `Renderers`: vazio (ele procura no Visual Root)
  - `Gas Visual`: opcional; arraste o `GasVFX` se criar
  - `Tint Renderers`: ligado
  - `Toggle Gas Visual`: ligado
  - `Gas Scale`: 1.25
  - `Solid Scale`: 1

- `XRGrabInteractable`
  - `Interaction Manager`: vazio
  - `Colliders`: vazio (encontra o SphereCollider do root)
  - `Attach Transform`: vazio
  - `Use Dynamic Attach`: ligado
  - `Match Attach Position`: ligado
  - `Match Attach Rotation`: ligado
  - `Snap to Collider Volume`: ligado
  - `Movement Type`: Instantaneous
  - `Throw On Detach`: ligado
  - `Throw Smoothing`: 0.25
  - `Select Mode`: Single
  - `Interaction Layer Mask`: Default (ou o mesmo do TestCube)

- `Rigidbody`
  - Mass: 0.2
  - Drag: 0.5
  - Angular Drag: 5
  - Use Gravity: ligado
  - Is Kinematic: desligado
  - Interpolate: Interpolate
  - Collision Detection: Continuous Speculative

- `SphereCollider`
  - Radius: 0.05
  - Is Trigger: desligado
  - Center: (0, 0, 0)

### 4.2 Filho `Visual`

1. Crie `GameObject > 3D Object > Sphere` e nomeie `Visual`.
2. Remova o `SphereCollider` que o Unity adiciona ao primitivo.
3. Scale: `(0.1, 0.1, 0.1)`.
4. Local Position: `(0, 0, 0)`.
5. MeshRenderer Material: `M_Item_Base`.

### 4.3 Filho `Label`

1. Crie `GameObject > 3D Object > Text - TextMeshPro` e nomeie `Label`.
2. Remova qualquer Collider, se houver.
3. Use o font asset `Inter-Regular SDF` (do template) ou o padrão TMP.
4. Configuração inicial:
   - Font Size: 2
   - Alignment: Center
   - Rect Transform Width: 2, Height: 1.5
   - Scale: `(0.1, 0.1, 0.1)`
   - Local Position: `(0, 0.12, 0)`
   - Rotation: `(0, 0, 0)`
   - Vertex Color: branco
5. Adicione `ItemLabel`:
   - `Item`: arraste o `AlchemicalItem` do root
   - `Label`: arraste o próprio TextMeshPro
   - `Grab Interactable`: vazio (ele encontra no pai automaticamente)
   - `Show On Hover`: ligado
   - `Show On Select`: ligado
   - `Hide When Socketed`: ligado
   - `Show Tags`: desligado
   - `Face Camera`: ligado
   - `Flip Facing`: desligado (ligue se o texto aparecer espelhado)
   - `Hide When Empty`: desligado

Deixe o texto do TMP vazio; o `ItemLabel` preenche no hover/seleção. O nome some quando o item entra em um socket.

Ajuste o tamanho da fonte/escala até o texto ficar legível a ~0,5 m.

### 4.4 Criar o prefab

1. Arraste o root `Item_Base` da Hierarchy para `Assets/FullironAlchemist/Prefabs`.
2. Apague a instância da cena, se não estiver usando para teste.
3. Confirme que o prefab abre e que os campos estão salvos.
4. Para um item colocado manualmente na cena, marque `Apply Initial On Awake` e arraste um `AlchemicalComponent` para `Initial Component`. A estante faz isso automaticamente para os itens que gera.

## 5. Criar o botão de reposição

Você pode criar um botão reutilizável e duplicar.

1. Crie um Empty `RestockButton`.
2. Adicione:
   - `XRSimpleInteractable`
   - `BoxCollider` (Size ~`0.1, 0.04, 0.1`; Is Trigger desligado)
   - `SimpleButton`
3. Crie um filho `Cap` (Cylinder ou Cube):
   - Scale ~`0.08, 0.02, 0.08`
   - Remova o Collider do filho
   - MeshRenderer com `M_Button`
4. No `SimpleButton`:
   - `Cap`: arraste o filho `Cap`
   - `Press Direction`: `(0, -1, 0)` para botão no topo; ajuste se for vertical
   - `Press Depth`: 0.02
   - `Press Duration`: 0.08
   - `Return Duration`: 0.15
   - `On Pressed`: deixe vazio se o `ShelfRow` vai assinar o botão automaticamente
5. Não coloque Rigidbody no botão. Ele é um interactable fixo.

## 6. Criar a estante `ShelfRow`

1. Crie um Empty `ShelfRow_Enxofre` posicionado sobre a prateleira oeste em `(-3.8, 1.25, 0.8)`.
2. Adicione `ShelfRow`.
3. Crie 3 filhos Empty para os slots. Use posições locais:
   - `Slot_01`: `(0, 0.03, -0.3)`
   - `Slot_02`: `(0, 0.03, 0)`
   - `Slot_03`: `(0, 0.03, 0.3)`
4. Posicione o `RestockButton` na frente/na borda da prateleira, ao alcance.
5. No `ShelfRow`:
   - `Component`: `Enxofre`
   - `Item Prefab`: `Item_Base`
   - `Slots`: arraste os 3 slots
   - `Occupied Radius`: 0.15
   - `Restock Button`: arraste o `SimpleButton`
   - `Restock Vfx`: opcional
   - `Restock On Start`: ligado
6. Duplique as fileiras:
   - Oeste: `ShelfRow_Enxofre` em z `0.8`, `ShelfRow_Mercurio` em z `1.45`, `ShelfRow_Sal` em z `2.1`.
   - Leste: `ShelfRow_Orvalho` em z `1.05`, `ShelfRow_Ferro` em z `1.9`.
   - Troque o `Component` de cada uma e use `frontSign` +1 no oeste e -1 no leste para o botão.

Observação: o `ShelfRow` assina o evento do botão automaticamente. Se você também arrastar `ShelfRow.Restock` no UnityEvent `On Pressed` do botão, o estoque será reposto duas vezes. Escolha um dos métodos.

## 7. Criar a bandeja `ComponentTray`

1. Na Mesa do Símbolo, crie um cubo raso `Tray`:
   - Position `(0, 0.94, 2.5)`
   - Scale `(0.6, 0.04, 0.4)`
   - Material `M_Tray`
   - Mantenha o BoxCollider para os itens não caírem.
2. Crie 6 slots filhos:
   - `TraySlot_01` ... `TraySlot_06`, distribuídos em 2 fileiras de 3.
   - Cada slot:
     - Adicione `XRSocketInteractor`.
     - Adicione um `BoxCollider` trigger com Size ~`0.08, 0.05, 0.08`.
     - Adicione `ItemSocket`.
     - `Element`: None.
     - `Log Changes`: ligado.
     - Crie um filho `Attach` no centro do slot e arraste para o `Attach Transform` do socket. Se preferir, deixe vazio; o socket usa o próprio transform.
     - No `XRSocketInteractor`, use `Interaction Layer Mask` Default e `Socket Active` ligado.
3. Adicione `ComponentTray` no root `Tray` e arraste os 6 `ItemSocket` para `Slots`.

## 8. Criar o `PrototypeLog`

1. Crie um Empty `PrototypeLog` na cena.
2. Adicione `PrototypeLog`.
3. Opcional: crie um TextMeshPro 3D em uma parede, legível, e arraste para `Display`. Defina `Max Lines` para 8–10.
4. Se não criar o display, os logs ainda aparecem no Console com o prefixo `[Fulliron]`.

## 9. Teste da Fase 1

Entre em Play Mode e faça:

- [ ] As fileiras repõem itens no início (log “repostos N item(ns)”).
- [ ] Você consegue pegar um item da estante.
- [ ] No hover/seleção, o label mostra nome e estado; ao entrar em um socket, o nome some.
- [ ] A cor do item corresponde ao componente.
- [ ] Você consegue colocar itens nos sockets da bandeja (log do socket).
- [ ] O botão de reposição se move e repõe apenas slots vazios.
- [ ] Selecionar um item na Hierarchy e usar `AlchemicalItem > Debug/Ciclar estado` muda a cor e o texto no próximo hover.
- [ ] O Console não tem erros.

## 10. Problemas comuns

### Itens não aparecem
- Confira `Component`, `Item Prefab`, `Slots` e `Restock On Start`.
- Veja o log da estante no Console.
- Confirme que o `Item_Base` prefab está na pasta `Prefabs` e tem `AlchemicalItem`.

### Itens caem da prateleira
- A prateleira precisa de collider.
- Coloque os slots ligeiramente acima da superfície e ajuste.
- O `Rigidbody` do item deve ter `Use Gravity` ligado e `Is Kinematic` desligado; se o item atravessa a prateleira, falta collider nela.

### Não consigo pegar o item
- O item precisa de `XRGrabInteractable`, `Rigidbody` e Collider.
- Confira o `Interaction Layer Mask` do item e dos interactors.
- Confirme que a cena tem o `XR Interaction Manager`.
- Compare com o `TestCube` da Fase 0 ou com `Cube Interactable` do template.

### O botão não pressiona
- O Collider deve estar no mesmo GameObject do `XRSimpleInteractable` e não ser trigger.
- O cap deve ser apenas visual.
- O botão precisa estar ao alcance do ray/direct interactor.
- O Console deve mostrar “Botão pressionado” se o evento disparar.

### O socket não encaixa
- O Collider do socket deve ser trigger.
- O item precisa de `XRGrabInteractable`.
- `Socket Active` deve estar ligado.
- Confira o `Attach Transform` e o `Interaction Layer Mask`.

### O texto está espelhado
- Ligue `Flip Facing` no `ItemLabel`.

### O texto está pequeno/grande
- Ajuste `Font Size`, a escala do Label e a largura/altura do Rect Transform.

### A cor não muda
- Use um material URP/Lit com Base Map branco em `M_Item_Base`.
- O `ComponentVisuals` define `_BaseColor` e `_Color`; shaders customizados podem ignorar.

## 11. O que você aprendeu nesta fase

- XR Origin, tracking space, teleporte, movimento contínuo e snap turn.
- ScriptableObjects para dados de componentes.
- Prefab genérico + dados definidos em runtime pela estante.
- `XRGrabInteractable`, `Rigidbody`, Collider e interaction layers.
- `XRSocketInteractor` como inventário físico.
- `MaterialPropertyBlock` para tingir itens sem criar materiais por instância.
- `UnityEvent` e assinatura de eventos em C#.
- Por que o laboratório é físico e não um menu.

## 12. Próximo passo

Quando o checklist passar, seguimos para a **Fase 2**: desenho livre do símbolo, sigilos, gesto de palmas e mesa de reação. Não vou commitar essas mudanças até você pedir.
