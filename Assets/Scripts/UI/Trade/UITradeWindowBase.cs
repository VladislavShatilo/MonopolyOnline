using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using Zenject;

public class UITradeWindowBase : MonoBehaviour
{
    [Header("Panels & Prefabs")]
    [SerializeField] protected Transform leftPanel;

    [SerializeField] protected Transform rightPanel;
    [SerializeField] protected GameObject companyCardPrefab;

    [Header("UI Elements")]
    [SerializeField] protected float animationDuration = 0.5f;

    [SerializeField] protected RectTransform windowRectTransform;
    [SerializeField] protected TextMeshProUGUI leftTotalAmountText;
    [SerializeField] protected TextMeshProUGUI rightTotalAmountText;
    [SerializeField] protected TextMeshProUGUI leftMoneyText;
    [SerializeField] protected TextMeshProUGUI rightMoneyText;
    [SerializeField] protected TextMeshProUGUI leftPlayerNameText;
    [SerializeField] protected TextMeshProUGUI rightPlayerNameText;

    private DiContainer _container;

    protected TradeOffer currentOffer;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(DiContainer container)
    {
        _container = container;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public virtual void RefreshUI()
    {
        if (currentOffer == null) return;

        ClearCompanies(leftPanel);
        ClearCompanies(rightPanel);

        PlayerData leftPlayer = currentOffer.FromPlayerData;
        PlayerData rightPlayer = currentOffer.ToPlayerData;

        if (leftPlayer == null || rightPlayer == null) return;

        leftPlayerNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(leftPlayer.PlayerColor.ToUnityColor())}>{leftPlayer.Name}</color>";
        rightPlayerNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(rightPlayer.PlayerColor.ToUnityColor())}>{rightPlayer.Name}</color>";

        int leftSum = currentOffer.FromMoney;
        int rightSum = currentOffer.ToMoney;

        leftMoneyText.text = leftSum.ToString();
        rightMoneyText.text = rightSum.ToString();

        PopulateCompanies(leftPanel, currentOffer.FromCompanies, currentOffer.FromPlayerData.Id, ref leftSum);
        PopulateCompanies(rightPanel, currentOffer.ToCompanies, currentOffer.ToPlayerData.Id, ref rightSum);

        leftTotalAmountText.text = leftSum.ToString("N0", CultureInfo.InvariantCulture);
        rightTotalAmountText.text = rightSum.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void ShowWindow() => windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);

    public void HideWindow() => windowRectTransform.DOAnchorPos(new Vector2(0, 450), animationDuration);

    #endregion PUBLIC_METHODS

    #region PROTECTED_METHODS

    protected void PopulateCompanies(Transform panel, List<Company> companies, int playerId, ref int sum)
    {
        if (companies == null) return;
        foreach (var company in companies)
        {
            if (company == null) continue;
            var card = _container.InstantiatePrefabForComponent<UICompanyTrade>(companyCardPrefab, panel);

            // var card = Instantiate(companyCardPrefab, panel);
            card.gameObject.SetActive(false);
            if (card.TryGetComponent<UICompanyTrade>(out var uiCompany))
                uiCompany.SetCompanyTradeUI(company, playerId);
            card.gameObject.SetActive(true);
            sum += company.Price;
        }
    }

    protected virtual void ClearUI()
    {
        currentOffer = null;
        leftTotalAmountText.text = rightTotalAmountText.text = "0";
        leftMoneyText.text = rightMoneyText.text = "0";
        ClearCompanies(leftPanel);
        ClearCompanies(rightPanel);
    }

    protected void ClearCompanies(Transform panel)
    {
        int childCount = panel.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (i > 0) // Удаляем всех, кроме первого (индекс 0)
            {
                Destroy(panel.GetChild(i).gameObject);
            }
        }
    }

    #endregion PROTECTED_METHODS
}