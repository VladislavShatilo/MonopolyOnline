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
    private Company company;
    private int id;

    public event Action<int> OnCompanyClicked;

    public void Init(int id)
    {
        this.id = id;
        showWindowButton.onClick.AddListener(OnClick);
    }
    private void OnClick()
    {
        company = CompanyDatabase.Instance.GetCompanyById(id);
        if (TradeManager.Instance != null && TradeManager.Instance.IsTradeActive && TradeManager.Instance.CurrentOffer.FromPlayerData.id == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log("OnClick");

            int ownerId = company.OwnerId;

            // Проверяем, есть ли компания уже в текущем предложении
            bool isAlreadyInOffer = TradeManager.Instance.CurrentOffer != null &&
                                    (TradeManager.Instance.CurrentOffer.FromCompanies.Contains(company) ||
                                     TradeManager.Instance.CurrentOffer.ToCompanies.Contains(company));

            if (isAlreadyInOffer)
            {
                // Если есть — удаляем
                TradeManager.Instance.RemoveCompanyFromOffer(ownerId, company);
            }
            else
            {
                // Если нет — добавляем
                TradeManager.Instance.AddCompanyToOffer(ownerId, company);
            }
        }
        else
        {
            OnCompanyClicked?.Invoke(id);
        }
    }
}
