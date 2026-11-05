using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aerofobia.Core
{
    /// <summary>
    /// Troca de cena da experiência. Fica em cada cena e é chamado pelos botões da UI
    /// (evento OnClick), por isso os métodos são públicos e recebem no máximo um parâmetro.
    /// </summary>
    /// <remarks>
    /// Carrega de forma assíncrona para não travar o headset durante a troca.
    /// Futuro (ROADMAP, Fase 2): fade para preto antes de trocar, para conforto em VR.
    /// </remarks>
    public class SceneLoader : MonoBehaviour
    {
        bool _isLoading;

        /// <summary>
        /// Carrega a cena com o nome informado (use <see cref="SceneNames"/>).
        /// Desfaz a pausa (tempo e áudio) antes de trocar, porque pode ser chamado pelo painel de pausa.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneLoader] A cena '{sceneName}' não está no Build Profile.", this);
                return;
            }

            _isLoading = true;
            // Sair de uma cena pausada não pode levar a pausa junto para a próxima.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadSceneAsync(sceneName);
        }

        /// <summary>Volta para o menu principal.</summary>
        public void LoadMenu()
        {
            LoadScene(SceneNames.Menu);
        }

        /// <summary>
        /// Avança para o próximo cenário da sequência (Compra → Observação → Voo).
        /// Depois do último cenário, volta ao menu.
        /// </summary>
        public void LoadNextScenario()
        {
            var current = SceneManager.GetActiveScene().name;
            var index = Array.IndexOf(SceneNames.Scenarios, current);
            var hasNext = index >= 0 && index < SceneNames.Scenarios.Length - 1;
            LoadScene(hasNext ? SceneNames.Scenarios[index + 1] : SceneNames.Menu);
        }

        /// <summary>Fecha o aplicativo (no Editor, só encerra o Play Mode).</summary>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
