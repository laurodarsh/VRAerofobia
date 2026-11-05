using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Aerofobia.UI
{
    /// <summary>
    /// Faz um painel World Space ser desenhado <b>por cima</b> de tudo (sem teste de profundidade).
    /// Dentro da cabine do avião, encostos e bagageiros ficariam na frente de um painel a 1,5 m;
    /// com isto o painel de pausa e o de fim de voo aparecem inteiros.
    /// </summary>
    /// <remarks>
    /// No <c>Awake</c>, troca o material de cada elemento de UI por uma cópia com
    /// <c>unity_GUIZTestMode = Always</c>, uma cópia por material original (para não multiplicar draw calls).
    /// O TextMeshPro precisa de tratamento à parte: ele desenha com o material da fonte
    /// (<c>fontSharedMaterial</c>), e não com o <c>material</c> do Graphic.
    /// Use só em painéis que precisam aparecer sempre (pausa, avisos).
    /// </remarks>
    public class UiAlwaysOnTop : MonoBehaviour
    {
        static readonly int ZTestModeId = Shader.PropertyToID("unity_GUIZTestMode");

        readonly Dictionary<Material, Material> _copies = new Dictionary<Material, Material>();

        void Awake()
        {
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if (graphic is TMP_Text text)
                {
                    var copy = GetCopy(text.fontSharedMaterial);
                    if (copy != null)
                    {
                        text.fontSharedMaterial = copy;
                    }
                    continue;
                }

                var material = GetCopy(graphic.materialForRendering);
                if (material != null)
                {
                    graphic.material = material;
                }
            }
        }

        void OnDestroy()
        {
            foreach (var copy in _copies.Values)
            {
                Destroy(copy);
            }
        }

        Material GetCopy(Material source)
        {
            if (source == null)
            {
                return null;
            }

            if (!_copies.TryGetValue(source, out var copy))
            {
                copy = new Material(source) { name = source.name + " (sempre visível)" };
                copy.SetInt(ZTestModeId, (int)CompareFunction.Always);
                _copies[source] = copy;
            }
            return copy;
        }
    }
}
