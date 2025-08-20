using Photon.Pun;

using UnityEngine;

public class QuestionManager : MonoBehaviourPun
{
    public static QuestionManager Instance { get; private set; }
    private readonly string[] gainMessages = new[]
    {
        "нашёл клад",
        "получил подарок",
        "выиграл лотерею"
    };

    private readonly string[] loseMessages = new[]
    {
        "потерял деньги",
        "оплатил штраф",
        "разбил имущество"
    };
    private readonly int minAmount = 5;
    private readonly int maxAmount = 30;

    private readonly System.Random random = new();
    private void Awake()
    {
        Debug.Log($"Awake QuestionManager {gameObject.GetInstanceID()} PhotonViewID={photonView.ViewID}");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

      
    }
    public void HandleQuestionCell(int playerID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
       
        photonView.RPC(nameof(RPC_RequestSpendMoney), RpcTarget.MasterClient, playerID);
    }

    [PunRPC]
    private void RPC_RequestSpendMoney(int playerID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        string message = "";
        int amount = random.Next(minAmount, maxAmount + 1) * 100;
        bool gain = random.NextDouble() > 0.5;

        var player = GameManager.Instance.GetPlayerById(playerID);

        message = gain
            ? gainMessages[random.Next(gainMessages.Length)]
            : loseMessages[random.Next(loseMessages.Length)];

        photonView.RPC(nameof(RPC_ConfirmSpend), RpcTarget.AllBuffered, playerID, amount, gain, message);

        TurnManager.Instance.RequestEndTurn();
    }
    [PunRPC]
    private void RPC_ConfirmSpend(int playerID, int amount, bool gain, string message)
    {
        var player = GameManager.Instance.GetPlayerById(playerID);

        if (gain)
        {
            Bank.Instance.AddMoney(player, amount);
        }
        else
        {
            Bank.Instance.RemoveMoney(player, amount);
        }

        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.playerColor)}>{player.Name}</color>";
        MessageLog.Instance.AddMessage($"{coloredName} {message} {amount:N0}k");
    }
}
