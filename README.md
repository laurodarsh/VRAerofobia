# Aerofobia VR

Aplicação de **realidade virtual para Meta Quest 2 e 3** que ajuda pessoas com **medo de voar (aerofobia)**
por meio de exposição gradual. Projeto do **Unity Lab**.

## A experiência

| # | Cenário | O que o usuário vive |
|---|---|---|
| 1 | **Compra da passagem** | Sentado a uma mesa, escolhe o destino e compra a passagem num computador |
| 2 | **Observação** | Sentado na sala de embarque, vê aviões decolando pelo vidro até a chamada do embarque |
| 3 | **Voo** | Na poltrona da janela, decola, voa e pousa; o médico controla chuva, noite e turbulência |

A experiência é toda **sentada**: o usuário só olha em volta com a cabeça.
Detalhes em [Docs/ESPECIFICACAO.md](Docs/ESPECIFICACAO.md).

## Tecnologia

- **Unity 6000.3.24f1** (Unity 6.3 LTS) + **URP**
- **OpenXR** (Meta Quest Support) + **XR Interaction Toolkit 3** + **Input System**
- Build **Android** (IL2CPP, ARM64, Vulkan) para Quest 2/3
- Por que essa stack: [Docs/decisoes/0002](Docs/decisoes/0002-stack-unity6-urp-openxr-xri.md)

## Começando

1. Instale o ambiente: **[Docs/SETUP.md](Docs/SETUP.md)**
2. Leia as regras do projeto: **[CLAUDE.md](CLAUDE.md)** (vale para humanos e para o Claude Code)
3. Veja como contribuir: **[Docs/CONTRIBUINDO.md](Docs/CONTRIBUINDO.md)**

### Ativar caminhos longos no Windows (uma vez por PC)

Os pacotes XR do Unity têm pastas muito profundas e passam do limite de 260 caracteres do Windows,
o que gera erros de "arquivo não encontrado". Rode uma vez, num **PowerShell como Administrador**, e reinicie o PC:

```powershell
New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name LongPathsEnabled -Value 1 -PropertyType DWORD -Force
```

E no Git (terminal normal):

```powershell
git config --global core.longpaths true
```

## ✅ Checklist de configuração pendente

Tarefas feitas uma única vez pelo responsável do repositório. Marque ao concluir.

- [x] **Cena inicial `00_Menu`** aplicada no `ProjectSettings` (fim do erro "SampleScene not found") (2026-09-23).
- [ ] **Ativar caminhos longos** em cada PC que usar o projeto (comandos acima).
- [x] **Testar no Quest 2 via APK**: menu, raio dos controles e navegação entre as cenas funcionando (2026-09-23).
- [ ] **Testar no Quest 2 pelo Play do Unity** (Quest Link ou Virtual Desktop), seguindo o [SETUP](Docs/SETUP.md).
- [ ] **Testar no Quest 3** do laboratório.

## Documentação

| Documento | Conteúdo |
|---|---|
| [CLAUDE.md](CLAUDE.md) | Regras, boas práticas, o que **nunca** fazer |
| [Docs/SETUP.md](Docs/SETUP.md) | Instalação, Quest em modo desenvolvedor, Link, build |
| [Docs/CONTRIBUINDO.md](Docs/CONTRIBUINDO.md) | Fluxo Git (direto na `main`), commits, checklist |
| [Docs/ARQUITETURA.md](Docs/ARQUITETURA.md) | Cenas, scripts, fluxo da aplicação |
| [Docs/ROADMAP.md](Docs/ROADMAP.md) | O que está feito e o que falta |
| [Docs/CHANGELOG.md](Docs/CHANGELOG.md) | Histórico de mudanças |
| [Docs/ASSETS.md](Docs/ASSETS.md) | Origem e licença de cada asset de terceiros |
| [Docs/decisoes/](Docs/decisoes/) | Registro de decisões técnicas (ADRs) |
| [Docs/ANALISE_PROJETO_ANTIGO.md](Docs/ANALISE_PROJETO_ANTIGO.md) | O que havia no projeto de 2023/2024 |

## Estrutura

```
Assets/_Project/   ← todo o conteúdo do projeto (cenas, scripts, prefabs, modelos...)
Assets/ThirdParty/ ← assets de terceiros (não editar)
Docs/              ← documentação
Packages/          ← pacotes do Unity (manifest.json)
ProjectSettings/   ← configurações do Unity (não editar à mão)
```

---
Desenvolvido por Unity Lab.
