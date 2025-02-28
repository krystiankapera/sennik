using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform modelRoot;

    Transform mTransform;

    public Vector3 Position => mTransform.position;
    public Vector3 Forward => mTransform.forward;

    void Awake() {
        mTransform = transform;
    }

    public void Move(float diff, Vector2 moveDirection) {
        var motion = (mTransform.forward * moveDirection.y + mTransform.right * moveDirection.x) * diff;
        characterController.Move(motion);
        modelRoot.LookAt(mTransform.position + motion);
    }

    public void Rotate(float diff) {
        mTransform.Rotate(axis: transform.up, angle: diff);
    }

    public void Teleport(Vector3 position, Vector3 forward) {
        characterController.enabled = false;
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        characterController.enabled = true;
    }
}
