using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;
public enum StatsWindowPosition
{
    Up,
    LeftUp,
    LeftDown,
    Down,
    RightDown,
    RightUp

}

public class CompanyWindowPopup : MonoBehaviour
{
    [SerializeField] private Button showWindowButton;

    private int companyId;
    private CompanyOfferInteractor interactor;

    public event Action<int> OnCompanyClicked; // если не в трейде

    public void Init(int id)
    {
        companyId = id;
        showWindowButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
       // bool handled = interactor.TryToggleCompanyInOffer(companyId);

       // if (!handled)
        {
            OnCompanyClicked?.Invoke(companyId);
        }
    }
}
