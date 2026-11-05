# Setup do ambiente

Passo a passo para quem vai trabalhar no projeto pela primeira vez.

## 1. Instalar as ferramentas

| Ferramenta | Versão | Onde |
|---|---|---|
| **Unity Hub** | mais recente | https://unity.com/download |
| **Unity Editor** | **6000.3.24f1** (exatamente essa) | Unity Hub → Installs → Install Editor → Archive, ou `unityhub://6000.3.24f1` |
| Módulos do Editor | **Android Build Support** + OpenJDK + Android SDK & NDK Tools | Marcar na instalação |
| **Git** | 2.40+ | https://git-scm.com |
| **Git LFS** | 3.x | Já vem com o Git for Windows. Rode `git lfs install` uma vez |
| **GitHub CLI** (opcional) | — | https://cli.github.com |
| **Meta Quest Link** (PC) | mais recente | https://www.meta.com/help/quest/articles/headsets-and-accessories/oculus-rift-s/install-app-for-link/ |
| IDE | Visual Studio 2022 (workload "Game development with Unity") ou VS Code / Rider | — |

> ⚠️ **Não abra o projeto com outra versão do Unity.** Se o Hub oferecer upgrade, cancele.

> ⚠️ **Windows:** ative os caminhos longos antes de abrir o projeto (comandos no [README](../README.md#ativar-caminhos-longos-no-windows-uma-vez-por-pc)).

## 2. Clonar o repositório

O repositório é **privado**: você precisa ter sido convidado como colaborador.

### Chave SSH (uma vez por PC)

```powershell
ssh-keygen -t ed25519 -C "seu-email@exemplo.com"      # aperte Enter em tudo
Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub | Set-Clipboard
```

Cole a chave em https://github.com/settings/ssh/new e teste com `ssh -T git@github.com`
(a resposta deve ser "Hi seu-usuario! You've successfully authenticated").
Nunca compartilhe o arquivo `id_ed25519` (sem `.pub`): ele é a sua chave privada.

### Clonar

```bash
git lfs install
git clone git@github.com:daniel-ramalho/aerofobia-vr.git
cd aerofobia-vr
git config user.name "Seu Nome"
git config user.email "seu-email@exemplo.com"
```

Página do repositório: https://github.com/daniel-ramalho/aerofobia-vr

A primeira abertura no Unity demora (ele gera a pasta `Library/`, que não vai para o Git).

## 3. Configurar o merge de cenas (UnityYAMLMerge) — uma vez por máquina

O `.gitattributes` já marca cenas e prefabs com `merge=unityyamlmerge`. Falta dizer ao Git
onde está a ferramenta (ajuste o caminho se o Unity estiver em outro lugar):

```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

## 4. Preparar o Quest (2 ou 3)

1. No celular, app **Meta Horizon** → Dispositivos → seu headset → **Modo de desenvolvedor: ligado**.
   (Exige uma conta de desenvolvedor Meta, que é gratuita: https://developers.meta.com/horizon/)
2. Ligue o headset no PC com cabo USB-C e aceite **"Permitir depuração USB"** dentro do headset.
3. No Unity: `File > Build Profiles > Android`. O Quest deve aparecer em **Run Device**.

### Testar sem gerar APK (Play Mode no PC)
1. Abra o app **Meta Quest Link** no PC e conecte o headset (cabo ou Air Link).
2. No app Link: `Settings > General > OpenXR Runtime → Set Meta Quest Link as active`.
3. No Unity, aperte **Play**: a cena roda no headset.

### Gerar APK e instalar
`File > Build Profiles > Android > Build And Run` com o Quest conectado.

### Testar sem headset
`Edit > Project Settings > XR Plug-in Management > XR Interaction Toolkit` →
marque **Use XR Interaction Simulator in scenes**. No Play Mode aparece um simulador
controlado por teclado e mouse. Isso é uma preferência local: não commite essa mudança se ela alterar arquivos em `Assets/XRI/`.

## 5. Configuração do projeto (já feita — só referência)

O projeto é configurado por scripts de Editor, no menu **Aerofobia**:

| Ordem | Menu | O que faz |
|---|---|---|
| 1 | Aerofobia > Importar Dependências | Samples do XRI (Starter Assets, Simulator) e recursos do TextMeshPro |
| 2 | Aerofobia > Configurar Projeto para Quest | Player Settings, XR (OpenXR + Meta Quest), URP para VR, validação OpenXR, cenas do build |
| 3 | Aerofobia > Criar Cenas que Faltam | Prefab `XR_Player` e cenas placeholder (nunca sobrescreve) |
| — | Aerofobia > Gerar APK para Quest | Build de desenvolvimento em `Builds/AerofobiaVR.apk` |

Rodar de novo é seguro: cada passo só corrige ou cria o que falta.
Se mudar alguma configuração, **mude no script** (`Assets/_Project/Scripts/Editor/ProjectSetup.cs`) e documente em uma ADR.

Pela linha de comando (sem abrir o Editor):

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe"
& $unity -batchmode -projectPath . -buildTarget Android -executeMethod Aerofobia.EditorTools.ProjectSetup.BatchImportDependencies -logFile setup1.log   # sem -quit (fecha sozinho)
& $unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod Aerofobia.EditorTools.ProjectSetup.BatchConfigure -logFile setup2.log
& $unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod Aerofobia.EditorTools.ProjectSetup.BatchCreateScenes -logFile setup3.log
& $unity -batchmode -projectPath . -buildTarget Android -executeMethod Aerofobia.EditorTools.BuildTools.BatchBuildQuest -logFile build.log
```

## Problemas comuns

| Problema | Solução |
|---|---|
| Quest não aparece no Run Device | Aceite a depuração USB no headset; troque o cabo; `adb devices` |
| Play não abre no headset | Meta Quest Link precisa ser o OpenXR Runtime ativo (passo 4) |
| Texturas cor-de-rosa | Material não convertido para URP: `Window > Rendering > Render Pipeline Converter` |
| Arquivos grandes aparecem como texto pequeno ("version https://git-lfs...") | Rode `git lfs install` e depois `git lfs pull` |
