using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class UIButton : MonoBehaviour {
    Button button;

    void Awake() {
        button = GetComponent<Button>();
    }

    void OnEnable() {
        button.onClick.AddListener(HandleClick);
    }

    void OnDisable() {
        button.onClick.RemoveListener(HandleClick);
    }

    protected abstract void HandleClick();
}
