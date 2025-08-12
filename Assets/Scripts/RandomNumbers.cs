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
    [SerializeField] private TextMeshProUGUI firstRandomText;
    [SerializeField] private TextMeshProUGUI secondRandomText;
    public static Action<int> playerMoveAction;

    public static RandomNumbers Instance { get; private set; }

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

    public void OnRollDiceButton()
    {
       
            TurnManager.Instance.RollDice();
       
    }

    public void SetDiceNumbers(int first, int second, int targetPlayerId)
    {
        firstRandomText.text = first.ToString();
        secondRandomText.text = second.ToString();

        // Ходит только тот, чей это ID
        if (PhotonNetwork.LocalPlayer.ActorNumber == targetPlayerId)
        {
            playerMoveAction?.Invoke(first + second);
        }
    }


}
