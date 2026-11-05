using System.Collections.Generic;
using UnityEngine;

namespace Aerofobia.Flight
{
    /// <summary>
    /// Gera as luzes de borda da pista como <b>uma única malha</b> (um draw call), com pequenos cubos
    /// ao longo das duas laterais e nas cabeceiras. A cor (apagada/acesa) é controlada pelo
    /// <see cref="NightLights"/>.
    /// </summary>
    /// <remarks>
    /// Fica na cabeceira da pista, com o eixo Z local no sentido da pista (como o <see cref="FlightCircuit"/>).
    /// A malha é gerada no <c>Awake</c>, então nada disso vai para o Git. Precisa de um MeshRenderer com
    /// material Unlit no mesmo objeto.
    /// </remarks>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RunwayLights : MonoBehaviour
    {
        [Tooltip("Comprimento da pista (m).")]
        [SerializeField, Min(10f)] private float _length = 460f;

        [Tooltip("Distância do eixo até a linha de luzes de cada lado (m).")]
        [SerializeField, Min(1f)] private float _halfWidth = 17f;

        [Tooltip("Espaço entre luzes (m).")]
        [SerializeField, Min(2f)] private float _spacing = 30f;

        [Tooltip("Tamanho de cada luz (m). Maior que o real para aparecer de longe no headset.")]
        [SerializeField, Min(0.1f)] private float _size = 0.8f;

        Mesh _mesh;

        void Awake()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (float z = 0f; z <= _length + 0.01f; z += _spacing)
            {
                AddCube(vertices, triangles, new Vector3(-_halfWidth, 0f, z));
                AddCube(vertices, triangles, new Vector3(_halfWidth, 0f, z));
            }

            // Cabeceiras: linha de luzes atravessando a pista nas duas pontas.
            for (float x = -_halfWidth + 3f; x < _halfWidth - 1f; x += 3f)
            {
                AddCube(vertices, triangles, new Vector3(x, 0f, 0f));
                AddCube(vertices, triangles, new Vector3(x, 0f, _length));
            }

            _mesh = new Mesh { name = "LuzesPista" };
            _mesh.SetVertices(vertices);
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

        void AddCube(List<Vector3> vertices, List<int> triangles, Vector3 center)
        {
            float h = _size * 0.5f;
            int start = vertices.Count;
            for (int i = 0; i < 8; i++)
            {
                vertices.Add(center + new Vector3((i & 1) == 0 ? -h : h, (i & 2) == 0 ? 0f : _size, (i & 4) == 0 ? -h : h));
            }

            int[] faces =
            {
                0, 2, 3, 0, 3, 1, // -Z
                4, 5, 7, 4, 7, 6, // +Z
                0, 4, 6, 0, 6, 2, // -X
                1, 3, 7, 1, 7, 5, // +X
                2, 6, 7, 2, 7, 3, // +Y
                0, 1, 5, 0, 5, 4, // -Y
            };
            foreach (var index in faces)
            {
                triangles.Add(start + index);
            }
        }
    }
}
