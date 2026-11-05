using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Estado das condições de voo que o médico controla: chuva, noite e turbulência leve.
    /// São independentes e combináveis (é assim que o médico sobe a dificuldade) e toda mudança
    /// é gradual, levando <c>_transitionTime</c> segundos (ESPECIFICACAO, seções 2 e 3.3).
    /// </summary>
    /// <remarks>
    /// Recebe os comandos do <c>DoctorInput</c> (eventos ligados na cena) e repassa a quantidade
    /// atual de cada condição (0 a 1) para os efeitos. A sessão sempre começa em "normal":
    /// dia, sem chuva e sem turbulência. Os métodos públicos também servem de botões de teste
    /// no Inspector (menu de contexto do componente).
    /// </remarks>
    public class WeatherController : MonoBehaviour
    {
        [Tooltip("Duração (s) de cada transição. A ESPECIFICACAO pede de 5 a 10 s.")]
        [SerializeField, Range(1f, 20f)] private float _transitionTime = 8f;

        [Header("Efeitos")]
        [SerializeField] private EnvironmentLighting _environment;
        [SerializeField] private RainEffect _rain;
        [SerializeField] private NightLights _nightLights;
        [SerializeField] private TurbulenceEffect _turbulence;

        bool _rainOn;
        bool _nightOn;
        bool _turbulenceOn;
        float _rainLevel;
        float _nightLevel;
        float _turbulenceLevel;

        /// <summary>Se a chuva está ligada (ou ligando).</summary>
        public bool IsRaining => _rainOn;

        /// <summary>Se a noite está ligada (ou ligando).</summary>
        public bool IsNight => _nightOn;

        /// <summary>Se a turbulência está ligada (ou ligando).</summary>
        public bool HasTurbulence => _turbulenceOn;

        void Start()
        {
            ApplyAll();
        }

        /// <summary>Liga ou desliga a chuva (botão X do médico).</summary>
        [ContextMenu("Chuva liga/desliga")]
        public void ToggleRain() => _rainOn = !_rainOn;

        /// <summary>Liga ou desliga a noite (botão Y do médico).</summary>
        [ContextMenu("Noite liga/desliga")]
        public void ToggleNight() => _nightOn = !_nightOn;

        /// <summary>Liga ou desliga a turbulência leve (grip do médico).</summary>
        [ContextMenu("Turbulência liga/desliga")]
        public void ToggleTurbulence() => _turbulenceOn = !_turbulenceOn;

        /// <summary>Volta ao normal: desliga chuva, noite e turbulência, aos poucos (clique do analógico).</summary>
        [ContextMenu("Voltar ao normal")]
        public void ResetToNormal()
        {
            _rainOn = false;
            _nightOn = false;
            _turbulenceOn = false;
        }

        void Update()
        {
            float step = Time.deltaTime / _transitionTime;
            bool changed = Approach(ref _rainLevel, _rainOn, step)
                         | Approach(ref _nightLevel, _nightOn, step)
                         | Approach(ref _turbulenceLevel, _turbulenceOn, step);
            if (changed)
            {
                ApplyAll();
            }
        }

        void ApplyAll()
        {
            // Rampa em "S": começa e termina devagar, sem degrau perceptível.
            float rain = Mathf.SmoothStep(0f, 1f, _rainLevel);
            float night = Mathf.SmoothStep(0f, 1f, _nightLevel);
            float turbulence = Mathf.SmoothStep(0f, 1f, _turbulenceLevel);

            if (_environment != null)
            {
                _environment.Apply(rain, night);
            }
            if (_rain != null)
            {
                _rain.SetAmount(rain);
            }
            if (_nightLights != null)
            {
                _nightLights.SetAmount(night);
            }
            if (_turbulence != null)
            {
                _turbulence.SetAmount(turbulence);
            }
        }

        static bool Approach(ref float level, bool on, float step)
        {
            float target = on ? 1f : 0f;
            if (Mathf.Approximately(level, target))
            {
                return false;
            }

            level = Mathf.MoveTowards(level, target, step);
            return true;
        }
    }
}
