using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Runtime.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Editor.Tools.MapValidator {
    public class MapValidator : EditorWindow {
        float playerHeight = 1f;
        float playerRadius = 0.3f;
        float checkPrecision = 0.1f;

        ValidationResult validationResult = ValidationResult.None;
        GUIStyle validLabelStyle;
        GUIStyle invalidLabelStyle;
        GUIStyle pendingLabelStyle;
        bool stylesSetUp;

        [MenuItem("Sennik/" + nameof(MapValidator))]
        public static void ShowSelf()
            => GetWindow<MapValidator>(nameof(MapValidator)).Show();

        void OnGUI() {
            playerHeight = EditorGUILayout.FloatField(label: nameof(playerHeight), playerHeight);
            playerRadius = EditorGUILayout.FloatField(label: nameof(playerRadius), playerRadius);
            checkPrecision = EditorGUILayout.FloatField(label: nameof(checkPrecision), checkPrecision);

            if (checkPrecision <= 0)
                checkPrecision = 0.1f;

            if (GUILayout.Button("Validate") && validationResult != ValidationResult.Pending)
                Validate();

            if (validationResult != ValidationResult.None)
                ShowValidationMessage();
        }

        async void ValidateAsync() {
            validationResult = ValidationResult.Pending;
            await Task.Run(() => Validate());
        }

        void Validate() {
            var allColliders = FindObjectsOfType<Collider>(false);
            List<Collider> obstacleColliders = new();
            List<Collider> portalTriggers = new();
            List<Portal> portals = new();
            Bounds mapBounds = new();

            foreach (var collider in allColliders) {
                mapBounds.Encapsulate(collider.bounds);

                if (collider.bounds.max.y <= 0.01f || collider.bounds.min.y > playerHeight)
                    continue;

                if (collider.isTrigger) {
                    var portal = collider.GetComponent<Portal>();

                    if (portal) {
                        portals.Add(portal);
                        portalTriggers.Add(collider);
                    }

                    continue;
                }

                obstacleColliders.Add(collider);
            }

            var mapGrid = CreateGrid(mapBounds);

            foreach (var obstacle in obstacleColliders)
                AddObstacleToGrid(obstacle, mapGrid, mapBounds);

            foreach (var portalCollider in portalTriggers)
                AddPortalToGrid(portalCollider, mapGrid, mapBounds);

            var walkableTiles = GetAllWalkableTiles(mapGrid);

            if (walkableTiles.Count == 0) {
                validationResult = ValidationResult.Invalid;
                Debug.LogWarning("MAP INVALID");
                return;
            }

            var portalConnections = GetPortalConnectedTiles(portals, mapBounds);
            var clusteredTiles = FloodFill(walkableTiles, portalConnections);

            if (clusteredTiles.Count == walkableTiles.Count) {
                validationResult = ValidationResult.Valid;
                Debug.LogWarning("MAP VALID");
            } else {
                validationResult = ValidationResult.Invalid;
                Debug.LogWarning("MAP INVALID");
            }
        }

        bool[][] CreateGrid(Bounds bounds) {
            var xGridSize = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / checkPrecision);
            var yGridSize = Mathf.CeilToInt((bounds.max.z - bounds.min.z) / checkPrecision);

            var grid = new bool[xGridSize][];

            for (int i = 0; i < xGridSize; i++) {
                grid[i] = new bool[yGridSize];
                for (int j = 0; j < yGridSize; j++) {
                    grid[i][j] = true;
                }
            }

            return grid;
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

        Vector2Int GetRandomTile(HashSet<Vector2Int> tiles) {
            foreach (var tile in tiles)
                return tile;

            return Vector2Int.zero;
        }

        void ShowValidationMessage() {
            GUILayout.Label(GetValidationMessage(), GetLabelStyle(validationResult));
        }

        string GetValidationMessage() {
            if (validationResult == ValidationResult.Pending)
                return "Validation in progress...";

            return new StringBuilder().Append("Validation Result: ").Append(validationResult.ToString()).ToString();
        }

        GUIStyle GetLabelStyle(ValidationResult result) {
            if (!stylesSetUp)
                SetupLabelStyles();

            switch (result) {
                case ValidationResult.None:
                    break;
                case ValidationResult.Pending:
                    return pendingLabelStyle;
                case ValidationResult.Valid:
                    return validLabelStyle;
                case ValidationResult.Invalid:
                    return invalidLabelStyle;
                default:
                    break;
            }

            return GUIStyle.none;
        }

        void SetupLabelStyles() {
            pendingLabelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 24,
                normal = { textColor = Color.white }
            };

            validLabelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 24,
                normal = { textColor = Color.green }
            };

            invalidLabelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 24,
                normal = { textColor = Color.red }
            };

            stylesSetUp = true;
        }

        private enum ValidationResult {
            None,
            Pending,
            Valid,
            Invalid
        }
    }
}
