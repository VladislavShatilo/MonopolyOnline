using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UICompanyTrade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI companyName;
    [SerializeField] private TextMeshProUGUI companyPrice;
    [SerializeField] private Button removeCompanyButton;

    private ITradeService tradeService;
    private Company company;
    private int ownerId;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITradeService tradeService)
    {
        this.tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        if (companyName == null)
            throw new ArgumentNullException(nameof(companyName));
        if (companyPrice == null)
            throw new ArgumentNullException(nameof(companyPrice));
        if (removeCompanyButton == null)
            throw new ArgumentNullException(nameof(removeCompanyButton));
    }

    private void OnEnable()
    {
        if (removeCompanyButton != null)
        {
            removeCompanyButton.onClick.RemoveListener(OnRemoveClicked); 
            removeCompanyButton.onClick.AddListener(OnRemoveClicked);
        }
    }

    private void OnDisable()
    {

        if (removeCompanyButton != null)
            removeCompanyButton.onClick.RemoveListener(OnRemoveClicked);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SetCompanyTradeUI(Company company, int ownerId)
    {
        this.company = company;
        this.ownerId = ownerId;

        companyName.text = company.Name;
        companyPrice.text = company.Price.ToString("N0", CultureInfo.InvariantCulture);
    }

    #endregion PUBLIC_METHODS

    #region CALLBACKS

    private void OnRemoveClicked()
    {
        if (tradeService.IsTradeActive)
        {
            tradeService.RemoveCompanyFromOffer(ownerId, company);
        }
        Destroy(gameObject);
    }

    #endregion CALLBACKS
}