using UnityEngine;

public class Main : MonoBehaviour {
    [SerializeField] Player playerPrefab;
    [SerializeField] Reflection reflectionPrefab;

    [SerializeField] Vector2 boardSize;

    Player player;

    float previousMousePosition;

    void Awake() {
        player = InstantiatePlayer();
        InstantiateReflection();
        return;

        Player InstantiatePlayer()
            => Instantiate(playerPrefab, GetRandomPosition(), rotation: Quaternion.identity);

        void InstantiateReflection()
            => Instantiate(reflectionPrefab, GetRandomPosition(), rotation: Quaternion.identity);

        Vector3 GetRandomPosition() {
            return new Vector3(
                GetRandomOffset() * boardSize.x,
                0f,
                GetRandomOffset() * boardSize.y
            );
        }

        float GetRandomOffset()
            => Random.value - 0.5f;
    }

    void Start() {
        previousMousePosition = GetMousePosition();
    }

    void Update() {
        CheckInput();
        return;

        void CheckInput() {
            if (Input.anyKey)
                player.Move(Config.MOVEMENT);

            var mousePosition = GetMousePosition();
            var mouseDelta = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!Mathf.Approximately(mouseDelta, 0f))
                player.Rotate(mouseDelta);
        }
    }

    float GetMousePosition()
        => Input.mousePosition.x;
}
