using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class    UITradeWindowBase : MonoBehaviour
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

    protected TradeOffer currentOffer;

    public virtual void RefreshUI()
    {
        if (currentOffer == null) return;

        ClearCompanies(leftPanel);
        ClearCompanies(rightPanel);

        PlayerData leftPlayer = GameManager.Instance.GetPlayerById(currentOffer.FromPlayerId);
        PlayerData rightPlayer = GameManager.Instance.GetPlayerById(currentOffer.ToPlayerId);

        if (leftPlayer == null || rightPlayer == null) return;

        leftPlayerNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(leftPlayer.playerColor)}>{leftPlayer.Name}</color>";
        rightPlayerNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(rightPlayer.playerColor)}>{rightPlayer.Name}</color>";

        int leftSum = currentOffer.FromMoney;
        int rightSum = currentOffer.ToMoney;

        leftMoneyText.text = leftSum.ToString();
        rightMoneyText.text = rightSum.ToString();

        PopulateCompanies(leftPanel, currentOffer.FromCompanies, currentOffer.FromPlayerId, ref leftSum);
        PopulateCompanies(rightPanel, currentOffer.ToCompanies, currentOffer.ToPlayerId, ref rightSum);

        leftTotalAmountText.text = leftSum.ToString("N0", CultureInfo.InvariantCulture);
        rightTotalAmountText.text = rightSum.ToString("N0", CultureInfo.InvariantCulture);
    }

    protected void PopulateCompanies(Transform panel, List<Company> companies, int playerId, ref int sum)
    {
        if (companies == null) return;
        foreach (var company in companies)
        {
            if (company == null) continue;
            var card = Instantiate(companyCardPrefab, panel);
            card.SetActive(false);
            if (card.TryGetComponent<UICompanyTrade>(out var uiCompany))
                uiCompany.SetCompanyTradeUI(company, playerId);
            card.SetActive(true);
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
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowWindow() => windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
    public void HideWindow() => windowRectTransform.DOAnchorPos(new Vector2(0, 450), animationDuration);
}
