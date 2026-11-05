using UnityEngine;
using UnityEngine.Rendering;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Aplica a iluminação do ambiente (sol, céu, luz ambiente, reflexos e neblina) a partir da
    /// quantidade de chuva e de noite, cada uma de 0 a 1. Chamado pelo <see cref="WeatherController"/>
    /// durante as transições, que são graduais.
    /// </summary>
    /// <remarks>
    /// Usa uma <b>cópia</b> do material do céu: mudar o material original no Play Mode alteraria o
    /// asset no disco. A luz ambiente é aplicada direto na sonda ambiente (<c>ambientProbe</c>),
    /// que é o que o URP lê, para valer na hora sem recalcular a iluminação global.
    /// </remarks>
    public class EnvironmentLighting : MonoBehaviour
    {
        static readonly int SkyTintId = Shader.PropertyToID("_SkyTint");
        static readonly int GroundColorId = Shader.PropertyToID("_GroundColor");
        static readonly int ExposureId = Shader.PropertyToID("_Exposure");
        static readonly int AtmosphereId = Shader.PropertyToID("_AtmosphereThickness");
        static readonly int SunSizeId = Shader.PropertyToID("_SunSize");

        [Tooltip("Luz direcional do sol.")]
        [SerializeField] private Light _sun;

        [Tooltip("Material do céu (Skybox/Procedural). Uma cópia é usada durante o jogo.")]
        [SerializeField] private Material _skyboxTemplate;

        [Header("Presets")]
        [SerializeField] private LightingPreset _day = LightingPreset.Day();
        [SerializeField] private LightingPreset _dayRain = LightingPreset.DayRain();
        [SerializeField] private LightingPreset _night = LightingPreset.Night();
        [SerializeField] private LightingPreset _nightRain = LightingPreset.NightRain();

        [Tooltip("A neblina começa nesta fração da distância final (0,15 = começa a 15% do FogEnd).")]
        [SerializeField, Range(0f, 0.9f)] private float _fogStartFraction = 0.15f;

        Material _sky;

        void Awake()
        {
            if (_skyboxTemplate != null)
            {
                _sky = new Material(_skyboxTemplate);
                RenderSettings.skybox = _sky;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            Apply(0f, 0f);
        }

        void OnDestroy()
        {
            if (_sky != null)
            {
                Destroy(_sky);
            }
        }

        /// <summary>Aplica a mistura de presets para a chuva e a noite informadas (0 a 1 cada).</summary>
        public void Apply(float rain, float night)
        {
            var dayMix = LightingPreset.Lerp(_day, _dayRain, rain);
            var nightMix = LightingPreset.Lerp(_night, _nightRain, rain);
            var p = LightingPreset.Lerp(dayMix, nightMix, night);

            if (_sun != null)
            {
                _sun.intensity = p.SunIntensity;
                _sun.color = p.SunColor;
            }

            if (_sky != null)
            {
                _sky.SetColor(SkyTintId, p.SkyTint);
                _sky.SetColor(GroundColorId, p.GroundColor);
                _sky.SetFloat(ExposureId, p.SkyExposure);
                _sky.SetFloat(AtmosphereId, p.AtmosphereThickness);
                _sky.SetFloat(SunSizeId, p.SunSize);
            }

            RenderSettings.ambientLight = p.AmbientColor;
            var probe = new SphericalHarmonicsL2();
            probe.AddAmbientLight(p.AmbientColor);
            RenderSettings.ambientProbe = probe;
            RenderSettings.reflectionIntensity = p.ReflectionIntensity;
            RenderSettings.fogColor = p.FogColor;
            RenderSettings.fogEndDistance = p.FogEnd;
            RenderSettings.fogStartDistance = p.FogEnd * _fogStartFraction;
        }
    }
}
