using Photon.Pun;
using System;
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
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;
    public static bool AllowCheats = true;
    [Inject]
    public void Construct(IRollDiceUseCase rollDiceUseCase, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.rollDiceUseCase = rollDiceUseCase ?? throw new ArgumentNullException(nameof(rollDiceUseCase));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
    }
    public void RequestDiceRoll(int playerId, bool isForJail, int cheatFirst = -1, int cheatSecond = -1)
    {
        if(photonView == null) throw new NullReferenceException(nameof(RequestDiceRoll));

        photonViewWrapper.RPC(photonView, nameof(RPC_RequestGetDiceResult), RpcTarget.MasterClient, playerId, isForJail, cheatFirst, cheatSecond);
        

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
        if (!photonNetworkWrapper.IsMasterClient) return;

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
            var diceResult = rollDiceUseCase.GetDiceResult();
            // предполагаю, что DiceResult хранит First и Second
            first = diceResult.First;
            second = diceResult.Second;
        }

        if (photonView == null) throw new NullReferenceException(nameof(RPC_RequestGetDiceResult));
        // рассылаем всем единый результат
        photonViewWrapper.RPC(photonView,nameof(RPC_RequestDiceHandle), RpcTarget.All, first, second, playerId, isForJail);
    }
    [PunRPC]
    private void RPC_RequestDiceHandle(int first,int second,int playerId, bool isForJail)
    {
       StartCoroutine( rollDiceUseCase.HandleDice(first, second, playerId, isForJail));

    }
  
}
