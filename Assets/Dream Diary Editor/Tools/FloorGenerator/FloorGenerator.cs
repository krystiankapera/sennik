using System.Collections.Generic;
using Runtime.Gameplay;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Tools.FloorGenerator {
    public class FloorGenerator : EditorWindow {
        const int MAX_ELEMENT_CREATION_ATTEMPTS = 10;

        MapGeneratorConfig config;
        int generatedSize;
        int wallsCount;
        int portalsCount;
        int obstaclesCount;

        Transform parent;
        float wallElementLength;
        float wallElementThickness;
        Quaternion horizontalWallRotation;
        Quaternion verticalWallRotation;

        List<Rect> horizontalWallConstraints = new();
        List<Rect> verticalWallConstraints = new();
        List<Rect> portalConstraints = new();
        List<Rect> obstacleConstraints = new();

        [MenuItem("Sennik/" + nameof(FloorGenerator))]
        public static void ShowSelf()
            => GetWindow<FloorGenerator>(nameof(FloorGenerator)).Show();

        void OnGUI() {
            config = EditorGUILayout.ObjectField(label: nameof(config), config, typeof(MapGeneratorConfig), false) as MapGeneratorConfig;
            generatedSize = EditorGUILayout.IntField(label: nameof(generatedSize), generatedSize);
            wallsCount = EditorGUILayout.IntField(label: nameof(wallsCount), wallsCount);
            portalsCount = EditorGUILayout.IntField(label: nameof(portalsCount), portalsCount);
            obstaclesCount = EditorGUILayout.IntField(label: nameof(obstaclesCount), obstaclesCount);

            if (GUILayout.Button("Generate"))
                Generate();
        }

        void Generate() {
            parent = new GameObject("Generated Map").transform;

            // TODO parent each type of map element into a new GO for easy enable/disable
            GenerateFloor();
            InitializeWallProperties();
            GenerateOuterWalls();
            GeneratePortals();
            GenerateRandomWalls();
            GenerateObstacles();
            ClearConstraints();
        }

        void GenerateFloor() {
            var offset = new Vector2(
                -config.FloorPrefabSize.x / 2 * generatedSize,
                -config.FloorPrefabSize.y / 2 * generatedSize
            );
            for (var x = 0; x < generatedSize; x++)
                for (var y = 0; y < generatedSize; y++) {
                    var position = new Vector3(
                        offset.x + (x + 0.5f) * config.FloorPrefabSize.x,
                        0,
                        offset.y + (y + 0.5f) * config.FloorPrefabSize.y
                    );
                    Instantiate(config.FloorPrefab, position, rotation: Quaternion.identity, parent);
                }
        }
        void InitializeWallProperties() {
            if (config.WallPrefabSize.x >= config.WallPrefabSize.y) {
                horizontalWallRotation = Quaternion.identity;
                verticalWallRotation = Quaternion.AngleAxis(90f, Vector3.up);
                wallElementLength = config.WallPrefabSize.x;
                wallElementThickness = config.WallPrefabSize.y;
            } else {
                horizontalWallRotation = Quaternion.AngleAxis(90f, Vector3.up);
                verticalWallRotation = Quaternion.identity;
                wallElementLength = config.WallPrefabSize.y;
                wallElementThickness = config.WallPrefabSize.x;
            }
        }

        void GenerateOuterWalls() {
            var minX = -config.FloorPrefabSize.x / 2f * generatedSize;
            var maxX = config.FloorPrefabSize.x / 2f * generatedSize;
            var minY = -config.FloorPrefabSize.y / 2f * generatedSize;
            var maxY = config.FloorPrefabSize.y / 2f * generatedSize;
            GenerateHorizontalWall(minY, minX, maxX);
            GenerateHorizontalWall(maxY, minX, maxX);
            GenerateVerticalWall(minX, minY, maxY);
            GenerateVerticalWall(maxX, minY, maxY);
        }
        void GeneratePortals() {
            List<Portal> createdPortals = new ();

            for (int i = 0; i < portalsCount; i++) {
                var remainingAttempts = MAX_ELEMENT_CREATION_ATTEMPTS;
                while (remainingAttempts > 0) {
                    var portal = GeneratePortal();

                    if (!portal) {
                        remainingAttempts--;
                        continue;
                    }

                    createdPortals.Add(portal);
                    break;
                }
            }

            for (int i = 1; i < createdPortals.Count; i++) {
                createdPortals[i].SetExitPortal(createdPortals[i-1]);
            }

            createdPortals[0].SetExitPortal(createdPortals[createdPortals.Count - 1]);
        }

        void GenerateRandomWalls() {
            for (int i = 0; i < wallsCount; i++) {
                var remainingAttempts = MAX_ELEMENT_CREATION_ATTEMPTS;

                while (remainingAttempts > 0) {
                    if (GenerateRandomWall())
                        break;

                    remainingAttempts--;
                }
            }
        }

        void GenerateObstacles() {
            for (int i = 0; i < obstaclesCount; i++) {
                var remainingAttempts = MAX_ELEMENT_CREATION_ATTEMPTS;

                while (remainingAttempts > 0) {
                    if (GenerateRandomObstacle())
                        break;

                    remainingAttempts--;
                }
            }
        }

        Portal GeneratePortal() {
            var minX = -config.FloorPrefabSize.x / 2 * generatedSize;
            var maxX = config.FloorPrefabSize.x / 2 * generatedSize;
            var rotate = Random.value > 0.5;
            var portalSizeX = rotate ? config.PortalPrefabSize.y : config.PortalPrefabSize.x;
            var portalSizeY = rotate ? config.PortalPrefabSize.x : config.PortalPrefabSize.y;

            var randomX = Random.Range(minX, maxX);
            var yRange = GetAvailableYRangeForPortal(randomX - portalSizeX / 2, randomX + portalSizeX / 2, portalSizeY);

            if (yRange == Vector2.zero) {
                return null;
            }

            var randomY = Random.Range(yRange.x, yRange.y);
            var rotation = rotate ? Quaternion.AngleAxis(90f, Vector3.up) : Quaternion.identity;
            var portalGameObject = Instantiate(config.PortalPrefab, new Vector3(randomX, 0f, randomY), rotation: rotation, parent);

            var constraintRect = new Rect(randomX - portalSizeX / 2 - config.PortalSpacing,
                                            randomY - portalSizeY / 2 - config.PortalSpacing,
                                            portalSizeX + 2 * config.PortalSpacing,
                                            portalSizeY + 2 * config.PortalSpacing);

            portalConstraints.Add(constraintRect);
            horizontalWallConstraints.Add(constraintRect);
            verticalWallConstraints.Add(constraintRect);
            obstacleConstraints.Add(constraintRect);

            return portalGameObject.GetComponent<Portal>();
        }

        bool GenerateRandomWall() {
            // TODO perhaps move these as class properties
            var minX = -config.FloorPrefabSize.x / 2f * generatedSize;
            var maxX = config.FloorPrefabSize.x / 2f * generatedSize;
            var minY = -config.FloorPrefabSize.y / 2f * generatedSize;
            var maxY = config.FloorPrefabSize.y / 2f * generatedSize;

            var vertical = Random.value > 0.5f;

            if (vertical) {
                var randomX = Random.Range(minX, maxX);
                var yRange = GetAvailableYRangeForVerticalWall(randomX - wallElementLength / 2f, randomX + wallElementLength / 2f, wallElementThickness);
                if (yRange == Vector2.zero)
                    return false;

                var randomLength = GetRandomWallLength(yRange.y - yRange.x);
                var randomStart = Random.Range(yRange.x, yRange.y - randomLength);
                GenerateVerticalWall(randomX, randomStart, randomStart + randomLength);
            } else {
                var randomY = Random.Range(minY, maxY);
                var xRange = GetAvailableXRangeForHorizontalWall(randomY - wallElementLength / 2f, randomY + wallElementLength / 2f, wallElementThickness);
                if (xRange == Vector2.zero)
                    return false;

                var randomLength = GetRandomWallLength(xRange.y - xRange.x);
                var randomStart = Random.Range(xRange.x, xRange.y - randomLength);
                GenerateHorizontalWall(randomY, randomStart, randomStart + randomLength);
            }

            return true;
        }

        void GenerateHorizontalWall(float y, float minX, float maxX) {
            var wallPivotOffset = wallElementLength / 2f;
            var currentPositionX = minX + wallPivotOffset;

            while (maxX - currentPositionX + wallPivotOffset >= wallElementLength) {
                var position = new Vector3(currentPositionX, 0, y);
                Instantiate(config.WallPrefab, position, rotation: horizontalWallRotation, parent);
                currentPositionX += wallElementLength;
            }

            horizontalWallConstraints.Add(new Rect(minX - config.WallSpacing,
                                            y - wallElementThickness / 2f - config.WallSpacing,
                                            maxX - minX + 2 * config.WallSpacing,
                                            2f * config.WallSpacing + wallElementThickness));

            portalConstraints.Add(new Rect(minX - config.PortalSpacing,
                                            y - wallElementThickness / 2f - config.PortalSpacing,
                                            maxX - minX + 2f * config.PortalSpacing,
                                            2f * config.PortalSpacing + wallElementThickness));

            obstacleConstraints.Add(new Rect(minX,
                                            y - wallElementThickness / 2f,
                                            maxX - minX,
                                            wallElementThickness));
        }

        void GenerateVerticalWall(float x, float minY, float maxY) {
            var wallPivotOffset = wallElementLength / 2f;
            var currentPositionY = minY + wallPivotOffset;

            while (maxY - currentPositionY + wallPivotOffset >= wallElementLength) {
                var position = new Vector3(x, 0, currentPositionY);
                Instantiate(config.WallPrefab, position, rotation: verticalWallRotation, parent);
                currentPositionY += wallElementLength;
            }

            verticalWallConstraints.Add(new Rect(x - config.WallSpacing - wallElementThickness / 2f,
                                            minY - config.WallSpacing,
                                            2f * config.WallSpacing + wallElementThickness,
                                            maxY - minY + 2f * config.WallSpacing));

            portalConstraints.Add(new Rect(x - config.PortalSpacing - wallElementThickness / 2f,
                                            minY - config.PortalSpacing,
                                            2f * config.PortalSpacing + wallElementThickness,
                                            maxY - minY + 2f * config.PortalSpacing));

            obstacleConstraints.Add(new Rect(x - wallElementThickness / 2f,
                                            minY,
                                            wallElementThickness,
                                            maxY - minY));
        }

        bool GenerateRandomObstacle() {
            var randomObstacle = config.Obstacles[Random.Range(0, config.Obstacles.Count)];
            var rotate = Random.value > 0.5;
            var rotation = rotate ? Quaternion.AngleAxis(90f, Vector3.up) : Quaternion.identity;

            var sizeX = rotate ? randomObstacle.Size.y : randomObstacle.Size.x;
            var sizeY = rotate ? randomObstacle.Size.x : randomObstacle.Size.y;
            var minX = -config.FloorPrefabSize.x / 2 * generatedSize + sizeX / 2;
            var maxX = config.FloorPrefabSize.x / 2 * generatedSize - sizeX / 2;

            var randomX = Random.Range(minX, maxX);
            var yRange = GetAvailableYRangeForObstacle(randomX - sizeX / 2, randomX + sizeX / 2, sizeY);

            if (yRange == Vector2.zero)
                return false;

            var randomY = Random.Range(yRange.x, yRange.y);
            Instantiate(randomObstacle.Prefab, new Vector3(randomX, 0f, randomY), rotation: rotation, parent);

            obstacleConstraints.Add(new Rect(randomX - sizeX / 2,
                                            randomY - sizeY / 2,
                                            sizeX,
                                            sizeY));

            return true;
        }

        Vector2 GetAvailableYRangeForPortal(float xMin, float xMax, float ySize) {
            List<Vector2> possibleRanges = new();
            possibleRanges.Add(new Vector2(-generatedSize / 2f + ySize / 2f, generatedSize / 2f - ySize / 2f));

            foreach (var constraint in portalConstraints) {
                if (!(xMax < constraint.min.x || xMin > constraint.max.x)) {
                    SplitRanges(possibleRanges, constraint.min.y, constraint.max.y, ySize);
                }

                if (possibleRanges.Count == 0)
                    return Vector2.zero;
            }

            var randomIndex = Random.Range(0, possibleRanges.Count);
            return possibleRanges[randomIndex];
        }

        Vector2 GetAvailableYRangeForVerticalWall(float xMin, float xMax, float ySize) {
            List<Vector2> possibleRanges = new();
            possibleRanges.Add(new Vector2(-generatedSize / 2f + ySize / 2f, generatedSize / 2f - ySize / 2f));

            foreach (var constraint in verticalWallConstraints) {
                if (!(xMax < constraint.min.x || xMin > constraint.max.x)) {
                    SplitRanges(possibleRanges, constraint.min.y, constraint.max.y, ySize);
                }

                if (possibleRanges.Count == 0)
                    return Vector2.zero;
            }

            var randomIndex = Random.Range(0, possibleRanges.Count);
            return possibleRanges[randomIndex];
        }

        Vector2 GetAvailableXRangeForHorizontalWall(float yMin, float yMax, float xSize) {
            List<Vector2> possibleRanges = new();
            possibleRanges.Add(new Vector2(-generatedSize / 2f + xSize / 2f, generatedSize / 2f - xSize / 2f));

            foreach (var constraint in horizontalWallConstraints) {
                if (!(yMax < constraint.min.y || yMin > constraint.max.y))
                    SplitRanges(possibleRanges, constraint.min.x, constraint.max.x, xSize);

                if (possibleRanges.Count == 0)
                    return Vector2.zero;
            }

            var randomIndex = Random.Range(0, possibleRanges.Count);
            return possibleRanges[randomIndex];
        }

        Vector2 GetAvailableYRangeForObstacle(float xMin, float xMax, float ySize) {
            List<Vector2> possibleRanges = new();
            possibleRanges.Add(new Vector2(-generatedSize / 2f + ySize / 2f, generatedSize / 2f - ySize / 2f));

            foreach (var constraint in obstacleConstraints) {
                if (!(xMax < constraint.min.x || xMin > constraint.max.x)) {
                    SplitRanges(possibleRanges, constraint.min.y, constraint.max.y, ySize);
                }

                if (possibleRanges.Count == 0)
                    return Vector2.zero;
            }

            var randomIndex = Random.Range(0, possibleRanges.Count);
            return possibleRanges[randomIndex];
        }

        void SplitRanges(List<Vector2> ranges, float min, float max, float objectSize) {
            List<Vector2> toAdd = new();
            List<Vector2> toRemove = new();

            foreach (var range in ranges) {
                var removeRange = false;
                if (min < range.y && min > range.x) {
                    if (min - range.x >= objectSize)
                        toAdd.Add(new Vector2(range.x + objectSize / 2f, min - objectSize / 2f));

                    removeRange = true;
                }
                if (max > range.x && max < range.y) {
                    if (range.y - max >= objectSize)
                        toAdd.Add(new Vector2(max + objectSize / 2f, range.y - objectSize / 2f));

                    removeRange = true;
                }
                if (max > range.y && min < range.x)
                    removeRange = true;

                if (removeRange)
                    toRemove.Add(range);
            }

            foreach (var item in toRemove) {
                ranges.Remove(item);
            }

            foreach (var item in toAdd) {
                ranges.Add(item);
            }
        }

        float GetRandomWallLength(float maxLength) {
            var maxElements = (int)(maxLength / wallElementLength);
            var random = Random.value;

            if (random < 0.03)
                return Random.Range(maxElements / 3, maxElements) * wallElementLength;

            if (random < 0.1)
                return Random.Range(maxElements / 6, maxElements / 3) * wallElementLength;

            return Random.Range(1, Mathf.Max(maxElements / 6, 1)) * wallElementLength;
        }

        void ClearConstraints() {
            horizontalWallConstraints.Clear();
            verticalWallConstraints.Clear();
            portalConstraints.Clear();
            obstacleConstraints.Clear();
        }
    }
}
