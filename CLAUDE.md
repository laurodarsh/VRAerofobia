# CLAUDE.md — Regras do projeto Aerofobia VR

Este arquivo é lido automaticamente pelo Claude Code. Ele define **o que é o projeto,
como trabalhar nele e o que nunca fazer**. Desde 2026-10-26 o projeto tem **um único
desenvolvedor (Daniel)**, que trabalha direto na `main` (ADR 0008).

---

## 1. O projeto

**Aerofobia VR** — aplicação de realidade virtual do **Unity Lab** para exposição gradual
de pessoas com medo de voar (aerofobia). O usuário passa por 3 cenários:

1. **Compra da passagem**: sentado a uma mesa, compra a passagem num computador.
2. **Observação**: na sala de embarque, vê aviões decolando pelo vidro.
3. **Voo**: dentro do avião, decolagem e pouso, com clima controlado pelo médico.

O que cada cena faz: [Docs/ESPECIFICACAO.md](Docs/ESPECIFICACAO.md).

| Item | Valor |
|---|---|
| Headsets alvo | **Meta Quest 2** (desenvolvimento em casa) e **Meta Quest 3** (laboratório) |
| Unity | **6000.3.24f1 (Unity 6.3 LTS)** — ver `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP (Universal Render Pipeline) |
| XR | OpenXR + Meta Quest Support + XR Interaction Toolkit 3 |
| Input | Input System (novo). O Input Manager antigo está desativado |
| Build | Android, IL2CPP, ARM64, Vulkan |
| Versões de pacotes | Fonte da verdade: `Packages/manifest.json` |

Documentação completa: [README.md](README.md) e a pasta [Docs/](Docs/).

---

## 2. ⛔ NUNCA fazer (sem aprovação explícita do responsável)

Estas ações podem quebrar o projeto para todo mundo. Se for realmente necessário,
**pergunte antes** e registre a decisão em `Docs/decisoes/`.

### Unity / assets
- **Nunca trocar a versão do Unity** (`ProjectSettings/ProjectVersion.txt`). Todo mundo precisa usar a mesma.
- **Nunca adicionar, remover ou atualizar pacotes** (`Packages/manifest.json`, `packages-lock.json`) sem uma ADR.
- **Nunca apagar, recriar ou editar à mão arquivos `.meta`.** O `.meta` guarda o GUID; perder o GUID quebra todas as referências a esse asset.
- **Nunca mover ou renomear assets pelo Explorer/terminal.** Sempre pelo Project window do Unity (ele move o `.meta` junto).
- **Nunca editar à mão arquivos `.unity`, `.prefab`, `.asset` e `.mat`** (YAML). Use o Editor ou um script de Editor.
- **Não mexer em `ProjectSettings/`** (Player, XR, Quality, Graphics, Tags/Layers) sem combinar. A configuração oficial fica em `Assets/_Project/Scripts/Editor/ProjectSetup.cs`: mude lá e documente.
- **Não editar nada em `Assets/ThirdParty/`, `Assets/Samples/`, `Assets/XR/`, `Assets/XRI/` ou `Assets/TextMesh Pro/`.** São arquivos de terceiros ou gerados por pacotes. Para customizar, copie para `_Project/`.
- **Não criar pastas `Resources/`**: tudo nelas entra no build, mesmo sem uso.

### Git
- **Nunca** `git push --force`, `git reset --hard`, `git clean -fdx`, `git checkout -- .` ou rebase de commits já enviados.
- **Nunca commitar** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`, `.apk`, `.sln`, `.csproj` (já estão no `.gitignore`).
- **Nunca commitar segredos** (chaves, tokens, keystore Android `.keystore`/`.jks` e senhas).
- **Nunca usar `--no-verify`** para pular hooks.

### Cenas
- Prefira trabalhar em **prefabs**: a cena só os posiciona. Cenas geradas por script (ex.: `FlightSceneBuilder`)
  devem ser alteradas no script, não à mão.

---

## 3. ✅ Regras de documentação (obrigatórias)

**Toda mudança precisa ser documentada.** Sem isso, a tarefa não está terminada.

1. **`Docs/CHANGELOG.md`**: adicione uma linha em `## [Não lançado]` para toda mudança visível
   (feature, correção, asset novo, configuração). Formato: `- Descrição curta (autor, AAAA-MM-DD)`.
2. **`Docs/decisoes/` (ADR)**: toda **decisão técnica** (pacote novo, mudança de arquitetura,
   troca de configuração do projeto, padrão novo) ganha um arquivo `NNNN-titulo-curto.md`
   usando o modelo [`0000-modelo.md`](Docs/decisoes/0000-modelo.md).
3. **`Docs/ARQUITETURA.md`**: atualize quando criar uma cena, um sistema/script principal,
   um prefab central ou quando mudar o fluxo entre cenas.
4. **`Docs/ROADMAP.md`**: marque `[x]` nas tarefas concluídas.
5. **Código**: toda classe pública e todo método público tem comentário XML `/// <summary>` em português,
   explicando **o que faz e por quê**, não o óbvio.
6. **Assets importados**: registre origem e licença em `Docs/ASSETS.md`.

Ao terminar uma tarefa, o Claude deve **sempre** atualizar esses arquivos antes de dizer que terminou.

---

## 4. Estrutura de pastas

```
Assets/
  _Project/            ← TODO o conteúdo nosso fica aqui
    Scenes/            ← 00_Menu, 01_Compra, 02_Observacao, 03_Voo (+ Sandbox/ para testes pessoais)
    Scripts/
      Core/            ← fluxo de cenas, sessão, configurações (Aerofobia.Core)
      Flight/          ← voo, avião, turbulência (Aerofobia.Flight)
      UI/              ← menus e HUD em World Space (Aerofobia.UI)
      Editor/          ← scripts de Editor, ProjectSetup, SceneBuilder (Aerofobia.EditorTools)
    Prefabs/  Materials/  Models/  Textures/  Audio/
    Settings/          ← assets do URP e perfis de qualidade
  ThirdParty/          ← assets baixados (não editar)
  Samples/  XR/  XRI/  TextMesh Pro/   ← gerados pelos pacotes (não editar)
Docs/                  ← documentação do projeto
```

- Cada pasta de script tem um **Assembly Definition** (`.asmdef`). Um script novo entra no asmdef da pasta.
- Nomes de pastas e arquivos: **sem acentos e sem espaços** (quebram o build Android e o Git).

---

## 5. Boas práticas de código (C# / Unity)

- **Namespace** obrigatório: `Aerofobia.Core`, `Aerofobia.Flight`, `Aerofobia.UI`, `Aerofobia.EditorTools` (não use `Aerofobia.Editor`: conflita com `UnityEditor.Editor`).
- **Nomes**: classes, métodos e propriedades em `PascalCase`; campos privados em `_camelCase`; um arquivo por classe, com o mesmo nome.
- **Campos no Inspector**: `[SerializeField] private`, nunca `public` só para aparecer no Inspector.
  Use `[Tooltip("...")]` e `[Header("...")]` em português.
- **Proibido em `Update`/`LateUpdate`/`FixedUpdate`**: `Find*`, `GetComponent`, `FindObjectOfType`,
  `Camera.main` repetido, alocações (`new`, LINQ, concatenação de string). Guarde referências no `Awake`.
- **Movimento independente do FPS**: sempre multiplique por `Time.deltaTime` e não some valores fixos por frame.
- **Nada de lógica por `OnCollisionEnter` com objeto movido por `transform`**: use distância ou triggers.
- **Tags como string**: evite. Prefira referências no Inspector ou componentes marcadores.
- **Singletons**: evite. Se for inevitável, só para serviço global e documentado na ARQUITETURA.
- **Logs**: `Debug.Log` só para depuração temporária; remova antes do commit.
- **Comentários e textos de UI em português.**

---

## 6. Regras específicas de VR (Quest)

### Desempenho (FPS baixo em VR causa enjoo)
- Metas: **72 FPS no Quest 2** e **90 FPS no Quest 3**.
- Orçamento por cena: **até ~150 draw calls** e **até ~750 mil triângulos**. Confira no Frame Debugger ou no OVR Metrics Tool.
- Texturas: **máximo 2048** (1024 para objetos pequenos), compressão **ASTC**.
- Sem sombras em tempo real em muitos objetos; prefira **luz baked**.
- Nada de pós-processamento pesado (bloom, SSAO, motion blur, depth of field).
- Juntar malhas estáticas e marcar objetos parados como **Static**.

### Conforto (evitar enjoo)
- **Experiência 100% sentada, sem locomoção**: nada de andar, teleporte, pulo ou giro pelo analógico.
  O usuário só olha em volta com a cabeça (ADR 0003).
- **Nunca mover ou girar a câmera diretamente.** A câmera é controlada pelo headset.
  Para deslocar o usuário, mova o **XR Origin** (ou o objeto pai, ex.: o avião).
- Acelerações sempre suaves (curvas/lerp), nunca instantâneas. Teleportes com **fade** para preto.
- No voo, o usuário fica **sentado dentro da cabine**: a cabine é o referencial fixo.
- Nunca criar efeitos de câmera como *camera shake* aplicado à câmera. Na turbulência, mexa o avião.

### UI e interação
- **Proibido Canvas "Screen Space – Overlay"**: não aparece no headset. Use **World Space**,
  a 1,5–3 m do usuário, texto ≥ 24 pt em escala 0,001.
  Exceção documentada: o monitor da cena de compra, a 0,9–1,2 m, com texto maior (ADR 0003).
- O `EventSystem` usa o **XR UI Input Module** e o Canvas usa o **Tracked Device Graphic Raycaster**.
- Interação com os controles via raio (XR Interaction Toolkit). Mãos (hand tracking) é opcional.

### Segurança do usuário (é uma aplicação terapêutica)
- Deve existir **sempre** uma forma fácil de **pausar e sair** da experiência (botão de menu do controle).
- A intensidade (turbulência, sons, clima) deve ser **configurável** e começar leve.
- Nada de sustos ou eventos súbitos que não tenham sido planejados com o professor/terapeuta.

---

## 7. Fluxo de trabalho com Git

- **Trabalho direto na `main`.** Sem branches de tarefa e sem Pull Request (ADR 0008).
  Uma branch só faz sentido para um experimento que talvez seja descartado.
- **Sempre `git pull` ANTES de começar qualquer tarefa** (o projeto pode ter sido mexido em outro PC).
- **Sempre `git push` no FINAL da tarefa**, como backup. O Claude **pede permissão
  antes de todo push** (ver seção 8).
- Commits em português, no padrão **Conventional Commits**:
  `feat: adiciona turbulência`, `fix: corrige velocidade negativa no pouso`,
  `docs: atualiza arquitetura`, `asset: importa modelo do terminal`, `chore: ...`.
- Commits pequenos e focados. Sempre revise com `git status` e `git diff` antes de commitar:
  confira se não entrou `.meta` órfão, cena alterada sem querer ou arquivo gigante fora do LFS.
- Arquivos binários (modelos, texturas, áudio) vão pelo **Git LFS** (configurado no `.gitattributes`).
- Detalhes em [Docs/CONTRIBUINDO.md](Docs/CONTRIBUINDO.md).

---

## 8. Instruções específicas para o Claude

- **No início de toda tarefa**: `git pull` (e `git status` para ver se há algo pendente). Se o pull der
  conflito ou houver mudanças locais não commitadas de outra pessoa, **pare e pergunte**.
- **No fim de toda tarefa**: commit na `main`, mostre o resumo (`git log`/`git diff --stat`) e
  **pergunte se pode dar `git push`. Nunca dê push sem permissão explícita** naquela conversa.
- **Antes de agir**, leia este arquivo e os docs relevantes (`Docs/ARQUITETURA.md`, a ADR relacionada).
- **Pergunte antes** de qualquer item da seção 2 (NUNCA fazer) e antes de apagar qualquer arquivo.
- Prefira **scripts de Editor** (`Assets/_Project/Scripts/Editor/`) a editar YAML do Unity à mão.
- Não rode o Unity em batchmode com o projeto aberto no Editor (dá conflito de lock).
- Mantenha as mudanças **mínimas e no escopo pedido**; não refatore o que não foi pedido.
- Ao terminar: atualize **CHANGELOG**, **ARQUITETURA/ADR/ROADMAP** (se aplicável) e diga
  exatamente o que foi feito, o que foi testado e o que **não** foi testado (ex.: "não testado no headset").
- Se algo falhar (compilação, build), relate o erro real e não esconda nada.
