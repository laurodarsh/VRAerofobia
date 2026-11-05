# 0005 — Modelos do projeto antigo: avião turbo-hélice, terminal otimizado e remoção de marcas

- **Data:** 2026-09-24
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Aceita

## Contexto

A [ADR 0004](0004-assets-greybox-e-cc0.md) previa reaproveitar modelos do projeto antigo depois de otimizados.
A avaliação (fotos renderizadas num projeto de rascunho, sem mexer no projeto antigo) mostrou:

| Modelo | O que é | Triângulos | Draw calls |
|---|---|---|---|
| `Avião.fbx` | Turbo-hélice regional (tipo Dash 8 / Q400) **com interior**: poltronas, corredor, janelas, cabine de comando | 68 mil | 94 |
| `aeroporto.obj` | Terminal (exportado do SketchUp) com **sala de espera com cadeiras** por dentro, estacionamento e pontes de embarque | 73 mil | **2.168** (1.924 peças soltas) |
| `AERODROME4.obj` | Pista, taxiways e pátio (726 × 389 m) | 20 mil | 15 |
| `predio1..4.obj` | Prédios (caixas com textura de fachada) | < 40 cada | 3 cada |

Problemas encontrados:
- o avião tinha ~78% do tamanho real (cabine com 1,96 m de largura; num Q400 ela tem ~2,5 m);
- as texturas da fuselagem, da cauda e do leme traziam o **logotipo de uma companhia aérea real**, e a do painel da
  cabine de comando era um **pôster comercial** (com marca e site do vendedor);
- a pasta do avião tinha ~120 MB de texturas 4K que **não eram usadas** por nenhum material;
- a origem e a licença dos modelos são desconhecidas (provavelmente baixados de sites de modelos 3D).

## Decisão

1. **O avião do projeto é o turbo-hélice do projeto antigo**, usado por dentro (cena 3) e por fora (aviões
   decolando na cena 2). Os sons de motor passam a ser de turbo-hélice (ver ESPECIFICACAO, seção 4).
2. **Importados:** `Aviao.fbx` (+ só a textura da fuselagem), `aeroporto.obj`, `AERODROME4.obj` e `predio1..4`, em
   `Assets/_Project/Models/`. **Não importados:** `aeroporto2.obj` (cópia usada no antigo "teleporte do pouso"),
   `aviao.obj` (mesmo avião em OBJ), personagens, texturas 4K sem uso, nuvens e céu (a decidir depois).
3. **Script de Editor `LegacyModels`** (menu *Aerofobia > Modelos > Preparar Modelos do Projeto Antigo*) faz a
   preparação de forma reproduzível:
   - importação: avião em escala **1,28×**, sem câmeras, luzes, animações ou tangentes desnecessárias;
   - materiais extraídos como URP/Lit para `Assets/_Project/Materials/<modelo>/`;
   - prefabs em `Assets/_Project/Prefabs/` (`Aviao/Aviao`, `Ambiente/Terminal`, `Ambiente/Pista`, `Ambiente/Predio1..4`),
     os de cenário marcados como Static;
   - **terminal combinado**: uma malha por material (2.168 → **39 draw calls**), salva em
     `Models/Aeroporto/Terminal_Combinado.asset`.
4. **Marcas removidas:** o logotipo foi apagado da textura da fuselagem; cauda, leme e painel viraram cor sólida
   (as texturas originais não entram no repositório). A grama em volta da pista, cuja textura se perdeu, virou cor verde.
5. **Pasta:** diferente do previsto na ADR 0004 (originais em `ThirdParty/`), estes modelos ficam em `_Project/Models/`,
   porque precisaram de ajustes de importação e de edição de textura. `ThirdParty/` continua valendo para packs
   baixados que usamos sem alterar.
6. **Git LFS para malhas geradas:** o `.gitattributes` ganhou a regra `*_Combinado.asset` → LFS. O terminal combinado
   tem ~10 MB em texto (146 mil vértices, faces planas do SketchUp) e não faz sentido em merge.

## Alternativas consideradas

- **Trocar tudo por modelos prontos CC0:** licença limpa, mas um interior de avião bom e leve é difícil de achar de graça,
  e o estilo dos packs low-poly destoaria do semi-realismo desejado para a terapia.
- **Um jato no lugar do turbo-hélice:** mais próximo dos voos que o paciente costuma pegar, mas exigiria outro modelo com
  interior. Fica como possível troca futura: a lógica do voo não depende do modelo.
- **Deixar o Unity fazer o static batching do terminal sem combinar:** menos arquivos no Git, mas continuariam
  ~1.900 objetos para o culling e o resultado depende da ordem dos objetos; combinar por material é previsível.
- **Gerar UV2 (lightmap) no terminal já agora:** dobraria o tamanho do asset. Será gerado quando fizermos luz baked.

## Consequências

- A cena 2 pode usar o interior do terminal (sala de espera) em vez de um greybox novo.
- O avião tem asa alta: da janela do meio da cabine vê-se o motor e a hélice. A fileira do paciente será escolhida no teste da cena 3.
- Pendências de arte: poltronas brancas sem textura, janelas sem vidro, painel da cabine sem detalhe.
- **Licença dos modelos antigos: verificar a origem** antes de publicar o app (registrado no [ASSETS.md](../ASSETS.md)).
- Para regenerar um asset gerado (prefab, terminal combinado ou materiais), apague-o e rode o menu de novo; o script
  nunca sobrescreve o que já existe, para não perder ajustes feitos pela equipe.
