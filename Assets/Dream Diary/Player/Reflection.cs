using UnityEngine;
using UnityEngine.Events;

public class Reflection : MonoBehaviour {
    public UnityAction OnPlayerCollision;

    private void OnTriggerEnter(Collider other) {
        var player = other.GetComponent<Player>();
        if (!player)
            return;

        OnPlayerCollision?.Invoke();
    }
}
