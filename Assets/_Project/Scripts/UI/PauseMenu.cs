using Aerofobia.Core;
using UnityEngine;

namespace Aerofobia.UI
{
    /// <summary>
    /// Pausa da experiência: mostra o painel com Continuar / Voltar ao menu / Sair e congela o tempo e o
    /// áudio. É a "forma fácil de pausar e sair" exigida pela segurança do paciente (CLAUDE.md, seção 6).
    /// </summary>
    /// <remarks>
    /// Aberto pelo botão Menu do controle esquerdo (evento do <see cref="DoctorInput"/>). Com
    /// <c>Time.timeScale = 0</c> o avião, as transições de clima e a turbulência param; o
    /// <see cref="SceneLoader"/> desfaz a pausa ao trocar de cena.
    /// </remarks>
    public class PauseMenu : MonoBehaviour
    {
        [Tooltip("Painel World Space da pausa (fica escondido até pausar).")]
        [SerializeField] private GameObject _panel;

        [Tooltip("Usado por \"Voltar ao menu\" e \"Sair\".")]
        [SerializeField] private SceneLoader _sceneLoader;

        bool _isPaused;

        /// <summary>Se a experiência está pausada agora.</summary>
        public bool IsPaused => _isPaused;

        void Awake()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        void OnDestroy()
        {
            // Garante que o tempo não fica congelado se a cena for descarregada durante a pausa.
            if (_isPaused)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }

        /// <summary>Pausa ou continua (botão Menu).</summary>
        public void TogglePause()
        {
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>Congela o tempo e o áudio e mostra o painel.</summary>
        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        /// <summary>Esconde o painel e volta de onde parou (botão "Continuar").</summary>
        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        /// <summary>Volta ao menu principal (botão "Voltar ao menu").</summary>
        public void ReturnToMenu()
        {
            Resume();
            if (_sceneLoader != null)
            {
                _sceneLoader.LoadMenu();
            }
        }

        /// <summary>Fecha o aplicativo (botão "Sair").</summary>
        public void Quit()
        {
            Resume();
            if (_sceneLoader != null)
            {
                _sceneLoader.QuitApplication();
            }
        }
    }
}
