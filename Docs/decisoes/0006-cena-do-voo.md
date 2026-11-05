# 0006 — Cena do voo: voo cinemático em circuito, jogador sentado e painéis sempre visíveis

- **Data:** 2026-09-26
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Proposta (validar no headset e com o terapeuta)

## Contexto

A cena 3 (`03_Voo`) já existia no projeto antigo, mas com problemas conhecidos
([ANALISE_PROJETO_ANTIGO](../ANALISE_PROJETO_ANTIGO.md)):
- avião movido por física e `OnCollisionEnter`, com velocidade somada por frame, dependente do FPS;
- teleporte do aeroporto durante o pouso;
- nuvens criadas e destruídas a cada 0,1 s;
- chuva ligada por um botão de UI.

A [ESPECIFICACAO](../ESPECIFICACAO.md) (seção 3.3) e a [ADR 0003](0003-escopo-v1-e-interacao.md) pedem outras coisas:
- voo em circuito que volta à mesma pista, sem teleporte;
- movimento suave;
- controles do médico no controle esquerdo;
- experiência sentada.

Ao montar a cena apareceram mais fatos:
- o `XR_Player` vinha com locomoção, teleporte e **gravidade** ligados. Dentro de um avião em movimento, o jogador cairia;
- a pista do modelo `AERODROME4` tem só **460 m** (32 m de largura, escala correta pela biruta);
- o terminal foi importado **7,7× maior** que o real. O projeto antigo o usava com escala 0,13;
- dentro da cabine, os encostos e os bagageiros ficam na frente de qualquer painel a 1,5 m do paciente.

## Decisão

1. **Voo cinemático.** O `FlightController` move o avião por código ao longo do `FlightCircuit`, sem física,
   com rampas suaves:
   - aceleração que começa devagar;
   - subida e descida em curva "S";
   - inclinação e arfagem suavizadas.

   O jogador é filho do avião e a câmera nunca é movida.
2. **Circuito oval.** A primeira reta passa pelo eixo da pista e as curvas são para a esquerda, o lado da poltrona
   do paciente. O pouso é sempre na mesma pista.
   - Valores padrão: retas de 4,5 km, raio de 1,2 km, cruzeiro a 450 m e 65 m/s, inclinação máxima de 12°.
   - Na pista de 460 m: o avião sai do chão a ~340 m, toca a 42 m/s e para a ~410 m da cabeceira.
3. **Pouso pedido a qualquer momento.** Quando o médico pede o pouso, a descida **começa na hora**, ao longo do
   oval, e termina tocando a pista. Se o avião está perto demais da pista (menos de 5 km de caminho), ele dá mais
   uma volta.
   - Do pedido até parar leva **entre ~1,5 e ~6 min**, conforme o ponto do oval.
   - Isso é mais do que os "até ~60 s" da especificação: um oval com curvas de no máximo 12° e velocidade
     realista tem ~16 km de volta.
   - Para encurtar, basta diminuir as retas e o raio no `FlightCircuit`, com curvas mais fechadas.
4. **Jogador sentado.** O `XR_Player` passa a usar o rastreamento **Device**:
   - a câmera começa 1,15 m acima do piso da poltrona, qualquer que seja a cadeira real;
   - o objeto `Locomotion` fica desativado (movimento, giro, teleporte, escalada, gravidade e pulo);
   - as referências de teleporte e giro dos controles ficam vazias;
   - o CharacterController fica desligado.

   Isso vale para todas as cenas (ADR 0003). O script de Editor `PlayerRigSetup` aplica essa configuração.
5. **Poltrona escolhida no Inspector.** Há três poltronas na janela esquerda: atrás da asa (padrão, hélice fora
   da vista), sob a asa e à frente da hélice. O `SeatSelector` troca de poltrona sem mexer na cena.
6. **Painéis sempre visíveis.** Os painéis de pausa e de "Voo concluído" ficam a 1,5 m, na altura dos olhos, e são
   desenhados por cima da geometria (`UiAlwaysOnTop`, ZTest Always), para não serem cobertos pelos encostos.
7. **Clima.** O `WeatherController` recebe os comandos do médico e mistura quatro presets de iluminação
   (dia, dia com chuva, noite, noite com chuva) em transições de 8 s.
   - A chuva é feita de partículas **só do lado de fora da janela** do paciente.
   - A turbulência mexe um objeto intermediário do avião ("Corpo"), com teto de 8 cm e 1,2° no código.
8. **Escala do terminal.** Na cena, o terminal usa escala 0,13, como no projeto antigo. O prefab `Terminal` não foi
   alterado: a cena 2 decide a própria escala.
9. **Cena gerada por script.** O `FlightSceneBuilder` recria a `03_Voo` inteira (menu *Aerofobia > Cenas*), como o
   `SceneBuilder` e o `LegacyModels`. Nuvens e luzes da pista são geradas no `Awake`, sem arquivos no Git.

## Alternativas consideradas

- **Física (Rigidbody), como no projeto antigo:** o resultado depende do FPS e de colisões, e as acelerações são
  difíceis de controlar. Isso é ruim para enjoo em VR.
- **Timeline/animação gravada:** fácil de ajustar visualmente, mas não permite pousar a qualquer momento nem
  combinar com os comandos do médico.
- **Aproximação por atalho** (sair do oval direto para a final): cumpriria os 60 s, mas exigiria curvas fechadas
  ou uma trajetória calculada na hora. Ficou para depois, se o terapeuta achar a espera longa.
- **Painel mais perto (0,8 m), acima dos encostos:** fica fora da regra de 1,5–3 m do CLAUDE.md e continua sendo
  coberto pelos bagageiros.
- **Rastreamento Floor (chão):** a posição depende de onde a pessoa está na área do guardião. Sentada fora do
  centro, ela "sentaria" no corredor ou fora do avião.

## Consequências

- A cena funciona no Editor sem headset. As teclas do asset `ControlesMedico` simulam o médico:
  - **Espaço** segurado 1 s: decolar/pousar;
  - **C**: chuva;
  - **N**: noite;
  - **T**: turbulência;
  - **R**: normal;
  - **P**: pausa.

  Os componentes também têm atalhos no menu de contexto, como "Teste: ir direto para o cruzeiro".
- A troca para o rastreamento Device afeta o menu e as outras cenas: a cabeça fica a 1,15 m do chão virtual.
  **Testar no Quest.**
- A pista curta limita as velocidades de decolagem e pouso. Se trocarmos a pista por uma maior, dá para subir
  `_liftoffSpeed` e `_approachSpeed`.
- O chão é uma cor lisa: de 450 m de altura a paisagem é pobre. A arte do terreno fica para a troca do greybox.
- Os sons têm lugar reservado (`FlightAudio`, `RainEffect`), mas os clipes ainda não existem.
- Ajustes feitos à mão na cena se perdem se ela for recriada pelo script. Ajuste pelo Inspector **e** leve o
  valor para o `FlightSceneBuilder`, ou pare de recriar a cena.
