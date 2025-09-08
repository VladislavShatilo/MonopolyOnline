using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static ChanceBuff;

[Serializable]
public class ChanceBuff
{
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

    public BuffType Type;
    public int Count;      // сколько осталось
    public int InitialCount; // чтобы восстановить при исчерпании
    public int MinAmount;  // для рандомных денег
    public int MaxAmount;  // для рандомных денег

    public ChanceBuff(BuffType type, int count, int minAmount = 0, int maxAmount = 0)
    {
        Type = type;
        Count = count;
        InitialCount = count;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }

    public bool IsMoneyBuff() => Type.ToString().Contains("Money");
}
public class ChanceManager : MonoBehaviourPun
{
    public static ChanceManager Instance { get; private set; }
    private System.Random random = new System.Random();

    private List<ChanceBuff> buffs = new List<ChanceBuff>();
    [Inject] private IPlayerRepository playerRepository;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeBuffs();
    }

    private void InitializeBuffs()
    {
        buffs.Clear();

        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyGainRandom1, 2, 250, 1000));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyGainRandom2, 3, 500, 1500));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyGainFixed, 1, 1500, 1500));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyLoseRandom1, 2, 250, 1000));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyLoseRandom2, 2, 500, 1500));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.MoneyLoseFixed, 2, 1500, 1500));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.Teleport, 2));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.SkipTurn, 2));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.ReverseMove, 1));
        buffs.Add(new ChanceBuff(ChanceBuff.BuffType.Jail, 1));
    }

    // Вызывается когда игрок наступает на поле "Шанс"
    public void GiveRandomBuff(int playerID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Фильтруем только оставшиеся баффы
        List<ChanceBuff> available = buffs.FindAll(b => b.Count > 0);

        if (available.Count == 0)
        {
            // Если все исчерпаны — восстанавливаем
            InitializeBuffs();
            available = buffs.FindAll(b => b.Count > 0);
        }

        // Случайный выбор
        ChanceBuff selected = available[random.Next(available.Count)];
        selected.Count--; // уменьшаем количество

        // Отправляем всем клиентам
        photonView.RPC(nameof(RPC_RequestBuff), RpcTarget.MasterClient, playerID, (int)selected.Type, selected.MinAmount, selected.MaxAmount);

    }
    [PunRPC]
    private void RPC_RequestBuff(int playerID, int typeInt, int minAmount, int maxAmount)
    {   
        if(!PhotonNetwork.IsMasterClient) { return; }
        photonView.RPC(nameof(RPC_ApplyBuff), RpcTarget.AllBuffered, playerID, typeInt, minAmount, maxAmount);
        if (typeInt != (int)BuffType.Jail && typeInt != (int)BuffType.Teleport)
        {
           // TurnManager.Instance.RequestEndTurn();
        }

    }
    [PunRPC]
    private void RPC_ApplyBuff(int playerID, int typeInt, int minAmount, int maxAmount)
    {
        var player = playerRepository.GetPlayerById(playerID);
        var type = (ChanceBuff.BuffType)typeInt;
        string message = "";

        switch (type)
        {
            case ChanceBuff.BuffType.MoneyGainRandom1:
            case ChanceBuff.BuffType.MoneyGainRandom2:
                int gain = UnityEngine.Random.Range(minAmount, maxAmount + 1);
                Bank.Instance.AddMoney(playerID, gain);
                message = $"получил {gain}k!";
                break;

            case ChanceBuff.BuffType.MoneyGainFixed:
                Bank.Instance.AddMoney(playerID, minAmount);
                message = $"получил {minAmount}k!";

                break;

            case ChanceBuff.BuffType.MoneyLoseRandom1:
            case ChanceBuff.BuffType.MoneyLoseRandom2:
                int lose = UnityEngine.Random.Range(minAmount, maxAmount + 1);
                Bank.Instance.RemoveMoney(playerID, lose);
                message = $"потерял {lose}k!";

                break;

            case ChanceBuff.BuffType.MoneyLoseFixed:
                Bank.Instance.RemoveMoney(playerID, minAmount);
                message = $"потерял {minAmount}k!";

                break;

            case ChanceBuff.BuffType.Teleport:
                message = "телепортировался!";
                EventBus.Publish(new OnPlayerTeleportEvent(player.Id));

                // TODO: реализовать телепорт игрока
                break;

            case ChanceBuff.BuffType.SkipTurn:
                player.SkipNextTurn = true;
                message = "пропускает ход!";
                // TODO: реализовать пропуск хода
                break;

            case ChanceBuff.BuffType.ReverseMove:
                player.NextMoveBackward = true;

                message = "идёт в обратную сторону!";
                // TODO: реализовать логику обратного хода
                break;

            case ChanceBuff.BuffType.Jail:
                JailManager.Instance.SendToJail(player.Id);

                message = "попал в тюрьму!";
                // TODO: отправить игрока в тюрьму
                break;
        }

        MessageLog.Instance.AddMessage(message, playerID);
    }
}
public class OnPlayerTeleportEvent
{
    public int PlayerId { get; }
    public OnPlayerTeleportEvent(int playerId)
    {
        PlayerId = playerId;
    }
}