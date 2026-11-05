using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Luzes que importam à noite: as luzes da cabine (luzes reais, poucas) e as luzes da pista
    /// (material sem iluminação que "acende" mudando de cor, sem custo de luz real).
    /// </summary>
    /// <remarks>
    /// A cabine fica sempre um pouco iluminada, como num voo de verdade; à noite a luz sobe para o
    /// interior não ficar escuro, o que seria desconfortável para o paciente.
    /// A cor das luzes da pista muda por <see cref="MaterialPropertyBlock"/>, sem criar materiais.
    /// </remarks>
    public class NightLights : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Cabine")]
        [SerializeField] private Light[] _cabinLights = new Light[0];

        [Tooltip("Intensidade das luzes da cabine de dia.")]
        [SerializeField, Min(0f)] private float _dayCabinIntensity = 0.4f;

        [Tooltip("Intensidade das luzes da cabine à noite.")]
        [SerializeField, Min(0f)] private float _nightCabinIntensity = 1.5f;

        [Header("Pista e cidade")]
        [Tooltip("Renderers com material Unlit que representam luzes (ex.: luzes da pista).")]
        [SerializeField] private Renderer[] _lightRenderers = new Renderer[0];

        [Tooltip("Cor das luzes apagadas (dia).")]
        [SerializeField] private Color _offColor = new Color(0.35f, 0.35f, 0.3f);

        [Tooltip("Cor das luzes acesas (noite).")]
        [SerializeField] private Color _onColor = new Color(1f, 0.85f, 0.5f);

        MaterialPropertyBlock _block;

        void Awake()
        {
            _block = new MaterialPropertyBlock();
            SetAmount(0f);
        }

        /// <summary>Quanto de noite agora (0 a 1), vindo da transição do <see cref="WeatherController"/>.</summary>
        public void SetAmount(float night)
        {
            float intensity = Mathf.Lerp(_dayCabinIntensity, _nightCabinIntensity, night);
            foreach (var cabinLight in _cabinLights)
            {
                if (cabinLight != null)
                {
                    cabinLight.intensity = intensity;
                }
            }

            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, Color.Lerp(_offColor, _onColor, night));
            foreach (var lightRenderer in _lightRenderers)
            {
                if (lightRenderer != null)
                {
                    lightRenderer.SetPropertyBlock(_block);
                }
            }
        }
    }
}
