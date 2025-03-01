using UnityEngine;

namespace Editor.Tools.FloorGenerator {
    [System.Serializable]
    public class MapObstacleData {
        [SerializeField] GameObject prefab;
        [SerializeField] Vector2 size;

        public GameObject Prefab => prefab;
        public Vector2 Size => size;
    }
}
