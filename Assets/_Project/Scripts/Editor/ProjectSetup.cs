using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aerofobia.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager.UI;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Configuração oficial do projeto para Meta Quest 2/3.
    /// É a <b>fonte da verdade</b> das configurações: para mudar uma configuração de
    /// Player, XR ou qualidade, mude aqui, rode o menu de novo e registre uma ADR.
    /// Rodar várias vezes é seguro, porque o script só corrige o que estiver diferente.
    /// </summary>
    /// <remarks>
    /// Também pode ser executado sem abrir o Editor (batchmode), em etapas, porque
    /// importar pacotes/samples exige recompilar antes da etapa seguinte:
    /// <c>-executeMethod Aerofobia.EditorTools.ProjectSetup.BatchImportDependencies</c>, depois
    /// <c>BatchConfigure</c> e por fim <c>BatchCreateScenes</c>.
    /// </remarks>
    public static class ProjectSetup
    {
        // ---------------------------------------------------------------
        // Valores oficiais do projeto (documentados na ADR 0002)
        // ---------------------------------------------------------------
        public const string CompanyName = "UnityLab";
        public const string ProductName = "Aerofobia VR";
        public const string AndroidPackageId = "com.unitylab.aerofobiavr";
        const AndroidSdkVersions MinAndroidSdk = AndroidSdkVersions.AndroidApiLevel32;

        public const string ScenesFolder = "Assets/_Project/Scenes";
        public const string SettingsFolder = "Assets/_Project/Settings";
        const string XRSettingsFolder = "Assets/XR";
        const string OpenXRLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";
        const string XriPackage = "com.unity.xr.interaction.toolkit";

        static readonly BuildTargetGroup[] XRTargets = { BuildTargetGroup.Android, BuildTargetGroup.Standalone };

        static readonly StringBuilder Report = new StringBuilder();

        // ---------------------------------------------------------------
        // Menus
        // ---------------------------------------------------------------

        [MenuItem("Aerofobia/Configurar Projeto para Quest", priority = 1)]
        public static void ConfigureAllMenu()
        {
            ConfigureAll();
            var report = Report.ToString();
            FlushReport("Configurar Projeto para Quest (menu)");
            EditorUtility.DisplayDialog("Aerofobia", "Configuração aplicada. Detalhes no Console.\n\n" + report, "OK");
        }

        [MenuItem("Aerofobia/Importar Dependências (samples XRI + TMP)", priority = 2)]
        public static void ImportDependenciesMenu()
        {
            ImportDependencies();
            EditorUtility.DisplayDialog("Aerofobia", Report.ToString(), "OK");
        }

        // ---------------------------------------------------------------
        // Pontos de entrada para batchmode (linha de comando)
        // ---------------------------------------------------------------

        /// <summary>
        /// Etapa 1 (batch): importa samples do XRI e recursos do TextMeshPro; organiza pastas do template.
        /// Rode <b>sem</b> <c>-quit</c>: a importação de .unitypackage é assíncrona, então este método
        /// fecha o Editor sozinho quando ela termina.
        /// </summary>
        public static void BatchImportDependencies()
        {
            var waitingForTmp = !IsTmpReady();
            if (waitingForTmp)
            {
                AssetDatabase.importPackageCompleted += name => FinishBatchImport($"Pacote importado: {name}", 0);
                AssetDatabase.importPackageFailed += (name, error) => FinishBatchImport($"ERRO ao importar {name}: {error}", 1);
            }

            ImportDependencies();
            OrganizeTemplateAssets();

            if (!waitingForTmp)
            {
                FinishBatchImport("TextMeshPro já estava pronto.", 0);
            }
        }

        static void FinishBatchImport(string message, int exitCode)
        {
            AssetDatabase.Refresh();
            Log(message);
            Log(IsTmpReady() ? "TextMeshPro: TMP Settings e fonte padrão OK." : "ERRO: TextMeshPro continua sem TMP Settings.");
            FlushReport("BatchImportDependencies");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(IsTmpReady() ? exitCode : 1);
            }
        }

        /// <summary>O TextMeshPro precisa do asset TMP Settings (com a fonte padrão) no projeto.</summary>
        public static bool IsTmpReady()
        {
            return AssetDatabase.FindAssets("t:TMP_Settings").Length > 0;
        }

        /// <summary>Etapa 2 (batch): aplica Player/XR/qualidade e cenas do build.</summary>
        public static void BatchConfigure()
        {
            ConfigureAll();
            FlushReport("BatchConfigure");
        }

        /// <summary>Etapa 3 (batch): cria o prefab do jogador e as cenas iniciais, se ainda não existirem.</summary>
        public static void BatchCreateScenes()
        {
            SceneBuilder.CreateMissingScenes(Report);
            ConfigureBuildScenes();
            FlushReport("BatchCreateScenes");
        }

        // ---------------------------------------------------------------
        // Configuração completa
        // ---------------------------------------------------------------

        /// <summary>Aplica todas as configurações oficiais do projeto.</summary>
        public static void ConfigureAll()
        {
            Report.Clear();
            ConfigureEditorSettings();
            ConfigurePlayer();
            ConfigureXR();
            ConfigureRenderPipeline();
            FixOpenXRValidation();
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Serialização em texto + meta files visíveis: obrigatório para Git.</summary>
        static void ConfigureEditorSettings()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            Log("Editor: serialização ForceText, meta files visíveis.");
        }

        /// <summary>Player Settings para Android/Quest.</summary>
        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, AndroidPackageId);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = MinAndroidSdk;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            // Cena aberta na 1ª vez que alguém abre o projeto (o template apontava para a SampleScene, já removida).
            // Não há API pública para isso, por isso o SerializedObject sobre o PlayerSettings.
            var playerSettings = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
            var defaultScene = playerSettings.FindProperty("templateDefaultScene");
            if (defaultScene == null)
            {
                Log("ERRO: propriedade 'templateDefaultScene' não encontrada no PlayerSettings; cena inicial não alterada.");
            }
            else
            {
                defaultScene.stringValue = $"{ScenesFolder}/{SceneNames.Menu}.unity";
                playerSettings.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }

            Log($"Player: {ProductName} ({AndroidPackageId}), IL2CPP, ARM64, Vulkan, Linear, minSdk {(int)MinAndroidSdk}, ASTC, cena inicial {SceneNames.Menu}.");
        }

        /// <summary>XR Plug-in Management: OpenXR no Android (Quest) e no Windows (Quest Link).</summary>
        static void ConfigureXR()
        {
            var perBuildTarget = GetOrCreateXRGeneralSettings();

            foreach (var group in XRTargets)
            {
                if (!perBuildTarget.HasSettingsForBuildTarget(group))
                {
                    perBuildTarget.CreateDefaultSettingsForBuildTarget(group);
                }

                if (!perBuildTarget.HasManagerSettingsForBuildTarget(group))
                {
                    perBuildTarget.CreateDefaultManagerSettingsForBuildTarget(group);
                }

                var generalSettings = perBuildTarget.SettingsForBuildTarget(group);
                generalSettings.InitManagerOnStart = true;

                var manager = perBuildTarget.ManagerSettingsForBuildTarget(group);
                if (!manager.activeLoaders.Any(l => l != null && l.GetType().FullName == OpenXRLoaderType))
                {
                    XRPackageMetadataStore.AssignLoader(manager, OpenXRLoaderType, group);
                }

                EditorUtility.SetDirty(generalSettings);
                EditorUtility.SetDirty(manager);
            }

            EditorUtility.SetDirty(perBuildTarget);
            AssetDatabase.SaveAssets();

            // Features do OpenXR
            foreach (var group in XRTargets)
            {
                FeatureHelpers.RefreshFeatures(group);
                var openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                if (openXR == null)
                {
                    Log($"ERRO: OpenXRSettings não encontrado para {group}.");
                    continue;
                }

                openXR.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
                EnableFeature<OculusTouchControllerProfile>(openXR, group);         // Quest 2
                EnableFeature<MetaQuestTouchPlusControllerProfile>(openXR, group);  // Quest 3
                if (group == BuildTargetGroup.Android)
                {
                    EnableFeature<MetaQuestFeature>(openXR, group);
                }

                EditorUtility.SetDirty(openXR);
            }

            AssetDatabase.SaveAssets();
            Log("XR: OpenXR (Android + Windows), Single Pass Instanced, Meta Quest Support, perfis Touch e Touch Plus.");
        }

        static void EnableFeature<T>(OpenXRSettings settings, BuildTargetGroup group) where T : OpenXRFeature
        {
            var feature = settings.GetFeature<T>();
            if (feature == null)
            {
                Log($"AVISO: feature {typeof(T).Name} não encontrada para {group}.");
                return;
            }

            feature.enabled = true;
            EditorUtility.SetDirty(feature);
        }

        static XRGeneralSettingsPerBuildTarget GetOrCreateXRGeneralSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget existing) && existing != null)
            {
                return existing;
            }

            var guids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
            if (guids.Length > 0)
            {
                existing = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(guids[0]));
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, existing, true);
                return existing;
            }

            EnsureFolder(XRSettingsFolder);
            var created = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(created, $"{XRSettingsFolder}/XRGeneralSettingsPerBuildTarget.asset");
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, created, true);
            return created;
        }

        /// <summary>
        /// Ajusta o asset URP "Mobile" (usado pelo Android/Quest) para VR:
        /// MSAA 4x (essencial contra serrilhado em VR), sem HDR, escala 1, sombras curtas.
        /// </summary>
        static void ConfigureRenderPipeline()
        {
            var mobilePath = $"{SettingsFolder}/Mobile_RPAsset.asset";
            var mobile = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(mobilePath);
            if (mobile == null)
            {
                Log($"AVISO: {mobilePath} não encontrado; URP não ajustado.");
                return;
            }

            mobile.msaaSampleCount = 4;
            mobile.supportsHDR = false;
            mobile.renderScale = 1f;
            mobile.shadowDistance = 30f;
            EditorUtility.SetDirty(mobile);
            Log("URP (Mobile/Quest): MSAA 4x, sem HDR, render scale 1, sombras até 30 m.");

            // SSAO é caro demais em VR (a cena é renderizada 2x). Só desativa: dá para religar no Inspector.
            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRendererData", new[] { SettingsFolder }))
            {
                var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var feature in renderer.rendererFeatures.Where(f => f != null && f.isActive && f.GetType().Name == "ScreenSpaceAmbientOcclusion"))
                {
                    feature.SetActive(false);
                    EditorUtility.SetDirty(feature);
                    EditorUtility.SetDirty(renderer);
                    Log($"URP: SSAO desativado em {renderer.name}.");
                }
            }
        }

        /// <summary>
        /// Roda a validação de projeto do OpenXR (a mesma do botão "Fix All" em
        /// Project Settings &gt; XR Plug-in Management &gt; Project Validation) e aplica as correções automáticas.
        /// </summary>
        static void FixOpenXRValidation()
        {
            var issues = new List<OpenXRFeature.ValidationRule>();
            foreach (var group in XRTargets)
            {
                for (var pass = 0; pass < 3; pass++)
                {
                    OpenXRProjectValidation.GetCurrentValidationIssues(issues, group);
                    var fixable = issues.Where(i => i.fixIt != null && i.fixItAutomatic).ToList();
                    if (fixable.Count == 0)
                    {
                        break;
                    }

                    foreach (var issue in fixable)
                    {
                        try
                        {
                            issue.fixIt();
                            Log($"Validação OpenXR ({group}) corrigida: {issue.message}");
                        }
                        catch (System.Exception e)
                        {
                            Log($"ERRO ao corrigir '{issue.message}': {e.Message}");
                        }
                    }
                }

                // As correções alteram o OpenXRSettings mas não o marcam como sujo: sem isso, não são salvas.
                EditorUtility.SetDirty(OpenXRSettings.GetSettingsForBuildTargetGroup(group));
                AssetDatabase.SaveAssets();

                OpenXRProjectValidation.GetCurrentValidationIssues(issues, group);
                foreach (var issue in issues)
                {
                    Log($"PENDENTE ({group}, {(issue.error ? "erro" : "aviso")}): {issue.message}");
                }
            }
        }

        /// <summary>Define as cenas do build na ordem de <see cref="SceneNames.All"/>.</summary>
        public static void ConfigureBuildScenes()
        {
            var scenes = SceneNames.All
                .Select(name => $"{ScenesFolder}/{name}.unity")
                .Where(File.Exists)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();

            if (scenes.Length == 0)
            {
                Log("Cenas do build: nenhuma cena criada ainda.");
                return;
            }

            EditorBuildSettings.scenes = scenes;
            Log($"Cenas do build: {string.Join(", ", scenes.Select(s => Path.GetFileNameWithoutExtension(s.path)))}.");
        }

        // ---------------------------------------------------------------
        // Dependências (samples) e organização do template
        // ---------------------------------------------------------------

        /// <summary>
        /// Importa os samples do XR Interaction Toolkit usados pelo projeto e os recursos
        /// essenciais do TextMeshPro. Não reimporta o que já existe.
        /// </summary>
        public static void ImportDependencies()
        {
            foreach (var sampleName in new[] { "Starter Assets", "XR Interaction Simulator" })
            {
                var sample = Sample.FindByPackage(XriPackage, string.Empty).FirstOrDefault(s => s.displayName == sampleName);
                if (string.IsNullOrEmpty(sample.displayName))
                {
                    Log($"ERRO: sample '{sampleName}' do XRI não encontrado.");
                    continue;
                }

                if (sample.isImported)
                {
                    Log($"Sample XRI '{sampleName}' já importado.");
                    continue;
                }

                sample.Import(Sample.ImportOptions.HideImportWindow);
                Log($"Sample XRI '{sampleName}' importado.");
            }

            if (!IsTmpReady())
            {
                // Assíncrono: a importação termina depois deste método retornar.
                TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
                Log("TextMeshPro: importação dos recursos essenciais iniciada.");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Move os assets úteis do template URP para dentro de <c>_Project</c> (mantendo GUIDs)
        /// e remove o conteúdo de exemplo do template.
        /// </summary>
        static void OrganizeTemplateAssets()
        {
            EnsureFolder(SettingsFolder);

            if (AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets/Settings" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        continue;
                    }

                    var target = $"{SettingsFolder}/{Path.GetFileName(path)}";
                    var error = AssetDatabase.MoveAsset(path, target);
                    Log(string.IsNullOrEmpty(error) ? $"Movido: {path} → {target}" : $"ERRO ao mover {path}: {error}");
                }
            }

            const string inputActions = "Assets/InputSystem_Actions.inputactions";
            if (File.Exists(inputActions))
            {
                AssetDatabase.MoveAsset(inputActions, $"{SettingsFolder}/InputSystem_Actions.inputactions");
                Log("Movido: InputSystem_Actions para _Project/Settings.");
            }

            // Conteúdo de exemplo do template (não usado pelo projeto)
            foreach (var junk in new[]
                     {
                         "Assets/TutorialInfo", "Assets/Readme.asset", "Assets/Scenes/SampleScene.unity",
                         $"{SettingsFolder}/SampleSceneProfile.asset",
                     })
            {
                if (AssetDatabase.DeleteAsset(junk))
                {
                    Log($"Removido do template: {junk}");
                }
            }

            foreach (var emptyFolder in new[] { "Assets/Settings", "Assets/Scenes" })
            {
                if (AssetDatabase.IsValidFolder(emptyFolder) && AssetDatabase.FindAssets(string.Empty, new[] { emptyFolder }).Length == 0)
                {
                    AssetDatabase.DeleteAsset(emptyFolder);
                }
            }

            AssetDatabase.SaveAssets();
        }

        // ---------------------------------------------------------------
        // Utilitários
        // ---------------------------------------------------------------

        /// <summary>Cria a pasta (e as pastas-pai) no AssetDatabase, se não existir.</summary>
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        static void Log(string line)
        {
            Report.AppendLine("• " + line);
        }

        static void FlushReport(string step)
        {
            Debug.Log($"[Aerofobia.ProjectSetup] {step}\n{Report}");
            Report.Clear();
        }
    }
}
