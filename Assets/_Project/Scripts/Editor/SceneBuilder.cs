using System.Linq;
using System.Text;
using Aerofobia.Core;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Gera as cenas iniciais (placeholders) de cada cenário, já com o jogador XR,
    /// EventSystem para VR e um painel World Space com navegação entre cenas.
    /// Só cria o que <b>não existe</b>: nunca sobrescreve uma cena já editada pela equipe.
    /// </summary>
    public static class SceneBuilder
    {
        public const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/XR_Player.prefab";
        const string StarterRigName = "XR Origin (XR Rig)";

        internal static readonly Color PanelColor = new Color(0.08f, 0.12f, 0.2f, 0.92f);
        static readonly Color ButtonColor = new Color(0.2f, 0.45f, 0.85f, 1f);

        [MenuItem("Aerofobia/Criar Cenas que Faltam", priority = 20)]
        static void CreateMissingScenesMenu()
        {
            var report = new StringBuilder();
            CreateMissingScenes(report);
            ProjectSetup.ConfigureBuildScenes();
            EditorUtility.DisplayDialog("Aerofobia", report.ToString(), "OK");
        }

        /// <summary>Cria o prefab do jogador e as cenas que ainda não existem.</summary>
        public static void CreateMissingScenes(StringBuilder report)
        {
            if (TMP_Settings.defaultFontAsset == null)
            {
                report.AppendLine("• ERRO: TextMeshPro sem fonte padrão (TMP Settings). Rode 'Aerofobia > Importar Dependências' antes.");
                return;
            }

            var player = GetOrCreatePlayerPrefab(report);
            if (player == null)
            {
                return;
            }

            CreateScene(report, player, SceneNames.Menu, "Aerofobia VR",
                "Bem-vindo! Escolha um cenário.\nAponte com o controle e aperte o gatilho.", isMenu: true);
            CreateScene(report, player, SceneNames.Compra, "Cenário 1 — Compra da passagem",
                "Cenário em construção.", isMenu: false);
            CreateScene(report, player, SceneNames.Observacao, "Cenário 2 — Observação",
                "Cenário em construção: ver um avião decolar.", isMenu: false);
            CreateScene(report, player, SceneNames.Voo, "Cenário 3 — Voo",
                "Cenário em construção: decolagem e pouso dentro do avião.", isMenu: false);
        }

        /// <summary>
        /// Cria <c>XR_Player.prefab</c> como <b>Prefab Variant</b> do rig dos Starter Assets do XRI.
        /// Customizações do jogador vão na variante; o sample original não é editado.
        /// </summary>
        static GameObject GetOrCreatePlayerPrefab(StringBuilder report)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            var sourcePath = AssetDatabase.FindAssets($"\"{StarterRigName}\" t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.Contains("Starter Assets") && p.EndsWith($"{StarterRigName}.prefab"));
            if (sourcePath == null)
            {
                report.AppendLine("• ERRO: rig dos Starter Assets do XRI não encontrado. Rode 'Importar Dependências' antes.");
                return null;
            }

            ProjectSetup.EnsureFolder("Assets/_Project/Prefabs/Player");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
            instance.name = "XR_Player";
            var variant = PrefabUtility.SaveAsPrefabAsset(instance, PlayerPrefabPath);
            Object.DestroyImmediate(instance);
            report.AppendLine($"• Prefab do jogador criado: {PlayerPrefabPath} (variante de {sourcePath}).");
            return variant;
        }

        static void CreateScene(StringBuilder report, GameObject playerPrefab, string sceneName, string title, string description, bool isMenu)
        {
            var path = $"{ProjectSetup.ScenesFolder}/{sceneName}.unity";
            if (System.IO.File.Exists(path))
            {
                report.AppendLine($"• Cena já existe (não alterada): {path}");
                return;
            }

            ProjectSetup.EnsureFolder(ProjectSetup.ScenesFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEnvironment();
            PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
            var loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();

            var content = CreatePanel(title, description);
            if (isMenu)
            {
                AddSceneButton(content, loader, "1. Compra da passagem", SceneNames.Compra);
                AddSceneButton(content, loader, "2. Ver um avião decolar", SceneNames.Observacao);
                AddSceneButton(content, loader, "3. Voo", SceneNames.Voo);
                UnityEventTools.AddPersistentListener(CreateButton(content, "Sair").onClick, loader.QuitApplication);
            }
            else
            {
                if (sceneName != SceneNames.Scenarios.Last())
                {
                    UnityEventTools.AddPersistentListener(CreateButton(content, "Próximo cenário").onClick, loader.LoadNextScenario);
                }

                UnityEventTools.AddPersistentListener(CreateButton(content, "Voltar ao menu").onClick, loader.LoadMenu);
            }

            EditorSceneManager.SaveScene(scene, path);
            report.AppendLine($"• Cena criada: {path}");
        }

        static void CreateEnvironment()
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Chao";
            floor.transform.localScale = new Vector3(5f, 1f, 5f);
            GameObjectUtility.SetStaticEditorFlags(floor, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
        }

        /// <summary>Canvas World Space a 2 m do usuário, na altura dos olhos (sentado/em pé).</summary>
        static Transform CreatePanel(string title, string description)
        {
            var canvasGo = new GameObject("Canvas_Painel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasGo.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var canvasRect = (RectTransform)canvasGo.transform;
            canvasRect.sizeDelta = new Vector2(1000f, 800f);
            canvasRect.position = new Vector3(0f, 1.4f, 2f);
            canvasRect.localScale = Vector3.one * 0.001f;

            var panel = new GameObject("Painel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)panel.transform);
            panel.GetComponent<Image>().color = PanelColor;
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(60, 60, 50, 50);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(panel.transform, "Titulo", title, 64f, FontStyles.Bold, 90f);
            CreateText(panel.transform, "Descricao", description, 36f, FontStyles.Normal, 140f);
            return panel.transform;
        }

        static void AddSceneButton(Transform parent, SceneLoader loader, string label, string sceneName)
        {
            UnityEventTools.AddStringPersistentListener(CreateButton(parent, label).onClick, loader.LoadScene, sceneName);
        }

        internal static Button CreateButton(Transform parent, string label)
        {
            var go = new GameObject($"Botao_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = ButtonColor;
            go.GetComponent<LayoutElement>().preferredHeight = 100f;

            var text = CreateText(go.transform, "Texto", label, 40f, FontStyles.Bold, 0f);
            Stretch(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;
            return go.GetComponent<Button>();
        }

        internal static TextMeshProUGUI CreateText(Transform parent, string name, string value, float size, FontStyles style, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            if (height > 0f)
            {
                go.AddComponent<LayoutElement>().preferredHeight = height;
            }

            return text;
        }

        internal static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
