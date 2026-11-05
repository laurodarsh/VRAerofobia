# Roadmap

> Marque as tarefas com `[x]` quando concluídas e registre no [CHANGELOG](CHANGELOG.md).

## Fase 0 — Fundação do projeto
- [x] Estrutura de pastas, `.gitignore`, `.gitattributes` (LFS), `.editorconfig`
- [x] `CLAUDE.md` com regras do projeto
- [x] Documentação inicial (README, SETUP, ARQUITETURA, ADRs)
- [x] Projeto Unity criado (Unity 6.3 LTS + URP)
- [x] Pacotes XR instalados (OpenXR, XR Interaction Toolkit, Input System)
- [x] Player Settings para Quest (Android, IL2CPP, ARM64, Vulkan, Linear)
- [x] OpenXR com Meta Quest Support + perfis de controle Touch
- [x] Cenas placeholder (Menu + 3 cenários) com XR Origin e UI World Space
- [x] Testado no Quest 2 via APK (Android 14, Vulkan, OpenXR): menu, raio e navegação OK (2026-09-23)
- [ ] Testar no Quest 2 pelo Play do Unity (Quest Link ou Virtual Desktop)
- [x] Repositório no GitHub criado (sem colaboradores: um só desenvolvedor desde 2026-10-26, [ADR 0008](decisoes/0008-desenvolvedor-unico-direto-na-main.md))

## Fase 1 — Especificação e assets
> Estratégia: greybox primeiro, depois modelos leves ([ADR 0004](decisoes/0004-assets-greybox-e-cc0.md)).

- [x] Especificação da v1 alinhada ([ESPECIFICACAO.md](ESPECIFICACAO.md), [ADR 0003](decisoes/0003-escopo-v1-e-interacao.md))
- [x] Lista de sons com licença CC0 escolhida (ESPECIFICACAO, seção 4)
- [x] Sons de motor trocados para turbo-hélice (Dash 8), já que o avião é turbo-hélice ([ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md))
- [ ] Baixar, recortar e importar os sons (registrar no [ASSETS.md](ASSETS.md))
- [ ] Escolher e importar pack low-poly CC0 para a sala da cena 1 (mesa, cadeira, monitor)
- [x] Trazer do projeto antigo só o necessário: avião com interior, terminal, pista, prédios ([ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md))
- [x] Converter materiais para URP (script `LegacyModels`)
- [x] Otimizar o terminal: 2.168 → 39 draw calls (malhas combinadas por material)
- [x] Texturas no máximo 1024 (ASTC vem do build); texturas 4K sem uso não foram trazidas
- [x] Nomes sem acentos/espaços (`Avião` → `Aviao`, `Pìsta` → `Pista`)
- [x] Remover marcas registradas das texturas (logotipo na fuselagem, cauda e leme; pôster no painel)
- [ ] Verificar origem/licença dos modelos antigos ([ASSETS.md](ASSETS.md)): avião e pista achados (provável CC BY 4.0); terminal e fotos das fachadas continuam sem origem
- [ ] Arte do avião: poltronas com cor/tecido, vidro nas janelas
- [ ] Gerar UV2 do terminal quando formos fazer luz baked

## Fase 2 — Núcleo de VR
- [x] Rig do jogador (XR Origin) padrão como prefab (`XR_Player`)
- [x] Desligar locomoção contínua, teleporte, pulo, giro e gravidade no `XR_Player`; rastreamento Device (experiência sentada, [ADR 0006](decisoes/0006-cena-do-voo.md))
- [ ] Raio só no controle direito; opção de desligar o raio por cena (cenas 2 e 3)
- [ ] UI em World Space com raio do controle
- [ ] Fade para preto nas trocas de cena
- [ ] `SessionData`: destino escolhido passando entre as cenas (padrão: Paris)
- [x] Input Action Asset do médico (controle esquerdo: gatilho segurado, X, Y, grip, clique do analógico, Menu) + teclas de teste
- [ ] Painel de pausa (Continuar / Voltar ao menu / Sair) no botão Menu, em todas as cenas (**feito na cena 3**; falta 0, 1 e 2)
- [ ] Menu `00_Menu`: Começar do início, entrar direto em cada cena, Sair

## Fase 3 — Cenários
### Cenário 1 — Compra da passagem (`01_Compra`)
- [ ] Greybox: sala, mesa, cadeira, monitor grande a 0,9–1,2 m
- [ ] Tela 1: escolher entre 4 destinos (Paris, Malta, Lisboa, Buenos Aires; lista configurável)
- [ ] Tela 2: detalhes do voo (Voltar / Confirmar)
- [ ] Tela 3: "Processando pagamento" simulado (~3 s)
- [ ] Tela 4: cartão de embarque + "Ir para o aeroporto" (fade para a cena 2)
- [ ] Sons de clique e confirmação
- [ ] Arte final (troca do greybox)

### Cenário 2 — Sala de embarque (`02_Observacao`)
- [ ] Greybox: cadeira de espera, parede de vidro, pista à frente
- [ ] Aviões decolando em ciclo (a cada ~40–60 s, pool de 1–2 aviões)
- [ ] Som de decolagem 3D abafado pelo vidro, música ambiente e som de terminal
- [ ] Painel de voos com o destino escolhido
- [ ] Chamada de embarque (após 3 min ou gatilho do médico): chime + painel "Embarcando" + fade para a cena 3
- [ ] Arte final (troca do greybox)

### Cenário 3 — Voo (`03_Voo`)
> Cena montada pelo `FlightSceneBuilder` ([ADR 0006](decisoes/0006-cena-do-voo.md)). Testada no Editor por simulação; **falta testar no headset**.

- [x] Greybox: cabine (modelo do avião), poltrona na janela (3 opções no `SeatSelector`), pista, terminal e cidade
- [x] Controlador de voo (máquina de estados, substitui o `Voo.cs` antigo): Pronto → Decolagem → Subida → Cruzeiro (oval) → Aproximação → Pouso → Concluído
- [x] Decolar/pousar pelo gatilho do médico (segurar 1 s)
- [x] Nuvens passando (low-poly, geradas numa malha só)
- [x] Chuva na janela (botão X), com lugar para o som (clipe pendente)
- [x] Noite (botão Y): céu, luz, luz da cabine e luzes da pista (luzes da cidade ainda não)
- [x] Turbulência leve no avião (grip), intensidade configurável e com teto
- [x] Voltar ao normal (clique do analógico), com transições graduais
- [ ] Sons: ambiente da cabine, decolagem, pouso, chime de cintos (código pronto no `FlightAudio`; faltam os clipes)
- [x] Painel "Voo concluído" e volta ao menu
- [ ] Testar no Quest: conforto, escala da cabine, altura dos olhos, leitura dos painéis, FPS
- [ ] Luzes da cidade à noite; terreno com textura (hoje é cor lisa)
- [ ] Arte final (troca do greybox)

## Fase 4 — Terapia e polimento
- [ ] Revisar intensidades (chuva, turbulência, volumes) com o professor/terapeuta
- [ ] Testes de conforto (enjoo) e desempenho (72 FPS no Quest 2)
- [ ] Tela de créditos (**obrigatória**: o avião e a pista são provavelmente CC BY 4.0; ver [ASSETS](ASSETS.md))
- [ ] Build final para o Quest 3

## Futuro (depois da v1)
- Controle do médico por app/tablet separado
- Mais destinos, climas e eventos (combinados com o terapeuta)
- Histórico de sessões do paciente
