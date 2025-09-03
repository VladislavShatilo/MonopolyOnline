using Photon.Pun;
using Photon.Realtime;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPayRentWindow : UIWindowBase<UIPayRentWindow>
{
    [Header("UI Elements")]
    [SerializeField] private Button payRentButton;
    [SerializeField] private TextMeshProUGUI payButtonText;
    [SerializeField] private Button cantPayRentButton;
    [SerializeField] private TextMeshProUGUI cantPayRentText;

    private UIPayRentPresenter presenter;
    public Button PayRentButton => payRentButton;
    public TextMeshProUGUI PayButtonText => payButtonText;
    public Button CantPayRentButton => cantPayRentButton;
    public TextMeshProUGUI CantPayRentText => cantPayRentText;

    public Button PayRentButtonSetter { set => payRentButton = value; }
    public TextMeshProUGUI PayButtonTextSetter { set => payButtonText = value; }
    public Button CantPayRentButtonSetter { set => cantPayRentButton = value; }
    public TextMeshProUGUI CantPayRentTextSetter { set => cantPayRentText = value; }
    public WindowAnimation WindowAnimationSetter { set => windowAnimation = value; }
    protected override void OnEnable()
    {
        base.OnEnable();
        if (payRentButton != null)
        {
            payRentButton.onClick.AddListener(HandlePayRentClicked);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable(); 
        if (payRentButton != null)
        {
            payRentButton.onClick.RemoveListener(HandlePayRentClicked);
        }
    }

    public void Show(PlayerData player, int cellIndex, float rent)
    {
        presenter = new UIPayRentPresenter(player, cellIndex, rent);
        ApplyPresenterState();
        windowAnimation.ShowWindow();
    }

    private void ApplyPresenterState()
    {
        var state = presenter.GetState();

        payButtonText.text = state.RentText;
        cantPayRentText.text = state.RentText;

        payRentButton.gameObject.SetActive(state.CanPay);
        cantPayRentButton.gameObject.SetActive(!state.CanPay);
    }

    private void HandlePayRentClicked()
    {
        presenter.OnPayRent();
        HideWindow();
    }
}
