using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CellsManager теперь отвечает только за координацию клеток и события.
/// Логика UI и работы с компаниями вынесена в отдельные сервисы.
/// </summary>
public class CellsManager : MonoBehaviourPun
{
    public static CellsManager Instance { get; private set; }

    [Header("Board Setup")]
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private Transform parentTransform;

    private BoardService boardService;
    private CompanyUIService companyUIService;
    private CellEventHandler cellEventHandler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (boardConfig == null || parentTransform == null)
        {
            Debug.LogError("CellsManager: BoardConfig or ParentTransform is not assigned!");
            enabled = false;
            return;
        }

        // Инициализация сервисов
        boardService = new BoardService(boardConfig, parentTransform);
        companyUIService = new CompanyUIService(boardService);
        cellEventHandler = new CellEventHandler(boardService);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<HandleCellEvent>(cellEventHandler.OnHandleCell);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<HandleCellEvent>(cellEventHandler.OnHandleCell);
    }

    private void Start()
    {
        boardService.InitializeBoard();
        companyUIService.InitializeUI();
    }

    #region Public API

    public bool TryGetCellData(int index, out CellData cellData)
    {
        cellData = boardService.GetCellData(index);
        return cellData != null;
    }
    public GameObject GetCellByIndex(int index) => boardService.GetCellGameObject(index);
    public CellData GetCellDataByIndex(int index) => boardService.GetCellData(index);
    public List<CellData> GetCellDataList() => boardService.GetAllCellData();

    public bool TryGetCompanyUI(int index, out UICompanyCell ui)
    {
        ui = companyUIService.GetCompanyUI(index);
        return ui != null;
    }

    public UICompanyCell GetCompanyUI(int index) => companyUIService.GetCompanyUI(index);

    #endregion
}
