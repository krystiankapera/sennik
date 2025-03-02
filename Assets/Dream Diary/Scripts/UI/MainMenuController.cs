using Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
    public class MainMenuController : MonoBehaviour {
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] NetworkModeMenuPanelController hostMenuPanel;
        [SerializeField] NetworkModeMenuPanelController clientMenuPanel;
        [SerializeField] Button clientButton;
        [SerializeField] Button hostButton;
        [SerializeField] Button playButton;
        [SerializeField] Button backButton;
        [SerializeField] string gameplaySceneName;

        NetworkModeLauncher networkModeLauncher;

        private void Start() {
            networkModeLauncher = new NetworkModeLauncher(gameplaySceneName);
            hostMenuPanel.SetNetworkModeLauncher(networkModeLauncher);
            clientMenuPanel.SetNetworkModeLauncher(networkModeLauncher);
        }

        void OnEnable() {
            clientButton.onClick.AddListener(ShowClientMenu);
            hostButton.onClick.AddListener(ShowHostMenu);
            playButton.onClick.AddListener(StartSingleplayerGame);
            backButton.onClick.AddListener(ShowMainMenu);
            ShowMainMenu();
        }

        void OnDisable() {
            clientButton.onClick.RemoveListener(ShowClientMenu);
            hostButton.onClick.RemoveListener(ShowHostMenu);
            playButton.onClick.RemoveListener(StartSingleplayerGame);
            backButton.onClick.RemoveListener(ShowMainMenu);
        }

        private void StartSingleplayerGame() {
            networkModeLauncher.StartSingleplayerGame();
        }

        private void ShowMainMenu() {
            mainMenuPanel.SetActive(true);
            hostMenuPanel.SetActive(false);
            clientMenuPanel.SetActive(false);
            backButton.gameObject.SetActive(false);
        }

        private void ShowHostMenu() {
            mainMenuPanel.SetActive(false);
            hostMenuPanel.SetActive(true);
            backButton.gameObject.SetActive(true);
        }

        private void ShowClientMenu() {
            mainMenuPanel.SetActive(false);
            clientMenuPanel.SetActive(true);
            backButton.gameObject.SetActive(true);
        }
    }
}
