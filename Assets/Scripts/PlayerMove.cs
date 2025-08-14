using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviourPun
{
    [SerializeField] private BoardConfig boardConfig; // если используешь - можешь читать клетки отсюда
    [SerializeField] private int currentCellIndex = 0;
    [SerializeField] private float moveDuration = 0.1f;

    public static Action<int> ShowBuyMenuAction;

    private Transform rootCellsObject;
    private List<Transform> boardCells = new List<Transform>();
    public int id;

    private int instColorIndex = -1;

    private void Awake()
    {
        // ѕолучаем данные инстанциации (colorIndex) если они были переданы PhotonNetwork.Instantiate
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            try
            {
                instColorIndex = Convert.ToInt32(photonView.InstantiationData[0]);
            }
            catch { instColorIndex = -1; }
        }
    }

    private void Start()
    {
        // ”становим id по владельцу, если не выставлен
        if (id == 0 && photonView.Owner != null)
            id = photonView.Owner.ActorNumber;

        // ѕопробуем назначить родител€ UI/Canvas если он задан в GameManager (делаем это на всех клиентах, чтобы иерархи€ была одинаковой)
        if (GameManager.Instance != null && GameManager.Instance.PlayerRootTransform != null)
        {
            try
            {
                transform.SetParent(GameManager.Instance.PlayerRootTransform, false);
            }
            catch { /* на случай разных типов Transform/RectTransform */ }
        }

        // ѕрименим цвет (тот, что пришЄл в instantiationData, иначе по ActorNumber)
        ApplySkinColor();

        // ѕопытаемс€ инициализировать клетки доски (на всех клиентах)
        EnsureBoardCellsInitialized();
    }

    private void ApplySkinColor()
    {
        var skin = GetComponent<PlayerSkin>();
        if (skin == null) return;

        Color c = Color.white;
        if (photonView.Owner != null && GameManager.Instance != null)
        {
            c = GameManager.Instance.GetColorForActor(photonView.Owner.ActorNumber);
        }

        skin.SetSkinColor(c);
    }

    private void OnEnable()
    {
        RandomNumbers.playerMoveAction += Move;
    }

    private void OnDisable()
    {
        RandomNumbers.playerMoveAction -= Move;
    }

    private void Move(int steps)
    {
        if (!photonView.IsMine) return;

        // посылаем всем команду переместить этот токен
        photonView.RPC(nameof(RPC_MoveSteps), RpcTarget.AllBuffered, steps);
    }

    [PunRPC]
    private void RPC_MoveSteps(int steps)
    {
        // перед запуском корутины убедимс€, что boardCells инициализированы
        EnsureBoardCellsInitialized();

        if (boardCells == null || boardCells.Count == 0)
        {
            Debug.LogError($"[{name}] RPC_MoveSteps: boardCells не инициализированы, отмен€ю движение.");
            return;
        }

        StartCoroutine(MoveStepsCoroutine(steps));
    }

    private IEnumerator MoveStepsCoroutine(int steps)
    {
        if (boardCells == null || boardCells.Count == 0)
        {
            Debug.LogError("MoveStepsCoroutine: нет клеток доски.");
            yield break;
        }

        for (int i = 0; i < steps; i++)
        {
            currentCellIndex = (currentCellIndex + 1) % boardCells.Count;
            Vector3 targetPos = boardCells[currentCellIndex].position;
            yield return MoveToPosition(targetPos);
        }

        CellHandle(boardCells[currentCellIndex].gameObject, currentCellIndex);

       
    }

    public void CellHandle(GameObject cellGO, int currentCellID)
    {
        Debug.Log(currentCellID);
        string name = GameManager.Instance.GetPlayerById(id).Name;
        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(GameManager.Instance.GetPlayerById(id).playerColor)}>{name}</color>";
        switch (boardConfig.cells[currentCellID].cellType)
        {
            case CellType.Company:
                {
                   
                    MessageLog.Instance.AddMessage(coloredName + " попал в сектор " + boardConfig.cells[currentCellID].companyData.name);
                    var cell = CompanyManager.Instance.GetCompany(currentCellID);
                    if (cell != null && !cell.isBought)
                    {
                        CompanyManager.Instance.CompanyHandle(currentCellID, id);
                    }

                    break;
                }
            case CellType.Question:
                {
                    MessageLog.Instance.AddMessage(coloredName + " попал в сектор говно и у вас забрали 1,000k");

                    Bank.Instance.RemoveMoney(GameManager.Instance.GetPlayerById(id), 1000);
                    if (photonView.IsMine)
                    {
                        // «авершение хода запрашивает только владелец фишки
                        TurnManager.Instance.RequestEndTurn();
                    }
                    break;
                }
            case CellType.Spend:
                {
                    MessageLog.Instance.AddMessage(coloredName + " попал в сектор говно-говно и у вас забрали 2,000k");
                    Bank.Instance.RemoveMoney(GameManager.Instance.GetPlayerById(id), 2000);
                    if (photonView.IsMine)
                    {
                        // «авершение хода запрашивает только владелец фишки
                        TurnManager.Instance.RequestEndTurn();
                    }
                    break;
                }
            case CellType.Corner:
                {
                    MessageLog.Instance.AddMessage(coloredName + " попали в сектор говно-говно-говно");
                    if (photonView.IsMine)
                    {
                        // «авершение хода запрашивает только владелец фишки
                        TurnManager.Instance.RequestEndTurn();
                    }
                    break;
                }
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    public void SetRootTransform(Transform root)
    {
        rootCellsObject = root;
        BuildBoardCellsFromRoot();
    }

    private void EnsureBoardCellsInitialized()
    {
        if (boardCells != null && boardCells.Count > 0) return;

        // ѕопробуем использовать root, если он уже есть
        if (rootCellsObject != null)
        {
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        // ѕопробуем вз€ть из GameManager
        if (GameManager.Instance != null && GameManager.Instance.CellsRootTransforms != null)
        {
            rootCellsObject = GameManager.Instance.CellsRootTransforms;
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        // fallback: найти объект в сцене с именем "CellsRoot"
        var found = GameObject.Find("CellsRoot");
        if (found != null)
        {
            rootCellsObject = found.transform;
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        // если всЄ ещЄ нет Ч выдаЄм предупреждение
        if (boardCells.Count == 0)
        {
            Debug.LogWarning($"[{name}] Ќе удалось инициализировать boardCells. ”бедись, что GameManager.CellsRootTransforms задан или в сцене есть объект 'CellsRoot'.");
        }
    }

    private void BuildBoardCellsFromRoot()
    {
        boardCells = new List<Transform>();
        if (rootCellsObject == null) return;

        foreach (Transform child in rootCellsObject)
        {
            boardCells.Add(child);
        }
    }
}