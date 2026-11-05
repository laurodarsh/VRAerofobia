# Registro de assets

Todo asset de terceiros (modelo, textura, som, fonte, pacote da Asset Store) precisa estar listado aqui,
com origem e licença. Isso evita problemas de direitos autorais na apresentação e publicação do projeto.

| Asset | Pasta no projeto | Origem (URL/autor) | Licença | Adicionado por / data |
|---|---|---|---|---|
| XRI Starter Assets 3.3.2 | `Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets` | Unity (sample do pacote `com.unity.xr.interaction.toolkit`) | Unity Companion License | Daniel/Claude, 2026-09-23 |
| XRI XR Interaction Simulator 3.3.2 | `Assets/Samples/XR Interaction Toolkit/3.3.2/XR Interaction Simulator` | Unity (sample do pacote XRI) | Unity Companion License | Daniel/Claude, 2026-09-23 |
| TextMeshPro Essential Resources | `Assets/TextMesh Pro` | Unity (`com.unity.ugui`) | Unity Companion License; fonte Liberation Sans (SIL OFL) | Daniel/Claude, 2026-09-23 |

## Assets vindos do projeto antigo

Quando migrar algo do projeto `Aerofobia atual (22-03-2024)`, registre aqui e tente descobrir a origem original.
Se a origem for desconhecida, marque como **"origem desconhecida — verificar"**.
Detalhes da migração e da preparação: [ADR 0005](decisoes/0005-modelos-do-projeto-antigo.md).

| Asset | Pasta no projeto | Origem (URL/autor) | Licença | Adicionado por / data |
|---|---|---|---|---|
| Avião turbo-hélice com interior (`Aviao.fbx`, era `Avião/Avião.fbx`) | `Assets/_Project/Models/Aviao/` | **Provável:** [Dash-8 q400 (with cockpit) ver IX — TonyWony, Sketchfab](https://sketchfab.com/3d-models/dash-8-q400-with-cockpitver-ix-140a2d7680504e9b98ec4def5d4a7bdf) (2020). **Modificado:** escala 1,28×, logotipos removidos | **CC BY 4.0** (provável): exige crédito no app | Daniel/Claude, 2026-09-24 |
| Textura da fuselagem (`Aviao_Fuselagem.png`, era `g1.vert_base.png`) | `Assets/_Project/Models/Aviao/Textures/` | Do mesmo modelo acima. **Modificada:** logotipo de companhia aérea apagado | **CC BY 4.0** (provável) | Daniel/Claude, 2026-09-24 |
| Terminal do aeroporto (`aeroporto.obj` + `.mtl`) | `Assets/_Project/Models/Aeroporto/` | Projeto antigo; exportado do SketchUp 2012 (provavelmente do 3D Warehouse). Sem nomes nem metadados que permitam achar o original | **origem desconhecida — verificar** | Daniel/Claude, 2026-09-24 |
| Pista e pátio (`AERODROME4.obj` + `.mtl`, era `Pìsta/`) | `Assets/_Project/Models/Pista/` | **Provável:** [AERODROME — ririnhasra88, Sketchfab](https://sketchfab.com/3d-models/aerodrome-ec5a7b76380448f38a39e45f2e848d16) (2021). **Modificado:** grama com cor sólida (a textura se perdeu) | **CC BY 4.0** (provável): exige crédito no app | Daniel/Claude, 2026-09-24 |
| Prédios 1–4 (`predioN.obj` + texturas de fachada) | `Assets/_Project/Models/Predios/` | Projeto antigo; caixas feitas no SketchUp (texturas padrão do SketchUp) com fotos de fachada de origem desconhecida | Geometria: feita pela equipe antiga (provável). **Fotos de fachada: origem desconhecida — trocar por CC0** | Daniel/Claude, 2026-09-24 |

### Como a origem foi encontrada (2026-09-26)

- **Avião:** o FBX guarda o caminho de exportação `/home/minh-tri/Desktop/dash-8/` (Blender 2.81, Linux) e texturas do
  Poliigon. O modelo do TonyWony no Sketchfab tem as mesmas características: 68,3 mil triângulos (o nosso tem 68 mil),
  Blender + Ubuntu + Poliigon, publicado em fev/2020. **Confirmar com quem baixou**, se possível.
- **Pista:** o nome do arquivo (`AERODROME4`) e um material com nome em indonésio (`LINE_KUNING_JALAN_ACCESS`,
  "linha amarela da via") batem com o modelo "AERODROME" de ririnhasra88 no Sketchfab (47,8 mil triângulos; o nosso
  tem 20 mil, então partes parecem ter sido apagadas: o `.mtl` ainda cita um `Boeing_787_8_fbx`).
- **CC BY 4.0** permite uso comercial e modificação, mas exige **crédito** (autor, título, link e licença) e a
  indicação do que foi modificado. Por isso a **tela de créditos** (ROADMAP, Fase 4) passa a ser obrigatória.

### Assets do projeto antigo que não foram trazidos (para referência)

| Arquivo | Origem identificada | Licença |
|---|---|---|
| `sunflowers_puresky_4k.exr` (céu) | Poly Haven, "Sunflowers (Pure Sky)" | CC0 |
| `CasualDay4K.hdr`, `FluffballDay4k.hdr` e outros 11 céus | Unity Asset Store, "Skybox Series Free" (Avionx) | Asset Store EULA (uso em projetos Unity) |
| `058_hdrmaps_com_free_2K.exr` | HDRMaps (versão gratuita) | Licença do HDRMaps (verificar) |
| Personagens `rp_carla`, `rp_eric`, `rp_nathan`, `rp_claudia` | Renderpeople (amostras gratuitas) | Licença Renderpeople (verificar) |
| `Cloud_Polygon_Blender_1.fbx` (nuvem) | desconhecida | não usada: as nuvens da cena 3 são geradas por código |

**Não trazidos de propósito:** texturas da cauda e do leme (logotipo de companhia aérea real) e do painel da cabine
de comando (pôster comercial com marca e site do vendedor). Esses materiais usam cor sólida.
