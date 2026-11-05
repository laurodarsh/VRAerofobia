using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Turbulência leve: balança o <b>avião</b> (a cabine com o jogador dentro) com ruído suave em
    /// posição e rotação. Nunca mexe na câmera (CLAUDE.md, seção 6).
    /// </summary>
    /// <remarks>
    /// Fica num objeto intermediário ("Corpo"), filho da raiz que o <see cref="FlightController"/> move,
    /// para os dois não brigarem pelo mesmo Transform. A amplitude tem teto no código
    /// (<see cref="MaxOffset"/> e <see cref="MaxAngle"/>): nenhum ajuste no Inspector passa disso.
    /// Usa ruído de Perlin (contínuo), sem solavancos. Para no pause porque usa <c>Time.time</c>.
    /// </remarks>
    public class TurbulenceEffect : MonoBehaviour
    {
        /// <summary>Deslocamento máximo permitido (m), mesmo com intensidade 1.</summary>
        public const float MaxOffset = 0.08f;

        /// <summary>Rotação máxima permitida (graus), mesmo com intensidade 1.</summary>
        public const float MaxAngle = 1.2f;

        [Tooltip("Intensidade em relação ao teto (0 a 1). Comece baixo e suba com o terapeuta.")]
        [SerializeField, Range(0f, 1f)] private float _intensity = 0.5f;

        [Tooltip("Rapidez do balanço (Hz). Valores baixos parecem ondulação; altos, trepidação.")]
        [SerializeField, Range(0.1f, 2f)] private float _frequency = 0.7f;

        Vector3 _basePosition;
        Quaternion _baseRotation;
        float _amount;
        float _seed;

        void Awake()
        {
            _basePosition = transform.localPosition;
            _baseRotation = transform.localRotation;
            _seed = Random.Range(0f, 100f);
        }

        /// <summary>Quanto da turbulência está ligado agora (0 a 1), vindo da transição do <see cref="WeatherController"/>.</summary>
        public void SetAmount(float amount)
        {
            _amount = Mathf.Clamp01(amount);
            if (_amount <= 0f)
            {
                transform.localPosition = _basePosition;
                transform.localRotation = _baseRotation;
            }
        }

        void Update()
        {
            if (_amount <= 0f)
            {
                return;
            }

            float strength = _amount * _intensity;
            float t = Time.time * _frequency;
            // Sobe e desce é o principal; balanço lateral e arfagem menores.
            float heave = Noise(t, 0f) * MaxOffset;
            float sway = Noise(t, 10f) * MaxOffset * 0.3f;
            float roll = Noise(t * 0.8f, 20f) * MaxAngle;
            float pitch = Noise(t * 0.9f, 30f) * MaxAngle * 0.6f;

            transform.localPosition = _basePosition + new Vector3(sway, heave, 0f) * strength;
            transform.localRotation = _baseRotation * Quaternion.Euler(pitch * strength, 0f, roll * strength);
        }

        /// <summary>Ruído suave entre -1 e 1 (duas oitavas de Perlin).</summary>
        float Noise(float t, float channel)
        {
            float a = Mathf.PerlinNoise(t, _seed + channel) * 2f - 1f;
            float b = Mathf.PerlinNoise(t * 2.3f, _seed + channel + 5f) * 2f - 1f;
            return Mathf.Clamp(a * 0.75f + b * 0.25f, -1f, 1f);
        }
    }
}
