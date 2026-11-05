# Changelog

Todas as mudanças relevantes do projeto ficam registradas aqui.
Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).

Regra: toda mudança entra em **[Não lançado]** com `- Descrição (autor, AAAA-MM-DD)`.
Ao gerar uma versão para o laboratório, mova os itens para uma seção com o número da versão.

## [Não lançado]

### Adicionado
- Estrutura inicial do repositório: pastas, `.gitignore`, `.gitattributes` com Git LFS e merge de cenas, `.editorconfig` (Daniel/Claude, 2026-09-23)
- `CLAUDE.md` com regras do projeto, boas práticas de código, regras de VR e de documentação (Daniel/Claude, 2026-09-23)
- Documentação: README, SETUP, CONTRIBUINDO, ARQUITETURA, ROADMAP, ASSETS, análise do projeto antigo (Daniel/Claude, 2026-09-23)
- ADRs 0001 (projeto novo) e 0002 (stack Unity 6.3 LTS + URP + OpenXR + XRI) (Daniel/Claude, 2026-09-23)
- Projeto Unity 6000.3.24f1 criado a partir do template Universal 3D (URP); conteúdo de exemplo removido e assets de configuração movidos para `_Project/Settings` (Daniel/Claude, 2026-09-23)
- Pacotes XR: XR Interaction Toolkit 3.3.2, OpenXR 1.16.1, XR Management 4.6.1; removidos Visual Scripting, Collab, Multiplayer Center e AI Navigation (Daniel/Claude, 2026-09-23)
- `ProjectSetup` (menu Aerofobia): configuração reproduzível para Quest — Android/IL2CPP/ARM64/Vulkan/Linear, OpenXR + Meta Quest Support, perfis Touch/Touch Plus, Single Pass Instanced, URP para VR, validação OpenXR automática (Daniel/Claude, 2026-09-23)
- `SceneBuilder`: prefab `XR_Player` (variante do rig do XRI) e cenas placeholder 00_Menu, 01_Compra, 02_Observacao, 03_Voo com UI World Space e navegação (Daniel/Claude, 2026-09-23)
- Scripts `SceneNames` e `SceneLoader` (troca de cenas) (Daniel/Claude, 2026-09-23)
- `BuildTools`: menu para gerar APK de desenvolvimento para Quest (Daniel/Claude, 2026-09-23)
- README: comandos para ativar caminhos longos no Windows e checklist de configuração pendente (Daniel/Claude, 2026-09-23)
- CLAUDE.md: regra de `git pull` no início de toda tarefa e `git push` no fim, sempre com permissão (Daniel/Claude, 2026-09-23)
- Primeiro teste no Quest 2 via APK: menu, raio dos controles e navegação entre cenas funcionando (Daniel, 2026-09-23)
- `Docs/ESPECIFICACAO.md`: especificação da v1 (fluxo, controles do médico, clima, sequência do voo, sons com licença, modelos) (Daniel/Claude, 2026-09-23)
- ADR 0003 (escopo da v1: experiência sentada, controles do médico, clima combinável, voo em circuito) e ADR 0004 (greybox primeiro, depois modelos leves e sons CC0) (Daniel/Claude, 2026-09-23)
- ARQUITETURA: fluxo entre cenas atualizado e lista de sistemas planejados para a v1 (Daniel/Claude, 2026-09-23)
- ROADMAP: Fases 1–3 detalhadas em tarefas a partir da especificação (Daniel/Claude, 2026-09-23)

- Modelos do projeto antigo em `Assets/_Project/Models/`: avião turbo-hélice com interior, terminal, pista e 4 prédios (Daniel/Claude, 2026-09-24)
- `LegacyModels` (menu Aerofobia > Modelos): configura a importação, extrai materiais URP, cria os prefabs `Aviao`, `Terminal`, `Pista` e `Predio1..4` e combina o terminal por material (2.168 → 39 draw calls) (Daniel/Claude, 2026-09-24)
- ADR 0005 (modelos do projeto antigo, avião turbo-hélice, remoção de marcas) (Daniel/Claude, 2026-09-24)
- `Docs/PENDENCIAS.md`: decisões e tarefas em aberto para a equipe responder (Daniel/Claude, 2026-09-24)
- `.gitattributes`: malhas geradas `*_Combinado.asset` vão pelo Git LFS (Daniel/Claude, 2026-09-24)

- Cena 3 (`03_Voo`) jogável no Editor: decolagem, subida acima das nuvens, cruzeiro em oval em volta do aeroporto, aproximação e pouso na mesma pista, painel "Voo concluído" (Daniel/Claude, 2026-09-26)
- Assembly `Aerofobia.Flight`: `FlightController` (máquina de estados do voo), `FlightCircuit`, `WeatherController`, `EnvironmentLighting` + `LightingPreset`, `RainEffect`, `TurbulenceEffect`, `NightLights`, `RunwayLights`, `CloudField`, `SeatSelector`, `FlightAudio` (Daniel/Claude, 2026-09-26)
- Comandos do médico: Input Action Asset `Settings/Input/ControlesMedico` (controle esquerdo + teclas para testar no Editor) e `DoctorInput` (Daniel/Claude, 2026-09-26)
- Assembly `Aerofobia.UI`: `PauseMenu` (Continuar / Voltar ao menu / Sair no botão Menu) e `UiAlwaysOnTop` (painel desenhado por cima da cabine) (Daniel/Claude, 2026-09-26)
- `FlightSceneBuilder` (menu Aerofobia > Cenas > Montar Cena do Voo) e `PlayerRigSetup` (menu Aerofobia > Jogador) (Daniel/Claude, 2026-09-26)
- Materiais gerados em `Materials/Voo`: céu procedural, nuvem, luz da pista, gota de chuva (Daniel/Claude, 2026-09-26)
- ADR 0006 (cena do voo: voo cinemático em circuito, jogador sentado, painéis sempre visíveis) (Daniel/Claude, 2026-09-26)

### Alterado
- Fluxo Git para um só desenvolvedor: commits direto na `main`, sem branch de tarefa nem Pull Request; saem as regras de equipe (proteger a `main`, convidar colaboradores, avisar antes de editar cena). CLAUDE.md, CONTRIBUINDO, README, PENDENCIAS e ROADMAP atualizados; modelo de PR e branches antigas apagados. ADR 0008 (Daniel/Claude, 2026-10-26)
- `XR_Player` para experiência sentada: rastreamento Device (olhos a 1,15 m), locomoção, teleporte, giro, pulo e gravidade desligados (Daniel/Claude, 2026-09-26)
- `SceneLoader` desfaz a pausa (tempo e áudio) antes de trocar de cena (Daniel/Claude, 2026-09-26)
- ESPECIFICACAO: tempo da aproximação da cena 3 atualizado conforme a ADR 0006 (Daniel/Claude, 2026-09-26)
- ASSETS e PENDENCIAS: origem provável do avião (Sketchfab, TonyWony, CC BY 4.0) e da pista (Sketchfab, ririnhasra88, CC BY 4.0) (Daniel/Claude, 2026-09-26)
- ESPECIFICACAO: sons de motor trocados por gravações de turbo-hélice (Dash 8), CC0; seção de modelos atualizada com os prefabs importados (Daniel/Claude, 2026-09-24)
- Avião: logotipo de companhia aérea apagado da textura da fuselagem; cauda, leme e painel da cabine com cor sólida (texturas originais tinham marcas registradas) (Daniel/Claude, 2026-09-24)
- README e CLAUDE.md: descrição dos cenários conforme a especificação; regra de experiência sentada sem locomoção; exceção de distância da UI no monitor da cena de compra (Daniel/Claude, 2026-09-23)

### Corrigido
- `XR_Player`: paciente flutuando acima da poltrona (tinha de abaixar a cabeça para ver a janela). `SeatedHeadCalibration` põe os olhos na altura configurada acima do piso da poltrona, olhando para a frente, ao iniciar, ao recentralizar e com o botão A do controle direito (tecla H no Editor); altura ajustável ao vivo no Inspector durante o Play. Olhos passam de 1,15 m para 1,22 m, valor ajustado no Quest 2 olhando pela janela. ADR 0007 (Daniel/Claude, 2026-09-26)
- Cena 3: comandos do médico não funcionavam (teclado e controle). O `FlightSceneBuilder` salvava a cena sem o Input Action Asset (`DoctorInput`: "Input Action Asset não definido"); agora carrega o asset na hora de ligar e confere as referências depois de salvar. Testado em Play Mode com teclado e controle virtuais (Daniel/Claude, 2026-09-27)
- SETUP: URL real do repositório (SSH) e passo a passo da chave SSH para os colegas (Daniel/Claude, 2026-09-23)
- Erro "Scene file not found: Assets/Scenes/SampleScene.unity" ao abrir o projeto pela 1ª vez: `ProjectSetup` passa a definir a cena inicial como `00_Menu`, já aplicado ao `ProjectSettings` (Daniel/Claude, 2026-09-23)
