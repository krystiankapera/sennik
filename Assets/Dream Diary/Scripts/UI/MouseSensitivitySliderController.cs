using Runtime.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
    public class MouseSensitivitySliderController : MonoBehaviour {
        [SerializeField] Slider slider;

        void OnEnable() {
            slider.value = GameplaySettings.Instance.Sensitivity;
            slider.onValueChanged.AddListener(HandleSliderValueChange);
        }

        void OnDisable() {
            slider.onValueChanged.RemoveListener(HandleSliderValueChange);
        }

        void HandleSliderValueChange(float value) {
            GameplaySettings.Instance.Sensitivity = value;
        }
    }
}
