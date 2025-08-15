using UnityEngine;

public class CompanyUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform companyInfoWindow;
    [SerializeField] private RectTransform fieldCompanyInfoWindow;
    [SerializeField] private RectTransform diceCompanyInfoWindow;

    [SerializeField] private UICompanyStats statsCompanyPanel;
    [SerializeField] private UIFieldCompanyStats statsFieldCompanyPanel;
    [SerializeField] private UIDiceStats statsDiceCompanyPanel;

    [SerializeField] private float cellWidth = 70;
    [SerializeField] private float offset = 80;

    private void ConfigureWindowPosition(RectTransform window, RectTransform companyCell, StatsWindowPosition position)
    {
        Vector2 newPos = companyCell.anchoredPosition;
        Vector2 pivot = Vector2.zero;

        switch (position)
        {
            case StatsWindowPosition.Up:
                newPos.y -= offset;
                pivot = new Vector2(0.5f, 1f);
                break;
            case StatsWindowPosition.Down:
                newPos.y += offset;
                pivot = new Vector2(0.5f, 0f);
                break;
            case StatsWindowPosition.LeftUp:
                newPos.x += offset;
                newPos.y += cellWidth / 2;
                pivot = new Vector2(0f, 1f);
                break;
            case StatsWindowPosition.LeftDown:
                newPos.x += offset;
                newPos.y -= cellWidth / 2;
                pivot = new Vector2(0f, 0f);
                break;
            case StatsWindowPosition.RightUp:
                newPos.x -= offset;
                newPos.y += cellWidth / 2;
                pivot = new Vector2(1f, 1f);
                break;
            case StatsWindowPosition.RightDown:
                newPos.x -= offset;
                newPos.y -= cellWidth / 2;
                pivot = new Vector2(1f, 0f);
                break;
        }

        window.pivot = pivot;
        window.anchoredPosition = newPos;
        window.gameObject.SetActive(true);
    }

    public void ShowCompanyWindow(RectTransform companyCell, StatsWindowPosition position, CompanyData data)
    {
        ConfigureWindowPosition(companyInfoWindow, companyCell, position);

        statsCompanyPanel.SetCompanyName(data.name);
        statsCompanyPanel.SetGroupName(data.group.ToString());
        statsCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsCompanyPanel.SetRentPrices(data.rent);
        statsCompanyPanel.SetCellPrice(data.price.ToString());
        statsCompanyPanel.SetPledgePrice(data.pledgePrice.ToString());
        statsCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString());
        statsCompanyPanel.SetBranchPrice(data.branchPrice.ToString());
    }

    public void ShowFieldCompanyWindow(RectTransform companyCell, StatsWindowPosition position, FieldCompanyData data)
    {
        ConfigureWindowPosition(fieldCompanyInfoWindow, companyCell, position);

        statsFieldCompanyPanel.SetCompanyName(data.name);
        statsFieldCompanyPanel.SetGroupName(data.group.ToString());
        statsFieldCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsFieldCompanyPanel.SetFieldPrices(data.rentField);
        statsFieldCompanyPanel.SetCellPrice(data.price.ToString());
        statsFieldCompanyPanel.SetPledgePrice(data.pledgePrice.ToString());
        statsFieldCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString());
    }

    public void ShowDiceCompanyWindow(RectTransform companyCell, StatsWindowPosition position, DiceCompanyData data)
    {
        ConfigureWindowPosition(diceCompanyInfoWindow, companyCell, position);

        statsDiceCompanyPanel.SetCompanyName(data.name);
        statsDiceCompanyPanel.SetGroupName(data.group.ToString());
        statsDiceCompanyPanel.SetTopBarColor(GroupColors.Colors[(int)data.group]);
        statsDiceCompanyPanel.SetDiceFieldMultiTexts(data.rentMultiplier);
        statsDiceCompanyPanel.SetCellPrice(data.price.ToString());
        statsDiceCompanyPanel.SetPledgePrice(data.pledgePrice.ToString());
        statsDiceCompanyPanel.SetBuyoutPrice(data.buyoutPrice.ToString());
    }

    public void HideAllWindows()
    {
        companyInfoWindow.gameObject.SetActive(false);
        fieldCompanyInfoWindow.gameObject.SetActive(false);
        diceCompanyInfoWindow.gameObject.SetActive(false);
    }
}