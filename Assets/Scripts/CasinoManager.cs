using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasinoManager : MonoBehaviourPun
{
    public static CasinoManager Instance { get; private set; }
    private int playerID;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CasinoOffer(int playerId)
    {
        this.playerID = playerId;
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_ShowPurchaseOffer), PhotonNetwork.CurrentRoom.GetPlayer(playerId));
    }

    [PunRPC]
    private void RPC_ShowPurchaseOffer()
    {
       // UICasinoWindow.Instance.ShowWindow();
    }

    public void PlayGame(int[] selectedNumbers)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // бросаем кубик
        int rolled = Random.Range(1, 7);
        Debug.Log($"Casino dice rolled: {rolled}");

        int reward = CalculateReward(selectedNumbers, rolled);

        // применяем результат к игроку
        photonView.RPC(nameof(RPC_CasinoChangeMoney), RpcTarget.AllBuffered, reward, playerID);
        // завершаем ход
       // TurnManager.Instance.RequestEndTurn();
    }
    [PunRPC]
    private void RPC_CasinoChangeMoney(int reward, int playerID)
    {
        if(reward > 0)
        {
            Bank.Instance.AddMoney(playerID, reward);

        }
        else
        {
            reward = Mathf.Abs(reward);
            Bank.Instance.AddMoney(playerID, reward);
        }

    }

    private int CalculateReward(int[] selectedNumbers, int rolled)
    {
        int count = selectedNumbers.Length;
        bool guessed = System.Array.Exists(selectedNumbers, num => num == rolled);

        if (!guessed)
            return -1000;

        return count switch
        {
            1 => 6000,
            2 => 3000,
            3 => 2000,
            _ => -1000
        };
    }
}
