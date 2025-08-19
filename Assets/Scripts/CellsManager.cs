using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    public static CellsManager Instance { get; private set; }

    [SerializeField] private BoardConfig boardConfig; // —сылка на ScriptableObject с данными
    [SerializeField] private Transform parentTransform;
    [SerializeField] private CompanyUIManager companyUIManager;
    private List<Transform> boardCellsTransforms = new List<Transform>();

    private Dictionary<int, UICellBase> cellUIMap = new Dictionary<int, UICellBase>();
    private Dictionary<int, UICompanyCell> companyUIs = new Dictionary<int, UICompanyCell>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void RegisterCompanyUI(int id, UICompanyCell ui)
    {
        if (!companyUIs.ContainsKey(id))
            companyUIs[id] = ui;
    }

    public UICompanyCell GetCompanyUI(int id)
    {
        companyUIs.TryGetValue(id, out var ui);
        return ui;
    }
    private void Start()
    {
        foreach (Transform child in parentTransform)
        {
            boardCellsTransforms.Add(child);
        }
        for (int i = 0; i < boardCellsTransforms.Count; i++)
        {
            cellUIMap[i] = boardCellsTransforms[i].GetComponent<UICellBase>();
        }
        for (int i = 0; i < boardConfig.cells.Count; i++)
        {
            var cellData = boardConfig.cells[i];

            if (cellData.cellType == CellType.Company ||
                cellData.cellType == CellType.FieldCompany || 
                cellData.cellType == CellType.DiceCompany )
            {
                if(cellData.cellType == CellType.Company)
                {
                    var companyUI = boardCellsTransforms[i].GetComponent<UICompanyCell>();
                    companyUI.Init(i);
                    RegisterCompanyUI(i, companyUI);
                }
                CompanyDatabase.Instance.AddComponyData(i,cellData.companyData);
                var popup = boardCellsTransforms[i].GetComponent<CompanyWindowPopup>();
                popup.Init(i);
                if (cellData.cellType == CellType.Company)
                {
                    popup.OnCompanyClicked += (id) =>
                    {
                        var pos = boardConfig.cells[id].companyData.popupData;

                        companyUIManager.ShowCompanyWindow(boardCellsTransforms[id] as RectTransform, pos, cellData.companyData);
                    };
                }
                else if (cellData.cellType == CellType.FieldCompany)
                {
                    popup.OnCompanyClicked += (id) =>
                    {
                        var pos = boardConfig.cells[id].fieldCompanyData.popupData;
                        companyUIManager.ShowFieldCompanyWindow(boardCellsTransforms[id] as RectTransform, pos, cellData.fieldCompanyData);
                    };
                }
                else if (cellData.cellType == CellType.DiceCompany)
                {
                    popup.OnCompanyClicked += (id) =>
                    {
                        var pos = boardConfig.cells[id].diceCompanyData.popupData;

                        companyUIManager.ShowDiceCompanyWindow(boardCellsTransforms[id] as RectTransform, pos, cellData.diceCompanyData);
                    };
                }
            }
           
        }

        CompanyManager.Instance.InitializeCompanies(boardConfig.cells);
    }
    public void ShowBranchButtons(int currentPlayerId)
    {
       

        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {
            var ui = GetCompanyUI(company.Id);
            if (ui == null) continue;

            // ѕровер€ем, локальный ли это игрок и его ли сейчас ход
            bool isMyTurn = currentPlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
            bool canBuyBranch = isMyTurn &&
                                company.IsBought &&
                                company.OwnerId == currentPlayerId &&
                                company.RentLevel < 6 &&
                                CompanyManager.Instance.PlayerOwnsWholeGroup(company.CompanyBranchData.group, currentPlayerId);

            if (!canBuyBranch)
            {
                // скрываем все кнопки (чтобы лишние не оставались активными)
                ui.HideAllBranchButtons();
                continue;
            }

            int currentLevel = company.RentLevel;
            Debug.Log(company.RentLevel);
            if (currentLevel == 0)
            {
                ui.ShowBuyFirstBranchButton();
            }
            else if (currentLevel == 5)
            {
                ui.ShowSellFirstButton();
            }
            else
            {
                ui.ShowBuySellButtons();
            }
        }
    }
    public void HideAllBranchButtons(int currentPlayerId)
    {
        if (currentPlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;
        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {
            var ui = GetCompanyUI(company.Id);
            if (ui == null) continue;
            ui.HideAllBranchButtons();
        }
    }
    public void HideAllBranchButtonsByGroup(int currentPlayerId,CompanyGroup group)
    {
        if (currentPlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;
        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {      
            if(company.CompanyBranchData.group == group)
            {
                var ui = GetCompanyUI(company.Id);
                if (ui == null) continue;
                ui.HideAllBranchButtons();
            }
        }
    }
    public void RefreshCellUI(int cellIndex, PlayerData owner)
    {
        if (cellUIMap.TryGetValue(cellIndex, out var ui))
        {
            var cellData = boardConfig.cells[cellIndex];
            ui.UpdateUI(cellData, owner);
        }
    }
  
    public GameObject GetCellByIndex(int index)
    {
        return boardCellsTransforms[index].gameObject;
    }
    public CellData GetCellDataByIndex(int index)
    {
        return boardConfig.cells[index];
    }
}