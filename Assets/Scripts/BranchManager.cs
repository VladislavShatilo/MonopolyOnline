//using Photon.Pun;
//using UnityEngine;
//using Zenject;


//public class BranchManager : MonoBehaviourPun
//{
//   // public static BranchManager Instance { get; private set; }
//    private IPlayerRepository playerRepository;
//    private ICompanyUIService companyUIService;
//    private ICompanyRepository companyRepository;

//    [Inject]
//    public void Construct(IPlayerRepository playerRepository, ICompanyUIService companyUIService, ICompanyRepository companyRepository)
//    {
//        this.companyRepository = companyRepository;
//        this.playerRepository = playerRepository;
//        this.companyUIService = companyUIService;
//    }
//    private void Awake()
//    {
//        //if (Instance != null && Instance != this)
//        //{
//        //    Destroy(gameObject);
//        //    return;
//        //}

//        //Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    private void OnEnable()
//    {
//       // EventBus.Subscribe<TurnStartEvent>(OnTurnStart);
//        //EventBus.Subscribe<RollDiceButtonEvent>(OnRollDice);
//    }

//    private void OnDisable()
//    {
//       // EventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
//       // EventBus.Unsubscribe<RollDiceButtonEvent>(OnRollDice);
//    }

//    #region Public Requests

//    public void RequestBuyBranch(int companyId)
//    {
//        photonView.RPC(nameof(RPC_BuyBranchRequest), RpcTarget.MasterClient,
//            companyId, PhotonNetwork.LocalPlayer.ActorNumber);
//    }

//    public void RequestSellBranch(int companyId)
//    {
//        photonView.RPC(nameof(RPC_SellBranchRequest), RpcTarget.MasterClient,
//            companyId, PhotonNetwork.LocalPlayer.ActorNumber);
//    }

//    #endregion

//    #region RPC Handlers (Server-side)

//    [PunRPC]
//    private void RPC_BuyBranchRequest(int companyId, int playerId)
//    {
//        if (!PhotonNetwork.IsMasterClient) return;

//        if (!TryGetCompanyAndPlayer(companyId, playerId, out var company, out var player))
//            return;

//        if (!CanBuyBranch(company, player)) return;

//        //company.RentLevel++;

//        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All,
//            playerId, companyId, company.RentLevel, true);
//        photonView.RPC(nameof(RPC_HideButtons), RpcTarget.All, playerId, companyId);
//    }

//    [PunRPC]
//    private void RPC_SellBranchRequest(int companyId, int playerId)
//    {
//        if (!PhotonNetwork.IsMasterClient) return;

//        if (!TryGetCompanyAndPlayer(companyId, playerId, out var company, out var player))
//            return;

//        if (!CanSellBranch(company, player)) return;

//        //company.RentLevel--;

//        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All,
//            playerId, companyId, company.RentLevel, false);
//        photonView.RPC(nameof(RPC_HideSellButtons), RpcTarget.All, playerId, companyId);
//    }

//    [PunRPC]
//    private void RPC_UpdateBranchUI(int playerId, int companyId, int newLevel, bool isBuy)
//    {


//        var uiCompany = companyUIService.GetCompanyUI(companyId);
//        if (uiCompany == null) return;

//        var company = companyRepository.GetCompanyById(companyId);
//        uiCompany.UpdateBranchStars(newLevel);
//        //uiCompany.SetRentText(company?.CompanyData.rent[newLevel] ?? 0);
//      //  company.RentLevel = newLevel;
//        if (!isBuy)
//        {
//            //Bank.Instance.AddMoney(playerId, company.CompanyData.branchPrice);

//        }
//        else
//        {
//           // Bank.Instance.RemoveMoney(playerId, company.CompanyData.branchPrice);

//        }
//    }

//    [PunRPC]
//    private void RPC_HideButtons(int playerId, int companyId)
//    {
//        var company = companyRepository.GetCompanyById(companyId);
//        if (company == null) return;
//        HideAllBranchButtonsByGroup(playerId, company.Group);
//    }

//    [PunRPC]
//    private void RPC_HideSellButtons(int playerId, int companyId)
//    {
//        var company = companyRepository.GetCompanyById(companyId);
//        var ui = companyUIService.GetCompanyUI(companyId);
//        if (company == null || ui == null) return;

//        if (company.RentLevel == 0)
//            ui.HideAllBranchButtons();
//        else if (company.RentLevel == 5)
//            ui.ShowBuySellButtons();
//    }

//    #endregion

//    #region Event Handlers

//    private void OnTurnStart(TurnStartEvent e)
//    {
       

//        //if (CompanyManager.Instance == null)
//        //{
//        //    Debug.LogError("CompanyManager.Instance == null!");
//        //    return;
//        //}

//        foreach (var company in companyRepository.GetAll())
//        {
//            if (company == null)
//            {
//                Debug.LogWarning($"CompanyData == null для компании {company.Id}");
//                continue;
//            }

//            var ui = companyUIService.GetCompanyUI(company.Id);
//            if (ui == null)
//            {
//                Debug.LogWarning($"UICompanyCell == null для компании {company.Id}");
//                continue;
//            }

//            bool isMyTurn = e.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
//            //bool ownsGroup = CompanyManager.Instance.PlayerOwnsWholeGroup(company.CompanyData.group, e.PlayerId);

//            //if (!isMyTurn || !company.IsBought || company.OwnerId != e.PlayerId || !ownsGroup || company.IsMortgaged)
//            //{
//            //    ui.HideAllBranchButtons();
//            //    continue;
//            //}

//            ShowBranchButtonsForLevel(ui, company.RentLevel);
//        }
//    }

//    private void OnRollDice( e)
//    {
//        if (e.PlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;

//        foreach (var company in companyRepository.GetAll())
//        {
//            var ui = companyUIService.GetCompanyUI(company.Id);
//            ui?.HideAllBranchButtons();
//        }
//    }

//    #endregion

//    #region Private Helpers

//    private bool TryGetCompanyAndPlayer(int companyId, int playerId, out Company company, out PlayerData player)
//    {
//        company = companyRepository.GetCompanyById(companyId);
        
//        player = playerRepository.GetPlayerById(playerId);

//        return company != null && player != null;
//    }

//    private bool CanBuyBranch(Company company, PlayerData player)
//    {
//        return company.OwnerId == player.Id &&
//               company.RentLevel < 5; 
//               //Bank.Instance.HasEnoughMoney(player.Id, company.CompanyData.branchPrice);
//    }

//    private bool CanSellBranch(Company company, PlayerData player)
//    {
//        return company.OwnerId == player.Id && company.RentLevel > 0;
//    }

//    public void ShowBranchButtonsForLevel(UICompanyCell ui, int level)
//    {
//        switch (level)
//        {
//            case 0: ui.ShowBuyFirstBranchButton(); break;
//            case 5: ui.ShowSellFirstButton(); break;
//            default: ui.ShowBuySellButtons(); break;
//        }
//    }

//    private void HideAllBranchButtonsByGroup(int currentPlayerId, CompanyGroup group)
//    {
//        if (currentPlayerId != PhotonNetwork.LocalPlayer.ActorNumber) return;

//        foreach (var company in companyRepository.GetAll())
//        {
//            if (company.Type != CompanyType.Company || company.Group != group) continue;

//            var ui = companyUIService.GetCompanyUI(company.Id);
//            ui?.HideAllBranchButtons();
//        }
//    }

//    #endregion
//}
