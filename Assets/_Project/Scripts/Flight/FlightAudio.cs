using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Sons do voo por etapa: chime de "apertem os cintos" no início, som da decolagem, ambiente da
    /// cabine em loop e som do pouso (ESPECIFICACAO, seção 4, sons 5 a 8).
    /// </summary>
    /// <remarks>
    /// Os áudios ainda não foram importados (PENDENCIAS, seção 4): com os campos vazios, nada toca e
    /// nada quebra. Ao importar, basta arrastar os clipes para os campos. O volume do ambiente muda
    /// aos poucos entre as etapas, sem cortes secos.
    /// </remarks>
    public class FlightAudio : MonoBehaviour
    {
        [SerializeField] private FlightController _flight;

        [Header("Fontes de áudio")]
        [Tooltip("Fonte para sons que tocam uma vez (chime, decolagem, pouso).")]
        [SerializeField] private AudioSource _oneShots;

        [Tooltip("Fonte do ambiente da cabine, em loop. O clipe fica nela.")]
        [SerializeField] private AudioSource _cabinLoop;

        [Header("Clipes")]
        [Tooltip("Chime de aviso tocado ao entrar na cena (som 5).")]
        [SerializeField] private AudioClip _seatbeltChime;

        [Tooltip("Decolagem vista de dentro (som 6).")]
        [SerializeField] private AudioClip _takeoff;

        [Tooltip("Aproximação e pouso vistos de dentro (som 8).")]
        [SerializeField] private AudioClip _landing;

        [Header("Volume do ambiente por etapa")]
        [SerializeField, Range(0f, 1f)] private float _groundVolume = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _flightVolume = 0.8f;

        [Tooltip("Velocidade da mudança de volume (volume por segundo).")]
        [SerializeField, Min(0.01f)] private float _fadeSpeed = 0.2f;

        float _targetVolume;

        void Start()
        {
            if (_flight == null)
            {
                enabled = false;
                return;
            }

            _flight.StateChanged += OnStateChanged;
            _targetVolume = _groundVolume;
            if (_cabinLoop != null && _cabinLoop.clip != null)
            {
                _cabinLoop.loop = true;
                _cabinLoop.volume = _targetVolume;
                _cabinLoop.Play();
            }
            PlayOnce(_seatbeltChime);
        }

        void OnDestroy()
        {
            if (_flight != null)
            {
                _flight.StateChanged -= OnStateChanged;
            }
        }

        void Update()
        {
            if (_cabinLoop != null)
            {
                _cabinLoop.volume = Mathf.MoveTowards(_cabinLoop.volume, _targetVolume, _fadeSpeed * Time.deltaTime);
            }
        }

        void OnStateChanged(FlightState state)
        {
            switch (state)
            {
                case FlightState.Decolagem:
                    PlayOnce(_takeoff);
                    _targetVolume = _flightVolume;
                    break;
                case FlightState.Aproximacao:
                    PlayOnce(_landing);
                    break;
                case FlightState.Concluido:
                    _targetVolume = _groundVolume;
                    break;
            }
        }

        void PlayOnce(AudioClip clip)
        {
            if (clip != null && _oneShots != null)
            {
                _oneShots.PlayOneShot(clip);
            }
        }
    }
}
