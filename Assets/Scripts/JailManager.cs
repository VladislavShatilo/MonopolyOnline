using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// Управляет логикой тюрьмы: помещение, проверка бросков, выкуп, выход.
/// Работает в сетевой среде Photon и использует EventBus.
/// </summary>
public class JailManager : MonoBehaviourPun
{
    public static JailManager Instance { get; private set; }

    [Header("Настройки тюрьмы")]
    [SerializeField] private int jailTurns = 3;          // Кол-во ходов, которые игрок сидит в тюрьме
    [SerializeField] private int jailFine = 500;         // Штраф за выход из тюрьмы

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Инициализация статуса для локального игрока
        EventBus.Publish(new SetTurnsJailEvent(PhotonNetwork.LocalPlayer.ActorNumber, 0));
    }

    private void OnEnable()
    {
        EventBus.Subscribe<StartTurnJailEvent>(OnStartTurnJail);
        EventBus.Subscribe<CheckDiceJailEvent>(OnCheckDiceJail);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<StartTurnJailEvent>(OnStartTurnJail);
        EventBus.Unsubscribe<CheckDiceJailEvent>(OnCheckDiceJail);
    }

    #region Jail Logic

    /// <summary> Отправляет игрока в тюрьму (только мастер-клиент). </summary>
    public void SendToJail(int playerID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_SendToJail), PhotonNetwork.CurrentRoom.GetPlayer(playerID), playerID);
        TurnManager.Instance.RequestEndTurn();
    }

    [PunRPC]
    private void RPC_SendToJail(int playerID)
    {
        var player = GameManager.Instance.GetPlayerById(playerID);
        player.IsInJail = true;
        player.JailTurnsLeft = jailTurns;

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            EventBus.Publish(new MoveToJailEvent(playerID));
            EventBus.Publish(new SetTurnsJailEvent(playerID, player.JailTurnsLeft));
        }
       
    }

    /// <summary> Выпускает игрока из тюрьмы. </summary>
    public void ReleaseFromJail(int playerID, bool isPaidExit)
    {
        var player = GameManager.Instance.GetPlayerById(playerID);

        player.IsInJail = false;
        player.JailTurnsLeft = 0;

        EventBus.Publish(new SetTurnsJailEvent(playerID, player.JailTurnsLeft));

        if (isPaidExit)
        {
            Bank.Instance.RemoveMoney(player, jailFine);
            EventBus.Publish(new RollDiceButtonEvent(playerID)); // сразу бросаем кубики
        }
    }

    #endregion

    #region Turn Handling

    private void OnStartTurnJail(StartTurnJailEvent e)
    {
        var player = GameManager.Instance.GetPlayerById(e.PlayerId);
        if (!player.IsInJail) return;

        Debug.Log($"[Jail] Игрок {e.PlayerId} в тюрьме ({player.JailTurnsLeft} ходов осталось)");

        photonView.RPC(nameof(RPC_ShowJailOffer), PhotonNetwork.CurrentRoom.GetPlayer(e.PlayerId), e.PlayerId);
        EventBus.Publish(new SetTurnsJailEvent(e.PlayerId, player.JailTurnsLeft));
    }

    private void OnCheckDiceJail(CheckDiceJailEvent e)
    {
        if (e.FirstDice == e.SecondDice)
        {
            // Удвоенные кости ? выход
            ReleaseFromJail(e.PlayerID, false);

            if (PhotonNetwork.LocalPlayer.ActorNumber == e.PlayerID)
                EventBus.Publish(new OnPlayerMoveEvent(e.FirstDice + e.SecondDice,true));
        }
        else
        {
            HandleJailTurn(e.PlayerID);
        }
    }

    private void HandleJailTurn(int playerID)
    {
        var player = GameManager.Instance.GetPlayerById(playerID);
        if (!player.IsInJail) return;

        if (player.JailTurnsLeft > 0)
        {
            player.JailTurnsLeft--;
            EventBus.Publish(new SetTurnsJailEvent(playerID, player.JailTurnsLeft));

            if (player.JailTurnsLeft == 0)
            {
                photonView.RPC(nameof(RPC_ShowRansomJailOffer), PhotonNetwork.CurrentRoom.GetPlayer(player.id), player.id);
                return;
            }
        }

        TurnManager.Instance.RequestEndTurn();
    }

    #endregion

    #region RPC UI

    [PunRPC] private void RPC_ShowJailOffer(int playerID) => UIJailWindow.Instance.ShowWindow(playerID);

    [PunRPC] private void RPC_ShowRansomJailOffer(int playerID) => UIRansomJailWindow.Instance.ShowWindow(playerID);

    #endregion
}

#region Events

public class PlayerMovedToJailEvent
{
    public int PlayerID { get; }
    public int JailCellIndex { get; }

    public PlayerMovedToJailEvent(int playerId, int jailCellIndex)
    {
        PlayerID = playerId;
        JailCellIndex = jailCellIndex;
    }
}

public class PlayerSkippedTurnEvent
{
    public int PlayerID { get; }
    public string Reason { get; }

    public PlayerSkippedTurnEvent(int playerId, string reason)
    {
        PlayerID = playerId;
        Reason = reason;
    }
}

public class CheckDiceJailEvent
{
    public int PlayerID { get; }
    public int FirstDice { get; }
    public int SecondDice { get; }

    public CheckDiceJailEvent(int firstDice, int secondDice, int playerId)
    {
        PlayerID = playerId;
        FirstDice = firstDice;
        SecondDice = secondDice;
    }
}

#endregion
