using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] CharacterController characterController;

    public void Move(float diff) {
        var motion = transform.forward * diff;
        characterController.Move(motion);
    }

    public void Rotate(float diff) {
        transform.Rotate(axis: transform.up, angle: diff);
    }
}
