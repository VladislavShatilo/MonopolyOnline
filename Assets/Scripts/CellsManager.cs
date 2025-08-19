using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет всеми клетками на игровом поле: инициализация UI, регистрация компаний,
/// отображение кнопок филиалов и обновление состояния клеток.
/// </summary>
public class CellsManager : MonoBehaviour
{
    public static CellsManager Instance { get; private set; }

    [Header("Board Setup")]
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private Transform parentTransform;

    [Header("UI Managers")]
    [SerializeField] private CompanyUIManager companyUIManager;

    private readonly List<Transform> boardCellsTransforms = new();
    private readonly Dictionary<int, UICellBase> cellUIMap = new();
    private readonly Dictionary<int, UICompanyCell> companyUIs = new();

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CacheBoardCells();
        InitializeCells();
    }

    #endregion

    #region Инициализация поля

    private void CacheBoardCells()
    {
        foreach (Transform child in parentTransform)
            boardCellsTransforms.Add(child);

        for (int i = 0; i < boardCellsTransforms.Count; i++)
        {
            if (boardCellsTransforms[i].TryGetComponent(out UICellBase uiCell))
                cellUIMap[i] = uiCell;
        }
    }

    private void InitializeCells()
    {
        for (int i = 0; i < boardConfig.cells.Count; i++)
        {
            var cellData = boardConfig.cells[i];
            switch (cellData.cellType)
            {
                case CellType.Company:
                    InitCompanyCell(i, cellData.companyData);
                    break;

                case CellType.FieldCompany:
                    CompanyDatabase.Instance.AddComponyData(i, cellData.fieldCompanyData);
                    InitPopup(i, cellData.cellType);
                    break;

                case CellType.DiceCompany:
                    CompanyDatabase.Instance.AddComponyData(i, cellData.diceCompanyData);
                    InitPopup(i, cellData.cellType);
                    break;
            }
        }
    }

    private void InitCompanyCell(int index, CompanyData companyData)
    {
        if (boardCellsTransforms[index].TryGetComponent(out UICompanyCell companyUI))
        {
            companyUI.Init(index);
            RegisterCompanyUI(index, companyUI);
        }

        CompanyDatabase.Instance.AddComponyData(index, companyData);
        InitPopup(index, CellType.Company);
    }

    private void InitPopup(int index, CellType type)
    {
        if (!boardCellsTransforms[index].TryGetComponent(out CompanyWindowPopup popup))
            return;

        popup.Init(index);

        popup.OnCompanyClicked += (id) =>
        {
            RectTransform rect = boardCellsTransforms[id] as RectTransform;

            switch (type)
            {
                case CellType.Company:
                    companyUIManager.ShowCompanyWindow(rect, boardConfig.cells[id].companyData.popupData, boardConfig.cells[id].companyData);
                    break;

                case CellType.FieldCompany:
                    companyUIManager.ShowFieldCompanyWindow(rect, boardConfig.cells[id].fieldCompanyData.popupData, boardConfig.cells[id].fieldCompanyData);
                    break;

                case CellType.DiceCompany:
                    companyUIManager.ShowDiceCompanyWindow(rect, boardConfig.cells[id].diceCompanyData.popupData, boardConfig.cells[id].diceCompanyData);
                    break;
            }
        };
    }

    #endregion

    #region Работа с UI компаний

    public void RegisterCompanyUI(int id, UICompanyCell ui)
    {
        if (!companyUIs.ContainsKey(id))
            companyUIs[id] = ui;
    }

    public UICompanyCell GetCompanyUI(int id) =>
        companyUIs.TryGetValue(id, out var ui) ? ui : null;

    public void RefreshCellUI(int cellIndex, PlayerData owner)
    {
        if (!cellUIMap.TryGetValue(cellIndex, out var ui)) return;
        var cellData = boardConfig.cells[cellIndex];
        ui.UpdateUI(cellData, owner);
    }

    #endregion

    #region Управление кнопками филиалов

    public void ShowBranchButtons(int currentPlayerId)
    {
        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {
            var ui = GetCompanyUI(company.Id);
            if (ui == null) continue;

            bool isMyTurn = currentPlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
            bool ownsGroup = CompanyManager.Instance.PlayerOwnsWholeGroup(company.CompanyData.group, currentPlayerId);

            bool canBuyBranch =
                isMyTurn &&
                company.IsBought &&
                company.OwnerId == currentPlayerId &&
                company.RentLevel < 6 &&
                ownsGroup;

            if (!canBuyBranch)
            {
                ui.HideAllBranchButtons();
                continue;
            }

            ShowBranchButtonsForLevel(ui, company.RentLevel);
        }
    }

    private void ShowBranchButtonsForLevel(UICompanyCell ui, int level)
    {
        switch (level)
        {
            case 0:
                ui.ShowBuyFirstBranchButton();
                break;
            case 5:
                ui.ShowSellFirstButton();
                break;
            default:
                ui.ShowBuySellButtons();
                break;
        }
    }

    public void HideAllBranchButtons(int currentPlayerId)
    {
        if (currentPlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;

        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {
            var ui = GetCompanyUI(company.Id);
            ui?.HideAllBranchButtons();
        }
    }

    public void HideAllBranchButtonsByGroup(int currentPlayerId, CompanyGroup group)
    {
        if (currentPlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;

        foreach (var company in CompanyDatabase.Instance.GetAllCompanies())
        {
            if (company.Type != CompanyType.Company) continue;
            if (company.CompanyData.group != group) continue;

            var ui = GetCompanyUI(company.Id);
            ui?.HideAllBranchButtons();
        }
    }

    #endregion

    #region Утилиты

    public GameObject GetCellByIndex(int index) =>
        boardCellsTransforms[index].gameObject;

    public CellData GetCellDataByIndex(int index) =>
        boardConfig.cells[index];

    public List<CellData> GetCellDataList() =>
        boardConfig.cells;

    #endregion
}
