using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<UIPlayerStats> playersStats = new List<UIPlayerStats>();
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private UIBuyWindow uiBuyWindow;
    [SerializeField] private CellsManager cellsManager;
    private List<Player> players = new List<Player>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        PlayerMove.ShowBuyMenuAction += ShowBuyMenu;
    }
    private void OnDisable()
    {
        PlayerMove.ShowBuyMenuAction = ShowBuyMenu;
    }

    private void Start()
    {
        players.Add(new Player("Владислав", 15000, 0,Color.red));
        SetPlayerUIStats();
        Bank.Instance.OnBalanceChanged += (player, money) =>
        {
            Debug.Log($"{player.Name} теперь имеет {money}$");
            foreach (var playerStats in playersStats)
            {
                playerStats.SetMoneyPlayerText(money);
            }

        };

        // Bank.Instance.RemoveMoney(player2, 100);  // -100 у Игрока 2
        // Bank.Instance.TransferMoney(player1, player2, 300); // Перевод
    }
    public Player GetPlayerById(int id)
    {
        return players.Find(p => p.id == id);
    }
    private void SetPlayerUIStats()
    {
        for(int i = 0;i < playersStats.Count; i++)
        {
            playersStats[i].SetPlayerStats(players[i]);
        }
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
            Bank.Instance.AddMoney(players[0], 200);  

        }
    }
}
