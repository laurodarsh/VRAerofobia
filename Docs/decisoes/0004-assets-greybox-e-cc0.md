# 0004 — Assets: greybox primeiro, depois modelos leves e sons CC0

- **Data:** 2026-09-23
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Aceita

## Contexto

O projeto novo ainda não tem modelos próprios: os únicos em `Assets/` são os do XRI (controles,
ponteiros e formas de teste). Os modelos do projeto antigo estão pesados (aeroporto com 1.924 malhas,
texturas 4K, milhares de materiais duplicados; ver [ANALISE_PROJETO_ANTIGO.md](../ANALISE_PROJETO_ANTIGO.md)).
A cena 1 (sala com computador) e a cena 2 (sala de embarque por dentro) pedem ambientes que o
projeto antigo não tinha. Também não havia nenhum som.

## Decisão

1. **Greybox primeiro.** Cada cena é montada com formas simples do Unity (e as formas do XRI, quando
   servirem) para validar fluxo, escala, distâncias e conforto no headset antes de investir em arte.
2. **Depois, modelos leves**, nesta ordem de preferência:
   - modelos do projeto antigo que valham a pena **depois de otimizados** (pista, aviões, prédios);
   - packs low-poly **CC0** (ex.: Kenney, Quaternius, Poly Pizza com licença CC0) para mobília, sala
     de embarque e o que faltar;
   - modelos com licença CC BY só se não houver alternativa, com créditos no app.
3. **Sons com licença CC0 sempre que possível** (Freesound CC0, Kenney, OpenGameArt CC0). A lista
   escolhida está na seção 4 da [ESPECIFICACAO.md](../ESPECIFICACAO.md).
4. Todo asset importado é registrado no [ASSETS.md](../ASSETS.md) e vai para `Assets/ThirdParty/<nome>/`
   (original, sem editar); versões ajustadas ficam em `Assets/_Project/`. Os modelos do projeto antigo, que
   precisaram de ajustes, ficam direto em `Assets/_Project/Models/` ([ADR 0005](0005-modelos-do-projeto-antigo.md)).

## Alternativas consideradas

- **Migrar todo o projeto antigo e otimizar depois:** traria os problemas de desempenho para dentro
  do projeto novo e atrasaria o primeiro teste no headset.
- **Assets pagos da Asset Store:** qualidade maior, mas custo, licença por usuário e repositório mais pesado.
- **Sons CC BY / Noncommercial:** o CC BY exige créditos em todo lugar; o Noncommercial pode impedir
  apresentações e publicações do projeto.

## Consequências

- O primeiro teste de cada cena no headset acontece cedo, com visual simples.
- A troca de greybox por arte final é feita por prefab, sem mexer na lógica.
- O CC0 dispensa créditos, mas mesmo assim registramos a origem no ASSETS.md.
- Arquivos binários grandes (FBX, texturas, áudio) vão pelo Git LFS, como já definido no `.gitattributes`.
