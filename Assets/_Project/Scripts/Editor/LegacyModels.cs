using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Prepara os modelos migrados do projeto antigo (ADR 0005): configura a importação,
    /// extrai os materiais (URP) para <c>_Project/Materials</c>, liga as texturas do avião,
    /// cria os prefabs e junta as ~1.900 peças do terminal em uma malha por material
    /// (de ~2.100 para ~40 draw calls).
    /// Como o <see cref="SceneBuilder"/>, só cria o que <b>não existe</b>: prefabs e malhas já
    /// gerados não são sobrescritos. Para gerar de novo, apague o asset gerado e rode outra vez.
    /// Batchmode: <c>-executeMethod Aerofobia.EditorTools.LegacyModels.BatchPrepare</c>.
    /// </summary>
    public static class LegacyModels
    {
        const string ModelsRoot = "Assets/_Project/Models";
        const string MaterialsRoot = "Assets/_Project/Materials";
        const string EnvironmentPrefabs = "Assets/_Project/Prefabs/Ambiente";

        public const string AviaoModelPath = ModelsRoot + "/Aviao/Aviao.fbx";
        public const string AviaoPrefabPath = "Assets/_Project/Prefabs/Aviao/Aviao.prefab";
        public const string TerminalPrefabPath = EnvironmentPrefabs + "/Terminal.prefab";
        const string TerminalModelPath = ModelsRoot + "/Aeroporto/aeroporto.obj";
        const string TerminalMeshesPath = ModelsRoot + "/Aeroporto/Terminal_Combinado.asset";
        const string PistaModelPath = ModelsRoot + "/Pista/AERODROME4.obj";

        /// <summary>
        /// O avião veio com ~78% do tamanho real (cabine com 1,96 m de largura; num Dash 8 Q400 ela tem ~2,5 m).
        /// Em VR a escala errada é percebida na hora, então corrigimos na importação.
        /// </summary>
        const float AviaoScale = 1.28f;

        /// <summary>
        /// Material do FBX (nome normalizado) → textura. Os demais materiais do avião são cor sólida.
        /// As texturas originais da cauda, do leme e do painel da cabine tinham marcas registradas
        /// (logotipo de companhia aérea e um pôster comercial) e não foram importadas: viraram cor sólida
        /// (ver <see cref="NewMaterialColors"/>). O logotipo da fuselagem foi apagado da textura.
        /// </summary>
        static readonly Dictionary<string, string> AviaoTextures = new Dictionary<string, string>
        {
            { "fuselarge", "Aviao_Fuselagem" },
        };

        /// <summary>
        /// Cor aplicada <b>só quando o material acaba de ser extraído</b> ("pasta/nome normalizado"),
        /// para nunca sobrescrever um ajuste feito depois pela equipe.
        /// </summary>
        static readonly Dictionary<string, Color> NewMaterialColors = new Dictionary<string, Color>
        {
            { "Aviao/black.001", new Color(0.05f, 0.05f, 0.05f) },          // cauda (sem o logotipo)
            { "Aviao/black.002", new Color(0.05f, 0.05f, 0.05f) },          // leme (sem o logotipo)
            { "Aviao/dashborasdabove", new Color(0.18f, 0.18f, 0.2f) },    // painel da cabine de comando
            { "Pista/material.022", new Color(0.3f, 0.42f, 0.22f) },       // grama (a textura original se perdeu)
        };

        [MenuItem("Aerofobia/Modelos/Preparar Modelos do Projeto Antigo", priority = 60)]
        static void PrepareMenu()
        {
            var report = new StringBuilder();
            Prepare(report);
            EditorUtility.DisplayDialog("Aerofobia", report.ToString(), "OK");
        }

        /// <summary>Ponto de entrada para batchmode. Escreve o relatório no log do Unity.</summary>
        public static void BatchPrepare()
        {
            var report = new StringBuilder();
            Prepare(report);
            Debug.Log("[Aerofobia.Modelos]\n" + report);
            EditorApplication.Exit(0);
        }

        /// <summary>Executa todas as etapas, na ordem. Cada etapa escreve o que fez em <paramref name="report"/>.</summary>
        public static void Prepare(StringBuilder report)
        {
            var predios = Enumerable.Range(1, 4).Select(i => $"{ModelsRoot}/Predios/predio{i}/predio{i}.obj").ToArray();

            // 1. Importação
            ConfigureModel(AviaoModelPath, AviaoScale, readable: false, lightmapUVs: false, report);
            ConfigureModel(TerminalModelPath, 1f, readable: true, lightmapUVs: false, report);
            ConfigureModel(PistaModelPath, 1f, readable: false, lightmapUVs: true, report);
            foreach (var predio in predios)
            {
                ConfigureModel(predio, 1f, readable: false, lightmapUVs: true, report);
            }
            ConfigureTextures(ModelsRoot, 1024);

            // 2. Materiais (URP) extraídos para poderem ser editados
            ExtractMaterials(AviaoModelPath, MaterialsRoot + "/Aviao", report);
            ExtractMaterials(TerminalModelPath, MaterialsRoot + "/Aeroporto", report);
            ExtractMaterials(PistaModelPath, MaterialsRoot + "/Pista", report);
            for (int i = 0; i < predios.Length; i++)
            {
                ExtractMaterials(predios[i], $"{MaterialsRoot}/Predios/predio{i + 1}", report);
            }
            AssignAviaoTextures(report);

            // 3. Prefabs
            CreateModelPrefab(AviaoModelPath, AviaoPrefabPath, isStatic: false, report);
            CreateModelPrefab(PistaModelPath, EnvironmentPrefabs + "/Pista.prefab", isStatic: true, report);
            for (int i = 0; i < predios.Length; i++)
            {
                CreateModelPrefab(predios[i], $"{EnvironmentPrefabs}/Predio{i + 1}.prefab", isStatic: true, report);
            }
            CreateCombinedTerminal(report);

            AssetDatabase.SaveAssets();
        }

        static void ConfigureModel(string path, float scale, bool readable, bool lightmapUVs, StringBuilder report)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                report.AppendLine($"• ERRO: modelo não encontrado: {path}");
                return;
            }

            importer.globalScale = scale;
            importer.isReadable = readable;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.generateSecondaryUV = lightmapUVs;
            // Os OBJ antigos não usam normal map: tangentes só ocupariam memória (e tamanho no Git, no terminal combinado).
            if (path.EndsWith(".obj"))
            {
                importer.importTangents = ModelImporterTangents.None;
            }
            importer.SaveAndReimport();
        }

        static void ConfigureTextures(string folder, int maxSize)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                if (importer.maxTextureSize != maxSize)
                {
                    importer.maxTextureSize = maxSize;
                    importer.SaveAndReimport();
                }
            }
        }

        /// <summary>
        /// Tira os materiais de dentro do modelo e salva como <c>.mat</c>. O importador do URP já os
        /// cria como URP/Lit com a cor original; extraídos, podem ser ajustados no Inspector.
        /// </summary>
        static void ExtractMaterials(string modelPath, string folder, StringBuilder report)
        {
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            EnsureFolder(folder);
            var remapped = new HashSet<string>(importer.GetExternalObjectMap().Keys
                .Where(k => k.type == typeof(Material)).Select(k => k.name));
            var extractedPaths = new List<string>();
            foreach (var material in AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Material>())
            {
                if (remapped.Contains(material.name))
                {
                    continue;
                }

                var target = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{SafeName(material.name)}.mat");
                var error = AssetDatabase.ExtractAsset(material, target);
                if (!string.IsNullOrEmpty(error))
                {
                    report.AppendLine($"• ERRO ao extrair {material.name} de {modelPath}: {error}");
                    continue;
                }
                extractedPaths.Add(target);
            }

            if (extractedPaths.Count > 0)
            {
                AssetDatabase.WriteImportSettingsIfDirty(modelPath);
                AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
            }

            var folderName = System.IO.Path.GetFileName(folder);
            foreach (var path in extractedPaths)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null && NewMaterialColors.TryGetValue($"{folderName}/{Normalize(material.name)}", out var color))
                {
                    material.mainTexture = null;
                    material.color = color;
                    EditorUtility.SetDirty(material);
                }
            }
            var shaders = importer.GetExternalObjectMap().Values.OfType<Material>()
                .Select(m => m.shader != null ? m.shader.name : "?").Distinct();
            report.AppendLine($"• {System.IO.Path.GetFileName(modelPath)}: {extractedPaths.Count} material(is) extraído(s), " +
                              $"{remapped.Count} já existiam. Shaders: {string.Join(", ", shaders)}.");
        }

        static void AssignAviaoTextures(StringBuilder report)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(AviaoModelPath);
            int assigned = 0;
            foreach (var pair in importer.GetExternalObjectMap())
            {
                var material = pair.Value as Material;
                if (material == null || material.mainTexture != null
                    || !AviaoTextures.TryGetValue(Normalize(pair.Key.name), out var textureName))
                {
                    continue;
                }

                var textureGuid = AssetDatabase.FindAssets($"{textureName} t:Texture2D", new[] { ModelsRoot + "/Aviao" }).FirstOrDefault();
                if (textureGuid == null)
                {
                    report.AppendLine($"• ERRO: textura {textureName} não encontrada.");
                    continue;
                }

                material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuid));
                material.color = Color.white; // no projeto antigo a cor era branca e a textura dava o tom
                EditorUtility.SetDirty(material);
                assigned++;
            }
            if (assigned > 0)
            {
                report.AppendLine($"• Avião: {assigned} textura(s) ligada(s).");
            }
        }

        /// <summary>Salva o modelo como Prefab Variant. Objetos parados ficam Static (batching, oclusão e luz baked).</summary>
        static void CreateModelPrefab(string modelPath, string prefabPath, bool isStatic, StringBuilder report)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                report.AppendLine($"• {prefabPath} já existe (mantido).");
                return;
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                return;
            }

            EnsureFolder(System.IO.Path.GetDirectoryName(prefabPath).Replace('\\', '/'));
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (isStatic)
            {
                SetStaticRecursive(instance);
            }
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            report.AppendLine($"• Prefab criado: {prefabPath} ({CountTriangles(model):N0} triângulos, {CountDrawCalls(model)} draw calls).");
        }

        /// <summary>
        /// O terminal veio do SketchUp com ~1.900 peças soltas (~2.100 draw calls, inviável no Quest).
        /// Junta tudo o que usa o mesmo material numa malha só, salva as malhas em um asset e
        /// monta o prefab <c>Terminal</c> com um filho por material.
        /// </summary>
        static void CreateCombinedTerminal(StringBuilder report)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TerminalPrefabPath) != null)
            {
                report.AppendLine($"• {TerminalPrefabPath} já existe (mantido).");
                return;
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(TerminalModelPath);
            if (model == null)
            {
                report.AppendLine($"• ERRO: {TerminalModelPath} não encontrado.");
                return;
            }

            var groups = new Dictionary<Material, List<CombineInstance>>();
            var toRoot = model.transform.worldToLocalMatrix;
            foreach (var renderer in model.GetComponentsInChildren<MeshRenderer>())
            {
                var filter = renderer.GetComponent<MeshFilter>();
                var mesh = filter != null ? filter.sharedMesh : null;
                var materials = renderer.sharedMaterials;
                if (mesh == null || materials.Length == 0)
                {
                    continue;
                }

                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    var material = materials[Mathf.Min(sub, materials.Length - 1)];
                    if (!groups.TryGetValue(material, out var list))
                    {
                        groups[material] = list = new List<CombineInstance>();
                    }
                    list.Add(new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = sub,
                        transform = toRoot * renderer.transform.localToWorldMatrix,
                    });
                }
            }

            var root = new GameObject("Terminal");
            bool firstMesh = true;
            int totalVertices = 0;
            foreach (var group in groups.OrderBy(g => g.Key.name))
            {
                var combined = new Mesh { name = "Terminal_" + SafeName(group.Key.name) };
                int vertices = group.Value.Sum(c => c.mesh.vertexCount);
                combined.indexFormat = vertices > 65535
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16;
                combined.CombineMeshes(group.Value.ToArray(), mergeSubMeshes: true, useMatrices: true);
                combined.RecalculateBounds();
                combined.Optimize();
                totalVertices += combined.vertexCount;

                if (firstMesh)
                {
                    AssetDatabase.CreateAsset(combined, TerminalMeshesPath);
                    firstMesh = false;
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(combined, TerminalMeshesPath);
                }

                var child = new GameObject(combined.name);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = combined;
                child.AddComponent<MeshRenderer>().sharedMaterial = group.Key;
            }
            AssetDatabase.SaveAssets();

            // Sem UV2 por enquanto (dobraria o asset). Quando formos fazer luz baked, gerar o UV2 e ligar ContributeGI.
            SetStaticRecursive(root, contributeGI: false);
            EnsureFolder(EnvironmentPrefabs);
            PrefabUtility.SaveAsPrefabAsset(root, TerminalPrefabPath);
            Object.DestroyImmediate(root);
            report.AppendLine($"• Terminal combinado: {CountDrawCalls(model)} → {groups.Count} draw calls " +
                              $"({CountTriangles(model):N0} triângulos, {totalVertices:N0} vértices). Prefab: {TerminalPrefabPath}");
        }

        static void SetStaticRecursive(GameObject root, bool contributeGI = true)
        {
            var flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic
                | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
            if (contributeGI)
            {
                flags |= StaticEditorFlags.ContributeGI;
            }
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
            }
        }

        static int CountTriangles(GameObject go)
        {
            long total = 0;
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                for (int i = 0; mesh != null && i < mesh.subMeshCount; i++)
                {
                    total += mesh.GetIndexCount(i) / 3;
                }
            }
            return (int)total;
        }

        static int CountDrawCalls(GameObject go) =>
            go.GetComponentsInChildren<Renderer>().Sum(r => r.sharedMaterials.Length);

        static string Normalize(string name) => name.ToLowerInvariant().Replace(" ", "").Replace("_", "");

        static string SafeName(string name)
        {
            var chars = name.Select(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' ? c : '_').ToArray();
            return new string(chars);
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }
            var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
