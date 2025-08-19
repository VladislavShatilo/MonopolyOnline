using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerSkin))]
public class PlayerMove : MonoBehaviourPun
{
    [Header("Board Settings")]
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private float moveDuration = 0.1f;

    public static Action<int> ShowBuyMenuAction;

    public int id;
    private int currentCellIndex = 0;
    private int instColorIndex = -1;

    private Transform rootCellsObject;
    private List<Transform> boardCells = new List<Transform>();

    private void Awake()
    {
        // Получаем данные при инстанциации
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            instColorIndex = Convert.ToInt32(photonView.InstantiationData[0]);
        }
    }

    private void Start()
    {
        id = id != 0 ? id : photonView.Owner?.ActorNumber ?? 0;

        // Назначаем родителя
        if (GameManager.Instance?.PlayerRoot != null)
            transform.SetParent(GameManager.Instance.PlayerRoot, false);

        ApplySkinColor();
        EnsureBoardCellsInitialized();
    }

    private void ApplySkinColor()
    {
        var skin = GetComponent<PlayerSkin>();
        if (!skin) return;

        Color c = Color.white;
        if (photonView.Owner != null)
            c = GameManager.Instance.GetColorForActor(photonView.Owner.ActorNumber);

        skin.SetSkinColor(c);
    }

    private void OnEnable() => RandomNumbers.playerMoveAction += Move;
    private void OnDisable() => RandomNumbers.playerMoveAction -= Move;

    private void Move(int steps)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_MoveSteps), RpcTarget.AllBuffered, steps);
    }

    [PunRPC]
    private void RPC_MoveSteps(int steps)
    {
        EnsureBoardCellsInitialized();
        if (boardCells.Count == 0)
        {
            Debug.LogError("Board cells not initialized, canceling move.");
            return;
        }

        StartCoroutine(MoveStepsCoroutine(steps));
    }

    private IEnumerator MoveStepsCoroutine(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            currentCellIndex = (currentCellIndex + 1) % boardCells.Count;
            Vector3 targetPos = boardCells[currentCellIndex].position;
            yield return MoveToPosition(targetPos);
        }

        HandleCell(boardCells[currentCellIndex].gameObject, currentCellIndex);
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }

    private void HandleCell(GameObject cellGO, int cellIndex)
    {
        if (boardConfig == null || cellIndex >= boardConfig.cells.Count) return;

        var playerData = GameManager.Instance.GetPlayerById(id);
        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(playerData.playerColor)}>{playerData.Name}</color>";
        var cellData = boardConfig.cells[cellIndex];

        switch (cellData.cellType)
        {
            case CellType.Company:
            case CellType.FieldCompany:
            case CellType.DiceCompany:
                HandleCompanyCell(cellIndex, coloredName);
                break;

            case CellType.Question:
                Bank.Instance.RemoveMoney(playerData, 1_000);
                MessageLog.Instance.AddMessage($"{coloredName} попал в сектор и потерял 1,000k");
                EndTurnIfMine();
                break;

            case CellType.Spend:
                Bank.Instance.RemoveMoney(playerData, 2_000);
                MessageLog.Instance.AddMessage($"{coloredName} попал в сектор и потерял 2,000k");
                EndTurnIfMine();
                break;

            case CellType.Corner:
                MessageLog.Instance.AddMessage($"{coloredName} попал в сектор");
                EndTurnIfMine();
                break;
        }
    }

    private void HandleCompanyCell(int cellIndex, string coloredName)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        MessageLog.Instance.AddMessage($"{coloredName} попал в сектор {company.CompanyData.name}");

        if (company.IsBought && company.OwnerId == id)
            EndTurnIfMine();

        CompanyManager.Instance.HandleCell(cellIndex, id);
    }

    private void EndTurnIfMine()
    {
        if (photonView.IsMine)
            TurnManager.Instance.RequestEndTurn();
    }

    #region Board Initialization

    public void SetRootTransform(Transform root)
    {
        rootCellsObject = root;
        BuildBoardCellsFromRoot();
    }

    private void EnsureBoardCellsInitialized()
    {
        if (boardCells.Count > 0) return;

        if (rootCellsObject != null)
        {
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        if (GameManager.Instance?.CellsRoot != null)
        {
            rootCellsObject = GameManager.Instance.CellsRoot;
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        var found = GameObject.Find("CellsRoot");
        if (found != null)
        {
            rootCellsObject = found.transform;
            BuildBoardCellsFromRoot();
            if (boardCells.Count > 0) return;
        }

        if (boardCells.Count == 0)
            Debug.LogWarning($"[{name}] Не удалось инициализировать boardCells!");
    }

    private void BuildBoardCellsFromRoot()
    {
        boardCells = new List<Transform>();
        if (rootCellsObject == null) return;

        foreach (Transform child in rootCellsObject)
            boardCells.Add(child);
    }

    #endregion
}
