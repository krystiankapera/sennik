using Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
    public abstract class NetworkModeMenuPanelController : MonoBehaviour {
        [SerializeField] Button play;

        protected NetworkModeLauncher networkModeLauncher;

        void OnEnable() {
            play.onClick.AddListener(HandlePlayClicked);
        }

        void OnDisable() {
            play.onClick.RemoveListener(HandlePlayClicked);
        }

        protected abstract void HandlePlayClicked();

        public void SetActive(bool active) {
            gameObject.SetActive(active);
        }

        public void SetNetworkModeLauncher(NetworkModeLauncher launcher) {
            networkModeLauncher = launcher;
        }
    }
}
