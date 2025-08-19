using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomNumbers : MonoBehaviour
{
    [SerializeField] private Button randomButton;

    public static Action<int> playerMoveAction;

    private int sumOfDices;
    private int cheatMoves =1;

    public static RandomNumbers Instance { get; private set; }

    public int SumOfDices()=>sumOfDices;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        randomButton.onClick.AddListener(OnRollDiceButton);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cheatMoves = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            cheatMoves = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            cheatMoves = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            cheatMoves = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            cheatMoves = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            cheatMoves = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            cheatMoves = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            cheatMoves = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            cheatMoves = 9;
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            cheatMoves = 10;
        }
    }
    public void OnRollDiceButton()
    {
        // Любой игрок может нажать кнопку броска
        CellsManager.Instance.HideAllBranchButtons(PhotonNetwork.LocalPlayer.ActorNumber);
        TurnManager.Instance.RequestRollDice(PhotonNetwork.LocalPlayer.ActorNumber);
    }
    public void SetDiceNumbers(int first, int second, int targetPlayerId)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == targetPlayerId)
        {
            playerMoveAction?.Invoke(cheatMoves);
        }
        sumOfDices = cheatMoves;
    }

  
}
