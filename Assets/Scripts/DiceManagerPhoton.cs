using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// DiceManagerPhoton отвечает за синхронные броски кубиков в Photon.
/// Работает как для обычных ходов, так и для проверок выхода из тюрьмы.
/// </summary>
public class DiceManagerPhoton : MonoBehaviourPun, IPhotonDiceManager
{
    private IRollDiceUseCase rollDiceUseCase;

    [Inject]
    public void Construct(IRollDiceUseCase rollDiceUseCase)
    {
        this.rollDiceUseCase = rollDiceUseCase;
    }
    public void RequestDiceRoll(int playerId, bool isForJail)
    {
        photonView.RPC(nameof(RPC_RequestGetDiceResult), RpcTarget.MasterClient, playerId, isForJail);
    }

    [PunRPC]
    private void RPC_RequestGetDiceResult(int playerId, bool isForJail)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        DiceResult diceResult=  rollDiceUseCase.GetDiceResult(playerId, isForJail);
        photonView.RPC(nameof(RPC_RequestDiceHandle), RpcTarget.All, diceResult.First,diceResult.Second, playerId, isForJail);

    }
    [PunRPC]
    private void RPC_RequestDiceHandle(int first,int second,int playerId, bool isForJail)
    {
        rollDiceUseCase.HandleDice(first, second, playerId, isForJail);

    }
}

#region Events

public class RollDiceButtonEvent
{
    public int PlayerId { get; }
    public RollDiceButtonEvent(int playerId) => PlayerId = playerId;
}

public class RollDiceJailButtonEvent
{
    public int PlayerId { get; }
    public RollDiceJailButtonEvent(int playerId) => PlayerId = playerId;
}

public class OnPlayerMoveEvent
{
    public int PlayerId { get; }
    public int Steps { get; }
    public bool Forward { get; }
    public OnPlayerMoveEvent(int playerId,int steps,bool forward)
    {
        PlayerId= playerId;
        Steps = steps;
        Forward = forward;
    }
}
public class PlayerRolledDoubleEvent
{
    public int PlayerId { get; }
    public bool IsDouble { get; }

    public PlayerRolledDoubleEvent(int playerId, bool isDouble)
    {
        PlayerId = playerId;
        IsDouble = isDouble;
    }
}
public class DiceFadeEvent
{

    public int CellId;
    public bool IsMovementStart;
    public DiceFadeEvent(int cellId, bool isMovementStart)
    {

        CellId = cellId;
        IsMovementStart = isMovementStart;
    }
}


#endregion
