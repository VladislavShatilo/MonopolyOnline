using UnityEngine;

public abstract class UIWindowBase: MonoBehaviour
{
    [SerializeField] protected WindowAnimation windowAnimation;

    #region PUBLIC_METHODS

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

    #endregion PUBLIC_METHODS

}
