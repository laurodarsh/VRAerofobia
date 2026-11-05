# Especificação da v1 — Aerofobia VR

> Documento de design: **o que** cada cena faz, do ponto de vista do paciente e do médico.
> Como isso é implementado fica na [ARQUITETURA](ARQUITETURA.md). As decisões por trás
> estão nas ADRs [0003](decisoes/0003-escopo-v1-e-interacao.md), [0004](decisoes/0004-assets-greybox-e-cc0.md)
> e [0005](decisoes/0005-modelos-do-projeto-antigo.md).
> Alinhado com o Daniel em 2026-09-23. Mudou algo no escopo? Atualize este arquivo **e** registre uma ADR.

## 1. Visão geral

A v1 tem um menu e três cenas de exposição gradual. Cada cena é um passo a mais em direção ao voo.

```
00_Menu ──▶ 01_Compra ──▶ 02_Observacao ──▶ 03_Voo ──▶ "Voo concluído" ──▶ 00_Menu
   │             ▲               ▲               ▲
   └─────────────┴───────────────┴───────────────┘
        o menu permite entrar direto em qualquer cena
```

| Cena | O paciente… | Duração típica |
|---|---|---|
| `00_Menu` | Escolhe começar do início ou ir direto a uma cena | — |
| `01_Compra` | Sentado a uma mesa, compra a passagem num computador | 1–2 min |
| `02_Observacao` | Sentado na sala de embarque, vê aviões decolando pelo vidro | 2–5 min (o médico decide) |
| `03_Voo` | Sentado na janela do avião, decola, voa e pousa | livre (o médico decide) |

## 2. Regras gerais (valem para todas as cenas)

### Experiência sentada, sem locomoção
- O paciente fica **sentado o tempo todo**. Não existe andar, teleporte, pulo nem giro pelo analógico.
- A única forma de olhar em volta é **mexer a cabeça** (rastreamento do headset).
- Por isso, tudo o que importa fica **à frente** ou ao lado do paciente, nunca atrás.
- Se a visão ficar torta, use o recentrar do sistema: segurar o botão Meta (Oculus) no controle direito.

### Quem segura o controle

| Cena | Controle direito | Controle esquerdo |
|---|---|---|
| `00_Menu`, `01_Compra` | **Paciente**: aponta o raio e aperta o gatilho | — |
| `02_Observacao`, `03_Voo` | Não é usado (raio desligado) | **Médico**: comandos da tabela abaixo |

### Mapeamento dos botões do médico (controle esquerdo)

| Botão | Cena 2 | Cena 3 |
|---|---|---|
| **Gatilho** (segurar 1 s) | Chamar o embarque agora | Decolar / pousar |
| **X** | — | Chuva liga/desliga |
| **Y** | — | Noite liga/desliga |
| **Grip** (botão lateral) | — | Turbulência leve liga/desliga |
| **Clique do analógico** | — | Voltar ao normal (desliga chuva, noite e turbulência) |
| **Menu (≡)** | Pausa | Pausa |

- "Segurar 1 s" no gatilho evita decolar ou pousar por um toque sem querer.
- O botão Meta (Oculus) é reservado pelo sistema e não pode ser usado.
- Os botões ficam num Input Action Asset do projeto: mudar o mapeamento não exige mexer no código.

### Pausa e saída (segurança do paciente)
- O botão **Menu (≡)** abre, em qualquer cena, um painel de pausa à frente do usuário com
  **Continuar**, **Voltar ao menu** e **Sair**.
- Enquanto a pausa estiver aberta, o voo, os aviões e os sons param.

### Transições
- Toda troca de cena é feita com **fade para preto** (cerca de 1 s).
- Mudanças de clima e de horário são **graduais** (5 a 10 s), nunca instantâneas.
- **Sem sustos**: nada de trovão, relâmpago, alarme, solavanco forte ou som alto repentino.
  Um evento novo desse tipo só entra se for combinado com o professor ou o terapeuta.

### Acompanhamento pelo médico
- O médico acompanha o que o paciente vê espelhando o headset (**Transmitir** no app Meta Horizon,
  no celular ou em `oculus.com/casting` no PC). Não há HUD para o médico dentro da cena, para não
  quebrar a imersão do paciente.

## 3. Cenas

### 3.0 `00_Menu`
- Painel World Space com:
  - **Começar do início** (vai para `01_Compra`);
  - **Compra da passagem**, **Sala de embarque** e **Voo** (entram direto na cena);
  - **Sair**.
- Quem pula a cena 1 viaja para o destino padrão (**Paris**).

### 3.1 `01_Compra` — compra da passagem
**Ambiente:** uma sala simples (escritório ou quarto), o paciente sentado a uma mesa com um
**monitor grande** à frente. A tela do monitor é um Canvas World Space.

**Passo a passo na tela:**
1. **Escolha seu destino**: 4 cartões com foto/ícone e nome da cidade.
   Destinos iniciais: **Paris, Malta, Lisboa e Buenos Aires** (a lista é configurável).
2. **Detalhes do voo**: destino, data, horário e **assento na janela** já escolhidos.
   Botões *Voltar* e *Confirmar*.
3. **Processando pagamento…**: barra de progresso de ~3 s. É **só simulação**: não há campos
   de cartão nem de dados pessoais.
4. **Passagem emitida**: cartão de embarque com destino, número do voo, portão e assento.
   Botão **Ir para o aeroporto**, que leva à cena 2 com fade.

**Sons:** clique em cada botão, som de confirmação na passagem emitida, ambiente de sala bem baixo (opcional).

**Distância da tela:** o `CLAUDE.md` pede UI a 1,5–3 m. Um monitor numa mesa fica mais perto,
então esta cena é uma **exceção** (ver [ADR 0003](decisoes/0003-escopo-v1-e-interacao.md)): monitor grande (~32") a
**0,9–1,2 m** e texto maior que o padrão.

### 3.2 `02_Observacao` — sala de embarque
**Ambiente:** o paciente sentado numa cadeira da sala de embarque, de frente para uma
**parede de vidro** que dá para a pista. Ao lado, um **painel de voos** mostra
"Voo AF 1234 — <destino> — Portão 3 — Embarque em breve".

**O que acontece:**
- Aviões **decolam em ciclo** pela pista, um a cada ~40–60 s (configurável). São 1 ou 2 modelos
  de avião reaproveitados (pool), sem criar e destruir objetos.
- O som da decolagem é 3D, distante e **abafado pelo vidro** (filtro passa-baixa).
- **Música ambiente** baixa e som de terminal (pessoas conversando ao longe).

**Ida para o voo:** depois de um tempo configurável (padrão **3 min**) **ou** quando o médico
segurar o gatilho:
1. toca o chime de aviso;
2. o painel muda para **"Embarcando"** e aparece o texto do anúncio
   ("Voo AF 1234 para <destino>, embarque no portão 3");
3. após ~5 s, fade para preto e troca para a cena 3.

Voz no anúncio é opcional na v1. Se houver, será gravada em português pela equipe (sem problema de licença).

### 3.3 `03_Voo` — o voo
**Ambiente:** interior da cabine, o paciente numa **poltrona na janela**. A cabine é o referencial
fixo: quem se move é o **avião** (o `XR_Player` é filho dele), nunca a câmera.

**Sequência:**

| Estado | Como entra | O que acontece |
|---|---|---|
| **Pronto** | Início da cena | Avião parado na cabeceira da pista. Ambiente da cabine e chime de "apertem os cintos" |
| **Decolagem** | Médico segura o gatilho | Acelera na pista e sai do chão (~1 min, aceleração por curva suave) |
| **Subida** | Automático | Sobe até ficar **acima das nuvens** |
| **Cruzeiro** | Automático | Voa num **oval amplo** em volta do aeroporto, inclinação de no máximo 10–15° nas curvas |
| **Aproximação** | Médico segura o gatilho | Desce pelo oval até alinhar com a pista e tocar. Na implementação atual leva de ~1,5 a ~6 min, conforme o ponto do oval (estimativa inicial era ~60 s; ver [ADR 0006](decisoes/0006-cena-do-voo.md)) |
| **Pouso** | Automático | Toca a pista **no mesmo aeroporto de onde saiu** e desacelera até parar (~1,5 min) |
| **Concluído** | Automático | Painel "Voo concluído" com botão para voltar ao menu |

- **Não há teleporte.** O projeto antigo teleportava o aeroporto durante o pouso; aqui o avião
  faz um circuito fechado e volta à mesma pista.
- Velocidade, altitude, tamanho do oval e durações são configuráveis no Inspector.

**Condições de clima (comandos do médico):**

| Condição | Botão | Efeito | Som |
|---|---|---|---|
| Nuvens | sempre ligadas | Nuvens passando pela janela | — |
| **Chuva** | X | Gotas na janela, céu mais escuro e cinza | Chuva na janela |
| **Noite** | Y | Céu noturno, luz do sol baixa, luzes da cabine e da cidade/pista acesas | — |
| **Turbulência leve** | Grip | O **avião** balança de leve (ruído suave em posição e rotação), intensidade configurável | Leve rangido da cabine (opcional) |
| **Normal** | Clique do analógico | Desliga chuva, noite e turbulência (gradual) | — |

- As condições são **independentes e combináveis** (ex.: chuva + noite + turbulência).
  É assim que o médico sobe o nível de dificuldade.
- Todas funcionam em qualquer fase do voo, inclusive parado na pista.
- A turbulência nunca mexe a câmera, só o avião, e a amplitude máxima tem teto no código.

## 4. Sons

Todos com licença que permite uso sem pedir autorização. **Registrar no [ASSETS.md](ASSETS.md) ao importar.**
O Freesound pede login para baixar; o Kenney tem download direto.

| # | Uso | Cena | Arquivo sugerido | Licença |
|---|---|---|---|---|
| 1 | Cliques e confirmação de UI | 0, 1 | [UI Audio — Kenney](https://kenney.nl/assets/ui-audio) (50 sons) | CC0 |
| 2 | Música ambiente do aeroporto | 2 | [Elevator Music — Alex McCulloch](https://opengameart.org/content/elevator-music) (jazz leve); alternativas na coleção [CC0 Calm/Relaxing Music](https://opengameart.org/content/cc0-calm-relaxing-music) | CC0 |
| 3 | Ambiente do terminal | 2 | [airport_ambiance_01 — joedeshon](https://freesound.org/people/joedeshon/sounds/273468/) (3:51) | CC0 |
| 4 | Decolagem de turbo-hélice vista de fora | 2 | [turboprop engine take off — mycompasstv](https://freesound.org/people/mycompasstv/sounds/476932/) (4:15, Dash 8 Q400; recortar) | CC0 |
| 5 | Chime de aviso (cintos / embarque) | 2, 3 | [Airplane, Seatbelt Sign Beep — Kinoton](https://freesound.org/people/Kinoton/sounds/670297/) (0:03) | CC0 |
| 6 | Decolagem por dentro | 3 | [Bombardier Dash 8-300 Runway Takeoff Cruise — Pack489](https://freesound.org/people/Pack489/sounds/399371/) (2:38, sem vozes) | CC0 |
| 7 | Ambiente da cabine em cruzeiro (loop) | 3 | [bombardier-Q400-Passager-cabin-recording — birdOfTheNorth](https://freesound.org/people/birdOfTheNorth/sounds/572544/) (1:45, gravado com celular) | CC0 |
| 8 | Aproximação e pouso por dentro | 3 | [Bombardier Dash 8-300 Landing — Pack489](https://freesound.org/people/Pack489/sounds/399372/) (2:33; tem voz de comissária em francês ao fundo: recortar ou abafar) | CC0 |
| 9 | Chuva na janela (loop) | 3 | [Rain on Window — CBeeching](https://freesound.org/people/CBeeching/sounds/75008/) (1:02) | CC0 |

O avião do projeto é um **turbo-hélice** (tipo Dash 8 / Q400; ver [ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md)), então os sons de
motor (#4, #6, #7, #8) são de turbo-hélice, de preferência do próprio Dash 8. Sons de jato não combinam com a hélice vista pela janela.

Preparação dos áudios (no Audacity, antes de importar):
- recortar só o trecho útil e fazer loops sem emenda audível (#3, #7, #9);
- normalizar o volume e exportar em **OGG** (música e ambientes longos) ou **WAV** (efeitos curtos);
- no Unity: ambientes longos com *Load Type = Streaming*, efeitos curtos com *Decompress On Load*.

Alternativas, se as de cima não ficarem boas:
- CC0: [Fokker 50 Turboprop Taxi and Takeoff — Pack489](https://freesound.org/people/Pack489/sounds/457131/) (decolagem por dentro).
- CC BY 4.0 (atribuição obrigatória): [Fokker F27 Turbo Prop approach and landing — Kevaaq](https://freesound.org/people/Kevaaq/sounds/443938/)
  (gravado na cabine de comando). Se usar, os créditos precisam aparecer no app (tela de créditos no menu) e no ASSETS.md.

## 5. Modelos e ambientes

Estratégia na [ADR 0004](decisoes/0004-assets-greybox-e-cc0.md): primeiro **greybox** (formas simples do Unity) para
validar fluxo, escala e conforto no headset; depois trocar por modelos leves.

| Cena | Precisa de | Origem |
|---|---|---|
| 1 | Sala, mesa, cadeira, monitor | Pack low-poly CC0 (ex.: Kenney Furniture Kit) — **a escolher** |
| 2 | Sala de embarque com cadeiras e vidro; pista; aviões decolando | **Prefabs `Terminal`** (tem sala de espera com cadeiras por dentro), **`Pista`** e **`Aviao`**, do projeto antigo |
| 3 | Interior da cabine (poltronas, janela); aeroporto visto de cima; nuvens | **Prefab `Aviao`** (turbo-hélice com interior), `Terminal`, `Pista` e `Predio1–4`; nuvens a definir |

- Os modelos do projeto antigo já estão importados e otimizados ([ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md)):
  o terminal caiu de 2.168 para 39 draw calls e o avião foi corrigido para a escala real.
- **Asa alta:** no turbo-hélice, a asa e a hélice ficam em cima das janelas do meio da cabine. A fileira do paciente
  (à frente, sobre ou atrás da asa, com a hélice à vista ou não) será escolhida no teste de greybox da cena 3,
  de preferência com o terapeuta.
- Os modelos do XRI que estão em `Assets/Samples/` são **controles, ponteiros e formas de teste**:
  servem para a interação e o greybox, mas não como cenário.

## 6. Orçamento de desempenho

Vale o do `CLAUDE.md`: **72 FPS no Quest 2 e 90 FPS no Quest 3**, até ~150 draw calls e ~750 mil
triângulos por cena, texturas de no máximo 2048 com ASTC e luz baked.

Pontos de atenção desta v1:
- **Chuva:** gotas só **na janela** (textura/partículas perto do paciente), não uma chuva no mundo inteiro.
- **Nuvens:** poucas nuvens low-poly ou billboards reaproveitadas, sem nuvens volumétricas.
- **Noite:** troca de skybox + luz; as luzes "acesas" da cidade e da pista são material emissivo, não luzes reais.
- **Cena 2:** os aviões que decolam são poucos, reaproveitados, e somem na distância.

## 7. Fora do escopo da v1

- Andar, teleporte ou qualquer locomoção.
- Hand tracking (os controles bastam).
- Trovão, relâmpago, tempestade forte, turbulência forte ou emergências.
- Taxiamento longo, embarque andando pelo avião, comissários animados.
- Salvar histórico de sessões do paciente.
- Destino diferente do aeroporto de partida (o avião sempre volta à mesma pista).
