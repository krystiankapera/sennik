using UnityEngine;

public class ReplayButton : UIButton {
    [SerializeField] Main main;

    protected override void HandleClick() {
        main.Restart();
    }
}
