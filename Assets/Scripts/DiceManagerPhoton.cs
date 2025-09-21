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
    public static bool AllowCheats = true;
    [Inject]
    public void Construct(IRollDiceUseCase rollDiceUseCase)
    {
        this.rollDiceUseCase = rollDiceUseCase;
    }
    public void RequestDiceRoll(int playerId, bool isForJail, int cheatFirst = -1, int cheatSecond = -1)
    {
        // посылаем запрос мастеру с возможными override (чита)
        photonView.RPC(nameof(RPC_RequestGetDiceResult), RpcTarget.MasterClient, playerId, isForJail, cheatFirst, cheatSecond);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            AllowCheats = true;
            Debug.Log(AllowCheats);
        }
        else if (Input.GetKeyUp(KeyCode.V))
        {
            AllowCheats = false;
            Debug.Log(AllowCheats);


        }
    }
    [PunRPC]
    private void RPC_RequestGetDiceResult(int playerId, bool isForJail, int cheatFirst, int cheatSecond)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int first, second;
        // Доп: используем флаг AllowCheats, чтобы разрешать только на мастере и только при включенном флаге
        if (AllowCheats && cheatFirst > -1 && cheatSecond > -1)
        {
            first = cheatFirst;
            second = cheatSecond;
        }
        else
        {
            // честный бросок — можно использовать rollDiceUseCase для консистентности
            var diceResult = rollDiceUseCase.GetDiceResult(playerId, isForJail);
            // предполагаю, что DiceResult хранит First и Second
            first = diceResult.First;
            second = diceResult.Second;
        }

        // рассылаем всем единый результат
        photonView.RPC(nameof(RPC_RequestDiceHandle), RpcTarget.All, first, second, playerId, isForJail);
    }
    [PunRPC]
    private void RPC_RequestDiceHandle(int first,int second,int playerId, bool isForJail)
    {
        rollDiceUseCase.HandleDice(first, second, playerId, isForJail);

    }
}

#region Events

public class OnPlayerMoveEvent
{
    public int PlayerId { get; }
    public int Steps { get; }
    public bool Forward { get; }
    public  OnPlayerMoveEvent(int playerId,int steps,bool forward)
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
