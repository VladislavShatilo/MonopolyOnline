using UnityEngine;

public abstract class UIWindowBase: MonoBehaviour
{
    [SerializeField] protected WindowAnimation windowAnimation;

    #region PUBLIC_METHODS

    public virtual void ShowWindow()
    {
        if (windowAnimation != null)
        {
            windowAnimation.ShowWindow();
        }
    }

    public virtual void HideWindow()
    {
        if (windowAnimation != null)
        {
            windowAnimation.HideWindow();
        }
    }
    public virtual void HardHideWindow()
    {
        if (windowAnimation != null)
        {
            windowAnimation.HardHideWindow();

        }
    }

    #endregion PUBLIC_METHODS

}
