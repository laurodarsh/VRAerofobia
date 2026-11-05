# Arquitetura

> Atualize este documento sempre que criar uma cena, um sistema/script principal
> ou mudar o fluxo entre cenas (regra do [CLAUDE.md](../CLAUDE.md)).

## Visão geral

> O **que** cada cena faz está na [ESPECIFICACAO.md](ESPECIFICACAO.md); este documento trata do **como**.

```
            ┌──────────────┐
            │   00_Menu    │  começar do início / entrar direto numa cena / sair
            └──────┬───────┘
                   │
   ┌───────────────┼────────────────┐
   ▼               ▼                ▼
┌──────────┐  ┌──────────────┐  ┌──────────┐
│01_Compra │─▶│02_Observacao │─▶│  03_Voo  │─▶ "Voo concluído" ─▶ volta ao menu
└──────────┘  └──────────────┘  └──────────┘
  paciente      médico chama      médico decola/pousa
  escolhe o     o embarque (ou    e controla o clima
  destino       após 3 min)
```

Todas as trocas de cena passam por um fade para preto.

Cada cena é **autossuficiente**: tem o próprio jogador (`XR_Player`), `EventSystem` e `SceneLoader`.
Não há objetos persistentes entre cenas (`DontDestroyOnLoad`), o que é mais simples e evita bugs de estado.
O único dado que passa de uma cena para outra (o destino escolhido) fica numa classe estática, `SessionData`
([ADR 0003](decisoes/0003-escopo-v1-e-interacao.md)).

## Cenas

| Cena | Arquivo | Estado | Conteúdo |
|---|---|---|---|
| Menu | `Assets/_Project/Scenes/00_Menu.unity` | Placeholder | Painel com os 3 cenários e o botão Sair |
| Compra | `Assets/_Project/Scenes/01_Compra.unity` | Placeholder | Painel "em construção" + navegação |
| Observação | `Assets/_Project/Scenes/02_Observacao.unity` | Placeholder | Painel "em construção" + navegação |
| Voo | `Assets/_Project/Scenes/03_Voo.unity` | **Greybox jogável** (gerada pelo `FlightSceneBuilder`) | Voo completo com clima, comandos do médico, pausa e fim de voo |

Estrutura padrão das cenas placeholder (00, 01 e 02):

```
Directional Light
Chao                    (Plane estático)
XR_Player               (prefab — jogador XR)
EventSystem             (EventSystem + XR UI Input Module)
SceneLoader             (troca de cenas)
Canvas_Painel           (World Space, 2 m à frente, Tracked Device Graphic Raycaster)
  └─ Painel             (título, descrição, botões)
```

### Cena `03_Voo` ([ADR 0006](decisoes/0006-cena-do-voo.md))

**Não edite à mão:** a cena inteira é recriada por *Aerofobia > Cenas > Montar Cena do Voo (03_Voo)*.
Para mudar algo de forma permanente, mude no `FlightSceneBuilder` ou no valor padrão do componente.

```
Sol                         (luz direcional, sem sombras)
Ambiente
  ├─ Pista, Terminal (escala 0,13), Chao (30 km, grama)
  └─ Cidade                 (prédios ao norte, dentro do circuito)
Circuito                    FlightCircuit: oval do voo, na cabeceira oeste (Gizmo ciano quando selecionado)
Luzes_Pista                 RunwayLights: malha gerada no Awake, acende à noite
Nuvens                      CloudField: ~220 nuvens numa malha só, fora do caminho do avião
Aviao_Voo                   FlightController (move a raiz), FlightAudio, AudioSource
  ├─ Corpo                  TurbulenceEffect (balança só este objeto)
  │    ├─ Aviao             modelo; as rodas tocam y = 0 da raiz
  │    └─ Poltronas         SeatSelector: AtrasDaAsa (padrão), SobAAsa, FrenteDaHelice
  │         └─ Poltrona_AtrasDaAsa
  │              ├─ XR_Player        (olhos 1,22 m acima do piso; câmera com far 12 km)
  │              ├─ Luz_Cabine       (ponto, sem sombras)
  │              ├─ Chuva_Janela     (partículas do lado de fora da janela + som)
  │              └─ Paineis          Painel_Pausa e Painel_VooConcluido (1,5 m, sempre visíveis)
  └─ Som_Cabine
Clima                       WeatherController, EnvironmentLighting, NightLights
EventSystem, SceneLoader
Pausa                       PauseMenu
Medico                      DoctorInput → FlightController / WeatherController / PauseMenu (eventos no Inspector)
```

Teclas para testar no Editor (sem headset):
- Espaço segurado 1 s: decolar/pousar;
- C: chuva;
- N: noite;
- T: turbulência;
- R: normal;
- P: pausa.

No Inspector, o `FlightController` também tem "Teste: ir direto para o cruzeiro" no menu de contexto (⋮).

## Prefabs principais

| Prefab | Caminho | Descrição |
|---|---|---|
| `XR_Player` | `Assets/_Project/Prefabs/Player/XR_Player.prefab` | **Variante** do `XR Origin (XR Rig)` dos Starter Assets do XRI. Tem câmera, controles e raio para UI. **Sentado:** rastreamento Device, olhos calibrados a 1,22 m do piso (`SeatedHeadCalibration`; botão A do controle direito recentraliza), sem locomoção nem gravidade (`PlayerRigSetup`, [ADR 0007](decisoes/0007-cabeca-na-poltrona.md)). Customize **a variante**, nunca o sample original |
| `Aviao` | `Assets/_Project/Prefabs/Aviao/Aviao.prefab` | Turbo-hélice com interior (68 mil triângulos, 94 draw calls), escala real. **Não é Static** (se move). Cenas 2 e 3 |
| `Terminal` | `Assets/_Project/Prefabs/Ambiente/Terminal.prefab` | Terminal com sala de espera; malhas combinadas por material (39 draw calls). Static. Cenas 2 e 3 |
| `Pista` | `Assets/_Project/Prefabs/Ambiente/Pista.prefab` | Pista, taxiways, pátio e grama (15 draw calls). Static |
| `Predio1..4` | `Assets/_Project/Prefabs/Ambiente/PredioN.prefab` | Prédios simples para a cidade em volta. Static |

> Os prefabs acima (exceto `XR_Player`) são **gerados** pelo `LegacyModels` a partir de `Assets/_Project/Models/`
> ([ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md)). Para customizar, crie um **Prefab Variant** ou edite os materiais
> em `Assets/_Project/Materials/<modelo>/`; o script nunca sobrescreve o que já existe.

> O rig dos Starter Assets vem com **locomoção contínua, teleporte, giro e gravidade ligados**. A variante
> `XR_Player` desliga tudo isso (*Aerofobia > Jogador > Configurar XR_Player (Sentado)*), porque a experiência é
> 100% sentada ([ADR 0003](decisoes/0003-escopo-v1-e-interacao.md), [ADR 0006](decisoes/0006-cena-do-voo.md)).

## Scripts

### Runtime: `Aerofobia.Core` (`Assets/_Project/Scripts/Core/`)

| Script | Responsabilidade |
|---|---|
| `SceneNames` | Constantes com os nomes das cenas e a ordem dos cenários |
| `SceneLoader` | Carrega cenas (async), avança para o próximo cenário, volta ao menu e fecha o app. Chamado pelos botões. Desfaz a pausa antes de trocar |
| `DoctorInput` | Lê o Input Action Asset `Settings/Input/ControlesMedico` (controle esquerdo + teclas de teste) e dispara um UnityEvent por comando. Com o jogo pausado, só repassa a pausa |
| `SeatedHeadCalibration` | No XR Origin do `XR_Player`: move o `Camera Offset` para os olhos ficarem a 1,22 m do piso da poltrona, olhando para a frente. Calibra ao iniciar, com o botão A (direito) / tecla H, ao recentralizar pelo sistema e ao voltar o foco. Altura ajustável ao vivo no Inspector durante o Play |

### Runtime: `Aerofobia.Flight` (`Assets/_Project/Scripts/Flight/`)

| Script | Responsabilidade |
|---|---|
| `FlightController` | Máquina de estados do voo (`FlightState`: Pronto → Decolagem → Subida → Cruzeiro → Aproximação → Pouso → Concluído). Move o avião pelo circuito de forma cinemática, com rampas suaves de velocidade, altitude, arfagem e inclinação |
| `FlightCircuit` | Geometria do oval (retas + meias-voltas) a partir da cabeceira da pista; posição, direção e curvatura por distância |
| `WeatherController` | Estado de chuva, noite e turbulência (combináveis) e transições graduais (8 s); repassa a quantidade (0–1) aos efeitos |
| `EnvironmentLighting` + `LightingPreset` | Sol, céu procedural (cópia do material), luz ambiente, reflexos e neblina, misturando 4 presets (dia, dia+chuva, noite, noite+chuva) |
| `RainEffect` | Gotas escorrendo do lado de fora da janela do paciente + som da chuva |
| `TurbulenceEffect` | Balanço suave (Perlin) do "Corpo" do avião, com teto no código (8 cm, 1,2°) |
| `NightLights` | Luz da cabine e cor das luzes da pista conforme a noite |
| `RunwayLights` | Gera no `Awake` as luzes de borda da pista numa malha só |
| `CloudField` | Gera no `Awake` a camada de nuvens low-poly numa malha só, sem nuvens no caminho do avião |
| `SeatSelector` | Escolhe a poltrona do paciente e leva o jogador, a chuva e os painéis para ela |
| `FlightAudio` | Sons por etapa do voo (chime, decolagem, ambiente, pouso); funciona sem clipes |

### Runtime: `Aerofobia.UI` (`Assets/_Project/Scripts/UI/`)

| Script | Responsabilidade |
|---|---|
| `PauseMenu` | Painel de pausa (Continuar / Voltar ao menu / Sair); congela tempo e áudio |
| `UiAlwaysOnTop` | Desenha um painel World Space por cima da geometria (ZTest Always), inclusive textos TextMeshPro |

### Editor: `Aerofobia.EditorTools` (`Assets/_Project/Scripts/Editor/`)

| Script | Menu | Responsabilidade |
|---|---|---|
| `ProjectSetup` | **Aerofobia > Configurar Projeto para Quest** | **Fonte da verdade das configurações**: Player (Android/IL2CPP/ARM64/Vulkan), XR (OpenXR + Meta Quest), URP para VR, validação OpenXR e cenas do build |
| `ProjectSetup` | Aerofobia > Importar Dependências | Importa os samples do XRI (Starter Assets, XR Interaction Simulator) e os recursos do TextMeshPro |
| `SceneBuilder` | Aerofobia > Criar Cenas que Faltam | Cria o `XR_Player` e as cenas placeholder **que não existirem** (nunca sobrescreve) |
| `BuildTools` | Aerofobia > Gerar APK para Quest | Gera `Builds/AerofobiaVR.apk` (development) com as cenas do build |
| `LegacyModels` | Aerofobia > Modelos > Preparar Modelos do Projeto Antigo | Configura a importação dos modelos migrados, extrai os materiais (URP), cria os prefabs e combina o terminal por material. Só cria o que não existe |
| `PlayerRigSetup` | Aerofobia > Jogador > Configurar XR_Player (Sentado) | Deixa o `XR_Player` sentado: rastreamento Device, calibração da cabeça na poltrona, sem locomoção, teleporte, giro, pulo e gravidade. Idempotente |
| `FlightSceneBuilder` | Aerofobia > Cenas > Montar Cena do Voo (03_Voo) | **Recria** a cena do voo inteira (pede confirmação). Cria os materiais de `Materials/Voo` que faltarem |

## Assemblies

| Assembly | Pasta | Depende de |
|---|---|---|
| `Aerofobia.Core` | `Scripts/Core` | Input System |
| `Aerofobia.Flight` | `Scripts/Flight` | Core |
| `Aerofobia.UI` | `Scripts/UI` | Core, UGUI, TextMeshPro |
| `Aerofobia.EditorTools` | `Scripts/Editor` | Core, Flight, UI, XR Management, OpenXR, XRI, URP, TMP, UI, Input System (só no Editor) |

## Sistemas planejados (v1)

Ainda **não existem**. A lista serve para dividir o trabalho e manter os nomes coerentes.
Ao criar cada um, mova a linha para a seção "Scripts" acima. Os nomes podem mudar na implementação.

| Sistema | Assembly | Cena | Responsabilidade |
|---|---|---|---|
| `SessionData` | Core | todas | Classe estática com o destino escolhido (padrão: Paris) |
| `ScreenFader` | Core | todas | Fade para preto nas trocas de cena, usado pelo `SceneLoader` |
| `PauseMenu` nas cenas 0–2 | UI | 0, 1, 2 | Já existe (cena 3); falta pôr nas outras cenas |
| `DestinationList` | Core | 1, 2, 3 | ScriptableObject com os destinos (nome, imagem, código do voo) |
| `TicketPurchaseFlow` | UI | 1 | As 4 telas da compra no monitor |
| `DepartureBoard` | UI | 2 | Painel de voos com o destino e o status (Embarque em breve / Embarcando) |
| `BoardingCall` | Core | 2 | Chama o embarque após o tempo configurado ou pelo médico e troca para a cena 3 |
| `DepartureLoop` | Flight | 2 | Aviões decolando em ciclo, reaproveitados (pool) |

Regras de dependência: `DoctorInput` só emite eventos; quem reage são `BoardingCall`, `FlightController`,
`WeatherController` e `PauseMenu`. Assim os efeitos podem ser testados no Editor sem controle
(teclas de teste e menu de contexto dos componentes no Inspector).

## Configuração de XR

| Item | Valor |
|---|---|
| Loader | OpenXR, no Android (Quest) e no Windows (Quest Link, para testar no Play Mode) |
| Features (Android) | Meta Quest Support |
| Perfis de interação | Oculus Touch (Quest 2) e Meta Quest Touch Plus (Quest 3) |
| Render mode | Single Pass Instanced (multiview) |
| Assets gerados | `Assets/XR/` (não editar à mão) |

## Renderização (URP)

| Plataforma | Asset | Observação |
|---|---|---|
| Android / Quest | `Assets/_Project/Settings/Mobile_RPAsset.asset` | MSAA 4x, sem HDR, render scale 1, sombras até 30 m |
| PC / Editor | `Assets/_Project/Settings/PC_RPAsset.asset` | Padrão do template (usado no Play Mode com Link). SSAO **desativado** no `PC_Renderer` |

> A validação do OpenXR mostra um aviso permanente sobre SSAO. O próprio pacote informa que
> **desativar** já elimina o custo; o aviso só some se o recurso for removido. É esperado.

## Scripting defines

Aplicados pela validação do OpenXR (Android, Standalone):
`USE_INPUT_SYSTEM_POSE_CONTROL` e `USE_STICK_CONTROL_THUMBSTICKS` (APIs de input recomendadas pelo OpenXR 1.16).

## Pacotes

| Pacote | Versão | Uso |
|---|---|---|
| `com.unity.render-pipelines.universal` | 17.3.0 | URP |
| `com.unity.xr.openxr` | 1.16.1 | Runtime XR + Meta Quest Support |
| `com.unity.xr.management` | 4.6.1 | XR Plug-in Management |
| `com.unity.xr.interaction.toolkit` | 3.3.2 | Interação, rig do jogador, UI em VR |
| `com.unity.inputsystem` | 1.20.0 | Input |
| `com.unity.ugui` | 2.0.0 | UI + TextMeshPro |
| `com.unity.timeline` | 1.8.13 | Sequências (decolagem, pouso) |
| `com.unity.test-framework` | 1.6.0 | Testes |
| `com.unity.ide.visualstudio` / `ide.rider` | — | Integração com VS Code/Visual Studio/Rider |

Samples do XRI importados em `Assets/Samples/` (não editar): **Starter Assets** e **XR Interaction Simulator**
(o simulador permite testar no Editor sem headset).
