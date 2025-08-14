using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    public static CellsManager Instance { get; private set; }
 
    [SerializeField] private BoardConfig boardConfig; // Ссылка на ScriptableObject с данными
    [SerializeField] private Transform parentTransform; // Родитель для клеток на сцене
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
        for(int i = 0; i < boardCellsTransforms.Count; i++)
        {
            cellUIMap[i] = boardCellsTransforms[i].GetComponent<UICellBase>();
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