using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    [SerializeField] Portal exitPortal;

    HashSet<Player> teleportedPlayers = new HashSet<Player>();
    Transform mTransform;

    private void Awake() {
        mTransform = transform;
    }

    void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<Player>();
        if (player && !teleportedPlayers.Contains(player)) {
            var localOffset = mTransform.InverseTransformPoint(player.Position);
            var localForward = mTransform.InverseTransformDirection(player.Forward);
            exitPortal.Use(player, localOffset, localForward);
        }
    }

    void OnTriggerExit(Collider other) {
        var player = other.GetComponent<Player>();
        if (player && teleportedPlayers.Contains(player)) {
            teleportedPlayers.Remove(player);
        }
    }

    public void Use(Player player, Vector3 localOffset, Vector3 relativeForward) {
        teleportedPlayers.Add(player);
        var newPosition = mTransform.TransformPoint(localOffset);
        var localForward = mTransform.TransformDirection(relativeForward);
        player.Teleport(newPosition, localForward);
    }
}
