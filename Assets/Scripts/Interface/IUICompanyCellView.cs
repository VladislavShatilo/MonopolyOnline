using System;
using UnityEngine;

public interface IUICompanyCellView
{
    void Init(int id);
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

    void SetMortgageTurnsText(int turns);
    void HideAllMortgageButtons();
    void LoseCompanyUI(Company company);
}