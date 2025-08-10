using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BoardGeneratorEditor : EditorWindow
{
    public BoardConfig boardConfig;

    public GameObject companyPrefab;
    public GameObject bonusPrefab;
    public GameObject questionPrefab;
    public GameObject cornerPrefab;

    public Transform parentTransform;

    private const float CornerSize = 140f;
    private const float CellWidth = 70f;
    private const float CellHeight = 140f;
    private const float interval = 2f;

    [MenuItem("Monopoly/Build Board")]
    public static void ShowWindow()
    {
        GetWindow<BoardGeneratorEditor>("Board Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Board Builder", EditorStyles.boldLabel);

        boardConfig = (BoardConfig)EditorGUILayout.ObjectField("Board Config", boardConfig, typeof(BoardConfig), false);
        companyPrefab = (GameObject)EditorGUILayout.ObjectField("Company Prefab", companyPrefab, typeof(GameObject), false);
        bonusPrefab = (GameObject)EditorGUILayout.ObjectField("Bonus Prefab", bonusPrefab, typeof(GameObject), false);
        questionPrefab = (GameObject)EditorGUILayout.ObjectField("Question Prefab", questionPrefab, typeof(GameObject), false);
        cornerPrefab = (GameObject)EditorGUILayout.ObjectField("Corner Prefab", cornerPrefab, typeof(GameObject), false);
        parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);

        if (GUILayout.Button("Build Board"))
        {
            if (boardConfig == null || parentTransform == null)
            {
                Debug.LogError("Board config or parent transform missing!");
                return;
            }

            BuildBoard();
        }

        if (GUILayout.Button("Clear Board"))
        {
            ClearChildren();
        }
    }
    private void BuildBoard()
    {
        ClearChildren();

        for (int i = 0; i < boardConfig.cells.Count; i++)
        {
            var cell = boardConfig.cells[i];
            GameObject prefab = GetPrefab(cell.cellType);

            if (prefab == null)
            {
                Debug.LogWarning($"Missing prefab for cell {i}");
                continue;
            }

            GameObject cellGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentTransform);
            cellGO.name = $"Cell_{i}_{cell.cellName}";

            var uiComponent = cellGO.GetComponent<UICellBase>();
            if (uiComponent != null)
            {
                uiComponent.UpdateUI(cell, null); // при инициализации владелец неизвестен (null)
            }

            RectTransform rt = cellGO.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1); // левый верх
            rt.localRotation = Quaternion.identity;

            switch (cell.cellType)
            {
                case CellType.Company:
                    UICompanyCell uiCompanyCell = cellGO.GetComponent<UICompanyCell>();
                    if(i > 20 &&  i < 30)
                    {
                        uiCompanyCell.RotateLogoText(270);                   
                        uiCompanyCell.RotatePriceText();                     
                    }
                    if(i > 30 && i < 40)
                    {
                        uiCompanyCell.RotateLogoText(270);
                    }
                    break;
                case CellType.Spend:
                    UISpendCell uiSpendCell = cellGO.GetComponent<UISpendCell>();
                    if (i > 30 && i < 40)
                    {
                        uiSpendCell.RotateIcon();
                    }
                    break;
                case CellType.Question:
                    UIQuestionCell uiQuestionCell = cellGO.GetComponent<UIQuestionCell>();
                    if (i > 20 && i < 30 )
                    {
                        uiQuestionCell.RotateQuestionText();

                    }
                    break;
                default:
                    break;

            }
            Vector2 anchoredPos;
            float rotationZ;
            Vector2 size;

            GetCellTransform(i, out anchoredPos, out rotationZ, out size);

            rt.anchoredPosition = anchoredPos;
            rt.localEulerAngles = new Vector3(0, 0, rotationZ);
            rt.sizeDelta = size;
        }

        Debug.Log("Board built.");
    }

    private void GetCellTransform(int index, out Vector2 position, out float rotation, out Vector2 size)
    {
        bool isCorner = (index % 10 == 0);
        int side = index / 10;
        float halfCell = CellWidth / 2f;

        if (isCorner)
        {
            size = new Vector2(CornerSize, CornerSize);

            switch (index)
            {
                case 0:
                    position = new Vector2(CornerSize / 2f, -CornerSize / 2f);
                    break;
                case 10:
                    position = new Vector2(6f * CornerSize, -CornerSize / 2f);
                    break;
                case 20:
                    position = new Vector2(6f * CornerSize, -6f * CornerSize);
                    break;
                case 30:
                    position = new Vector2(CornerSize / 2f, -6f * CornerSize);
                    break;
                default:
                    position = Vector2.zero;
                    break;
            }
            rotation = 0;

            return;
        }

        // Non-corner
        size = new Vector2(CellWidth, CellHeight);
        rotation = 0;

        switch (side)
        {
            case 0: // top row (1Ц9)
                rotation = 0;
                position = new Vector2(index * CellWidth+ CellWidth*1.5f, -CellWidth);
                break;

            case 1: // right column (11Ц19)
                rotation = -90;
                position = new Vector2(12f * CellWidth, -(index - 10) * CellWidth - 3f*halfCell);
                break;

            case 2: // bottom row (21Ц29)
                rotation = -180;
                position = new Vector2((30 - index) * CellWidth + CellWidth * 1.5f, -12f * CellWidth);
                break;

            case 3: // left column (31Ц39)
                rotation = -270;
                position = new Vector2(CellWidth, -(40 - index) * CellWidth - 3f * halfCell);
                break;
            default:
                position = Vector2.zero;
                break;
        }
    }

    private GameObject GetPrefab(CellType type)
    {
        return type switch
        {
            CellType.Company => companyPrefab,
            CellType.Spend => bonusPrefab,
            CellType.Question => questionPrefab,
            CellType.Corner => cornerPrefab,
            _ => null
        };
    }

    private void ClearChildren()
    {
        while (parentTransform.childCount > 0)
        {
            DestroyImmediate(parentTransform.GetChild(0).gameObject);
        }
    }
}
