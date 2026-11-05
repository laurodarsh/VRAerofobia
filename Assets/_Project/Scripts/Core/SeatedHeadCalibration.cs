using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Aerofobia.Core
{
    /// <summary>
    /// Põe os olhos do paciente no lugar certo da poltrona virtual, seja qual for a cadeira real e a
    /// origem que o headset informa. Mede onde a cabeça está e move o <c>Camera Offset</c> do XR Origin
    /// para que ela fique na altura <see cref="EyeHeight"/>, no centro da poltrona e olhando para a frente.
    /// </summary>
    /// <remarks>
    /// Só com o <c>CameraYOffset</c> do XR Origin, a altura depende do ponto zero que o Quest escolhe para
    /// o espaço "Device": se ele não estiver na cabeça, o paciente fica flutuando acima da poltrona (bug de
    /// 2026-09-26). Calibra ao iniciar (quando o headset começa a rastrear), com o botão de recentralizar
    /// (A no controle direito, H no teclado), quando o sistema recentraliza (segurar o botão Meta) e quando
    /// o app volta ao foco (headset tirado e recolocado).
    /// Para achar a altura ideal: em Play, mude "Altura dos olhos" no Inspector; a visão sobe ou desce na hora.
    /// Move o <c>Camera Offset</c>, nunca a câmera (CLAUDE.md, seção 6). Deve ficar no XR Origin, pai do
    /// <c>Camera Offset</c>. ADR 0007.
    /// </remarks>
    public class SeatedHeadCalibration : MonoBehaviour
    {
        [Header("Rig")]
        [Tooltip("Objeto 'Camera Offset' do XR Origin (pai da câmera e dos controles). É ele que se move, nunca a câmera.")]
        [SerializeField] private Transform _cameraOffset;

        [Tooltip("Câmera do headset (Main Camera).")]
        [SerializeField] private Transform _head;

        [Header("Posição dos olhos")]
        [Tooltip("Altura dos olhos acima do XR Origin (piso da poltrona), em metros. Pode ser ajustada durante o Play.")]
        [SerializeField, Range(0.8f, 1.5f)] private float _eyeHeight = 1.22f;

        [Tooltip("Gira o paciente para olhar para a frente da poltrona, para qualquer lado que a cadeira real esteja virada.")]
        [SerializeField] private bool _alignYaw = true;

        [Header("Botão de recentralizar")]
        [Tooltip("Recoloca a cabeça na poltrona, na altura dos olhos. Padrão: botão A do controle direito e tecla H.")]
        [SerializeField] private InputAction _recenterAction = CreateDefaultRecenterAction();

        [Header("Espera pelo rastreamento")]
        [Tooltip("Tempo máximo (s) esperando o headset começar a rastrear. Depois disso calibra assim mesmo (Editor sem headset).")]
        [SerializeField, Min(0f)] private float _trackingTimeout = 3f;

        readonly List<XRInputSubsystem> _subsystems = new List<XRInputSubsystem>();
        Coroutine _pending;
        float _appliedEyeHeight;

        /// <summary>Altura dos olhos acima do XR Origin (m).</summary>
        public float EyeHeight => _eyeHeight;

        /// <summary>Onde ficam os olhos depois da calibração, no espaço deste objeto (o XR Origin).</summary>
        public Vector3 EyePoint => new Vector3(0f, _eyeHeight, 0f);

        /// <summary>Verdadeiro depois da primeira calibração.</summary>
        public bool IsCalibrated { get; private set; }

        /// <summary>Disparado a cada calibração concluída.</summary>
        public event Action Calibrated;

        static InputAction CreateDefaultRecenterAction()
        {
            var action = new InputAction("Recentralizar", InputActionType.Button);
            action.AddBinding("<XRController>{RightHand}/{PrimaryButton}");
            action.AddBinding("<Keyboard>/h");
            return action;
        }

        void OnEnable()
        {
            _recenterAction.performed += OnRecenterPressed;
            _recenterAction.Enable();
            Recalibrate();
        }

        void OnDisable()
        {
            _recenterAction.performed -= OnRecenterPressed;
            _recenterAction.Disable();
            _pending = null;
            foreach (var subsystem in _subsystems)
            {
                subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
            _subsystems.Clear();
        }

        void OnValidate()
        {
            // Ajuste da altura no Inspector durante o Play: sobe ou desce na hora, sem recentralizar.
            if (Application.isPlaying && IsCalibrated && _cameraOffset != null && !Mathf.Approximately(_eyeHeight, _appliedEyeHeight))
            {
                _cameraOffset.localPosition += Vector3.up * (_eyeHeight - _appliedEyeHeight);
                _appliedEyeHeight = _eyeHeight;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            // Headset recolocado ou volta do menu do sistema: a pessoa pode ter mudado de posição.
            if (hasFocus && isActiveAndEnabled)
            {
                Recalibrate();
            }
        }

        /// <summary>
        /// Recalibra assim que o headset estiver rastreando. Público para poder ser ligado a um
        /// comando do médico ou a um botão da UI (evento no Inspector).
        /// </summary>
        public void Recalibrate()
        {
            if (_pending != null)
            {
                StopCoroutine(_pending);
            }
            _pending = StartCoroutine(CalibrateWhenTracked());
        }

        void OnRecenterPressed(InputAction.CallbackContext context)
        {
            Recalibrate();
        }

        IEnumerator CalibrateWhenTracked()
        {
            // Tempo sem escala: a calibração tem de funcionar também com o jogo pausado.
            float waited = 0f;
            while (!IsHeadTracked() && waited < _trackingTimeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            // Um quadro a mais: a pose nova só chega à câmera no Update do TrackedPoseDriver, e o XR Origin
            // reposiciona o Camera Offset quando a origem muda; a calibração tem de vir depois dos dois.
            yield return null;

            SubscribeToRecenter();
            Calibrate();
            _pending = null;
        }

        static bool IsHeadTracked()
        {
            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            return device.isValid && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) && tracked;
        }

        void Calibrate()
        {
            if (_cameraOffset == null || _head == null || _cameraOffset.parent != transform)
            {
                Debug.LogError("[SeatedHeadCalibration] Precisa do Camera Offset (filho deste objeto) e da câmera.", this);
                return;
            }

            // Pose da cabeça dentro do Camera Offset: é o que o rastreamento informa, antes da correção.
            var headInOffset = _cameraOffset.InverseTransformPoint(_head.position);
            var forwardInOffset = _cameraOffset.InverseTransformDirection(_head.forward);
            float yaw = _alignYaw ? Mathf.Atan2(forwardInOffset.x, forwardInOffset.z) * Mathf.Rad2Deg : 0f;

            var rotation = Quaternion.Euler(0f, -yaw, 0f);
            _cameraOffset.localRotation = rotation;
            _cameraOffset.localPosition = EyePoint - rotation * headInOffset;
            _appliedEyeHeight = _eyeHeight;

            IsCalibrated = true;
            Calibrated?.Invoke();
        }

        void SubscribeToRecenter()
        {
            if (_subsystems.Count > 0)
            {
                return;
            }

            SubsystemManager.GetSubsystems(_subsystems);
            foreach (var subsystem in _subsystems)
            {
                subsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            }
        }

        void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            Recalibrate();
        }
    }
}
