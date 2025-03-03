using System.Threading;
using Multiplayer;
using Runtime.Ads;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Gameplay {
    public class Main : MonoBehaviour {
        [SerializeField] Player playerPrefab;
        [SerializeField] Reflection reflectionPrefab;
        [SerializeField] GameObject victoryPopup;
        [SerializeField] Vector2 boardSize;
        [SerializeField] AdController adController;

        Player player;
        Reflection reflection;
        CancellationTokenSource multiplayerCTS = new();

        void Awake() {
            player = InstantiatePlayer();
            reflection = InstantiateReflection();
            reflection.OnPlayerCollision += ShowVictoryPopup;
            adController.AddShowCondition(new PlayerDistanceTravelledShowAdCondition(player, 10f));
            SetupMultiplayer();
            return;

            void SetupMultiplayer() {
                if (NetworkSettings.Mode == NetworkGameMode.Host)
                    new HostHandler(player, reflection, multiplayerCTS.Token).Initialize();
                else if (NetworkSettings.Mode == NetworkGameMode.Client)
                    new ClientHandler(player, reflection, multiplayerCTS.Token).Initialize();
            }

            Player InstantiatePlayer()
                => Instantiate(playerPrefab, GetRandomPosition(), rotation: Quaternion.identity);

            Reflection InstantiateReflection()
                => Instantiate(reflectionPrefab, GetRandomPosition(), rotation: Quaternion.identity);
        }

        void Update() {
            CheckInput();
            return;

            void CheckInput() {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Quit();

                Vector2 moveDirection = Vector2.zero;
                if (Input.GetKey(KeyCode.W))
                    moveDirection += Vector2.up;
                if (Input.GetKey(KeyCode.S))
                    moveDirection -= Vector2.up;
                if (Input.GetKey(KeyCode.A))
                    moveDirection -= Vector2.right;
                if (Input.GetKey(KeyCode.D))
                    moveDirection += Vector2.right;

                if (moveDirection.magnitude > 0)
                    player.Move(GameplaySettings.Instance.MovementSpeed * Time.deltaTime, moveDirection.normalized);

                if (Input.GetMouseButton(0) && !IsPointerOverUI()) {
                    var input = Input.GetAxis("Mouse X");

                    if (!Mathf.Approximately(input, 0f))
                        player.Rotate(input * GameplaySettings.Instance.Sensitivity);
                }

                bool IsPointerOverUI() {
                    return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                }
            }
        }

        void OnDestroy() {
            reflection.OnPlayerCollision -= ShowVictoryPopup;
        }

        void ShowVictoryPopup() {
            victoryPopup.SetActive(true);
        }

        Vector3 GetRandomPosition() {
            return new Vector3(
                GetRandomOffset() * boardSize.x,
                0f,
                GetRandomOffset() * boardSize.y
            );
        }

        float GetRandomOffset()
            => UnityEngine.Random.value - 0.5f;

        public void Restart() {
            player.transform.position = GetRandomPosition();
            reflection.transform.position = GetRandomPosition();
            victoryPopup.SetActive(false);
        }

        public void Quit() {
            multiplayerCTS.Cancel();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }
    }
}
