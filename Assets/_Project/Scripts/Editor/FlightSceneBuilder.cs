using System.Linq;
using System.Reflection;
using System.Text;
using Aerofobia.Core;
using Aerofobia.Flight;
using Aerofobia.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Monta a cena <c>03_Voo</c> do zero: aeroporto, cidade, nuvens, céu, o avião com o paciente na
    /// poltrona, o voo em circuito, o clima, os comandos do médico, a pausa e o painel de fim de voo
    /// (ESPECIFICACAO, seção 3.3; ADR 0006).
    /// </summary>
    /// <remarks>
    /// Recria a cena inteira a cada execução (pede confirmação no menu). Os ajustes finos devem ir para os
    /// componentes no Inspector <b>ou para este script</b>; ajuste feito só na cena se perde se ela for
    /// recriada. Materiais gerados em <c>Materials/Voo</c> só são criados se não existirem.
    /// Batchmode: <c>-executeMethod Aerofobia.EditorTools.FlightSceneBuilder.BatchBuild</c>.
    /// </remarks>
    public static class FlightSceneBuilder
    {
        const string ScenePath = ProjectSetup.ScenesFolder + "/" + SceneNames.Voo + ".unity";
        const string MaterialsFolder = "Assets/_Project/Materials/Voo";
        const string DoctorActionsPath = "Assets/_Project/Settings/Input/ControlesMedico.inputactions";
        const string EnvironmentPrefabs = "Assets/_Project/Prefabs/Ambiente";
        const string GrassMaterialPath = "Assets/_Project/Materials/Pista/Material.022.mat";

        // Pista do modelo AERODROME4, medida em 2026-09-26: eixo em z = -187,46, cabeceira oeste em
        // x = -222,95, asfalto em y ≈ 0,21, 460 m de comprimento no sentido +X e 32 m de largura.
        static readonly Vector3 RunwayThreshold = new Vector3(-222.95f, 0.22f, -187.46f);
        static readonly Quaternion RunwayRotation = Quaternion.Euler(0f, 90f, 0f);
        const float RunwayLength = 460f;

        // O terminal foi importado 7,7× maior que o real; o projeto antigo usava 0,13 (prédio térreo de ~110 m).
        const float TerminalScale = 0.13f;

        // Poltronas na janela esquerda, no espaço do modelo do avião (raiz do prefab na origem, nariz em +Z).
        // Medidas nas fotos de 2026-09-26: poltronas de z -5,6 a 8,6; asa de -1,3 a 1,3; hélice em 3,4.
        const float SeatX = -0.75f;
        static readonly (string name, float z)[] Seats =
        {
            ("Poltrona_AtrasDaAsa", -3.5f),     // padrão: vê o chão, a hélice fica fora da vista
            ("Poltrona_SobAAsa", 0f),           // vê o trem de pouso e o motor
            ("Poltrona_FrenteDaHelice", 6f),    // vê a hélice de perto
        };

        [MenuItem("Aerofobia/Cenas/Montar Cena do Voo (03_Voo)", priority = 30)]
        static void BuildMenu()
        {
            if (!EditorUtility.DisplayDialog("Aerofobia",
                    $"Isto recria a cena {SceneNames.Voo} do zero. Ajustes feitos à mão na cena serão perdidos.\n\n" +
                    "Avise a equipe antes: só uma pessoa edita uma cena por vez.", "Recriar", "Cancelar"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var report = new StringBuilder();
            Build(report);
            EditorUtility.DisplayDialog("Aerofobia", report.ToString(), "OK");
        }

        /// <summary>Ponto de entrada para batchmode. Escreve o relatório no log do Unity.</summary>
        public static void BatchBuild()
        {
            var report = new StringBuilder();
            Build(report);
            Debug.Log("[Aerofobia.Voo]\n" + report);
            EditorApplication.Exit(report.ToString().Contains("ERRO") ? 1 : 0);
        }

        /// <summary>Monta e salva a cena do voo. Cada etapa escreve o que fez em <paramref name="report"/>.</summary>
        public static void Build(StringBuilder report)
        {
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SceneBuilder.PlayerPrefabPath);
            var aviaoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LegacyModels.AviaoPrefabPath);
            if (playerPrefab == null || aviaoPrefab == null || !System.IO.File.Exists(DoctorActionsPath) || TMP_Settings.defaultFontAsset == null)
            {
                report.AppendLine("• ERRO: faltam XR_Player, Aviao, ControlesMedico ou a fonte do TextMeshPro.");
                return;
            }

            PlayerRigSetup.ConfigureSeated(report);
            var materials = CreateMaterials(report);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ambiente e luz
            var sun = CreateSun();
            ConfigureRenderSettings(materials.Sky);
            CreateAirportAndCity(report);

            // Voo
            var circuit = new GameObject("Circuito").AddComponent<FlightCircuit>();
            circuit.transform.SetPositionAndRotation(RunwayThreshold, RunwayRotation);

            var runwayLights = CreateRunwayLights(materials.RunwayLight);
            CreateClouds(circuit, materials.Cloud);

            var aircraft = CreateAircraft(aviaoPrefab, playerPrefab, circuit, materials.RainDrop, out var cabinLight,
                out var rain, out var turbulence, out var seatPanelsParent);

            // Clima
            var climate = new GameObject("Clima");
            var environment = climate.AddComponent<EnvironmentLighting>();
            SetReference(environment, "_sun", sun);
            SetReference(environment, "_skyboxTemplate", materials.Sky);
            var nightLights = climate.AddComponent<NightLights>();
            SetArray(nightLights, "_cabinLights", new Object[] { cabinLight });
            SetArray(nightLights, "_lightRenderers", new Object[] { runwayLights });
            var weather = climate.AddComponent<WeatherController>();
            SetReference(weather, "_environment", environment);
            SetReference(weather, "_rain", rain);
            SetReference(weather, "_nightLights", nightLights);
            SetReference(weather, "_turbulence", turbulence);

            // Sistema: UI em VR, troca de cena, pausa, fim de voo e comandos do médico
            new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
            var loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
            var pause = CreatePausePanel(seatPanelsParent, loader);
            var completedPanel = CreateCompletedPanel(seatPanelsParent, loader);
            UnityEventTools.AddBoolPersistentListener(GetEvent(aircraft, "_flightCompleted"), completedPanel.SetActive, true);

            var doctor = new GameObject("Medico").AddComponent<DoctorInput>();
            // Carregado só agora: o .inputactions é importado por ScriptedImporter e pode ser reimportado durante a
            // montagem (prefab e materiais salvos acima). Uma referência carregada antes ficava inválida e a cena
            // era salva com o campo vazio ("Input Action Asset não definido" no Play).
            SetReference(doctor, "_actions", AssetDatabase.LoadAssetAtPath<InputActionAsset>(DoctorActionsPath));
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_takeoffOrLanding"), aircraft.RequestTakeoffOrLanding);
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_rainToggled"), weather.ToggleRain);
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_nightToggled"), weather.ToggleNight);
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_turbulenceToggled"), weather.ToggleTurbulence);
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_normalRequested"), weather.ResetToNormal);
            UnityEventTools.AddPersistentListener(GetEvent(doctor, "_pauseToggled"), pause.TogglePause);
            EditorUtility.SetDirty(doctor);
            EditorUtility.SetDirty(aircraft);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            report.AppendLine($"• Cena salva: {ScenePath}");

            // Referências a assets que a cena precisa para funcionar: se alguma ficou vazia, avisar em vez de seguir calado.
            RequireReference(report, doctor, "_actions");
            RequireReference(report, environment, "_skyboxTemplate");
            RequireReference(report, aircraft, "_circuit");
        }

        static void RequireReference(StringBuilder report, Object target, string field)
        {
            if (new SerializedObject(target).FindProperty(field).objectReferenceValue == null)
            {
                report.AppendLine($"• ERRO: {target.GetType().Name}.{field} ficou vazio na cena salva.");
            }
        }

        // ------------------------------------------------------------------ materiais

        struct FlightMaterials
        {
            public Material Sky;
            public Material Cloud;
            public Material RunwayLight;
            public Material RainDrop;
        }

        static FlightMaterials CreateMaterials(StringBuilder report)
        {
            ProjectSetup.EnsureFolder(MaterialsFolder);
            return new FlightMaterials
            {
                Sky = GetOrCreateMaterial(report, "Ceu", "Skybox/Procedural", m =>
                {
                    m.SetFloat("_SunSize", 0.04f);
                    m.SetFloat("_AtmosphereThickness", 1f);
                    m.SetFloat("_Exposure", 1.2f);
                }),
                Cloud = GetOrCreateMaterial(report, "Nuvem", "Universal Render Pipeline/Lit", m =>
                {
                    m.SetColor("_BaseColor", new Color(0.97f, 0.97f, 0.98f));
                    m.SetFloat("_Smoothness", 0f);
                    m.SetFloat("_SpecularHighlights", 0f);
                    m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    m.SetFloat("_EnvironmentReflections", 0f);
                    m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                }),
                RunwayLight = GetOrCreateMaterial(report, "LuzPista", "Universal Render Pipeline/Unlit", m =>
                {
                    m.SetColor("_BaseColor", new Color(0.35f, 0.35f, 0.3f));
                }),
                RainDrop = GetOrCreateMaterial(report, "GotaChuva", "Universal Render Pipeline/Particles/Unlit", m =>
                {
                    // Transparente com mistura alfa (o mesmo que escolher no Inspector do URP).
                    m.SetFloat("_Surface", 1f);
                    m.SetFloat("_Blend", 0f);
                    m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                    m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                    m.SetFloat("_ZWrite", 0f);
                    m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    m.renderQueue = (int)RenderQueue.Transparent;
                    m.SetColor("_BaseColor", new Color(0.82f, 0.87f, 0.92f, 0.45f));
                }),
            };
        }

        static Material GetOrCreateMaterial(StringBuilder report, string name, string shaderName, System.Action<Material> setup)
        {
            var path = $"{MaterialsFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                report.AppendLine($"• ERRO: shader {shaderName} não encontrado.");
                return null;
            }

            var material = new Material(shader) { name = name };
            setup(material);
            AssetDatabase.CreateAsset(material, path);
            report.AppendLine($"• Material criado: {path}");
            return material;
        }

        // ------------------------------------------------------------------ ambiente

        static Light CreateSun()
        {
            var sun = new GameObject("Sol").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            // Sem sombras: o interior da cabine ficaria escuro e o custo no Quest é alto (CLAUDE.md, seção 6).
            sun.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            return sun;
        }

        static void ConfigureRenderSettings(Material sky)
        {
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.72f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.72f, 0.8f, 0.88f);
            RenderSettings.fogStartDistance = 1350f;
            RenderSettings.fogEndDistance = 9000f;
        }

        static void CreateAirportAndCity(StringBuilder report)
        {
            var root = new GameObject("Ambiente").transform;

            SpawnPrefab($"{EnvironmentPrefabs}/Pista.prefab", root, Vector3.zero, Quaternion.identity, 1f);
            SpawnPrefab($"{EnvironmentPrefabs}/Terminal.prefab", root, new Vector3(-40f, 0f, -35f), Quaternion.Euler(0f, 180f, 0f), TerminalScale);

            // Chão até o horizonte, com o mesmo material da grama da pista (onde se sobrepõem, não se nota).
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Chao";
            Object.DestroyImmediate(ground.GetComponent<Collider>());
            ground.transform.SetParent(root, false);
            ground.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            ground.transform.localScale = new Vector3(3000f, 1f, 3000f); // 30 km
            var grass = AssetDatabase.LoadAssetAtPath<Material>(GrassMaterialPath);
            if (grass != null)
            {
                ground.GetComponent<MeshRenderer>().sharedMaterial = grass;
            }
            else
            {
                report.AppendLine($"• Aviso: {GrassMaterialPath} não encontrado; o chão ficou com o material padrão.");
            }
            ground.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.BatchingStatic);

            // Cidade ao norte, do lado de dentro do circuito (visível da janela esquerda nas curvas).
            var city = new GameObject("Cidade").transform;
            city.SetParent(root, false);
            var random = new System.Random(3);
            int count = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    if (random.NextDouble() < 0.2)
                    {
                        continue;
                    }

                    var position = new Vector3(-250f + col * 95f + (float)random.NextDouble() * 30f, 0f,
                        650f + row * 95f + (float)random.NextDouble() * 30f);
                    var rotation = Quaternion.Euler(0f, random.Next(4) * 90f, 0f);
                    SpawnPrefab($"{EnvironmentPrefabs}/Predio{random.Next(1, 5)}.prefab", city, position, rotation, 1f);
                    count++;
                }
            }
            report.AppendLine($"• Ambiente: pista, terminal (escala {TerminalScale}), chão de 30 km e {count} prédios.");
        }

        static GameObject SpawnPrefab(string path, Transform parent, Vector3 position, Quaternion rotation, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation * instance.transform.localRotation;
            instance.transform.localScale *= scale;
            return instance;
        }

        static MeshRenderer CreateRunwayLights(Material material)
        {
            var go = new GameObject("Luzes_Pista", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetPositionAndRotation(RunwayThreshold, RunwayRotation);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var lights = go.AddComponent<RunwayLights>();
            SetFloat(lights, "_length", RunwayLength);
            return renderer;
        }

        static void CreateClouds(FlightCircuit circuit, Material material)
        {
            var go = new GameObject("Nuvens", typeof(MeshFilter), typeof(MeshRenderer));
            // Centro do campo de nuvens no meio do circuito.
            go.transform.position = new Vector3(RunwayThreshold.x + 230f, 0f, RunwayThreshold.z + 1200f);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var clouds = go.AddComponent<CloudField>();
            SetReference(clouds, "_circuit", circuit);
        }

        // ------------------------------------------------------------------ avião e paciente

        static FlightController CreateAircraft(GameObject aviaoPrefab, GameObject playerPrefab, FlightCircuit circuit,
            Material rainMaterial, out Light cabinLight, out RainEffect rain, out TurbulenceEffect turbulence, out Transform panelsParent)
        {
            var root = new GameObject("Aviao_Voo");
            var flight = root.AddComponent<FlightController>();
            SetReference(flight, "_circuit", circuit);

            var body = new GameObject("Corpo").transform;
            body.SetParent(root.transform, false);
            turbulence = body.gameObject.AddComponent<TurbulenceEffect>();

            // Modelo com a rotação/escala do prefab; depois sobe até as rodas tocarem y = 0 da raiz.
            var model = (GameObject)PrefabUtility.InstantiatePrefab(aviaoPrefab, body);
            model.transform.localPosition = Vector3.zero;
            var renderers = model.GetComponentsInChildren<Renderer>();
            float groundOffset = -renderers.Min(r => body.InverseTransformPoint(r.bounds.min).y);
            model.transform.localPosition = new Vector3(0f, groundOffset, 0f);
            var chairs = renderers.FirstOrDefault(r => r.name == "cabin chairs");
            float floorY = chairs != null ? body.InverseTransformPoint(chairs.bounds.min).y : groundOffset - 0.81f;

            // Poltronas (pontos no piso, de frente para o nariz).
            var seatsRoot = new GameObject("Poltronas").transform;
            seatsRoot.SetParent(body, false);
            var seatTransforms = Seats.Select(seat =>
            {
                var t = new GameObject(seat.name).transform;
                t.SetParent(seatsRoot, false);
                t.localPosition = new Vector3(SeatX, floorY, seat.z);
                return t;
            }).ToArray();
            var seatDefault = seatTransforms[0];

            // Paciente
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, seatDefault);
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            var camera = player.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                // Override só nesta cena: enxergar o chão lá de cima. Perto da cabine, 5 cm basta.
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 12000f;
            }

            // Luz da cabine sobre a poltrona (sem sombras).
            cabinLight = new GameObject("Luz_Cabine").AddComponent<Light>();
            cabinLight.type = LightType.Point;
            cabinLight.range = 5f;
            cabinLight.color = new Color(1f, 0.93f, 0.82f);
            cabinLight.intensity = 0.4f;
            cabinLight.shadows = LightShadows.None;
            cabinLight.transform.SetParent(seatDefault, false);
            cabinLight.transform.localPosition = new Vector3(0.6f, 1.6f, 0f);

            // Chuva na janela
            rain = CreateWindowRain(seatDefault, rainMaterial);

            // Painéis (pausa e fim de voo) ficam à frente da poltrona.
            panelsParent = new GameObject("Paineis").transform;
            panelsParent.SetParent(seatDefault, false);

            var selector = seatsRoot.gameObject.AddComponent<SeatSelector>();
            SetArray(selector, "_seats", seatTransforms.Cast<Object>().ToArray());
            SetArray(selector, "_passengerObjects", new Object[] { player.transform, cabinLight.transform, rain.transform, panelsParent });

            // Áudio (clipes ainda não importados)
            var oneShots = root.AddComponent<AudioSource>();
            oneShots.playOnAwake = false;
            var cabinLoop = new GameObject("Som_Cabine").AddComponent<AudioSource>();
            cabinLoop.transform.SetParent(root.transform, false);
            cabinLoop.playOnAwake = false;
            cabinLoop.loop = true;
            var audio = root.AddComponent<FlightAudio>();
            SetReference(audio, "_flight", flight);
            SetReference(audio, "_oneShots", oneShots);
            SetReference(audio, "_cabinLoop", cabinLoop);

            // Pose inicial na cabeceira (o FlightController confirma no Start).
            root.transform.SetPositionAndRotation(RunwayThreshold + RunwayRotation * new Vector3(0f, 0f, 18f), RunwayRotation);
            return flight;
        }

        static RainEffect CreateWindowRain(Transform seat, Material material)
        {
            var go = new GameObject("Chuva_Janela");
            go.transform.SetParent(seat, false);
            // Do lado de fora da janela (a fuselagem tem ~1,25 m de raio; a poltrona fica em x = -0,75).
            go.transform.localPosition = new Vector3(-0.55f, PlayerRigSetup.SeatedEyeHeight - 0.05f, 0.3f);

            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = 0.9f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.006f, 0.014f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 400;

            var emission = particles.emission;
            emission.rateOverTime = 0f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.01f, 0.7f, 1.2f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f); // os três eixos precisam do mesmo modo de curva
            velocity.y = new ParticleSystem.MinMaxCurve(-0.35f, -0.15f);
            velocity.z = new ParticleSystem.MinMaxCurve(-1.3f, -0.8f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.08f;
            renderer.lengthScale = 2f;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = true;
            audio.spatialBlend = 0.6f;

            var rain = go.AddComponent<RainEffect>();
            SetReference(rain, "_drops", particles);
            SetReference(rain, "_audio", audio);
            return rain;
        }

        // ------------------------------------------------------------------ painéis

        static PauseMenu CreatePausePanel(Transform parent, SceneLoader loader)
        {
            var pause = new GameObject("Pausa").AddComponent<PauseMenu>();
            var (canvas, content) = CreateSeatPanel(parent, "Painel_Pausa", 640f, "Pausado",
                "O voo está parado. Quando quiser, é só continuar.");
            UnityEventTools.AddPersistentListener(SceneBuilder.CreateButton(content, "Continuar").onClick, pause.Resume);
            UnityEventTools.AddPersistentListener(SceneBuilder.CreateButton(content, "Voltar ao menu").onClick, pause.ReturnToMenu);
            UnityEventTools.AddPersistentListener(SceneBuilder.CreateButton(content, "Sair").onClick, pause.Quit);
            SetReference(pause, "_panel", canvas);
            SetReference(pause, "_sceneLoader", loader);
            canvas.SetActive(false);
            return pause;
        }

        static GameObject CreateCompletedPanel(Transform parent, SceneLoader loader)
        {
            var (canvas, content) = CreateSeatPanel(parent, "Painel_VooConcluido", 440f, "Voo concluído",
                "Você chegou! Parabéns por mais esta etapa.");
            UnityEventTools.AddPersistentListener(SceneBuilder.CreateButton(content, "Voltar ao menu").onClick, loader.LoadMenu);
            canvas.SetActive(false);
            return canvas;
        }

        /// <summary>
        /// Painel World Space 1,5 m à frente da poltrona, na altura dos olhos (CLAUDE.md: UI a 1,5–3 m).
        /// Desenhado por cima de tudo (<see cref="UiAlwaysOnTop"/>), senão os encostos e os bagageiros o cobririam.
        /// </summary>
        static (GameObject canvas, Transform content) CreateSeatPanel(Transform parent, string name, float height, string title, string description)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasGo.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var rect = (RectTransform)canvasGo.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(800f, height);
            rect.localScale = Vector3.one * 0.001f;
            rect.localPosition = new Vector3(0f, PlayerRigSetup.SeatedEyeHeight, 1.5f);
            canvasGo.AddComponent<UiAlwaysOnTop>();

            var panel = new GameObject("Painel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(canvasGo.transform, false);
            SceneBuilder.Stretch((RectTransform)panel.transform);
            panel.GetComponent<Image>().color = SceneBuilder.PanelColor;
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 36, 36);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            SceneBuilder.CreateText(panel.transform, "Titulo", title, 52f, FontStyles.Bold, 70f);
            SceneBuilder.CreateText(panel.transform, "Descricao", description, 32f, FontStyles.Normal, 90f);
            return (canvasGo, panel.transform);
        }

        // ------------------------------------------------------------------ campos privados serializados

        static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(Object target, string field, Object[] values)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Pega um UnityEvent privado (campo serializado) para ligar um listener persistente.</summary>
        static UnityEvent GetEvent(Object target, string field)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            return (UnityEvent)info.GetValue(target);
        }
    }
}
