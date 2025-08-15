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
            var popup = boardCellsTransforms[i].GetComponent<CompanyWindowPopup>();
            if(popup == null)
            {
                Debug.Log(boardConfig.cells.Count);
            }
        
            popup.Init(i);
            if (cellData.cellType == CellType.Company)
            {   
                popup.OnCompanyClicked += (id) =>
                {
                    var pos = boardConfig.cells[id].companyData.popupData;
                
                    companyUIManager.ShowCompanyWindow(boardCellsTransforms[id] as RectTransform, pos,cellData.companyData);
                };
            }
            else if(cellData.cellType == CellType.FieldCompany)
            {  
                popup.OnCompanyClicked += (id) =>
                {
                    var pos = boardConfig.cells[id].fieldCompanyData.popupData;
                    companyUIManager.ShowFieldCompanyWindow(boardCellsTransforms[id] as RectTransform, pos, cellData.fieldCompanyData);
                };
            }
            else if(cellData.cellType == CellType.FieldCompany)
            {
                popup.OnCompanyClicked += (id) =>
                {
                    var pos = boardConfig.cells[id].diceCompanyData.popupData;
                 
                    companyUIManager.ShowDiceCompanyWindow(boardCellsTransforms[id] as RectTransform, pos, cellData.diceCompanyData);
                };
            }
        }

        CompanyManager.Instance.InitializeCompanies(boardConfig.cells);
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
}