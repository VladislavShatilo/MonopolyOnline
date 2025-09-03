using UnityEngine;

public abstract class UIWindowBase<T> : MonoBehaviour where T : UIWindowBase<T>
{
    public static T Instance { get; private set; }

    [SerializeField] protected WindowAnimation windowAnimation;
    public WindowAnimation WindowAnimation
    {
        get => windowAnimation;
        set => windowAnimation = value;
    }
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = (T)this;
    }
    protected virtual void OnEnable()
    {
        SubscribeEvents();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeEvents();
    }

    protected virtual void SubscribeEvents() { }
    protected virtual void UnsubscribeEvents() { }
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
