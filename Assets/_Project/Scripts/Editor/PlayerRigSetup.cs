using System.Linq;
using System.Text;
using Aerofobia.Core;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

namespace Aerofobia.EditorTools
{
    /// <summary>
    /// Deixa o prefab <c>XR_Player</c> pronto para a experiência <b>sentada</b> (ADR 0003 e ADR 0006):
    /// desliga locomoção, teleporte, giro, pulo e gravidade, e usa o rastreamento "Device", em que a
    /// cabeça do paciente fica sempre na altura da poltrona, seja qual for a altura real da cadeira.
    /// </summary>
    /// <remarks>
    /// Edita a <b>variante</b> <c>XR_Player</c>, nunca o rig original dos Starter Assets. Pode ser rodado
    /// quantas vezes quiser (sempre chega ao mesmo resultado). O <see cref="FlightSceneBuilder"/> chama
    /// este passo antes de montar a cena do voo, porque o jogador vai dentro de um avião em movimento.
    /// </remarks>
    public static class PlayerRigSetup
    {
        /// <summary>
        /// Altura dos olhos acima do piso com a pessoa sentada (m). No modo "Device" o XR Origin
        /// fica no piso da poltrona e a câmera começa esta altura acima dele.
        /// </summary>
        public const float SeatedEyeHeight = 1.22f;

        static readonly string[] ControllerLocomotionFields =
        {
            "m_TeleportInteractor", "m_TeleportMode", "m_TeleportModeCancel", "m_Turn", "m_SnapTurn", "m_Move",
        };

        [MenuItem("Aerofobia/Jogador/Configurar XR_Player (Sentado)", priority = 40)]
        static void ConfigureMenu()
        {
            var report = new StringBuilder();
            ConfigureSeated(report);
            EditorUtility.DisplayDialog("Aerofobia", report.ToString(), "OK");
        }

        /// <summary>Aplica a configuração sentada ao prefab <c>XR_Player</c> e descreve o que fez em <paramref name="report"/>.</summary>
        public static void ConfigureSeated(StringBuilder report)
        {
            var path = SceneBuilder.PlayerPrefabPath;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                report.AppendLine($"• ERRO: {path} não existe. Rode 'Aerofobia > Criar Cenas que Faltam' antes.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var origin = root.GetComponent<XROrigin>();
                if (origin != null)
                {
                    origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
                    origin.CameraYOffset = SeatedEyeHeight;
                    AddSeatedCalibration(origin, report);
                }

                // Sem locomoção, a cápsula do CharacterController só atrapalharia (colisões e raios).
                var characterController = root.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                }

                // Move, Turn, Teleportation, Climb, Gravity e Jump ficam todos embaixo de "Locomotion".
                var locomotion = root.transform.Find("Locomotion");
                if (locomotion != null)
                {
                    locomotion.gameObject.SetActive(false);
                }

                // O ControllerInputActionManager (Starter Assets) reativa o raio de teleporte ao empurrar o
                // analógico; sem as referências, ele não faz mais nada de locomoção. Busca pelo nome do tipo
                // para não depender do assembly do sample.
                int controllers = 0;
                foreach (var manager in root.GetComponentsInChildren<MonoBehaviour>(true)
                             .Where(m => m != null && m.GetType().Name == "ControllerInputActionManager"))
                {
                    var serialized = new SerializedObject(manager);
                    foreach (var field in ControllerLocomotionFields)
                    {
                        var property = serialized.FindProperty(field);
                        if (property != null)
                        {
                            property.objectReferenceValue = null;
                        }
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    controllers++;
                }

                foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Teleport Interactor"))
                {
                    t.gameObject.SetActive(false);
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                report.AppendLine($"• XR_Player sentado: rastreamento Device (olhos a {SeatedEyeHeight} m), locomoção e gravidade " +
                                  $"desligadas, teleporte/giro removidos de {controllers} controle(s).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// Calibração da cabeça na poltrona (ADR 0007). O <c>CameraYOffset</c> sozinho deixava o paciente
        /// flutuando quando o Quest não punha a origem "Device" na cabeça.
        /// </summary>
        static void AddSeatedCalibration(XROrigin origin, StringBuilder report)
        {
            if (origin.Camera == null || origin.CameraFloorOffsetObject == null)
            {
                report.AppendLine("• ERRO: XR Origin sem câmera ou sem Camera Offset; calibração da cabeça não configurada.");
                return;
            }

            var calibration = origin.TryGetComponent<SeatedHeadCalibration>(out var existing)
                ? existing
                : origin.gameObject.AddComponent<SeatedHeadCalibration>();
            var serialized = new SerializedObject(calibration);
            serialized.FindProperty("_cameraOffset").objectReferenceValue = origin.CameraFloorOffsetObject.transform;
            serialized.FindProperty("_head").objectReferenceValue = origin.Camera.transform;
            serialized.FindProperty("_eyeHeight").floatValue = SeatedEyeHeight;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            report.AppendLine($"• Cabeça: olhos a {SeatedEyeHeight} m do piso da poltrona ao iniciar; botão A (direito) ou tecla H recoloca.");
        }
    }
}
