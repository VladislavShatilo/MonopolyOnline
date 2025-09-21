using System;
using UnityEngine;

public interface IUICompanyCellView
{
    int CompanyId();

    void UpdateUI(string name, int price, Color groupColor);

    void UpdateOwner(Color ownerColor);

    void SetRentText(int rent);

    void UpdateBranchStars(int level);

    void ShowBuyFirstBranchButton();

    void ShowBuySellButtons();

    void ShowSellFirstButton();

    void HideAllBranchButtons();

    void ShowMortgageButton();

    void ShowBuyoutButton();

    void MortgageUI();

    void BuyoutUI();

    void SetTurnsText(int turns);
}