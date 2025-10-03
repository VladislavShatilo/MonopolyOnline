using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeWindowUI : MonoBehaviour 
{
  //  [SerializeField] private TextMeshProUGUI leftNameText;
  //  [SerializeField] private TextMeshProUGUI rightNameText;
  //  [SerializeField] private TextMeshProUGUI leftMoneyText;
  //  [SerializeField] private TextMeshProUGUI rightMoneyText;
  //  [SerializeField] private TextMeshProUGUI leftTotalText;
  //  [SerializeField] private TextMeshProUGUI rightTotalText;
  //  [SerializeField] private Transform leftPanel;
  //  [SerializeField] private Transform rightPanel;
  //  [SerializeField] private GameObject companyCardPrefab;
  // // [SerializeField] private Button acceptButton;
  ////  [SerializeField] private Button cancelButton;

  //  public void Show()
  //  {
  //      leftNameText.text = model.LeftPlayerName;
  //      rightNameText.text = model.RightPlayerName;
  //      leftMoneyText.text = model.LeftMoney.ToString();
  //      rightMoneyText.text = model.RightMoney.ToString();
  //      leftTotalText.text = model.LeftTotal.ToString();
  //      rightTotalText.text = model.RightTotal.ToString();

  //      ClearCompanies(leftPanel);
  //      ClearCompanies(rightPanel);

  //      PopulateCompanies(leftPanel, model.LeftCompanies);
  //      PopulateCompanies(rightPanel, model.RightCompanies);

  //     // acceptButton.gameObject.SetActive(model.CanAccept);
  //     // cancelButton.gameObject.SetActive(model.CanCancel);

  //      gameObject.SetActive(true);
  //  }

  //  public void Hide() => gameObject.SetActive(false);

  //  public void Clear()
  //  {
  //      leftMoneyText.text = rightMoneyText.text = "0";
  //      leftTotalText.text = rightTotalText.text = "0";
  //      ClearCompanies(leftPanel);
  //      ClearCompanies(rightPanel);
  //  }

  //  private void PopulateCompanies(Transform panel, List<Company> companies)
  //  {
  //      foreach (var company in companies)
  //      {
  //          var card = Instantiate(companyCardPrefab, panel);
  //          if (card.TryGetComponent<UICompanyTrade>(out var uiCompany))
  //              uiCompany.SetCompanyTradeUI(company, company.OwnerId);
  //      }
  //  }

  //  private void ClearCompanies(Transform panel)
  //  {
  //      foreach (Transform child in panel)
  //          Destroy(child.gameObject);
  //  }
}
