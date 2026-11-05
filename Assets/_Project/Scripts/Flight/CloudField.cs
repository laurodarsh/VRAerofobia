using System.Collections.Generic;
using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Camada de nuvens low-poly em volta do aeroporto, gerada por código em <b>uma única malha</b>
    /// (um draw call). O avião sobe através da camada e cruza acima dela, vendo as nuvens passarem
    /// pela janela (ESPECIFICACAO, seções 3.3 e 6).
    /// </summary>
    /// <remarks>
    /// Cada nuvem é um grupo de esferas achatadas. Nenhuma nuvem é posta a menos de
    /// <c>_clearance</c> metros do oval do voo: atravessar uma nuvem opaca mostraria a malha dentro da cabine.
    /// A semente fixa faz as nuvens ficarem sempre no mesmo lugar (a mesma cena para todo paciente).
    /// Nada é salvo em disco: a malha é criada no <c>Awake</c>.
    /// </remarks>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class CloudField : MonoBehaviour
    {
        [Tooltip("Oval do voo, para deixar o caminho do avião livre de nuvens.")]
        [SerializeField] private FlightCircuit _circuit;

        [Tooltip("Semente do sorteio. Mude para ter outra disposição de nuvens.")]
        [SerializeField] private int _seed = 7;

        [Tooltip("Quantidade de nuvens.")]
        [SerializeField, Range(10, 300)] private int _cloudCount = 220;

        [Tooltip("Área coberta (m), centrada neste objeto.")]
        [SerializeField] private Vector2 _areaSize = new Vector2(11000f, 11000f);

        [Tooltip("Altitude mínima e máxima da base das nuvens (m, relativo a este objeto). O cruzeiro deve ficar acima.")]
        [SerializeField] private Vector2 _altitudeRange = new Vector2(250f, 330f);

        [Tooltip("Largura mínima e máxima de uma nuvem (m).")]
        [SerializeField] private Vector2 _cloudWidthRange = new Vector2(80f, 220f);

        [Tooltip("Distância horizontal livre (m) entre as nuvens e o caminho do avião.")]
        [SerializeField, Min(0f)] private float _clearance = 150f;

        Mesh _mesh;

        void Awake()
        {
            BuildIcosphere(out var sphereVertices, out var sphereTriangles);
            var random = new System.Random(_seed);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            int placed = 0;
            for (int attempt = 0; attempt < _cloudCount * 10 && placed < _cloudCount; attempt++)
            {
                float width = Range(random, _cloudWidthRange.x, _cloudWidthRange.y);
                var center = new Vector3(
                    Range(random, -0.5f, 0.5f) * _areaSize.x,
                    Range(random, _altitudeRange.x, _altitudeRange.y),
                    Range(random, -0.5f, 0.5f) * _areaSize.y);

                if (_circuit != null && _circuit.HorizontalDistanceTo(transform.TransformPoint(center)) < _clearance + width * 0.6f)
                {
                    continue;
                }

                int puffs = random.Next(4, 8);
                float yaw = Range(random, 0f, 360f);
                var rotation = Quaternion.Euler(0f, yaw, 0f);
                for (int p = 0; p < puffs; p++)
                {
                    float along = (puffs == 1 ? 0f : p / (puffs - 1f) - 0.5f) * width * 0.7f;
                    float radius = width * Range(random, 0.18f, 0.3f) * (1f - Mathf.Abs(along) / width);
                    var offset = new Vector3(along, radius * Range(random, 0.1f, 0.5f), Range(random, -0.15f, 0.15f) * width);
                    var scale = new Vector3(radius * 1.2f, radius * 0.75f, radius);
                    AddPuff(vertices, normals, triangles, sphereVertices, sphereTriangles, center + rotation * offset, rotation, scale);
                }
                placed++;
            }

            _mesh = new Mesh { name = "Nuvens" };
            if (vertices.Count > 65535)
            {
                _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            _mesh.SetVertices(vertices);
            _mesh.SetNormals(normals);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        static void AddPuff(List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
            List<Vector3> sphereVertices, List<int> sphereTriangles, Vector3 center, Quaternion rotation, Vector3 scale)
        {
            int start = vertices.Count;
            foreach (var v in sphereVertices)
            {
                // Base mais achatada, como uma nuvem de tempo bom.
                var local = v.y < 0f ? new Vector3(v.x, v.y * 0.35f, v.z) : v;
                vertices.Add(center + rotation * Vector3.Scale(local, scale));
                normals.Add(rotation * new Vector3(v.x / scale.x, v.y / scale.y, v.z / scale.z).normalized);
            }
            foreach (var index in sphereTriangles)
            {
                triangles.Add(start + index);
            }
        }

        /// <summary>Esfera de raio 1 com 42 vértices e 80 triângulos (icosaedro subdividido uma vez).</summary>
        static void BuildIcosphere(out List<Vector3> vertices, out List<int> triangles)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            vertices = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized;
            }

            var faces = new List<int>
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
            };

            var midpoints = new Dictionary<long, int>();
            triangles = new List<int>(faces.Count * 4);
            for (int f = 0; f < faces.Count; f += 3)
            {
                int a = faces[f], b = faces[f + 1], c = faces[f + 2];
                int ab = Midpoint(vertices, midpoints, a, b);
                int bc = Midpoint(vertices, midpoints, b, c);
                int ca = Midpoint(vertices, midpoints, c, a);
                triangles.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }
        }

        static int Midpoint(List<Vector3> vertices, Dictionary<long, int> cache, int a, int b)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (cache.TryGetValue(key, out int index))
            {
                return index;
            }

            vertices.Add(((vertices[a] + vertices[b]) * 0.5f).normalized);
            cache[key] = vertices.Count - 1;
            return vertices.Count - 1;
        }

        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
    }
}
