using UnityEditor;
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
        CheckPortals();
        CheckInput();
        return;

        void CheckInput() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
                Application.Quit();
            }

            if (Input.anyKey)
                player.Move(Config.MOVEMENT);

            var mousePosition = GetMousePosition();
            var mouseDelta = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!Mathf.Approximately(mouseDelta, 0f))
                player.Rotate(mouseDelta);
        }

        void CheckPortals() {
            var trigger = player.enteredTrigger;
            if (trigger == null)
                return;

            var portal = trigger.GetComponent<Portal>();
            if (portal != null)
                UsePortal(portal);

            player.enteredTrigger = null;
        }

        void UsePortal(Portal portal) {
            var positionDiff = portal.transform.position - player.transform.position;
            Debug.Log($"{nameof(UsePortal)}: from {portal.name} to {portal.GetExitPortal().name}"); //TODO remove once the issue is fixed
            player.transform.position = portal.GetExitPortal().transform.position - positionDiff * 2f;
        }
    }

    float GetMousePosition()
        => Input.mousePosition.x;
}
