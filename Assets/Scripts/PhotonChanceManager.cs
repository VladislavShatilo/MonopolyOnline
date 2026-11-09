using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public enum BuffType
{
    MoneyGainRandom1,   // +250-1000 (2 шт)
    MoneyGainRandom2,   // +500-1500 (3 шт)
    MoneyGainFixed,     // +1500 (1 шт)
    MoneyLoseRandom1,   // -250-1000 (2 шт)
    MoneyLoseRandom2,   // -500-1500 (2 шт)
    MoneyLoseFixed,     // -1500 (2 шт)
    Teleport,           // 2 шт
    SkipTurn,           // 2 шт
    ReverseMove,        // 1 шт
    Jail                // 1 шт
}

public class PhotonChanceManager : MonoBehaviourPun, IPhotonChanceManager
{
    private IChanceService chanceService;
    private IPlayerRepository playerRepository;
    private IBankService bankService;
    private IPhotonTurnManager photonTurnManager;
    private IPhotonPlayerMoveManager photonPlayerMove;
    private IPhotonJailManager photonJailManager;
    private IChatService chatService;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IChanceService chanceService, IPlayerRepository playerRepository, IBankService bankService, IPhotonTurnManager photonTurnManager,
       IPhotonPlayerMoveManager photonPlayerMove, IPhotonJailManager photonJailManager, IChatService chatService, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.chanceService = chanceService ?? throw new ArgumentNullException(nameof(chanceService));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.photonPlayerMove = photonPlayerMove ?? throw new ArgumentNullException(nameof(photonPlayerMove));
        this.photonJailManager = photonJailManager ?? throw new ArgumentNullException(nameof(photonJailManager));
        this.chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void GiveRandomBuff(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        ChanceBuff buff = chanceService.GetRandomBuff();
        if(buff == null) return; 
        photonViewWrapper.RPC(photonView, nameof(RPC_ApplyBuff), RpcTarget.All, playerId, (int)buff.Type, buff.MinAmount, buff.MaxAmount);

        if (buff.Type != BuffType.Jail && buff.Type != BuffType.Teleport)
        {
            photonTurnManager.RequestEndTurn();
        }


    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private string ApplyMoneyChange(PlayerData player, int min, int max, bool gain, bool fixedAmount = false)
    {
        if (!photonNetworkWrapper.IsMasterClient) return "";
        int amount = fixedAmount ? max : UnityEngine.Random.Range(min, max + 1);
        if (gain)
        {
            bankService.AddMoney(player.Id, amount);
        }
        else
        {
            bankService.RemoveMoney(player.Id, amount);
        }

        return gain ? $"получил {amount}k!" : $"потерял {amount}k!";
    }

    #endregion PRIVATE_METHODS

    #region RPC

    [PunRPC]
    private void RPC_ApplyBuff(int playerId, int typeInt, int minAmount, int maxAmount)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(RPC_ApplyBuff));
        var type = (BuffType)typeInt;

        string message;

        switch (type)
        {
            case BuffType.MoneyGainRandom1:
            case BuffType.MoneyGainRandom2:
                message = ApplyMoneyChange(player, minAmount, maxAmount, true);
                break;

            case BuffType.MoneyGainFixed:
                message = ApplyMoneyChange(player, minAmount, maxAmount, true, fixedAmount: true);
                break;

            case BuffType.MoneyLoseRandom1:
            case BuffType.MoneyLoseRandom2:
                message = ApplyMoneyChange(player, minAmount, maxAmount, false);
                break;

            case BuffType.MoneyLoseFixed:
                message = ApplyMoneyChange(player, minAmount, maxAmount, false, fixedAmount: true);
                break;

            case BuffType.Teleport:

                photonPlayerMove.RequestTeleport(playerId);
                message = "телепортировался!";
                break;

            case BuffType.SkipTurn:
                player.SkipNextTurn = true;
                message = "пропускает ход!";
                break;

            case BuffType.ReverseMove:
                player.NextMoveBackward = true;
                message = "идёт в обратную сторону!";
                break;

            case BuffType.Jail:
                photonJailManager.SendToJail(playerId);
                message = "попал в тюрьму!";
                break;

            default:
                message = "";
                break;
        }
        chatService.SendMessage(playerId, message, false);
    }

    #endregion RPC


}
