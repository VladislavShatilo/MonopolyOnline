using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICompanyTrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI companyName; 
    [SerializeField] private TextMeshProUGUI companyPrice;
    [SerializeField] private Button removeCompanyButton;

    public Company company;
    public int ownerId;
    public void SetCompanyTradeUI(Company company,int ownerId)
    {
        this.company = company; 
        this.ownerId = ownerId;

        companyName.text = company.Name;
        companyPrice.text = company.Price.ToString("N0", CultureInfo.InvariantCulture);
    }
    private void OnEnable()
    {
        removeCompanyButton.onClick.AddListener(OnRemoveClicked);
    }
    private void OnDisable()
    {
        removeCompanyButton.onClick.RemoveListener(OnRemoveClicked);

    }
    private void OnRemoveClicked()
    {
        if (TradeManager.Instance != null && TradeManager.Instance.IsTradeActive)
        {
            // Удаляем компанию из текущего предложения
            TradeManager.Instance.RemoveCompanyFromOffer(ownerId, company);
        }
        // Можно сразу уничтожить карточку из UI
        Destroy(gameObject);
    }
}
