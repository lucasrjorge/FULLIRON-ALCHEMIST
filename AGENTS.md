# AGENTS.md

Guia de entrada para agentes que vão implementar o FULLIRON ALCHEMIST. Leia este arquivo antes de mexer no projeto e, depois, siga a ordem de leitura abaixo.

## Ordem de leitura

1. `README.md` — visão geral, stack e como abrir/testar.
2. `Docs/GAME_DESIGN.md` — conceito, tom, loop, estações, itens e Grimório.
3. `Docs/MECHANICS.md` — regras detalhadas de processos, tags, desenho, conjuração e reações.
4. `Docs/TECHNICAL_ARCHITECTURE.md` — modelo de dados, classes, fluxos e estrutura de pastas.
5. `Docs/PROTOTYPE_PLAN.md` — fases, escopo, simplificações e critérios de teste.
6. `Docs/DESIGN_DECISIONS.md` — decisões do Q&A, alternativas descartadas e questões em aberto.
7. `Docs/PHASE_0_1_GUIDE.md` — guia de montagem da Fase 0+1 (scripts, cena, prefabs e assets).

## Fatos do projeto

- Unity **6000.3.8f1**, URP 17.3.0, XR Interaction Toolkit 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1.
- Alvo principal: **Meta Quest standalone** (Android/OpenXR, ARM64, IL2CPP, SDK 34).
- Interação do protótipo: **controles VR**. Não implementar hand tracking agora.
- `Assets/Scenes/BasicScene.unity` já tem XR Origin (XR Rig) do Starter Assets, XR Interaction Manager, EventSystem e chão com TeleportationArea.
- `Assets/Scenes/SampleScene.unity` é o tutorial original do template. **Não alterar.**
- A implementação começou em `Assets/FullironAlchemist/` com os scripts da Fase 0+1 (núcleo, itens, estantes, bandeja e log). A cena, os prefabs e os assets de dados são montados pelo usuário no Unity seguindo `Docs/PHASE_0_1_GUIDE.md`.
- O projeto usa o novo Input System e não tem assembly definitions próprias nesta fase. O código novo deve compilar em `Assembly-CSharp`.
- O ambiente do agente pode não ter Unity nem `Library`; não assuma que consegue compilar ou rodar Play Mode.

## Regras de design não negociáveis

- Todo componente é um **objeto físico único** no laboratório. Não criar inventário abstrato, menu de itens ou hotbar.
- Ingredientes-base ficam em estantes com **botão de reposição por fileira**. Componentes processados são objetos físicos.
- Processar errado **transforma** o componente em outro componente permanente. Não existe “consertar” o componente errado.
- Processos funcionam em **tempo real**. Alguns são ativos; outros continuam sozinhos. Ignorar gera um componente errado. Remover o item ou desligar a fonte pausa/interrompe.
- Dissolutio: menos giro = incompleto; mais giro = superdissolvido. Ambos são componentes novos.
- Calcinatio: estágios visíveis (cru, calcinado, supercalcinado, cinza). Cada estágio é um componente.
- Sublimatio: 3 estados de gás lidos pela cor do vapor. O jogador escolhe usar o gás ou cristalizar; a cristalização é binária e o cristal depende do estado do gás.
- O jogador desenha **tudo à mão livre, em uma sequência**: símbolo principal + sigilos dos elementos. O símbolo principal tem alta tolerância; a posição dos sigilos é restrita; o símbolo deve conectar os sigilos de forma congruente com o template.
- O gesto de conjurar é **juntar as palmas/controles como em uma prece**. A validação do desenho e a reação acontecem nesse gesto.
- Se o desenho for inválido, tocar um efeito de “magia não se concentra”: som/visual, sem reação e sem consumir componentes.
- Reações validam **componente exato e/ou tags**. Cada receita referencia um template de símbolo.
- Falhas de reação usam um **perfil por receita**: consumo, bloqueio, tipo de limpeza, anomalia, efeito no jogador e duração.
- O Grimório registra descobertas e falhas. Páginas de receita já existem incompletas desde o início.
- Não há dano, morte ou respawn do jogador. Efeitos permitidos: vinheta/blur e cor/alucinação temporários.
- No protótipo, a única anomalia é **distorção da sala**: objetos flutuam, o jogador pode movê-los, o efeito some após um tempo e **não bloqueia nada**.
- Locomoção: teleporte e movimento contínuo, alternáveis; snap turn.

## Convenções

- Documentação em **português**.
- Identificadores de código em **inglês** (`AlchemicalItem`, `DissolutionStation`, `EffectTag`).
- Namespace sugerido: `Fulliron.Alchemy`.
- Usar APIs do XR Interaction Toolkit 3.5.1. O arquivo `Assets/VRTemplateAssets/Scripts/XRKnob.cs` é uma boa referência de padrão de interactable.
- Não adicionar comentários no código sem pedido explícito. Explicações e decisões ficam na documentação e no chat.
- Preferir referências explícitas no Inspector, componentes pequenos e responsabilidade única. Evitar singletons desnecessários, mas um log de protótipo ou um gerente de Grimório pode ser singleton se isso simplificar o sandbox.
- Usar TextMeshPro para textos e placeholders com primitivas, materiais URP e partículas simples.
- Não alterar `Assets/Samples`, `Assets/VRTemplateAssets` ou `Assets/Scenes/SampleScene.unity` sem pedido explícito.
- Não criar gerador automático de cena. O usuário quer montar cena, prefabs e dados no Editor para aprender; o agente entrega scripts e guias passo a passo.

## Estrutura de código

A estrutura completa está em `Docs/TECHNICAL_ARCHITECTURE.md`. Resumo:

```text
Assets/FullironAlchemist/
  Scripts/Core
  Scripts/Interaction
  Scripts/Stations
  Scripts/Symbols
  Scripts/Reactions
  Scripts/Grimoire
  Scripts/Debug
  Data/Components
  Data/Processing
  Data/Reactions
  Data/Symbols
  Prefabs
  Materials
  Scenes/PrototypeLab.unity
```

## Fluxo de trabalho

1. Implemente uma fatia pequena e coesa de uma fase.
2. Entregue o guia de montagem com hierarquia, componentes e campos do Inspector.
3. Peça ao usuário para abrir o Unity, compilar e testar.
4. Corrija erros de Console e ajuste o wiring antes de avançar.
5. Só faça commit quando o usuário pedir.

## Critérios de qualidade

- O código compila em `Assembly-CSharp` com os pacotes atuais.
- Referências ausentes geram mensagens claras no Console, sem falhas silenciosas.
- O protótipo é legível em VR: textos grandes, contrastados e a uma distância confortável.
- As estações continuam funcionando quando o jogador se afasta e volta.
- O jogo não depende de física complexa para as regras alquímicas; as regras são determinísticas e testáveis.
- Nenhuma regra de puzzle definitivo deve ser inventada sem o usuário como arquiteto. Os dados demo são placeholders claramente marcados.
