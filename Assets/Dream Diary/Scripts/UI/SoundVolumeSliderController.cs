using Runtime.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
    public class SoundVolumeSliderController : MonoBehaviour {
        [SerializeField] Slider slider;

        void OnEnable() {
            slider.value = GameplaySettings.Instance.SoundVolume;
            slider.onValueChanged.AddListener(HandleSliderValueChange);
        }

        void OnDisable() {
            slider.onValueChanged.RemoveListener(HandleSliderValueChange);
        }

        void HandleSliderValueChange(float value) {
            GameplaySettings.Instance.SoundVolume = value;
        }
    }
}
