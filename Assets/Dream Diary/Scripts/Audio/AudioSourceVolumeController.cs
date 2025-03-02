using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.AudioControllers {
    public class AudioSourceVolumeController : MonoBehaviour {
        [SerializeField] AudioSource source;

        void OnEnable() {
            GameplaySettings.Instance.OnVolumeChanged += HandleVolumeChange;
        }

        void OnDisable() {
            GameplaySettings.Instance.OnVolumeChanged -= HandleVolumeChange;
        }

        void HandleVolumeChange(float volume) {
            source.volume = volume;
        }
    }
}
