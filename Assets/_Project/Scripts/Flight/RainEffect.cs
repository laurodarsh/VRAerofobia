using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Chuva vista da poltrona: gotas escorrendo do lado de fora da janela e o som da chuva.
    /// O céu cinza e a neblina da chuva ficam no <see cref="EnvironmentLighting"/>.
    /// </summary>
    /// <remarks>
    /// Por desempenho (ESPECIFICACAO, seção 6), só há gotas perto da janela do paciente, não uma chuva
    /// no mundo inteiro. O sistema de partículas simula no espaço local do avião, então as gotas
    /// acompanham a janela durante o voo.
    /// </remarks>
    public class RainEffect : MonoBehaviour
    {
        [Tooltip("Partículas das gotas, do lado de fora da janela.")]
        [SerializeField] private ParticleSystem _drops;

        [Tooltip("Som de chuva na janela (loop). Pode ficar vazio até o som ser importado.")]
        [SerializeField] private AudioSource _audio;

        [Tooltip("Gotas por segundo com a chuva no máximo.")]
        [SerializeField, Min(0f)] private float _maxRate = 120f;

        [Tooltip("Volume do som com a chuva no máximo.")]
        [SerializeField, Range(0f, 1f)] private float _maxVolume = 0.6f;

        const float Threshold = 0.001f;

        void Awake()
        {
            SetAmount(0f);
        }

        /// <summary>Quanto de chuva agora (0 a 1), vindo da transição do <see cref="WeatherController"/>.</summary>
        public void SetAmount(float amount)
        {
            if (_drops != null)
            {
                var emission = _drops.emission;
                emission.rateOverTime = amount * _maxRate;
                if (amount > Threshold && !_drops.isPlaying)
                {
                    _drops.Play();
                }
                else if (amount <= Threshold && _drops.isPlaying)
                {
                    _drops.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            if (_audio != null && _audio.clip != null)
            {
                _audio.volume = amount * _maxVolume;
                if (amount > Threshold && !_audio.isPlaying)
                {
                    _audio.Play();
                }
                else if (amount <= Threshold && _audio.isPlaying)
                {
                    _audio.Stop();
                }
            }
        }
    }
}
