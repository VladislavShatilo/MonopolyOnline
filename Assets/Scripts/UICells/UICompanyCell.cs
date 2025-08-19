using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Branch Buttons Icons")]
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
    public void Init(int id)
    {
        star1Image.gameObject.SetActive(false);
        star2Image.gameObject.SetActive(false);
        star3Image.gameObject.SetActive(false);
        star4Image.gameObject.SetActive(false);
        goldStarImage.gameObject.SetActive(false);

        companyId = id;
        buyFirstBranchButton.gameObject.SetActive(false);

        buyFirstBranchButton.onClick.AddListener(() =>
        {
            TurnManager.Instance.RequestBuyBranch(companyId);
        });
        buyBranchButton.onClick.AddListener(() =>
        {
            TurnManager.Instance.RequestBuyBranch(companyId);
        });
        sellBranchButton.onClick.AddListener(() =>
        {
            TurnManager.Instance.RequestSellBranch(companyId);
        });
        sellFirstBranchButton.onClick.AddListener(() =>
        {
            TurnManager.Instance.RequestSellBranch(companyId);
        });
    }
    public void ShowBuyFirstBranchButton()
    {
        HideAllBranchButtons();
        buyFirstBranchButton.gameObject.SetActive(true);
        BGPriceImage.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
    }

    public void ShowBuySellButtons()
    {
        HideAllBranchButtons();
        buyBranchButton.gameObject.SetActive(true);
        sellBranchButton.gameObject.SetActive(true);
        BGPriceImage.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
    }

    public void ShowSellFirstButton()
    {
        HideAllBranchButtons();
        sellFirstBranchButton.gameObject.SetActive(true);
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
    public override void UpdateUI(CellData cellData, PlayerData owner)
    {

        switch (cellData.cellType)
        {
            case CellType.Company:
                var company = cellData.companyData;
                companyNameText.text = company.name;
                priceText.text = company.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)company.group];
                break;
            case CellType.FieldCompany:
                var fieldCompany = cellData.fieldCompanyData;
                companyNameText.text = fieldCompany.name;
                priceText.text = fieldCompany.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)fieldCompany.group];
                break;
            case CellType.DiceCompany:
                var diceCompany = cellData.diceCompanyData;
                companyNameText.text = diceCompany.name;
                priceText.text = diceCompany.price.ToString("N0", CultureInfo.InvariantCulture);
                BGPriceImage.color = GroupColors.Colors[(int)diceCompany.group];
                break;

        }

        if (owner != null)
        {
            BGImage.color = owner.playerColor;
        }
        else
        {
            BGImage.color = Color.white; // или стандартный цвет
        }
    }
   
    public void SetRentText(int rent)
    {
        priceText.text= rent.ToString("N0", CultureInfo.InvariantCulture);
    }
    public void RotateLogoText(int angle)
    {
        companyNameText.rectTransform.eulerAngles = new Vector3(0,0, angle);
    }
    public void RotatePriceText()
    {
        priceText.rectTransform.eulerAngles = new Vector3(0, 0, 180);
    }
    public void RotateBrunchButtonIcons(int angle)
    {
        buyFirstBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        buyBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        sellBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
        sellFirstBranchIcon.rectTransform.eulerAngles += new Vector3(0, 0, angle);
    }
    public void ChangeBuySellBranchButtons()
    {
        
        Vector3 pos = buyBranchButton.GetComponent<RectTransform>().position;
        buyBranchButton.GetComponent<RectTransform>().position = sellBranchButton.GetComponent<RectTransform>().position;
        sellBranchButton.GetComponent<RectTransform>().position = pos;
    }
    public void UpdateBranchStars(int level)
    {
        switch (level)
        {
            case 0:
                star1Image.gameObject.SetActive(false);
                star2Image.gameObject.SetActive(false);
                star3Image.gameObject.SetActive(false);
                star4Image.gameObject.SetActive(false);
                goldStarImage.gameObject.SetActive(false);
                break;
            case 1:
                star1Image.gameObject.SetActive(true);
                star2Image.gameObject.SetActive(false);
                star3Image.gameObject.SetActive(false);
                star4Image.gameObject.SetActive(false);
                goldStarImage.gameObject.SetActive(false);
                break;
            case 2:
                star1Image.gameObject.SetActive(true);
                star2Image.gameObject.SetActive(true);
                star3Image.gameObject.SetActive(false);
                star4Image.gameObject.SetActive(false);
                goldStarImage.gameObject.SetActive(false);
                break;
            case 3:
                star1Image.gameObject.SetActive(true);
                star2Image.gameObject.SetActive(true);
                star3Image.gameObject.SetActive(true);
                star4Image.gameObject.SetActive(false);
                goldStarImage.gameObject.SetActive(false);
                break;
            case 4:
                star1Image.gameObject.SetActive(true);
                star2Image.gameObject.SetActive(true);
                star3Image.gameObject.SetActive(true);
                star4Image.gameObject.SetActive(true);
                goldStarImage.gameObject.SetActive(false);
                break;
            case 5:
                star1Image.gameObject.SetActive(false);
                star2Image.gameObject.SetActive(false);
                star3Image.gameObject.SetActive(false);
                star4Image.gameObject.SetActive(false);
                goldStarImage.gameObject.SetActive(true);
                break;
        }
    }

}
