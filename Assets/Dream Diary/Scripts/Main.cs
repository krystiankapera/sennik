using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Multiplayer;
using UnityEditor;
using UnityEngine;

namespace Runtime.Gameplay {
    public class Main : MonoBehaviour {
        [SerializeField] Player playerPrefab;
        [SerializeField] Reflection reflectionPrefab;
        [SerializeField] GameObject victoryPopup;
        [SerializeField] Vector2 boardSize;

        Player player;
        Reflection reflection;

        CancellationTokenSource multiplayerCTS = new();
        Client client;
        Host host;
        GamePeer peer;

        void Awake() {
            SetupMultiplayer();
            player = InstantiatePlayer();
            reflection = InstantiateReflection();
            reflection.OnPlayerCollision += ShowVictoryPopup;
            return;

            void SetupMultiplayer() {
                host = new(port: 1410);
                var peer = new Peer();
                peer.OnDataReceived += HandleDataReceived;
                host.Run(peer, multiplayerCTS.Token).Forget();
                this.peer = peer;

                void HandleDataReceived(byte[] data) {
                    var encoding = Encoding.UTF8;
                    Debug.Log($"Received data: {encoding.GetString(data)}");
                    peer.SendData(encoding.GetBytes("General Kenobi"));
                }
            }

            // void SetupMultiplayer() {
            //     client = new(ip: "127.0.0.1", port: 1410);
            //     var peer = new Peer();
            //     peer.OnDataReceived += HandleDataReceived;
            //     client.Run(peer, multiplayerCTS.Token).Forget();
            //     this.peer = peer;
            //     PingHost(multiplayerCTS.Token).Forget();

            //     void HandleDataReceived(byte[] data) {
            //         Debug.Log($"Received data: {Encoding.UTF8.GetString(data)}");
            //     }

            //     async UniTask PingHost(CancellationToken cancellationToken) {
            //         while (!cancellationToken.IsCancellationRequested) {
            //             await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);
            //             peer.SendData(Encoding.UTF8.GetBytes("Hello there"));
            //         }
            //     }
            // }

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
                    player.Move(Config.MOVEMENT * Time.deltaTime, moveDirection.normalized);

                if (Input.GetMouseButton(0)) {
                    var input = Input.GetAxis("Mouse X");

                    if (!Mathf.Approximately(input, 0f))
                        player.Rotate(input * 2f);
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

        float GetMousePosition()
            => Input.mousePosition.x;

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
