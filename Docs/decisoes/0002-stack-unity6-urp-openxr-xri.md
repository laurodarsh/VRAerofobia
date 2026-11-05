# 0002 — Stack: Unity 6.3 LTS + URP + OpenXR + XR Interaction Toolkit

- **Data:** 2026-09-23
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Aceita

## Contexto

Os headsets são o Meta Quest 2 (desenvolvimento em casa) e o Meta Quest 3 (laboratório).
O projeto é compartilhado por uma equipe de alunos, então precisa de uma stack padrão,
bem documentada e com suporte de longo prazo.

## Decisão

| Área | Escolha |
|---|---|
| Editor | **Unity 6000.3.24f1** (Unity 6.3 LTS) |
| Renderização | **URP**, com configuração para mobile/VR |
| Runtime XR | **OpenXR** (`com.unity.xr.openxr`) com o feature group **Meta Quest Support** |
| Interação | **XR Interaction Toolkit 3** (`com.unity.xr.interaction.toolkit`) + Starter Assets |
| Input | **Input System** (o Input Manager antigo fica desligado) |
| Build Android | IL2CPP, ARM64, Vulkan, Linear color space, Single Pass Instanced (multiview) |
| Perfis de controle | Oculus Touch (Quest 2) e Meta Quest Touch Plus (Quest 3) |
| Identificador Android | `com.unitylab.aerofobiavr`, nome "Aerofobia VR", empresa "UnityLab" |
| Android mínimo | API 32 (Android 12L, base do sistema do Quest) |
| Latência | OpenXR "Prioritize Input Polling" (recomendado para Quest) |
| Samples XRI | Starter Assets (rig `XR Origin (XR Rig)`) e XR Interaction Simulator |

## Alternativas consideradas

- **Meta XR SDK (Meta XR All-in-One / Interaction SDK):** tem mais recursos exclusivos da Meta
  (passthrough avançado, hand tracking refinado), mas prende o projeto à Meta, tem API maior e muda com frequência.
  Não precisamos desses recursos agora. Se precisarmos, o pacote `com.unity.xr.meta-openxr`
  adiciona recursos Meta por cima do OpenXR sem trocar a stack.
- **Built-in Render Pipeline:** está em fim de vida no Unity 6, e o URP tem melhor desempenho no Quest.
- **Unity 2022.3 LTS:** já está no fim do suporte.

## Consequências

- Os mesmos builds rodam no Quest 2 e no Quest 3. No Quest 3 dá para subir a qualidade (render scale e MSAA).
- No PC, o Play Mode funciona com o headset via **Quest Link / Air Link** (OpenXR no Standalone).
- Materiais importados do projeto antigo precisam ser convertidos para URP.
- A configuração é aplicada por script (`Aerofobia > Configurar Projeto para Quest`), então fica reproduzível e revisável.
