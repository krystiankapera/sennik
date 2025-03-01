using System.Collections.Generic;
using UnityEngine;

namespace Editor.Tools.FloorGenerator {
    [CreateAssetMenu(fileName = nameof(MapGeneratorConfig), menuName = "Configs/" + nameof(MapGeneratorConfig))]
    public class MapGeneratorConfig : ScriptableObject {
        [SerializeField] GameObject floorPrefab;
        [SerializeField] Vector2 floorPrefabSize;
        [SerializeField] GameObject wallPrefab;
        [SerializeField] Vector2 wallPrefabSize;
        [SerializeField] float wallSpacing;
        [SerializeField] GameObject portalPrefab;
        [SerializeField] Vector2 portalPrefabSize;
        [SerializeField] float portalSpacing;
        [SerializeField] List<MapObstacleData> obstacles;

        public GameObject FloorPrefab => floorPrefab;
        public Vector2 FloorPrefabSize => floorPrefabSize;
        public GameObject WallPrefab => wallPrefab;
        public Vector2 WallPrefabSize => wallPrefabSize;
        public float WallSpacing => wallSpacing;
        public GameObject PortalPrefab => portalPrefab;
        public Vector2 PortalPrefabSize => portalPrefabSize;
        public float PortalSpacing => portalSpacing;
        public List<MapObstacleData> Obstacles => obstacles;
    }
}
