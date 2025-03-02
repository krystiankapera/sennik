using Runtime.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
    public class SettingsPanelController : MonoBehaviour {
        [SerializeField] Button open;
        [SerializeField] Button close;
        [SerializeField] GameObject settingsPanel;

        void Start() {
            ClosePanel();
        }

        void OnEnable() {
            open.onClick.AddListener(OpenPanel);
            close.onClick.AddListener(ClosePanel);
        }

        void OnDisable() {
            open.onClick.RemoveListener(OpenPanel);
            close.onClick.RemoveListener(ClosePanel);
        }

        void OpenPanel() {
            settingsPanel.SetActive(true);
            open.gameObject.SetActive(false);
        }

        void ClosePanel() {
            settingsPanel.SetActive(false);
            open.gameObject.SetActive(true);
        }
    }
}
