using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент для отображения компании и управления кнопками филиалов.
/// </summary>
public class UICompanyCell : UICellBase
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image BGImage;
    [SerializeField] private Image BGPriceImage;

    [Header("Branch Buttons")]
    [SerializeField] private Button buyFirstBranchButton;
    [SerializeField] private Button buyBranchButton;
    [SerializeField] private Button sellBranchButton;
    [SerializeField] private Button sellFirstBranchButton;

    [Header("Branch Button Icons")]
    [SerializeField] private Image buyFirstBranchIcon;
    [SerializeField] private Image buyBranchIcon;
    [SerializeField] private Image sellBranchIcon;
    [SerializeField] private Image sellFirstBranchIcon;

    [Header("Stars")]
    [SerializeField] private Image star1Image;
    [SerializeField] private Image star2Image;
    [SerializeField] private Image star3Image;
    [SerializeField] private Image star4Image;
    [SerializeField] private Image goldStarImage;

    private int companyId;

    /// <summary> Инициализация UI клетки. </summary>
    public void Init(int id)
    {
        companyId = id;

        HideAllBranchButtons();
        HideStars();

        buyFirstBranchButton.onClick.AddListener(() => TurnManager.Instance.RequestBuyBranch(companyId));
        buyBranchButton.onClick.AddListener(() => TurnManager.Instance.RequestBuyBranch(companyId));
        sellBranchButton.onClick.AddListener(() => TurnManager.Instance.RequestSellBranch(companyId));
        sellFirstBranchButton.onClick.AddListener(() => TurnManager.Instance.RequestSellBranch(companyId));
    }

  
    #region UI Updates

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        if (cellData == null) return;
        Debug.Log("UpdateUI(CellData cellData, PlayerData owner)");
        switch (cellData.cellType)
        {
            case CellType.Company:
                companyNameText.text = cellData.companyData.name;
                priceText.text = cellData.companyData.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)cellData.companyData.group];
                break;

            case CellType.FieldCompany:
                companyNameText.text = cellData.fieldCompanyData.name;
                priceText.text = cellData.fieldCompanyData.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)cellData.fieldCompanyData.group];
                break;

            case CellType.DiceCompany:
                companyNameText.text = cellData.diceCompanyData.name;
                priceText.text = cellData.diceCompanyData.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)cellData.diceCompanyData.group];
                break;
        }
        Debug.Log(owner.playerColor);
        BGImage.color = owner.playerColor;
    }

    public void SetRentText(int rent)
    {
        priceText.text = rent.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void UpdateBranchStars(int level)
    {
        HideStars();
        switch (level)
        {
            case 1: star1Image.gameObject.SetActive(true); break;
            case 2: star1Image.gameObject.SetActive(true); star2Image.gameObject.SetActive(true); break;
            case 3: star1Image.gameObject.SetActive(true); star2Image.gameObject.SetActive(true); star3Image.gameObject.SetActive(true); break;
            case 4: star1Image.gameObject.SetActive(true); star2Image.gameObject.SetActive(true); star3Image.gameObject.SetActive(true); star4Image.gameObject.SetActive(true); break;
            case 5: goldStarImage.gameObject.SetActive(true); break;
        }
    }

    private void HideStars()
    {
        star1Image.gameObject.SetActive(false);
        star2Image.gameObject.SetActive(false);
        star3Image.gameObject.SetActive(false);
        star4Image.gameObject.SetActive(false);
        goldStarImage.gameObject.SetActive(false);
    }

    #endregion

    #region Branch Buttons

    public void ShowBuyFirstBranchButton() { ShowOnlyButton(buyFirstBranchButton); }
    public void ShowBuySellButtons() { ShowOnlyButtons(buyBranchButton, sellBranchButton); }
    public void ShowSellFirstButton() { ShowOnlyButton(sellFirstBranchButton); }
    private void ShowOnlyButton(Button button)
    {
        HideAllBranchButtons();
        button.gameObject.SetActive(true);
        BGPriceImage.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
    }

    private void ShowOnlyButtons(Button button1, Button button2)
    {
        HideAllBranchButtons();
        button1.gameObject.SetActive(true);
        button2.gameObject.SetActive(true);
        BGPriceImage.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
    }

    public void HideAllBranchButtons()
    {
        buyFirstBranchButton.gameObject.SetActive(false);
        buyBranchButton.gameObject.SetActive(false);
        sellBranchButton.gameObject.SetActive(false);
        sellFirstBranchButton.gameObject.SetActive(false);

        BGPriceImage.gameObject.SetActive(true);
        priceText.gameObject.SetActive(true);
    }
    public void ChangeBuySellBranchButtons()
    { 
        Vector3 pos = buyBranchButton.GetComponent<RectTransform>().position;
        buyBranchButton.GetComponent<RectTransform>().position = sellBranchButton.GetComponent<RectTransform>().position;
        sellBranchButton.GetComponent<RectTransform>().position = pos; 
    }
    #endregion

    #region Rotation Utilities

    public void RotateLogoText(int angle)
    {
        companyNameText.rectTransform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void RotatePriceText()
    {
        priceText.rectTransform.eulerAngles = new Vector3(0, 0, 180);
    }

    public void RotateBranchButtonIcons(int angle)
    {
        buyFirstBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        buyBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        sellBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        sellFirstBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
    }

    #endregion

    #region Event Handlers

    public void HandleCompanyBought(int cellIndex, int ownerId)
    {
    
        UpdateUI(CellsManager.Instance.GetCellDataByIndex(companyId), GameManager.Instance.GetPlayerById(ownerId));
    }

    #endregion
}
