# FULLIRON ALCHEMIST

Protótipo de jogo VR de alquimia para Meta Quest. O jogador interpreta um alquimista que descobre receitas por observação e experimentação: pega ingredientes físicos, passa por processos alquímicos em tempo real, desenha símbolos e sigilos, posiciona componentes e conjura reações. Erros não são apenas falhas: viram componentes novos, pistas e, às vezes, caos mágico que precisa ser cuidado.

> **Status atual:** documentação de conceito, mecânicas, arquitetura e plano do protótipo. Ainda não há scripts de gameplay próprios no repositório. Este README e a pasta `Docs/` são a fonte de verdade para os próximos agentes e para a implementação.

## Stack

- **Unity:** 6000.3.8f1
- **Render pipeline:** Universal Render Pipeline (URP) 17.3.0
- **XR:** XR Interaction Toolkit 3.5.1, OpenXR 1.17.1, Meta OpenXR 2.5.1, XR Hands 1.8.1
- **Input:** novo Input System
- **Alvo principal:** Meta Quest standalone (Android/OpenXR, ARM64, IL2CPP, SDK 34)
- **Interação do protótipo:** controles VR; hand tracking fica para depois

## Como abrir e testar

1. Abra o projeto no Unity Hub com a versão **6000.3.8f1**.
2. Aguarde a importação inicial dos pacotes. A pasta `Library` ainda não existe no clone; o primeiro carregamento pode demorar.
3. Abra `Assets/Scenes/BasicScene.unity` para testar o rig e a base do laboratório.
4. Teste no Editor com **XR Device Simulator** ou com o Quest conectado via Link.
5. Para build standalone, use o build target **Android** e o perfil **Meta Quest Touch Plus** já habilitado no OpenXR.

A cena `Assets/Scenes/SampleScene.unity` é o tutorial original do template VR. **Não altere essa cena nem os assets de `VRTemplateAssets`** sem necessidade explícita.

## Controles e conforto

- O jogador pode olhar em 360 graus e também virar com o controle (snap turn), inclusive sentado.
- Locomoção: teleporte e movimento contínuo, alternáveis; snap turn.
- O jogador precisa ver as mãos/controles e o que está segurando.
- Sessão sentada: tempo ilimitado. Em pé: cerca de 40 minutos de conforto.
- Efeitos no jogador são limitados a vinheta/blur e alteração de cor/alucinação. Não há dano, morte ou respawn.

## Documentação

Leia nesta ordem:

1. [`AGENTS.md`](AGENTS.md) — regras de trabalho para agentes, fatos do projeto e convenções.
2. [`Docs/GAME_DESIGN.md`](Docs/GAME_DESIGN.md) — conceito, tom, loop, estações, itens, símbolos, Grimório e erros.
3. [`Docs/MECHANICS.md`](Docs/MECHANICS.md) — regras detalhadas de processos, tags, desenho, conjuração e reações.
4. [`Docs/TECHNICAL_ARCHITECTURE.md`](Docs/TECHNICAL_ARCHITECTURE.md) — modelo de dados, classes, fluxos e estrutura de pastas.
5. [`Docs/PROTOTYPE_PLAN.md`](Docs/PROTOTYPE_PLAN.md) — fases, escopo, simplificações e critérios de teste.
6. [`Docs/DESIGN_DECISIONS.md`](Docs/DESIGN_DECISIONS.md) — decisões registradas do Q&A, alternativas descartadas e questões em aberto.

## Estrutura do repositório

```text
Assets/
  Scenes/
    BasicScene.unity          # cena base com XR Origin, manager e chão
    SampleScene.unity         # tutorial original do template; não alterar
  Samples/                    # amostras dos pacotes XR; não alterar sem pedido
  VRTemplateAssets/           # assets do template; reutilizar com cuidado
  XR/ XRI/                    # configurações de XR e XR Interaction Toolkit
Docs/                         # documentação de design, mecânicas e arquitetura
Packages/                     # manifesto de pacotes Unity
ProjectSettings/              # configurações do projeto
```

A implementação própria deve ficar em `Assets/FullironAlchemist/`, separada dos assets do template. A estrutura sugerida está em `Docs/TECHNICAL_ARCHITECTURE.md`.

## Escopo do protótipo

O primeiro protótipo é um **sandbox de estações**:

- Mesa do Símbolo com desenho livre, sigilos, âncoras, bandeja e gesto de conjurar.
- Dissolutio, Calcinatio/Terra e Sublimatio funcionando em tempo real.
- Duas estantes com botões de reposição por fileira.
- Grimório simplificado que registra descobertas e falhas.
- Um caso de anomalia por distorção da sala.
- Receitas demo: uma por componente exato e uma por tags.

Não entram nesta fase: reações auxiliares, outras anomalias, hand tracking, multiplayer, arte final e integração LAYA. O LAYA entra depois como modelo de decisão sobre a imagem renderizada do símbolo.

## Fluxo de trabalho

- O agente escreve scripts e um guia de montagem; o usuário monta cena, prefabs e assets no Unity e testa.
- Cada fase termina com um checklist de teste e um gate de aceite.
- O agente não deve assumir que consegue compilar ou rodar Unity no próprio ambiente; todo código entregue precisa vir com instruções claras de teste no Editor.
- Commits só devem ser feitos quando solicitados.
