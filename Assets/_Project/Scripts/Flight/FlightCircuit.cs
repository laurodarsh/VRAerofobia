using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Trajetória do voo vista de cima: um oval (duas retas e duas meias-voltas) cuja primeira reta
    /// passa pelo eixo da pista. O avião decola por essa reta, dá voltas no oval e pousa
    /// <b>na mesma pista</b>, sem teleporte (ESPECIFICACAO, seção 3.3).
    /// </summary>
    /// <remarks>
    /// Fica na cabeceira da pista: a origem do objeto é o início da pista, o eixo Z local aponta no
    /// sentido da decolagem e Y é a superfície do asfalto. A distância ao longo do oval (<c>s</c>)
    /// começa em <c>-_distanceBeforeRunway</c> no eixo da pista, então <c>s = z + _distanceBeforeRunway</c>
    /// na reta da pista. Só a posição horizontal sai daqui; a altitude é do <see cref="FlightController"/>.
    /// O oval aparece na Scene view (Gizmos) quando o objeto está selecionado.
    /// </remarks>
    public class FlightCircuit : MonoBehaviour
    {
        [Tooltip("Comprimento da reta final antes da cabeceira (m). É onde o avião alinha e desce para pousar.")]
        [SerializeField, Min(500f)] private float _distanceBeforeRunway = 2500f;

        [Tooltip("Até onde a reta da decolagem vai depois da cabeceira antes da primeira curva (m).")]
        [SerializeField, Min(500f)] private float _distanceAfterRunway = 2000f;

        [Tooltip("Raio das curvas do oval (m). Curvas maiores são mais suaves.")]
        [SerializeField, Min(200f)] private float _turnRadius = 1200f;

        [Tooltip("Curvas para a esquerda: o aeroporto e a cidade ficam do lado esquerdo do avião, o lado da poltrona do paciente.")]
        [SerializeField] private bool _turnLeft = true;

        /// <summary>Comprimento de cada reta do oval (m).</summary>
        public float StraightLength => _distanceBeforeRunway + _distanceAfterRunway;

        /// <summary>Perímetro total do oval (m): o quanto o avião anda em uma volta completa.</summary>
        public float Length => 2f * StraightLength + 2f * Mathf.PI * _turnRadius;

        /// <summary>
        /// Converte uma posição ao longo da pista (metros a partir da cabeceira) em distância no oval.
        /// Usado para achar o ponto de partida e o ponto de toque no pouso.
        /// </summary>
        public float RunwayToDistance(float metersFromThreshold) => metersFromThreshold + _distanceBeforeRunway;

        /// <summary>Distância de <paramref name="from"/> até <paramref name="to"/> andando para a frente no oval (sempre ≥ 0).</summary>
        public float DistanceAhead(float from, float to) => Mathf.Repeat(to - from, Length);

        /// <summary>
        /// Posição (no chão, espaço local), direção e curvatura do oval na distância <paramref name="s"/>.
        /// A curvatura é 1/raio nas curvas e 0 nas retas; o sinal é positivo em curvas para a esquerda.
        /// </summary>
        public void SampleLocal(float s, out Vector3 position, out Vector3 forward, out float curvature)
        {
            s = Mathf.Repeat(s, Length);
            float straight = StraightLength;
            float arc = Mathf.PI * _turnRadius;
            float side = _turnLeft ? -1f : 1f; // lado (X local) onde fica o centro das curvas
            float zStart = -_distanceBeforeRunway;
            float zEnd = _distanceAfterRunway;
            float signedCurvature = (_turnLeft ? 1f : -1f) / _turnRadius;

            if (s < straight)
            {
                // Reta da pista, no sentido da decolagem (+Z).
                position = new Vector3(0f, 0f, zStart + s);
                forward = Vector3.forward;
                curvature = 0f;
                return;
            }

            s -= straight;
            if (s < arc)
            {
                // Primeira meia-volta, em torno de (side·R, zEnd).
                float angle = s / _turnRadius;
                var center = new Vector3(side * _turnRadius, 0f, zEnd);
                position = center + new Vector3(-side * _turnRadius * Mathf.Cos(angle), 0f, _turnRadius * Mathf.Sin(angle));
                forward = new Vector3(side * Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                curvature = signedCurvature;
                return;
            }

            s -= arc;
            if (s < straight)
            {
                // Reta paralela, no sentido contrário (-Z).
                position = new Vector3(2f * side * _turnRadius, 0f, zEnd - s);
                forward = Vector3.back;
                curvature = 0f;
                return;
            }

            s -= straight;
            {
                // Segunda meia-volta, de volta ao eixo da pista.
                float angle = s / _turnRadius;
                var center = new Vector3(side * _turnRadius, 0f, zStart);
                position = center + new Vector3(side * _turnRadius * Mathf.Cos(angle), 0f, -_turnRadius * Mathf.Sin(angle));
                forward = new Vector3(-side * Mathf.Sin(angle), 0f, -Mathf.Cos(angle));
                curvature = signedCurvature;
            }
        }

        /// <summary>
        /// Menor distância horizontal (m) de um ponto do mundo até o oval. Usada para não pôr nuvens
        /// no caminho do avião (atravessar uma nuvem opaca mostraria a malha dentro da cabine).
        /// </summary>
        public float HorizontalDistanceTo(Vector3 worldPoint)
        {
            var p = transform.InverseTransformPoint(worldPoint);
            p.y = 0f;
            float side = _turnLeft ? -1f : 1f;
            float zStart = -_distanceBeforeRunway;
            float zEnd = _distanceAfterRunway;

            if (p.z >= zStart && p.z <= zEnd)
            {
                // Entre as curvas: a mais próxima das duas retas.
                return Mathf.Min(Mathf.Abs(p.x), Mathf.Abs(p.x - 2f * side * _turnRadius));
            }

            var center = new Vector3(side * _turnRadius, 0f, p.z > zEnd ? zEnd : zStart);
            return Mathf.Abs(Vector3.Distance(p, center) - _turnRadius);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            const int segments = 200;
            SampleLocal(0f, out var previous, out _, out _);
            for (int i = 1; i <= segments; i++)
            {
                SampleLocal(Length * i / segments, out var point, out _, out _);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }
    }
}
