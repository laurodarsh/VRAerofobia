# 0007 — Cabeça calibrada na poltrona e botão de recentralizar

- **Data:** 2026-09-26
- **Autor:** Daniel/Claude
- **Status:** Aceita

## Contexto

No primeiro teste da cena 3 no Quest 2 (via Link), o paciente parecia flutuar acima da poltrona e tinha de
abaixar a cabeça para ver a janela. O `XR_Player` usava só o rastreamento "Device" com `CameraYOffset = 1,15 m`
(ADR 0006). Esse valor é somado à altura que o headset informa, e o ponto zero do espaço "Device" é escolha do
runtime: se ele não estiver na cabeça, a altura final fica errada. Além disso, 1,15 m foi uma estimativa de
"olhos de pessoa sentada" e nunca foi comparada com a altura da janela do modelo do avião.

## Decisão

**`SeatedHeadCalibration`** (Core), no XR Origin do `XR_Player`, adicionado pelo `PlayerRigSetup`:

- quando o headset começa a rastrear, mede a pose da cabeça e move o **`Camera Offset`** (nunca a câmera) para os
  olhos ficarem em (0; altura dos olhos; 0) no XR Origin, que é o piso da poltrona, olhando para a frente dela;
- **botão de recentralizar**: A no controle direito (ou H no teclado) recoloca a cabeça na poltrona a qualquer momento;
- também recalibra quando o sistema recentraliza (segurar o botão Meta) e quando o app volta ao foco;
- a **altura dos olhos** pode ser mudada no Inspector durante o Play e a visão sobe ou desce na hora, para achar o
  valor certo olhando pela janela no headset. O valor encontrado passa a ser o padrão em
  `PlayerRigSetup.SeatedEyeHeight`;
- **altura dos olhos: 1,22 m** acima do piso da poltrona, ajustada pelo Daniel no Quest 2 (via Link) olhando pela
  janela da poltrona "atrás da asa" (antes: 1,15 m, ADR 0006).

## Alternativas consideradas

- **Rastreamento "Floor" + altura da poltrona:** a altura dos olhos passaria a depender da cadeira real e da
  altura da pessoa.
- **Só ajustar o `CameraYOffset`:** não resolve, porque o erro vem do ponto zero do runtime, que muda entre headsets
  e entre sessões.
- **Escurecer a visão quando a cabeça sai da poltrona** (para não atravessar a parede): implementado e **retirado a
  pedido do Daniel** em 2026-09-26. Hoje a cabeça pode atravessar a parede se a pessoa se inclinar muito.

## Consequências

- A pessoa pode sentar em qualquer cadeira, em qualquer altura e virada para qualquer lado: ao iniciar, fica na
  poltrona olhando para a frente. Se se ajeitar na cadeira depois, basta apertar A.
- O botão A do controle direito fica reservado para isso em todas as cenas.
- A calibração espera no máximo 3 s pelo rastreamento; sem headset (Editor), calibra assim mesmo depois disso.
- A chuva da janela e os painéis da cena do voo são posicionados pela mesma altura: ao mudá-la, recrie a cena.
