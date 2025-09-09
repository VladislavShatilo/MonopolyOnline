using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Zenject;

public class PlayerMove : MonoBehaviourPun
{
    [SerializeField] private float moveDuration = 0.1f;
    private IBoardService boardService;
    private ILocalPlayerService localPlayerService;


    private void OnEnable()
    {
        EventBus.Subscribe<MovePlayerEvent>(OnPlayerMove);
        EventBus.Subscribe<InitializePlayerMoveEvent>(Initialize);
        EventBus.Subscribe<PlayerMoveRegister>(OnOccupancyRegister);
        EventBus.Subscribe<PlayerMoveUnregister>(OnOccupancyUnregister);
        EventBus.Publish(new EnablePlayerMoveEvent());
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<MovePlayerEvent>(OnPlayerMove);
        EventBus.Unsubscribe<InitializePlayerMoveEvent>(Initialize);
        EventBus.Unsubscribe<PlayerMoveRegister>(OnOccupancyRegister);
        EventBus.Unsubscribe<PlayerMoveUnregister>(OnOccupancyUnregister);
    }
    private void Initialize(InitializePlayerMoveEvent e)
    {
        boardService = e.BoardService;
        localPlayerService = e.LocalPlayerService;
    }

    private void OnOccupancyRegister(PlayerMoveRegister e)
    {
        EventBus.Publish(new PlayerOccupancyRegisterEvent(e.CellIndex, this));
    }
    private void OnOccupancyUnregister(PlayerMoveUnregister e)
    {
        EventBus.Publish(new PlayerOccupancyUnregisterEvent(e.CellIndex, this));

    }
    private void OnPlayerMove(MovePlayerEvent e)
    {
        if (e.PlayerId == photonView.OwnerActorNr)
        {
            StopAllCoroutines();
            StartCoroutine(Move(e.Steps, e.CurrentCellIndex, e.IsForward));
        }
    }

    private IEnumerator Move( int steps, int currentCellIndex, bool forward)
    {
        int cellsCount = boardService.CellsCount;
        for (int i = 0; i < steps; i++)
        {
            if (forward)
            {
                currentCellIndex = (currentCellIndex + 1) % cellsCount;
            }
            else
            {
                currentCellIndex = (currentCellIndex - 1 + cellsCount) % cellsCount;
            }
            Vector3 stepPos = boardService.GetCellRectTransform(currentCellIndex).position;

            yield return MoveToPosition(stepPos);

            
        }

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
    public void SetTargetPosition(Vector3 target)
    {
        StartCoroutine(MoveToPosition(target));
    }
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
public class PlayerMoveRegister
{
    public int CellIndex;
    public int PlayerId;

    public PlayerMoveRegister(int cellIndex, int playerId)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
    }

}
public class PlayerMoveUnregister
{
    public int CellIndex;
    public int PlayerId;

    public PlayerMoveUnregister(int cellIndex, int playerId)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
    }

}
public class InitializePlayerMoveEvent
{
    public IBoardService BoardService;
    public ILocalPlayerService LocalPlayerService;
    public InitializePlayerMoveEvent(IBoardService boardService, ILocalPlayerService localPlayerService)
    {
        BoardService = boardService;
        LocalPlayerService = localPlayerService;

    }
}
public class EnablePlayerMoveEvent
{
    
}