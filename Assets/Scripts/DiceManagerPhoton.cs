using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceManagerPhoton : MonoBehaviourPun
{
  
    [Header("Dice Prefabs")]
    [SerializeField] private GameObject dice1GO;
    [SerializeField] private GameObject dice2GO;
    [SerializeField] private DiceRoll3D dice1Instance;
    [SerializeField] private DiceRoll3D dice2Instance;


    private int sumOfDices;
    private int cheatMoves = -1;

    private void Awake()
    {
        // Инициализация кубов
        dice1GO.SetActive(false);
        dice2GO.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<StartDiceRollEvent>(OnStartDiceRoll);
        EventBus.Subscribe<RollDiceButtonEvent>(OnRollDiceButton);

    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<StartDiceRollEvent>(OnStartDiceRoll);
        EventBus.Unsubscribe<RollDiceButtonEvent>(OnRollDiceButton);
    }

    private void Update()
    {
        // Читы (1–9, Q = 10)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
                cheatMoves = i;
        }
        if (Input.GetKeyDown(KeyCode.Q)) cheatMoves = 10;
    }

    private void OnRollDiceButton(RollDiceButtonEvent e)
    {
        photonView.RPC(nameof(RPC_RequestRollDice), RpcTarget.MasterClient, e.PlayerId);
    }
    
    [PunRPC]
    private void RPC_RequestRollDice(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int first = UnityEngine.Random.Range(1, 7);
        int second = UnityEngine.Random.Range(1, 7);
        photonView.RPC(nameof(RPC_SetDiceResult), RpcTarget.AllBuffered, first, second, playerId);
    }

    [PunRPC]
    private void RPC_SetDiceResult(int first, int second, int playerId)
    {
        EventBus.Publish(new StartDiceRollEvent(first, second, playerId));
    }

    private void OnStartDiceRoll(StartDiceRollEvent e)
    {
        // Мастер сказал всем запустить анимацию броска с результатом
        dice1GO.SetActive(true);
        dice2GO.SetActive(true);

        dice1Instance.RollToResult(e.FirstDiceValue);
        dice2Instance.RollToResult(e.SecondDiceValue);

        SetDiceNumbers(e.FirstDiceValue, e.SecondDiceValue, e.PlayerId);
    }

    private void SetDiceNumbers(int first, int second, int targetPlayerId)
    {
        int result = (cheatMoves > 0) ? cheatMoves : (first + second);

        if (PhotonNetwork.LocalPlayer.ActorNumber == targetPlayerId)
        {
            EventBus.Publish(new OnPlayerMoveEvent(result));
        }

        cheatMoves = -1; // сбрасываем чит
    }
}
public class RollDiceButtonEvent
{
    public int PlayerId;
    public RollDiceButtonEvent(int playerId)
    {
        PlayerId = playerId;
    }
}
public class OnPlayerMoveEvent
{
    public int Steps;
    public OnPlayerMoveEvent(int steps)
    {
        Steps = steps;
    }
}