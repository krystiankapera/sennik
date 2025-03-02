using UnityEngine.SceneManagement;

namespace Multiplayer {
    public class NetworkModeLauncher {
        string sceneName;

        public NetworkModeLauncher(string sceneName) {
            this.sceneName = sceneName;
        }

        void LoadGameplay() {
            SceneManager.LoadScene(sceneName);
        }

        public void StartSingleplayerGame() {
            NetworkSettings.SingleplayerSetup();
            LoadGameplay();
        }

        public void StartHostGame(int port) {
            NetworkSettings.HostSetup(port);
            LoadGameplay();
        }

        public void StartClientGame(string ip, int port) {
            NetworkSettings.ClientSetup(ip, port);
            LoadGameplay();
        }
    }
}
