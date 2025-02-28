using UnityEngine;

public class ExitButton : UIButton {
    [SerializeField] Main main;

    protected override void HandleClick() {
        main.Quit();
    }
}
