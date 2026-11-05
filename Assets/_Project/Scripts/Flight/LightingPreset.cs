using System;
using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Um "clima" de iluminação: sol, céu, luz ambiente, reflexos e neblina. O
    /// <see cref="EnvironmentLighting"/> mistura quatro destes (dia, dia com chuva, noite, noite com chuva)
    /// para que chuva e noite possam ser combinadas e mudar aos poucos.
    /// </summary>
    [Serializable]
    public struct LightingPreset
    {
        [Tooltip("Intensidade da luz do sol (ou da lua, à noite).")]
        [SerializeField] private float _sunIntensity;

        [Tooltip("Cor da luz do sol.")]
        [SerializeField] private Color _sunColor;

        [Tooltip("Tamanho do disco do sol no céu (0 esconde o sol).")]
        [SerializeField, Range(0f, 0.2f)] private float _sunSize;

        [Tooltip("Cor do céu (Skybox/Procedural: Sky Tint).")]
        [SerializeField] private Color _skyTint;

        [Tooltip("Brilho do céu (Skybox/Procedural: Exposure).")]
        [SerializeField] private float _skyExposure;

        [Tooltip("Espessura da atmosfera: mais alto deixa o céu mais leitoso.")]
        [SerializeField, Range(0f, 5f)] private float _atmosphereThickness;

        [Tooltip("Cor do céu abaixo do horizonte.")]
        [SerializeField] private Color _groundColor;

        [Tooltip("Luz ambiente (ilumina o que o sol não alcança, como o interior da cabine).")]
        [SerializeField] private Color _ambientColor;

        [Tooltip("Força dos reflexos do céu nos materiais metálicos. À noite, sem isso, a fuselagem refletiria um céu de dia.")]
        [SerializeField, Range(0f, 1f)] private float _reflectionIntensity;

        [Tooltip("Cor da neblina no horizonte.")]
        [SerializeField] private Color _fogColor;

        [Tooltip("Distância (m) em que a neblina cobre tudo. Menor = menos visibilidade.")]
        [SerializeField] private float _fogEnd;

        public float SunIntensity => _sunIntensity;
        public Color SunColor => _sunColor;
        public float SunSize => _sunSize;
        public Color SkyTint => _skyTint;
        public float SkyExposure => _skyExposure;
        public float AtmosphereThickness => _atmosphereThickness;
        public Color GroundColor => _groundColor;
        public Color AmbientColor => _ambientColor;
        public float ReflectionIntensity => _reflectionIntensity;
        public Color FogColor => _fogColor;
        public float FogEnd => _fogEnd;

        /// <summary>Mistura dois presets campo a campo (t = 0 → a, t = 1 → b).</summary>
        public static LightingPreset Lerp(LightingPreset a, LightingPreset b, float t)
        {
            return new LightingPreset
            {
                _sunIntensity = Mathf.Lerp(a._sunIntensity, b._sunIntensity, t),
                _sunColor = Color.Lerp(a._sunColor, b._sunColor, t),
                _sunSize = Mathf.Lerp(a._sunSize, b._sunSize, t),
                _skyTint = Color.Lerp(a._skyTint, b._skyTint, t),
                _skyExposure = Mathf.Lerp(a._skyExposure, b._skyExposure, t),
                _atmosphereThickness = Mathf.Lerp(a._atmosphereThickness, b._atmosphereThickness, t),
                _groundColor = Color.Lerp(a._groundColor, b._groundColor, t),
                _ambientColor = Color.Lerp(a._ambientColor, b._ambientColor, t),
                _reflectionIntensity = Mathf.Lerp(a._reflectionIntensity, b._reflectionIntensity, t),
                _fogColor = Color.Lerp(a._fogColor, b._fogColor, t),
                _fogEnd = Mathf.Lerp(a._fogEnd, b._fogEnd, t),
            };
        }

        /// <summary>Dia claro: ponto de partida de toda sessão (a terapia começa leve).</summary>
        public static LightingPreset Day() => new LightingPreset
        {
            _sunIntensity = 1.3f,
            _sunColor = new Color(1f, 0.96f, 0.9f),
            _sunSize = 0.04f,
            _skyTint = new Color(0.5f, 0.5f, 0.5f),
            _skyExposure = 1.2f,
            _atmosphereThickness = 1f,
            _groundColor = new Color(0.37f, 0.35f, 0.34f),
            _ambientColor = new Color(0.62f, 0.66f, 0.72f),
            _reflectionIntensity = 0.6f,
            _fogColor = new Color(0.72f, 0.8f, 0.88f),
            _fogEnd = 9000f,
        };

        /// <summary>Dia com chuva: céu cinza, sol escondido, pouca visibilidade.</summary>
        public static LightingPreset DayRain() => new LightingPreset
        {
            _sunIntensity = 0.45f,
            _sunColor = new Color(0.8f, 0.83f, 0.88f),
            _sunSize = 0f,
            // Atmosfera fina = céu cinza. Espessa (> 1,5) avermelha o céu procedural, como um pôr do sol.
            _skyTint = new Color(0.5f, 0.5f, 0.5f),
            _skyExposure = 1.4f,
            _atmosphereThickness = 0.35f,
            _groundColor = new Color(0.3f, 0.3f, 0.3f),
            _ambientColor = new Color(0.45f, 0.47f, 0.5f),
            _reflectionIntensity = 0.35f,
            _fogColor = new Color(0.52f, 0.55f, 0.58f),
            _fogEnd = 3500f,
        };

        /// <summary>Noite limpa: céu azul-escuro e luz fraca e azulada.</summary>
        public static LightingPreset Night() => new LightingPreset
        {
            _sunIntensity = 0.12f,
            _sunColor = new Color(0.55f, 0.65f, 0.9f),
            _sunSize = 0f,
            _skyTint = new Color(0.12f, 0.14f, 0.25f),
            _skyExposure = 0.12f,
            _atmosphereThickness = 0.6f,
            _groundColor = new Color(0.03f, 0.03f, 0.04f),
            _ambientColor = new Color(0.1f, 0.11f, 0.16f),
            _reflectionIntensity = 0.05f,
            _fogColor = new Color(0.03f, 0.04f, 0.07f),
            _fogEnd = 7000f,
        };

        /// <summary>Noite com chuva: o mais escuro e com menos visibilidade.</summary>
        public static LightingPreset NightRain() => new LightingPreset
        {
            _sunIntensity = 0.06f,
            _sunColor = new Color(0.5f, 0.55f, 0.7f),
            _sunSize = 0f,
            _skyTint = new Color(0.08f, 0.09f, 0.12f),
            _skyExposure = 0.08f,
            _atmosphereThickness = 0.35f,
            _groundColor = new Color(0.02f, 0.02f, 0.03f),
            _ambientColor = new Color(0.08f, 0.085f, 0.1f),
            _reflectionIntensity = 0.03f,
            _fogColor = new Color(0.03f, 0.035f, 0.045f),
            _fogEnd = 3000f,
        };
    }
}
