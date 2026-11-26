# DESIGN DECISIONS — FULLIRON ALCHEMIST

## 1. Contexto

Este documento registra as decisões tomadas em uma rodada de alinhamento de conceito entre o arquiteto do jogo (usuário) e o agente. O objetivo era resolver, antes da implementação, os pontos em que o design ainda estava aberto — principalmente a consequência de um componente passar pelo processo errado ou ficar tempo/calor demais.

As decisões abaixo são a base para os outros documentos. Quando houver conflito, este documento e `Docs/GAME_DESIGN.md` têm prioridade sobre suposições de implementação.

## 2. Decisões

### 2.1 Plataforma, interação e montagem

| Tema | Decisão |
|---|---|
| Plataforma alvo | Meta Quest standalone (Android/OpenXR). |
| Interação | Controles VR; sem hand tracking no protótipo. |
| Montagem da cena | O usuário monta cena, prefabs e dados no Unity para aprender; o agente entrega scripts e guias. Sem gerador de cena por enquanto. |
| Escopo do primeiro protótipo | Sandbox das estações, não uma campanha completa. |

**Por quê:** o Quest é o alvo do projeto, controles são mais confiáveis para o primeiro protótipo e a montagem manual preserva o aprendizado do usuário.

### 2.2 Erro de processo

**Decisão:** processar um componente da forma errada **transforma** o item em outro componente permanente. Não existe conserto ou reversão.

**Consequências registradas:**

- Componente de processo errado continua sendo um objeto físico válido.
- Pode ser útil em outras receitas.
- Não é lixo automático.
- O jogador precisa pegar outro ingrediente-base para tentar de novo.

**Alternativas descartadas:**

- Destruir o item e deixar só resíduo.
- Reduzir qualidade e permitir refinar.
- Bloquear o processo errado para o jogador não errar.

### 2.3 Excesso de tempo/calor

**Decisão:** depende da estação.

- **Dissolutio:** menos giro = incompleto; mais giro = superdissolvido. Ambos são componentes novos.
- **Calcinatio:** estágios visíveis (cru -> calcinado -> supercalcinado -> cinza). Cada estágio é um componente.
- **Sublimatio:** o tempo de aquecimento produz 3 estados de gás (lidos pela cor do vapor). O jogador pode usar o gás ou cristalizar; a cristalização é binária e o cristal depende do estado do gás.

**Por quê:** cada processo tem uma fantasia própria; uma regra única empobreceria as estações.

### 2.4 Ordem errada dos processos

**Decisão:** a ordem errada gera um componente novo e permanente. Se esse componente for usado na reação que esperava o componente formado pela ordem “certa”, a reação é adversa e dispara um evento que exige cuidado com o laboratório.

**Consequências:**

- O componente de ordem errada pode ser útil em outra receita.
- A reação adversa é um evento de falha, não apenas uma mensagem.
- O laboratório pode precisar de limpeza/contenção.

### 2.5 Falha de processo vs falha de reação

**Decisão:** são sistemas diferentes.

- Falha de processo: gera componente/resíduo; o erro é descoberto ao usar o componente.
- Falha de reação: aplica um perfil de falha na mesa, com consumo, bloqueio, limpeza, anomalia e efeito no jogador.

### 2.6 Consumo e bloqueio em reação adversa

**Decisão:** o **registro é geral para todas as receitas**, mas **consumo, bloqueio e forma de bloqueio são específicos por receita**.

- Cada `ReactionRecipe` tem um `FailureProfile`.
- O Grimório registra a falha.
- O bloqueio pode ser da mesa, de uma área ou do laboratório; no protótipo, a distorção não bloqueia nada.
- A forma de limpeza depende do tipo de falha (bagunça, fogo, sigilo, distorção).

### 2.7 Consequências e anomalias

**Decisão:** as consequências variam por erro/receita:

- bagunça física;
- perigo ocasional (fogo/fumaça);
- anomalia mágica.

**Protótipo:** apenas **distorção da sala**.

- Objetos começam a flutuar.
- O jogador pode pegá-los e colocá-los onde quiser.
- O efeito some após um tempo.
- A distorção **não bloqueia nada**.

**Futuro:** criatura simples e sigilo instável.

### 2.8 Recuperação

**Decisão:** o componente errado **nunca é recuperado/consertado**. Ele pode ser usado como novo componente, mas não volta ao estado anterior.

### 2.9 Grimório e erros

**Decisão:** o Grimório registra erros e resultados inesperados como descobertas/notas.

- Todas as páginas de receita existem desde o início, incompletas.
- O jogador descobre processos, tempos, símbolo e sigilos.
- Sucesso completa a página.
- Falha também é registrada.
- Componentes feios podem ganhar páginas próprias.

### 2.10 Recursos e inventário

**Decisão:** base + crafting, com objetos físicos únicos.

- Ingredientes-base ficam em fileiras de estante.
- Cada fileira tem um botão que repõe/materializa o estoque daquela fileira.
- Componentes processados são objetos físicos.
- A bandeja da Mesa do Símbolo é um inventário físico para carregar vários componentes.
- Não existe menu de itens, hotbar ou inventário abstrato.

### 2.11 Tags e estados

**Decisão:** cada componente tem tags que podem ser atribuídas/alteradas conforme o estado e o processo.

- O processo pode distribuir, ativar ou alterar tags.
- Um componente que passa pouco tempo em Calcinatio vira outro componente, com aparência própria e tags específicas daquele estágio.
- Receitas podem validar **componente exato e/ou tags**.
- O protótipo deve testar os dois modos.

### 2.12 Símbolo e desenho

**Decisão:** o jogador desenha **tudo à mão livre, em uma sequência**:

- símbolo principal;
- sigilos dos elementos;
- o sistema separa por traços, forma e posição.

Regras:

- Sigilos são os símbolos alquímicos clássicos de Aqua, Ignis, Terra e Aer.
- O símbolo principal tem alta tolerância e pode ser expresso com estilo próprio.
- A posição dos sigilos é restrita às âncoras do template.
- O símbolo principal deve conectar os sigilos de forma congruente com o template.
- O jogador pode criar variações intencionais aceitas.
- O protótipo usa validador geométrico com famílias de templates; o LAYA entra depois.

**Alternativas descartadas:**

- Sockets fixos nos pontos cardeais sem o símbolo definir posições.
- Desenho só do símbolo principal, com sigilos por carimbo.
- Símbolo escolhido por menu/paleta.

### 2.13 Validação do desenho e conjuração

**Decisão:** a validação acontece no **gesto de conjurar**.

- O jogador desenha, posiciona os componentes e junta as palmas/controles como em uma prece.
- A junção ativa uma animação contextual das palmas.
- No gesto, o jogo valida o desenho e a reação.
- Se o desenho for inválido: efeito sonoro/visual de “magia não se concentra”, a reação não acontece e os componentes ficam na mesa.
- Não há consumo de componentes por desenho inválido.

**Alternativas descartadas:**

- Validação automática enquanto desenha.
- Botão de confirmar separado.
- Gesto de tocar no centro do símbolo.

### 2.14 LAYA

**Decisão:** o LAYA entra depois, validando a **imagem renderizada** do desenho como modelo de decisão.

- O protótipo usa `ISymbolValidator` com implementação geométrica.
- O `SymbolCapture` já deve produzir uma textura renderizada para o LAYA futuro.
- O LAYA não é tratado como um reconhecedor de desenho dedicado; é um modelo de decisão (Choice/Score/Noul) aplicado à imagem/estado.

### 2.15 Efeitos no jogador

**Decisão:** o jogador pode sofrer efeitos temporários, principalmente visuais.

- Permitidos: vinheta/blur e cor/alucinação.
- Não usar inversão de controles ou efeitos fortemente desorientadores.
- Sem dano, morte ou respawn.
- Na maior parte do tempo, só o laboratório é afetado.

### 2.16 Locomoção e conforto

**Decisão:** teleporte e movimento contínuo, alternáveis; snap turn.

- O jogador pode andar livremente pelo laboratório.
- Pode olhar em 360 graus.
- Pode jogar sentado; nesse caso, o tempo é ilimitado.
- Em pé, conforto estimado em ~40 minutos.
- O jogador precisa ver as mãos/controles e o que está segurando.

### 2.17 Tom

**Decisão:** base científica de descoberta, observação e experimentação, com momentos de caos e consequências mágicas exageradas.

- Erros ajudam a chegar em outras respostas ou perto da resposta.
- Não é um jogo sombrio nem puramente cômico.

### 2.18 Reações auxiliares

**Decisão:** não entram no primeiro protótipo. A arquitetura deve permitir adicioná-las depois.

## 3. Questões em aberto

Estas questões não bloqueiam o protótipo, mas precisam de decisão do arquiteto antes do conteúdo final:

1. **Símbolo por receita:** cada receita terá um símbolo exclusivo, ou várias receitas poderão compartilhar o mesmo template? O modelo de dados suporta os dois.
2. **Conjunto de tags:** quais tags existem no jogo final e como elas se combinam? O protótipo começa com um conjunto pequeno.
3. **Âncoras móveis:** no protótipo, as 4 âncoras são fixas nos pontos cardeais. No jogo final, cada template pode ter âncoras em posições diferentes? Como o jogador posiciona os componentes físicos nessas âncoras?
4. **Templates de símbolo principal:** quais formas existem e quantas variantes cada uma aceita?
5. **LAYA na prática:** qual o pipeline de renderização, frequência de chamada e tratamento de latência no Quest? O LAYA roda local, servidor ou nuvem?
6. **Conteúdo dos puzzles:** quais são as reações, componentes, tags e efeitos reais? Os dados demo são apenas placeholders.
7. **Bandeja:** ela fica fixa na mesa ou pode ser carregada com as duas mãos? Qual a capacidade final?
8. **Armazenamento de processados:** existe uma estante/área de produtos, ou o jogador deixa os componentes fisicamente onde quiser?
9. **Anomalias futuras:** como criatura e sigilo instável funcionam e como são limpos?
10. **Arte e áudio:** direção visual, modelos, animações de mãos e sons definitivos.
11. **Efeitos nos controles:** o arquiteto mencionou que os controles podem ser afetados dependendo da reação/erro, mas na seleção de efeitos aceitáveis escolheu apenas vinheta/blur e cor/alucinação. Confirmar se controles entram, em que intensidade e com quais salvaguardas de conforto.
12. **Variações de símbolo no protótipo:** o validador geométrico aceita variações dentro das famílias de templates. Confirmar se o jogador pode cadastrar/ensinar novos símbolos antes do LAYA.

## 4. Alternativas descartadas e por quê

| Alternativa | Motivo do descarte |
|---|---|
| Inventário abstrato/menu | Quebra o pilar de objetos físicos e o aprendizado espacial. |
| Hand tracking no protótipo | Mais difícil de testar e de acertar no primeiro sandbox. |
| Gerador automático de cena | O usuário quer montar e aprender no Editor. |
| Processo errado destrói o item | Perde a descoberta e a “escada” de componentes. |
| Uma regra única de overprocessing | As estações têm fantasias e leituras diferentes. |
| Validação do símbolo por menu | Perde a mecânica de desenho e a expressão do jogador. |
| Bloqueio da distorção | O arquiteto definiu que a distorção não bloqueia. |
| LAYA como reconhecedor dedicado | LAYA/Jev são modelos de decisão; a validação por imagem é posterior. |
| Reações auxiliares no protótipo | Aumentaria o escopo antes de validar as bases. |

## 5. Consequências para os próximos agentes

- Não implementar uma regra de “processo errado = lixo” nem “processo errado = consertável”.
- Não criar inventário abstrato.
- Não validar o símbolo por botão/menu; o gesto de palmas é a entrada.
- Não bloquear a distorção.
- Não adicionar hand tracking, reações auxiliares ou LAYA no protótipo sem pedido.
- Manter os dados demo claramente separados do conteúdo definitivo.
- Perguntar ao arquiteto antes de inventar puzzles finais.
