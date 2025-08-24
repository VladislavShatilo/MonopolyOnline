using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// DiceManagerPhoton отвечает за синхронные броски кубиков в Photon.
/// Работает как для обычных ходов, так и для проверок выхода из тюрьмы.
/// </summary>
public class DiceManagerPhoton : MonoBehaviourPun
{
    [Header("Dice Prefabs")]
    [SerializeField] private GameObject dice1GO;
    [SerializeField] private GameObject dice2GO;
    [SerializeField] private DiceRoll3D dice1Instance;
    [SerializeField] private DiceRoll3D dice2Instance;

    [Header("Cheat Keys")]
    [SerializeField] private KeyCode cheat10 = KeyCode.Q;
    [SerializeField] private KeyCode cheat30 = KeyCode.W;
    [SerializeField] private KeyCode cheat40 = KeyCode.E;
    private int cheatMoves = -1;

    private void Awake()
    {
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RollDiceJailButtonEvent>(OnRollDiceJail);
        EventBus.Subscribe<RollDiceButtonEvent>(OnRollDiceButton);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RollDiceJailButtonEvent>(OnRollDiceJail);
        EventBus.Unsubscribe<RollDiceButtonEvent>(OnRollDiceButton);
    }

    private void Update()
    {
        HandleCheatInputs();
    }

    #region Dice Rolling

    private void OnRollDiceJail(RollDiceJailButtonEvent e) =>
        RequestDiceRoll(e.PlayerId, true);

    private void OnRollDiceButton(RollDiceButtonEvent e) =>
        RequestDiceRoll(e.PlayerId, false);

    private void RequestDiceRoll(int playerId, bool isForJail)
    {
        photonView.RPC(nameof(RPC_RequestDiceRoll), RpcTarget.MasterClient, playerId, isForJail);
    }

    [PunRPC]
    private void RPC_RequestDiceRoll(int playerId, bool isForJail)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int first = Random.Range(1, 7);
        int second = Random.Range(1, 7);

        photonView.RPC(nameof(RPC_SetDiceResult), RpcTarget.AllBuffered, first, second, playerId, isForJail);
    }

    [PunRPC]
    private void RPC_SetDiceResult(int first, int second, int playerId, bool isForJail)
    {
        StartCoroutine(RollDiceRoutine(first, second, playerId, isForJail));
    }

    private IEnumerator RollDiceRoutine(int first, int second, int playerId, bool isForJail)
    {
        dice1GO.SetActive(true);
        dice2GO.SetActive(true);

        dice1Instance.RollToResult(first);
        dice2Instance.RollToResult(second);

        yield return new WaitForSeconds(0.4f);

        if (!isForJail && PhotonNetwork.IsMasterClient)
        {
            bool isDouble = first == second;
            TurnManager.Instance.RegisterDoubleForTurn(playerId, isDouble);
        }

        if (isForJail)
        {
            EventBus.Publish(new CheckDiceJailEvent(first, second, playerId));
        }
        else
        {
            HandlePlayerMove(first, second, playerId);
        }
    }

    private void HandlePlayerMove(int first, int second, int playerId)
    {
        int result = (cheatMoves > 0) ? cheatMoves : (first + second);
        bool isDouble = first == second;

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            EventBus.Publish(new OnPlayerMoveEvent(result));
            if (isDouble)
            {
                EventBus.Publish(new PlayerRolledDoubleEvent(playerId, result));
            }
        }

        cheatMoves = -1; // сброс
    }

    #endregion

    #region Cheats

    private void HandleCheatInputs()
    {
        // Читы (1–9 → шаги)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
                cheatMoves = i;
        }

        if (Input.GetKeyDown(cheat10)) cheatMoves = 10;
        if (Input.GetKeyDown(cheat30)) cheatMoves = 30;
        if (Input.GetKeyDown(cheat40)) cheatMoves = 40;
    }

    #endregion
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
    public int Steps { get; }
    public OnPlayerMoveEvent(int steps) => Steps = steps;
}
public class PlayerRolledDoubleEvent
{
    public int PlayerId { get; }
    public int Steps { get; }

    public PlayerRolledDoubleEvent(int playerId, int steps)
    {
        PlayerId = playerId;
        Steps = steps;
    }
}
#endregion
