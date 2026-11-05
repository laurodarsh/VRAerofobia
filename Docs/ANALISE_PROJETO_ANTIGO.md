# Análise do projeto antigo (Aerofobia atual — 22-03-2024)

> Documento de referência. Registra o que existia no projeto antigo, o que funcionava,
> o que não funcionava e o que foi (ou não) migrado para este repositório.
> Análise feita em 23/09/2026.

## Resumo

| Item | Valor |
|---|---|
| Unity | 2022.3.7f1, Built-in Render Pipeline |
| Plataforma | Android (ARM64, OpenGL ES 3, minSdk 31) |
| VR | Google Cardboard XR Plugin (celular) — **não** Quest |
| Tamanho | `Assets/` 655 MB, `Library/` 4,3 GB |
| Código próprio | 6 scripts (~400 linhas) |
| Multiplayer | Photon PUN 2 (sem utilidade para o projeto) |

## Cenas

| Cena | No build | Conteúdo | Estado |
|---|---|---|---|
| `Lobby` | 1ª | Tela "Plane Simulator" → JOGAR → conecta no Photon → Sala 1/2/3 (todas abrem a SampleScene) | Só funciona com internet + conta Photon |
| `SampleScene` | 2ª | Aeroporto (`aeroporto.obj`, `aeroporto2.obj`, pista), ~170 prédios, 4 terrenos, skybox, 5 aviões parados, "aviao em uso" com câmera filha, pontos A–D, chuva, HUD com 4 botões | Cena principal (voo) |
| `Menu` | não | Escolha de clima (Ensolarado/Chuvoso/Tempestade) e horário (Dia/Noite) | Protótipo, só faz `Debug.Log` |
| `Sala` | não | Lista "Salas disponíveis" | Protótipo, não faz nada |

## Os 3 cenários planejados × o que existia

1. **Compra da passagem** — não existia.
2. **Ver um avião decolar** — não existia como cenário; havia aeroporto e aviões parados reaproveitáveis.
3. **Dentro do avião decolando e pousando** — parcial, na `SampleScene`, via `Voo.cs`:
   - *Decolar*: acelera até o Ponto A, sobe até o Ponto B (cruzeiro).
   - *Pousar*: teleporta o grupo "Aeroporto" para a frente do avião (simula o aeroporto de destino), vai até C e desacelera até D.
   - `AirPlane.cs` (versão com física + geração de nuvens) estava **desativado**.

## Problemas encontrados

### Não funcionava em VR
- Câmera sem `TrackedPoseDriver` → virar a cabeça não mexia a visão.
- HUD em *Screen Space – Overlay* → invisível no headset.
- `StandaloneInputModule` (mouse/toque) → sem interação por olhar/controle.
- Renderização estéreo *Multi-Pass* (lenta).

### Lógica
- Dependência do Photon/internet para sair do Lobby.
- `NetworkPlayerSpawner` fazia `FindGameObjectWithTag("Principal")` a cada frame (tag inexistente).
- Botões "Sair" e "Turbulência" sem função. `RainControl` não estava em nenhuma cena.
- `Voo.cs`:
  - velocidade cresce sem limite na decolagem;
  - `step += 0.2f` por frame → depende do FPS;
  - na fase "reduzir" a velocidade pode ficar negativa;
  - depende de `OnCollisionEnter` com objeto movido por `transform` (não confiável) — o correto é checar distância.
- Nenhum áudio na cena.

### Desempenho
- `aeroporto.obj` com **1.924 malhas separadas** (~1.924 draw calls só nele).
- Skybox EXR 4K de 73 MB; texturas 4K no avião; 4 terrenos.
- 3.923 materiais em `Assets/Materials` (metade duplicada por reimportações).

## Lixo identificado (não será migrado)

| Item | Tamanho | Motivo |
|---|---|---|
| `Plano de Fundo modelos/` (exceto `sunflowers_puresky_4k.exr` e `sky2.mat`) | ~320 MB | 12 HDRs + texturas de grama sem uso |
| `Avião/characters/` (Nathan, Carla, Eric) | ~67 MB | Não usados |
| `Photon/` + `NetworkManager.cs` + `NetworkPlayerSpawner.cs` + `Lobby` | 38 MB | Multiplayer desnecessário |
| `Assets/aeroporto/` | 7 MB | Cópia duplicada do `aeroporto.obj` |
| `Clouds/`, `Prefab/Cloud_Polygon_Blender_1.fbx` | 7 MB | Duplicados, sem uso |
| `Prefab/Cube.prefab`, `Cloud01.mat`, `Images/RoundedRectange.png`, `Resources/HUD.prefab`, `Resources/EventSystem.prefab` | — | Sem uso |
| `Samples/Google Cardboard...` e pacote Cardboard | — | Troca de plataforma para Quest |
| Visual Scripting, Collab, toolchain Linux, feature.development | — | Pacotes sem uso |

## O que vale reaproveitar

| Asset | Observação |
|---|---|
| Layout do aeroporto da `SampleScene` (posição de prédios, pista, terrenos) | Migrar via `.unitypackage`; otimizar malhas |
| `aviao/aviao.obj` (avião em uso) | Converter para FBX, reduzir materiais |
| `Avião/Avião.fbx` + `Avião/text/` (aviões estacionados, interior) | Reduzir texturas 4K → 1K/2K |
| `Predios/predio1..4` | Leves, ok |
| `Pìsta/AERODROME4.obj` | Renomear pasta (acento quebra ferramentas) |
| `sunflowers_puresky_4k.exr` | Reduzir para 2K |
| Lógica do `Voo.cs` (waypoints + teleporte do aeroporto) | **Reescrever** do zero com as correções acima |

## Status da migração

Feita em 2026-09-24 ([ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md)):

| Item | Resultado |
|---|---|
| `Avião/Avião.fbx` | ✅ Migrado como `Models/Aviao/Aviao.fbx`, escala corrigida (1,28×). É um turbo-hélice com interior. Das texturas, só a da fuselagem veio (sem o logotipo); as 4K da pasta nunca eram usadas |
| `aeroporto.obj` | ✅ Migrado; as 1.924 peças viraram 39 malhas (uma por material) no prefab `Terminal` |
| `Pìsta/AERODROME4.obj` | ✅ Migrado como `Models/Pista/`; os materiais eram só cores. A grama (textura perdida) virou cor verde |
| `Predios/predio1..4` | ✅ Migrados |
| `aeroporto2.obj`, `aviao/aviao.obj` | ❌ Não migrados (cópias do terminal e do avião) |
| `sunflowers_puresky_4k.exr`, nuvens | ⏳ A decidir quando montarmos o céu e as nuvens da cena 3 |
| Layout da `SampleScene` (posição dos prédios etc.) | ❌ Não migrado; o layout será remontado de forma mais simples nas cenas novas |
