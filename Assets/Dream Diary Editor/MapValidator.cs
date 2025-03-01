using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapValidator : EditorWindow {
    float playerRadius;
    float checkPrecision;

    [MenuItem("Sennik/" + nameof(MapValidator))]
    public static void ShowSelf()
        => GetWindow<MapValidator>(nameof(MapValidator)).Show();

    void OnGUI() {
        playerRadius = EditorGUILayout.FloatField(label: nameof(playerRadius), playerRadius);
        checkPrecision = EditorGUILayout.FloatField(label: nameof(checkPrecision), checkPrecision);

        if (checkPrecision <= 0)
            checkPrecision = 0.05f;

        if (GUILayout.Button("Validate"))
            Validate();
    }

    private void Validate() {
        var allColliders = FindObjectsOfType<Collider>(false);
        List<Collider> obstacleColliders = new();
        Bounds bounds = new Bounds();

        foreach (var collider in allColliders) {
            bounds.Encapsulate(collider.bounds);
            if (collider.bounds.center.y + collider.bounds.max.y <= 0.01f)
                continue;

            obstacleColliders.Add(collider);
        }

        var xGridSize = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / checkPrecision);
        var yGridSize = Mathf.CeilToInt((bounds.max.z - bounds.min.z) / checkPrecision);

        var walkableAreas = new bool[xGridSize][];

        for (int i = 0; i < xGridSize; i++) {
            walkableAreas[i] = new bool[yGridSize];
            for (int j = 0; j < yGridSize; j++){
                walkableAreas[i][j] = true;
            }
        }

        foreach (var obstacle in obstacleColliders){
            AddToGrid(obstacle, walkableAreas, bounds);
        }

        /*var walkable = 0;
        var occupied = 0;

        for (int i = 0; i < xGridSize; i++)
            for (int j = 0; j < yGridSize; j++) {
                if (walkableAreas[i][j])
                    walkable++;
                else occupied++;
            }*/

        var walkableTiles = GetAllWalkableTiles(walkableAreas);

        if (walkableTiles.Count == 0) {
            Debug.LogError("MAP INVALID");
            return;
        }

        var cluster = FloodFill(walkableTiles);

        // TODO change logging so instead of console it goes straight to the window with stats

        if (cluster.Count == walkableTiles.Count)
            Debug.LogError("MAP VALID");
        else
            Debug.LogError("MAP INVALID");
    }

    private void AddToGrid(Collider obstacle, bool[][] walkableAreas, Bounds bounds) {
        var minX = obstacle.bounds.min.x - playerRadius;
        var maxX = obstacle.bounds.max.x + playerRadius;
        var minY = obstacle.bounds.min.z - playerRadius;
        var maxY = obstacle.bounds.max.z + playerRadius;
        var offsetMinX = minX - bounds.min.x;
        var offsetMaxX = maxX - bounds.min.x;
        var offsetMinY = minY - bounds.min.z;
        var offsetMaxY = maxY - bounds.min.z;
        var indexMinX = (int)Mathf.Clamp(offsetMinX / checkPrecision, 0, walkableAreas.Length - 1);
        var indexMaxX = (int)Mathf.Clamp(offsetMaxX / checkPrecision, 0, walkableAreas.Length - 1);
        var indexMinY = (int)Mathf.Clamp(offsetMinY / checkPrecision, 0, walkableAreas[0].Length - 1);
        var indexMaxY = (int)Mathf.Clamp(offsetMaxY / checkPrecision, 0, walkableAreas[0].Length - 1);

        for (int i = indexMinX; i <= indexMaxX; i++)
            for (int j = indexMinY; j <= indexMaxY; j++){
                walkableAreas[i][j] = false;
            }
    }

    private HashSet<Vector2Int> GetAllWalkableTiles(bool[][] walkableAreas) {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        for (int x = 0; x < walkableAreas.Length; x++)
            for (int y = 0; y < walkableAreas[0].Length; y++) 
                if (walkableAreas[x][y]) {
                    tiles.Add(new Vector2Int(x, y));
                }

        return tiles;
    }

    private HashSet<Vector2Int> FloodFill(HashSet<Vector2Int> tiles) {
        if (tiles.Count == 0) 
            return new HashSet<Vector2Int>();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> processingQueue = new Queue<Vector2Int>();

        Vector2Int start = GetRandomTile(tiles);
        processingQueue.Enqueue(start);

        while (processingQueue.Count > 0) {
            Vector2Int current = processingQueue.Dequeue();
            if (visited.Contains(current)) {
                continue;
            }

            visited.Add(current);

            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
                Vector2Int next = current + direction;
                if (tiles.Contains(next) && !visited.Contains(next))
                    processingQueue.Enqueue(next);
            }

            // TODO add teleport handling
        }
        return visited;
    }

    private Vector2Int GetRandomTile(HashSet<Vector2Int> tiles) {
        foreach (var tile in tiles)
            return tile;

        return Vector2Int.zero;
    }
}
