# Como trabalhar no projeto

Fluxo Git do projeto. Desde 2026-10-26 só há um desenvolvedor, que trabalha direto na `main`
([ADR 0008](decisoes/0008-desenvolvedor-unico-direto-na-main.md)).
As regras completas estão no [CLAUDE.md](../CLAUDE.md).

## Primeira vez

Siga o [SETUP.md](SETUP.md): instalar Unity, Git LFS, clonar e configurar o merge de cenas.

## Dia a dia

```bash
# 1. Atualize (o projeto pode ter sido mexido em outro PC)
git checkout main
git pull

# 2. Trabalhe no Unity. Salve (Ctrl+S) cenas e o projeto (File > Save Project)

# 3. Revise o que mudou
git status
git diff

# 4. Commit (em português, Conventional Commits)
git add <arquivos>
git commit -m "feat: adiciona turbulência configurável no voo"

# 5. Envie (backup no GitHub)
git push
```

Para um experimento que talvez seja jogado fora, crie uma branch (`git checkout -b teste/<nome>`);
se der certo, faça merge na `main` e apague a branch.

## Tipos de commit

| Prefixo | Uso |
|---|---|
| `feat:` | Funcionalidade nova |
| `fix:` | Correção de bug |
| `docs:` | Só documentação |
| `asset:` | Importar ou otimizar modelos, texturas e sons |
| `chore:` | Configuração, limpeza, manutenção |
| `refactor:` | Reorganizar código sem mudar comportamento |

## Cenas

- Prefira criar ou editar **prefabs**. A cena só posiciona os prefabs.
- Cenas geradas por script (ex.: `03_Voo`, pelo `FlightSceneBuilder`) são alteradas no script e recriadas pelo menu.
- Para testes, use uma cena em `Assets/_Project/Scenes/Sandbox/`.

## Checklist antes do commit

- [ ] O projeto abre e compila sem erros no Console.
- [ ] Testei no Editor (Play) e, se mexi em VR, no headset (Link ou APK).
- [ ] Não há arquivos indesejados (`Library/`, `.csproj`, cenas alteradas sem querer).
- [ ] `.meta` de todo arquivo novo foi commitado junto.
- [ ] `Docs/CHANGELOG.md` atualizado.
- [ ] ADR criada se tomei uma decisão técnica.
- [ ] `Docs/ARQUITETURA.md` e `Docs/ROADMAP.md` atualizados, se aplicável.
