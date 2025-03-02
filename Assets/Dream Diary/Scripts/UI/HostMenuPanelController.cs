using UnityEngine;
using TMPro;

namespace Runtime.UI {
    public class HostMenuPanelController : NetworkModeMenuPanelController {
        const string INPUT_ERROR_MESSAGE = "Incorrect port number!";

        [SerializeField] TMP_InputField port;
        [SerializeField] TMP_Text placeholder;

        protected override void HandlePlayClicked() {
            if (!int.TryParse(port.text, out var portValue) || portValue < 1024 || portValue > 65535)
                ShowErrorInfo();
            else
                networkModeLauncher.StartHostGame(portValue);
        }

        void ShowErrorInfo() {
            port.text = string.Empty;
            placeholder.text = INPUT_ERROR_MESSAGE;
        }
    }
}
