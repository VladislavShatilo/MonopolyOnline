using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerSkin))]
public class PlayerMove : MonoBehaviourPun
{
    [Header("Move Settings")]
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float stackMoveDuration = 0.1f;

    private Transform rootCellsObject;
    private List<Transform> boardCells = new List<Transform>();
    private int currentCellIndex = 0;
    private const int JAIL_CELL_ID = 10;
    private void Start()
    {
        if (GameManager.Instance?.PlayerRoot != null)
        {
            transform.SetParent(GameManager.Instance.PlayerRoot, false);
        }

        if (GameManager.Instance?.PlayerRoot != null)
            transform.SetParent(GameManager.Instance.PlayerRoot, false);

        var c = GameManager.Instance.GetColorForActor(photonView.Owner.ActorNumber);
        GetComponent<PlayerSkin>().SetColorDirect(c);

        EnsureBoardCellsInitialized();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnPlayerMoveEvent>(Move);
        EventBus.Subscribe<OnPlayerTeleportEvent>(TeleportToRandomCell);

        EventBus.Subscribe<MoveToJailEvent>(MoveToJail);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerMoveEvent>(Move);
        EventBus.Unsubscribe<OnPlayerTeleportEvent>(TeleportToRandomCell);

        EventBus.Unsubscribe<MoveToJailEvent>(MoveToJail);
    }

    private void Move(OnPlayerMoveEvent e)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_MoveSteps), RpcTarget.AllBuffered, e.Steps,e.Forward);
    }
    [PunRPC]
    public void RPC_RegisterOnCell(int cellIndex)
    {
        // Сохраняем индекс
        this.currentCellIndex = cellIndex;


        // Центр клетки
        if (GameManager.Instance == null || GameManager.Instance.CellsRoot == null) return;
        Vector3 center = GameManager.Instance.CellsRoot.GetChild(cellIndex).position;


        // Сбросим твины и установим позицию в центр сразу
        transform.DOKill();
        transform.position = center;


        // Зарегистрируемся в менеджере (это вызовет UpdatePositions и DOTween раздвинет всех)
        CellOccupancyManager.RegisterPlayerOnCell(cellIndex, this, center);


        Debug.Log($"PLAYER_RPC: {gameObject.name} RPC_RegisterOnCell cell={cellIndex} center={center}");
    }
    public void TeleportToRandomCell(OnPlayerTeleportEvent e)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == e.PlayerId)
        {
            if (!photonView.IsMine) return;
            int randomIndex;
            do
            {
                randomIndex = UnityEngine.Random.Range(0, boardCells.Count);
            } while (randomIndex == currentCellIndex);
            int steps = 0;
            if (randomIndex > currentCellIndex)
            {
                steps = randomIndex - currentCellIndex;
            }
            else
            {
                steps = randomIndex + 40 - currentCellIndex;
            }
            photonView.RPC(nameof(RPC_MoveSteps), RpcTarget.AllBuffered, steps, true);
        }

    }

    [PunRPC]
    private void RPC_MoveSteps(int steps,bool forward)
    {
        if (boardCells.Count == 0)
        {
            Debug.LogError("Board cells not initialized, canceling move.");
            return;
        }

        StartCoroutine(MoveStepsCoroutine(steps, forward));
    }

    private IEnumerator MoveStepsCoroutine(int steps,bool forward)
    {

        int targetIndex = (currentCellIndex + steps) % boardCells.Count;
        if (!forward)
        {
            targetIndex = (currentCellIndex - steps + boardCells.Count) % boardCells.Count;
        }
        EventBus.Publish(new DiceFadeEvent(targetIndex, true));
        yield return new WaitForSeconds(0.8f);
        CellOccupancyManager.UnregisterPlayerFromCell(currentCellIndex, this);

        // Движение по шагам
        for (int i = 0; i < steps; i++)
        {
            if (forward)
            {
                currentCellIndex = (currentCellIndex + 1) % boardCells.Count;
            }
            else
            {
                currentCellIndex = (currentCellIndex - 1 + boardCells.Count) % boardCells.Count;
            }
            Vector3 stepPos = boardCells[currentCellIndex].position;

            yield return MoveToPosition(stepPos);

            PlayerData player = GameManager.Instance.GetPlayerById(photonView.Owner.ActorNumber);

            if (currentCellIndex == 0 && !player.IsInJail && !player.NextMoveBackward)
            {
                Bank.Instance.AddMoney(player, 2_000);
            }
        }

        EventBus.Publish(new DiceFadeEvent(targetIndex, false));

        // ⬇️ Сразу регистрируем игрока и он сам плавно встанет на свою позицию в паттерне
        CellOccupancyManager.RegisterPlayerOnCell(currentCellIndex, this, boardCells[currentCellIndex].position);

        // Запускаем обработку клетки
        EventBus.Publish(new HandleCellEvent(currentCellIndex, photonView.Owner.ActorNumber));
    }

    public void SetTargetPosition(Vector3 pos)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_SetTargetPosition), RpcTarget.AllBuffered,pos.x,pos.y,pos.z);
    }
    [PunRPC]
    private void RPC_SetTargetPosition(float x, float y, float z)
    {
        Vector3 pos = new Vector3(x,y,z);
        transform.DOMove(pos, stackMoveDuration);

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

    private void MoveToJail(MoveToJailEvent e)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_MoveToJail), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_MoveToJail()
    {
        if (boardCells.Count == 0)
        {
            Debug.LogError("Board cells not initialized, canceling move.");
            return;
        }

        StartCoroutine(MoveToJailCoroutine());
    }

    private IEnumerator MoveToJailCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        currentCellIndex = JAIL_CELL_ID;
        Vector3 targetPos = boardCells[JAIL_CELL_ID].position;
        yield return MoveToPosition(targetPos);

        //EventBus.Publish(new HandleCellEvent(currentCellIndex, photonView.Owner.ActorNumber));
    }

   

    #region Board Initialization

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

    #endregion Board Initialization
}

public class MoveToJailEvent
{
    public int PlayerID;

    public MoveToJailEvent(int playerId)
    {
        PlayerID = playerId;
    }
}

public class HandleCellEvent
{
    public int CellID;
    public int PlayerID;

    public HandleCellEvent(int cellID, int playerId)
    {
        CellID = cellID;
        PlayerID = playerId;
    }
}