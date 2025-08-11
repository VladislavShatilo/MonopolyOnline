using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private UIPlayerStats playersStatsPrefabs;
    [SerializeField] private Transform playersStatsContainerTrans;
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private UIBuyWindow uiBuyWindow;
    [SerializeField] private CellsManager cellsManager;
    [SerializeField] private Color [] playerColors;   
    private List<PlayerData> players = new List<PlayerData>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Я вошёл в комнату");
        SetupPlayer(PhotonNetwork.LocalPlayer);
        CheckStartGame();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} зашёл в комнату.");
        MessageLog.Instance.AddMessage($"{newPlayer.NickName} зашёл в комнату.");
        SetupPlayer(newPlayer);
        CheckStartGame();
    }

    // Общий метод для создания и настройки UI игрока
    private void SetupPlayer(Photon.Realtime.Player photonPlayer)
    {
        int idPlayerOnRoom = GetPlayerOrderId(photonPlayer);

        // Создаем новый UI элемент для игрока
        GameObject go = Instantiate(playersStatsPrefabs.gameObject, playersStatsContainerTrans);
        var uiPlayerStats = go.GetComponent<UIPlayerStats>();

        // Создаем PlayerData
        PlayerData player = new PlayerData(photonPlayer.NickName, 15000, idPlayerOnRoom, playerColors[idPlayerOnRoom], photonPlayer);

        // Настраиваем UI под этого игрока
        uiPlayerStats.SetPlayerStats(player);

        // Подписываемся на событие изменения баланса для этого игрока (лучше делать в Bank с проверкой по player)
        Bank.Instance.OnBalanceChanged += (changedPlayer, money) =>
        {
            if (changedPlayer.id == player.id)
            {
                Debug.Log($"{changedPlayer.Name} теперь имеет {money}$");
                uiPlayerStats.SetMoneyPlayerText(money);
            }
        };
    }

    // Проверяем условие старта игры
    private void CheckStartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StartGame();
        }
    }

    private int GetPlayerOrderId(Photon.Realtime.Player photonPlayer)
    {
        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].ActorNumber == photonPlayer.ActorNumber)
                return i;  // i — порядковый ID: 0,1,2...
        }
        return -1; // не найден
    }
    private void StartGame()
    {

    }
    public PlayerData GetPlayerById(int id)
    {
        return players.Find(p => p.id == id);
    }
    private void SetPlayerUIStats()
    {
        //for(int i = 0;i < playersStats.Count; i++)
        //{
        //    playersStats[i].SetPlayerStats(players[i]);
        //}
    }
    private void ShowBuyMenu(int currentCellID)
    {
        var cellData = boardConfig.cells[currentCellID];
        var company = cellData.companyData;

        if (company.isBought)
            return; 

        uiBuyWindow.SetBuyText(company.price[0]);
        uiBuyWindow.ShowBuyWindow();

        uiBuyWindow.BuyButton().onClick.RemoveAllListeners();
        uiBuyWindow.BuyButton().onClick.AddListener(() =>
        {
            var currentPlayer = GetPlayerById(0); // Текущий игрок, можно через ход
            if (Bank.Instance.BuyCompany(currentPlayer, company))
            {
                uiBuyWindow.HideBuyWindow();
                cellsManager.RefreshCellUI(currentCellID,currentPlayer);
            }
        });
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Bank.Instance.AddMoney(GetPlayerById(0), 200);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Bank.Instance.RemoveMoney(GetPlayerById(1), 200);
        }
    }
}
