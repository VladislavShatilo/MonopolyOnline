using Photon.Pun;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// UI-компонент для отображения компании и управления кнопками филиалов.
/// </summary>
public class UICompanyCell : MonoBehaviour,IUICompanyCellView,IInitializable,IDisposable
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

    [Header("Mortgage")]
    [SerializeField] private Button mortgageButton;
    [SerializeField] private Button buyoutButton;
    [SerializeField] private TextMeshProUGUI turnsText;
    [SerializeField] private GameObject mortgageStatsGO;
    [SerializeField] private Image mortgageFadeImage;

    [Header("Stars")]
    [SerializeField] private Image star1Image;
    [SerializeField] private Image star2Image;
    [SerializeField] private Image star3Image;
    [SerializeField] private Image star4Image;
    [SerializeField] private Image goldStarImage;

    private int companyId;

    // --- События, чтобы Presenter мог подписаться ---
    public event Action<int> OnBuyBranchClicked;
    public event Action<int> OnSellBranchClicked;
    public event Action<int> OnMortgageClicked;
    public event Action<int> OnBuyoutClicked;

    private IUICompanyCellRepository repository;

    [Inject]
    public void Construct(IUICompanyCellRepository repository)
    {

        this.repository = repository;
    }
    public int CompanyId()
    {
        return companyId; 
    }
    void IInitializable.Initialize()
    {
        repository.Register(this);
    }
    void IDisposable.Dispose()
    {
        repository.Unregister(this);
    }
   
    public void Init(int id)
    {
        companyId = id;

        HideAllBranchButtons();
        HideStars();

        buyFirstBranchButton.onClick.AddListener(() => OnBuyBranchClicked?.Invoke(companyId));
        buyBranchButton.onClick.AddListener(() => OnBuyBranchClicked?.Invoke(companyId));
        sellBranchButton.onClick.AddListener(() => OnSellBranchClicked?.Invoke(companyId));
        sellFirstBranchButton.onClick.AddListener(() => OnSellBranchClicked?.Invoke(companyId));
        mortgageButton.onClick.AddListener(() => OnMortgageClicked?.Invoke(companyId));
        buyoutButton.onClick.AddListener(() => OnBuyoutClicked?.Invoke(companyId));
    }

    public void UpdateUI(string name, int price, Color groupColor)
    {
        companyNameText.text = name;
        priceText.text = price.ToString("N0", CultureInfo.InvariantCulture);
        BGPriceImage.color = groupColor;
    }
    public void UpdateOwner(Color ownerColor)
    {
        BGImage.color = ownerColor;

    }
    public void SetRentText(int rent) =>
        priceText.text = rent.ToString("N0", CultureInfo.InvariantCulture);

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

    public void ShowBuyFirstBranchButton() => ShowOnlyButton(buyFirstBranchButton);
    public void ShowBuySellButtons() => ShowOnlyButtons(buyBranchButton, sellBranchButton);
    public void ShowSellFirstButton() => ShowOnlyButton(sellFirstBranchButton);

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

    public void ShowMortgageButton() => ShowButton(true);
    public void ShowBuyoutButton() => ShowButton(false);

    private void ShowButton(bool isMortgage)
    {
        HideAllBranchButtons();
        mortgageButton.gameObject.SetActive(isMortgage);
        buyoutButton.gameObject.SetActive(!isMortgage);
        BGPriceImage.gameObject.SetActive(false);
        priceText.gameObject.SetActive(false);
    }

    public void MortgageUI() => MortgageUIChange(true);
    public void BuyoutUI() => MortgageUIChange(false);

    private void MortgageUIChange(bool isMortgage)
    {
        HideAllBranchButtons();
        mortgageButton.gameObject.SetActive(false);
        buyoutButton.gameObject.SetActive(false);
        mortgageStatsGO.SetActive(isMortgage);
        mortgageFadeImage.gameObject.SetActive(isMortgage);
    }

    public void SetTurnsText(int turns) =>
        turnsText.text = turns.ToString();
}
