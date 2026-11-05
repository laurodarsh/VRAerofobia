# Pendências — decisões e tarefas em aberto

> Lista do que está esperando resposta ou ação de alguém. Quando resolver um item, marque `[x]`,
> anote a resposta ao lado e, se for uma decisão técnica, registre na ADR correspondente.
> Última atualização: 2026-10-26.

## 1. Git / GitHub (ação do Daniel)
- [x] Levar `docs/especificacao-v1`, `asset/modelos-projeto-antigo` e `feature/cena-voo` para a `main`:
      feito sem PR, por fast-forward, quando o projeto passou a ter um só desenvolvedor (2026-10-26, ADR 0008).
- [x] ~~Convidar os colaboradores~~: não se aplica, o projeto passou a ter um só desenvolvedor (2026-10-26).
- [x] Mudanças locais de build que nunca foram commitadas: **descartadas** (2026-10-26). Eram restos de build
      (`Assets/Resources/` do Performance Testing, `preloadedAssets`) e reserialização automática do Unity em
      `Assets/Samples/`. A única configuração real era *Optimize Buffer Discards (Vulkan)* do OpenXR ligada:
      ver a seção 6.

## 2. Decisões para confirmar (propostas feitas na ESPECIFICACAO; o padrão fica valendo se ninguém mudar)
- [ ] **Destinos da cena 1:** Paris, Malta, Lisboa e Buenos Aires?
- [ ] **Número do voo:** o exemplo "AF 1234" usa o código da Air France. Trocar por um código fictício (sugestão: "UL 1234", de Unity Lab)?
- [ ] **Chamada de embarque da cena 2:** automática após **3 min**, se o médico não chamar antes. Está bom?
- [ ] **Botões do médico:** segurar o gatilho **1 s** para decolar/pousar e **clique do analógico** para "voltar ao normal". Ok?
- [ ] **Monitor da cena 1 a 0,9–1,2 m** (exceção à regra de UI a 1,5–3 m). Ok?
- [ ] **Voz no anúncio de embarque:** só chime + texto, ou gravar voz em português? Se gravar, quem grava?
- [ ] **Fileira do paciente na cena 3:** o avião tem asa alta. Já dá para testar as três opções mudando o índice no
      `SeatSelector` (objeto `Aviao_Voo/Corpo/Poltronas`):
      - 0 = atrás da asa (padrão: vê o chão, sem a hélice);
      - 1 = sob a asa (vê o trem de pouso e o motor);
      - 2 = à frente da hélice.

      Decidir de preferência com o terapeuta.
- [ ] **Tempo até o pouso (cena 3):** do pedido do médico até o avião parar leva de ~1,5 a ~6 min, conforme o ponto
      do oval ([ADR 0006](decisoes/0006-cena-do-voo.md)). A especificação falava em "até ~60 s". Aceitar, diminuir o
      circuito (curvas mais fechadas) ou implementar um atalho de aproximação?

## 3. Com o professor / terapeuta
- [ ] Validar a lista de condições do voo (chuva, noite, turbulência **leve**) e que **não haverá** trovão, relâmpago ou turbulência forte.
- [ ] Validar se ver a hélice pela janela é adequado para a exposição (ou se é melhor uma fileira sem vista da hélice).
- [ ] Validar o tempo de cada etapa. Na cena atual:
      - decolagem: ~18 s na pista e ~1,5 min de subida;
      - cruzeiro: livre;
      - pouso: 1,5–6 min.
- [ ] Validar a intensidade da turbulência (`TurbulenceEffect`, padrão 50% do teto de 8 cm / 1,2°).

## 4. Assets
- [ ] **Licença dos modelos antigos:** investigada em 2026-09-26 (detalhes no [ASSETS.md](ASSETS.md)).
      - **Avião:** provavelmente o "Dash-8 q400 (with cockpit)" do TonyWony no Sketchfab, **CC BY 4.0**.
        Pode usar, mas exige crédito no app. Se possível, confirmar com quem baixou.
      - **Pista:** provavelmente o "AERODROME" de ririnhasra88 no Sketchfab, **CC BY 4.0** (crédito no app).
      - **Terminal:** exportado do SketchUp, origem não encontrada (talvez 3D Warehouse). Perguntar à equipe antiga.
      - **Prédios:** geometria simples do SketchUp; as **fotos de fachada** não têm origem. Trocar por texturas CC0.
- [ ] **Baixar os sons** do Freesound (exige login; lista com links na [ESPECIFICACAO](ESPECIFICACAO.md), seção 4). Depois o Claude recorta, importa e registra.
      Na cena 3 os lugares já existem: `FlightAudio` (chime, decolagem, pouso, ambiente) e `RainEffect` (chuva).
- [ ] **Pack da sala da cena 1** (mesa, cadeira, monitor): aprovar o Kenney Furniture Kit (CC0) ou sugerir outro.
- [ ] **Céu da cena 3:** hoje é o céu procedural do Unity (muda de cor com chuva e noite). O céu do projeto antigo
      `sunflowers_puresky_4k.exr` é do **Poly Haven (CC0)** e pode ser reaproveitado, se quiserem um céu com nuvens pintadas.
- [ ] **Arte do avião:** aprovar que o Claude dê cor às poltronas (sugestão: tecido azul-acinzentado) e ponha vidro nas janelas.
- [ ] **Escala do terminal:** o prefab `Terminal` foi importado 7,7× maior que o real. A cena 3 usa escala 0,13 (a do
      projeto antigo). Quem fizer a cena 2 deve usar a mesma escala, ou corrigir a importação no `LegacyModels`.
- [ ] Cópia local do projeto antigo (`Downloads\Aerofobia atual (22-03-2024)`): manter até a licença dos modelos ser verificada.

## 5. Testes que só uma pessoa com o Editor/headset pode fazer
- [ ] Abrir o projeto no Unity e olhar os prefabs novos (`Prefabs/Aviao/Aviao`, `Prefabs/Ambiente/Terminal`, `Pista`) com luz normal.
- [ ] **Cena 3 no Editor:** abrir `03_Voo`, dar Play e testar com as teclas:
      - Espaço segurado 1 s: decolar/pousar;
      - C: chuva;
      - N: noite;
      - T: turbulência;
      - R: normal;
      - P: pausa.

      Para ver de fora, selecione `Aviao_Voo` na Scene view durante o Play.
- [ ] **Cena 3 no Quest:** conforto na decolagem e nas curvas, altura dos olhos na poltrona, leitura dos painéis, FPS
      (a cena tem o avião com 94 draw calls, mais terminal, pista, cidade e nuvens: medir no OVR Metrics Tool).
- [ ] **Menu no Quest** depois da troca do `XR_Player` para o rastreamento Device: conferir se o painel continua na altura certa.
- [ ] **Aplicar a altura de 1,22 m:** rodar *Aerofobia > Cenas > Montar Cena do Voo (03_Voo)* e commitar a cena e o
      `XR_Player`. Até lá, o prefab e a cena continuam com 1,15 m (o código já tem 1,22).
- [x] **Altura dos olhos na poltrona** (ADR 0007): 1,22 m, ajustada no Quest 2 via Link (Daniel, 2026-09-26).
      Para reajustar: em Play, `XR_Player` → `SeatedHeadCalibration` → "Altura dos olhos"; gravar em
      `PlayerRigSetup.SeatedEyeHeight` e recriar a cena do voo (a chuva e os painéis usam esse valor).
- [ ] Testar o Play Mode com o Quest via Link / Virtual Desktop (pendente desde a Fase 0).

## 6. Próximo trabalho sugerido
- Sons da cena 3, assim que forem baixados.
- Fase 2 do [ROADMAP](ROADMAP.md): fade entre cenas, `SessionData`, pausa nas cenas 0–2, raio só no controle direito.
- Cena 2 (sala de embarque), reaproveitando `Terminal`, `Pista`, `Aviao` e o `DoctorInput`.
- Desempenho: avaliar ligar *Optimize Buffer Discards (Vulkan)* no OpenXR (otimização do Quest). Se for adotar,
  fazer pelo `ProjectSetup.cs` e medir o FPS antes e depois.
