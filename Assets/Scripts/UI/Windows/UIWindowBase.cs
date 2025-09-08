using UnityEngine;

public abstract class UIWindowBase: MonoBehaviour
{
    [SerializeField] protected WindowAnimation windowAnimation;
    public WindowAnimation WindowAnimation
    {
        get => windowAnimation;
        set => windowAnimation = value;
    }

    public virtual void ShowWindow()
    {
        windowAnimation?.ShowWindow();
    }

    public virtual void HideWindow()
    {
        windowAnimation?.HideWindow();
    }
    public virtual void HardHideWindow()
    {
        windowAnimation?.HardHideWindow();
    }
}
