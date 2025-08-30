using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private int distanceСorrection = 45;

    private GameObject tempCell; 
    private int cheatMoves = -1;

    private void Awake()
    {
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);
    }
  
    private void OnEnable()
    {
        EventBus.Subscribe<DiceFadeEvent>(HighlightCell);
        EventBus.Subscribe<RollDiceJailButtonEvent>(OnRollDiceJail);
        EventBus.Subscribe<RollDiceButtonEvent>(OnRollDiceButton);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DiceFadeEvent>(HighlightCell);
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
        RollDiceRoutine(first, second, playerId, isForJail);
    }

    private void RollDiceRoutine(int first, int second, int playerId, bool isForJail)
    {
        dice1GO.SetActive(true);
        dice2GO.SetActive(true);
        dice1Instance.RollToResult(first);
        dice2Instance.RollToResult(second);


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
    public void HighlightCell(DiceFadeEvent e)
    {
        if (e.IsMovementStart)
        {
            fadeImage.gameObject.SetActive(true);
            GameObject cell = CellsManager.Instance.GetCellByIndex(e.CellId);
            tempCell = Instantiate(cell, fadeImage.transform);
            RectTransform rectTransform = tempCell.GetComponent<RectTransform>();
            tempCell.GetComponent<RectTransform>().position = new Vector2(rectTransform.position.x + distanceСorrection, rectTransform.position.y - distanceСorrection);
        }
        else
        {
            fadeImage.gameObject.SetActive(false);
            if (tempCell != null) Destroy(tempCell);
            dice1GO.SetActive(false);
            dice2GO.SetActive(false);
        }
       
    }
    private void HandlePlayerMove(int first, int second, int playerId)
    {
        int result = (cheatMoves > 0) ? cheatMoves : (first + second);
        bool isDouble = first == second;
        PlayerData player = GameManager.Instance.GetPlayerById(playerId);

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            if (player.NextMoveBackward)
            {
                player.NextMoveBackward = false; // сбросим, чтобы только один ход был назад
                EventBus.Publish(new OnPlayerMoveEvent(result, false));
            }
            else
            {
                EventBus.Publish(new OnPlayerMoveEvent(result, true));
            }
            if (isDouble)
            {
                EventBus.Publish(new PlayerRolledDoubleEvent(playerId, isDouble));
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
    public bool Forward { get; }
    public OnPlayerMoveEvent(int steps,bool forward)
    {
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
