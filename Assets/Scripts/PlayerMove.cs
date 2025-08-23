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
        EventBus.Subscribe<MoveToJailEvent>(MoveToJail);


    } 
    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerMoveEvent>(Move);
        EventBus.Unsubscribe<MoveToJailEvent>(MoveToJail);

    }

    private void Move(OnPlayerMoveEvent e)
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_MoveSteps), RpcTarget.AllBuffered, e.Steps);
    }
  
    [PunRPC]
    private void RPC_MoveSteps(int steps)
    {
       
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
            PlayerData player = GameManager.Instance.GetPlayerById(photonView.Owner.ActorNumber);
            if (currentCellIndex == 0 && !player.IsInJail)
            {
                Bank.Instance.AddMoney(player, 2_000);
            }
        }
        EventBus.Publish(new HandleCellEvent(currentCellIndex, photonView.Owner.ActorNumber));
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

    #endregion
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

    public HandleCellEvent( int cellID,int playerId)
    {
        CellID = cellID;
        PlayerID= playerId;
    }
}