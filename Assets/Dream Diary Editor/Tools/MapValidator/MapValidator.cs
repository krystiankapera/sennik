using System.Collections.Generic;
using System.Text;
using Runtime.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Editor.Tools.MapValidator {
    public class MapValidator : EditorWindow {
        float playerHeight;
        float playerRadius;
        float checkPrecision;
        ValidationResult lastValidationResult = ValidationResult.Pending;

        private GUIStyle validLabelStyle;
        private GUIStyle invalidLabelStyle;

        [MenuItem("Sennik/" + nameof(MapValidator))]
        public static void ShowSelf()
            => GetWindow<MapValidator>(nameof(MapValidator)).Show();

        private void OnEnable() {
            validLabelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 24,
                normal = { textColor = Color.green }
            };

            invalidLabelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 24,
                normal = { textColor = Color.red }
            };
        }

        void OnGUI() {
            playerHeight = EditorGUILayout.FloatField(label: nameof(playerHeight), playerHeight);
            playerRadius = EditorGUILayout.FloatField(label: nameof(playerRadius), playerRadius);
            checkPrecision = EditorGUILayout.FloatField(label: nameof(checkPrecision), checkPrecision);

            if (checkPrecision <= 0)
                checkPrecision = 0.1f;

            if (GUILayout.Button("Validate"))
                Validate();

            if (lastValidationResult != ValidationResult.Pending)
                ShowValidationMessage();
        }

        void ShowValidationMessage() {
            var style = lastValidationResult == ValidationResult.Valid ? validLabelStyle : invalidLabelStyle;
            GUILayout.Label(new StringBuilder().Append("Validation Result: ").Append(lastValidationResult.ToString()).ToString(), style);
        }

        void Validate() {
            var allColliders = FindObjectsOfType<Collider>(false);
            List<Collider> obstacleColliders = new();
            List<Collider> portalColliders = new();
            List<Portal> portals = new();
            Bounds bounds = new Bounds();

            foreach (var collider in allColliders) {
                bounds.Encapsulate(collider.bounds);

                if (collider.bounds.max.y <= 0.01f || collider.bounds.min.y > playerHeight)
                    continue;

                if (collider.isTrigger) {
                    var portal = collider.GetComponent<Portal>();

                    if (portal) {
                        portals.Add(portal);
                        portalColliders.Add(collider);
                    }

                    continue;
                }

                obstacleColliders.Add(collider);
            }

            var xGridSize = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / checkPrecision);
            var yGridSize = Mathf.CeilToInt((bounds.max.z - bounds.min.z) / checkPrecision);

            var walkableAreas = new bool[xGridSize][];

            for (int i = 0; i < xGridSize; i++) {
                walkableAreas[i] = new bool[yGridSize];
                for (int j = 0; j < yGridSize; j++) {
                    walkableAreas[i][j] = true;
                }
            }

            foreach (var obstacle in obstacleColliders) {
                AddObstacleToGrid(obstacle, walkableAreas, bounds);
            }

            foreach (var portalCollider in portalColliders) {
                AddPortalToGrid(portalCollider, walkableAreas, bounds);
            }

            var walkableTiles = GetAllWalkableTiles(walkableAreas);

            if (walkableTiles.Count == 0) {
                lastValidationResult = ValidationResult.Invalid;
                Debug.LogWarning("MAP INVALID");
                return;
            }

            var portalConnections = GetPortalConnectedTiles(portals, bounds);
            var cluster = FloodFill(walkableTiles, portalConnections);

            if (cluster.Count == walkableTiles.Count) {
                lastValidationResult = ValidationResult.Valid;
                Debug.LogWarning("MAP VALID");
            } else {
                lastValidationResult = ValidationResult.Invalid;
                Debug.LogWarning("MAP INVALID");
            }
        }

        void AddObstacleToGrid(Collider obstacle, bool[][] walkableAreas, Bounds bounds) {
            RegisterInGrid(obstacle, walkableAreas, bounds, false);
        }

        void AddPortalToGrid(Collider collider, bool[][] walkableAreas, Bounds bounds) {
            RegisterInGrid(collider, walkableAreas, bounds, true);
        }

        void RegisterInGrid(Collider collider, bool[][] walkableAreas, Bounds bounds, bool walkable) {
            var minX = collider.bounds.min.x - playerRadius;
            var maxX = collider.bounds.max.x + playerRadius;
            var minY = collider.bounds.min.z - playerRadius;
            var maxY = collider.bounds.max.z + playerRadius;
            var offsetMinX = minX - bounds.min.x;
            var offsetMaxX = maxX - bounds.min.x;
            var offsetMinY = minY - bounds.min.z;
            var offsetMaxY = maxY - bounds.min.z;
            var indexMinX = (int)Mathf.Clamp(offsetMinX / checkPrecision, 0, walkableAreas.Length - 1);
            var indexMaxX = (int)Mathf.Clamp(offsetMaxX / checkPrecision, 0, walkableAreas.Length - 1);
            var indexMinY = (int)Mathf.Clamp(offsetMinY / checkPrecision, 0, walkableAreas[0].Length - 1);
            var indexMaxY = (int)Mathf.Clamp(offsetMaxY / checkPrecision, 0, walkableAreas[0].Length - 1);

            for (int i = indexMinX; i <= indexMaxX; i++)
                for (int j = indexMinY; j <= indexMaxY; j++) {
                    walkableAreas[i][j] = walkable;
                }
        }


        HashSet<Vector2Int> GetAllWalkableTiles(bool[][] walkableAreas) {
            HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
            for (int x = 0; x < walkableAreas.Length; x++)
                for (int y = 0; y < walkableAreas[0].Length; y++)
                    if (walkableAreas[x][y]) {
                        tiles.Add(new Vector2Int(x, y));
                    }

            return tiles;
        }

        Dictionary<Vector2Int, Vector2Int> GetPortalConnectedTiles(List<Portal> portals, Bounds mapBounds) {
            Dictionary<Vector2Int, Vector2Int> connections = new();

            foreach (var portal in portals) {
                var entryPortalCoords = GetTileCoordinates(portal.transform.position, mapBounds);
                var exitPortalCoords = GetTileCoordinates(portal.ExitPortal.transform.position, mapBounds);
                connections.Add(entryPortalCoords, exitPortalCoords);
            }

            return connections;
        }

        Vector2Int GetTileCoordinates(Vector3 position, Bounds mapBounds) {
            var offsetX = position.x - mapBounds.min.x;
            var offsetY = position.z - mapBounds.min.z;
            var indexX = (int)(offsetX / checkPrecision);
            var indexY = (int)(offsetY / checkPrecision);
            return new Vector2Int(indexX, indexY);
        }

        HashSet<Vector2Int> FloodFill(HashSet<Vector2Int> tiles, Dictionary<Vector2Int, Vector2Int> portalConnections) {
            if (tiles.Count == 0)
                return new HashSet<Vector2Int>();

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> processingQueue = new Queue<Vector2Int>();
            var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            Vector2Int start = GetRandomTile(tiles);
            processingQueue.Enqueue(start);

            while (processingQueue.Count > 0) {
                Vector2Int current = processingQueue.Dequeue();
                if (visited.Contains(current)) {
                    continue;
                }

                visited.Add(current);

                foreach (var direction in directions) {
                    Vector2Int next = current + direction;
                    if (tiles.Contains(next) && !visited.Contains(next))
                        processingQueue.Enqueue(next);
                }

                if (portalConnections.ContainsKey(current) && !visited.Contains(portalConnections[current])) {
                    processingQueue.Enqueue(portalConnections[current]);
                }
            }
            return visited;
        }

        private Vector2Int GetRandomTile(HashSet<Vector2Int> tiles) {
            foreach (var tile in tiles)
                return tile;

            return Vector2Int.zero;
        }

        private enum ValidationResult {
            Pending,
            Valid,
            Invalid
        }
    }
}
