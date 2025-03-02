using System.Net;
using TMPro;
using UnityEngine;

namespace Runtime.UI {
    public class ClientMenuPanelController : NetworkModeMenuPanelController {
        const string PORT_INPUT_ERROR_MESSAGE = "Incorrect port number!";
        const string IP_INPUT_ERROR_MESSAGE = "Incorrect IP Address!";

        [SerializeField] TMP_InputField port;
        [SerializeField] TMP_InputField ipAddress;
        [SerializeField] TMP_Text portPlaceholder;
        [SerializeField] TMP_Text ipAddressPlaceholder;

        protected override void HandlePlayClicked() {
            var inputError = false;

            if (!int.TryParse(port.text, out var portValue) || portValue < 1024 || portValue > 65535) {
                ShowPortErrorInfo();
                inputError = true;
            }

            if (!IsValidIPAddress(ipAddress.text)) {
                ShowIPAddressErrorInfo();
                inputError = true;
            }

            if (inputError)
                return;

            networkModeLauncher.StartClientGame(ipAddress.text, portValue);
        }

        bool IsValidIPAddress(string text) {
            return IPAddress.TryParse(text, out _);
        }

        void ShowPortErrorInfo() {
            port.text = string.Empty;
            portPlaceholder.text = PORT_INPUT_ERROR_MESSAGE;
        }

        void ShowIPAddressErrorInfo() {
            ipAddress.text = string.Empty;
            ipAddressPlaceholder.text = IP_INPUT_ERROR_MESSAGE;
        }
    }
}
