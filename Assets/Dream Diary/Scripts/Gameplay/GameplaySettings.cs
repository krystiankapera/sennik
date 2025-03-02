using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Gameplay {
    public class GameplaySettings {
        const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
        const string SOUND_VOLUME_KEY = "SoundVolume";
        const string CONFIG_FILE_NAME = "config.json";
        const float DEFAULT_MOVEMENT_SPEED = 5f;
        const float DEFAULT_MOUSE_SENSITIVITY = 3f;
        const float DEFAULT_SOUND_VOLUME = 1f;

        static GameplaySettings instance;

        float sensitivity;
        float soundVolume;

        public static GameplaySettings Instance => instance ??= new GameplaySettings();

        public UnityAction<float> OnVolumeChanged;

        public float MovementSpeed { get; private set; }

        public float Sensitivity { 
            get => sensitivity; 

            set {
                sensitivity = value;
                PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, sensitivity);
            }
        }

        public float SoundVolume {
            get => soundVolume;

            set {
                soundVolume = value;
                PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, sensitivity);
                OnVolumeChanged?.Invoke(soundVolume);
            }
        }

        private GameplaySettings() {
            LoadConfig();
            LoadPlayerPrefs();
        }

        void LoadConfig() {
            var path = Path.Combine(Application.persistentDataPath, CONFIG_FILE_NAME);

            if (File.Exists(path)) {
                var json = File.ReadAllText(path);
                var settings = JsonUtility.FromJson<PlayerSettings>(json);
                MovementSpeed = settings.movementSpeed;
            } else {
                MovementSpeed = DEFAULT_MOVEMENT_SPEED; 
                PlayerSettings settings = new PlayerSettings { movementSpeed = MovementSpeed };
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(path, json);
            }
        }

        void LoadPlayerPrefs() {
            if (PlayerPrefs.HasKey(MOUSE_SENSITIVITY_KEY))
                sensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY);
            else
                Sensitivity = DEFAULT_MOUSE_SENSITIVITY;

            if (PlayerPrefs.HasKey(SOUND_VOLUME_KEY))
                soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_KEY);
            else
                SoundVolume = DEFAULT_SOUND_VOLUME;
        }

        [System.Serializable]
        public class PlayerSettings {
            public float movementSpeed;
        }
    }
}
