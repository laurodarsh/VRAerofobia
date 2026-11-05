using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Aerofobia.Core
{
    /// <summary>
    /// Lê os comandos do médico (controle esquerdo) no Input Action Asset <c>ControlesMedico</c>
    /// e dispara um evento para cada um. Não sabe o que cada comando faz: quem reage são o voo,
    /// o clima e a pausa, ligados pelos eventos no Inspector. Assim o mapeamento dos botões
    /// muda no asset, sem mexer no código (ESPECIFICACAO, seção 2).
    /// </summary>
    /// <remarks>
    /// O asset também tem teclas para testar no Editor sem headset (Espaço, C, N, T, R e P).
    /// Com o jogo pausado (<c>Time.timeScale == 0</c>), só o comando de pausa é repassado.
    /// </remarks>
    public class DoctorInput : MonoBehaviour
    {
        const string MapName = "Medico";

        [Tooltip("Asset com o mapa de ações \"Medico\" (Assets/_Project/Settings/Input/ControlesMedico).")]
        [SerializeField] private InputActionAsset _actions;

        [Header("Eventos")]
        [Tooltip("Gatilho segurado por 1 s: decolar ou pousar.")]
        [SerializeField] private UnityEvent _takeoffOrLanding = new UnityEvent();

        [Tooltip("Botão X: liga/desliga a chuva.")]
        [SerializeField] private UnityEvent _rainToggled = new UnityEvent();

        [Tooltip("Botão Y: liga/desliga a noite.")]
        [SerializeField] private UnityEvent _nightToggled = new UnityEvent();

        [Tooltip("Grip: liga/desliga a turbulência leve.")]
        [SerializeField] private UnityEvent _turbulenceToggled = new UnityEvent();

        [Tooltip("Clique do analógico: volta ao normal (desliga chuva, noite e turbulência).")]
        [SerializeField] private UnityEvent _normalRequested = new UnityEvent();

        [Tooltip("Botão Menu: abre ou fecha a pausa.")]
        [SerializeField] private UnityEvent _pauseToggled = new UnityEvent();

        InputActionMap _map;

        void Awake()
        {
            if (_actions == null)
            {
                Debug.LogError("[DoctorInput] Input Action Asset não definido.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(MapName, throwIfNotFound: true);
            Bind("DecolarPousar", _takeoffOrLanding, allowWhilePaused: false);
            Bind("Chuva", _rainToggled, allowWhilePaused: false);
            Bind("Noite", _nightToggled, allowWhilePaused: false);
            Bind("Turbulencia", _turbulenceToggled, allowWhilePaused: false);
            Bind("Normal", _normalRequested, allowWhilePaused: false);
            Bind("Pausa", _pauseToggled, allowWhilePaused: true);
        }

        void OnEnable()
        {
            _map?.Enable();
        }

        void OnDisable()
        {
            _map?.Disable();
        }

        void Bind(string actionName, UnityEvent target, bool allowWhilePaused)
        {
            _map.FindAction(actionName, throwIfNotFound: true).performed += _ =>
            {
                if (allowWhilePaused || Time.timeScale > 0f)
                {
                    target.Invoke();
                }
            };
        }
    }
}
