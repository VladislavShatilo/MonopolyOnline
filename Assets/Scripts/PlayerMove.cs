using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using Zenject;

public class PlayerMove : MonoBehaviourPun
{
    [SerializeField] private float moveDuration = 0.1f;

    private IBoardService boardService;
    private int cellsCount;
    private IEventBus eventBus;

    public void Initialize(IBoardService boardService, IEventBus eventBus)
    {
        this.boardService = boardService;
        this.eventBus = eventBus;
        eventBus.Subscribe<MovePlayerEvent>(OnPlayerMove);
        eventBus.Subscribe<PlayerMoveRegister>(OnOccupancyRegister);
        eventBus.Subscribe<PlayerMoveUnregister>(OnOccupancyUnregister);
        eventBus.Subscribe<MoveToJailEvent>(MoveToJail);


        cellsCount = boardService.CellsCount;
    }

    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<MovePlayerEvent>(OnPlayerMove);
            eventBus.Unsubscribe<PlayerMoveRegister>(OnOccupancyRegister);
            eventBus.Unsubscribe<PlayerMoveUnregister>(OnOccupancyUnregister);
            eventBus.Unsubscribe<MoveToJailEvent>(MoveToJail);

        }
    }

    private void OnOccupancyRegister(PlayerMoveRegister e)
    {
        eventBus.Publish(new PlayerOccupancyRegisterEvent(e.CellIndex, this));
    }

    private void OnOccupancyUnregister(PlayerMoveUnregister e)
    {
        eventBus.Publish(new PlayerOccupancyUnregisterEvent(e.CellIndex, this));
    }

    private void OnPlayerMove(MovePlayerEvent e)
    {
        if (e.PlayerId == photonView.OwnerActorNr)
        {
            StopAllCoroutines();
            StartCoroutine(Move(e.Steps, e.CurrentCellIndex, e.IsForward));
        }
    }

    private IEnumerator Move(int steps, int currentCellIndex, bool forward)
    {
        yield return new WaitForSeconds(0.1f);
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
        eventBus.Publish(new HandleCellEvent(currentCellIndex, photonView.OwnerActorNr));
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
    private void MoveToJail(MoveToJailEvent e)
    {
        if (e.PlayerID == photonView.OwnerActorNr)
        {
            StopAllCoroutines();
            StartCoroutine(MoveToJailCoroutine());
        }
    }

  
    private IEnumerator MoveToJailCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        Vector3 targetPos = boardService.GetCellRectTransform(10).position;
        yield return MoveToPosition(targetPos);

        //EventBus.Publish(new HandleCellEvent(currentCellIndex, photonView.Owner.ActorNumber));
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
    public IEventBus eventBus;

    public InitializePlayerMoveEvent(IBoardService boardService, ILocalPlayerService localPlayerService, IEventBus eventBus)
    {
        BoardService = boardService;
        LocalPlayerService = localPlayerService;
        this.eventBus = eventBus;
    }
}

public class EnablePlayerMoveEvent
{
}