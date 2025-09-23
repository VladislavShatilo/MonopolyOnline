using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [Header("Settings")]
    [SerializeField] private PlayerSettings playerSettings;
    [SerializeField] private Color[] playerColors;
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private int jailTurnsCount =3;
    [SerializeField] private int jailRansom =500;

    [Header("Transforms")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private Transform playersStatsContainer;

    [Header("UI Windows")]
    [SerializeField] private UITurnWindow uiTurnWindow;
    [SerializeField] private UIBuyWindow uiBuyWindow;
    [SerializeField] private UIPayRentWindow uiPayRentWindow;
    [SerializeField] private UIAuctionWindow uiAuctionWindow;
    [SerializeField] private UIJailWindow uiJailWindow;
    [SerializeField] private UIRansomJailWindow uiRansomJailWindow;
    [SerializeField] private UILoanPayWindow uiLoanPayWindow;

    [SerializeField] private UIPlayerStats playerStatsPrefab;
    [SerializeField] private UICompanyCell[] companyCells; // все 28 view

    [Header("Photon Managers")]
    [SerializeField] private DiceManagerPhoton diceManagerPhoton;
    [SerializeField] private DiceManager3D diceManager3D;
    [SerializeField] private PhotonPlayerMoveManager playerMoveManager;
    [SerializeField] private PhotonTurnManager photonTurnManager;
    [SerializeField] private PhotonCompanyManager photonCompanyManager;
    [SerializeField] private PhotonLoanManager photonLoanManager;
    [SerializeField] private PhotonTradeManager photonTradeManager;
    [SerializeField] private PhotonBankNotifier photonBankNotifier;
    [SerializeField] private PhotonCompanySyncManager photonCompanySyncManager;
    [SerializeField] private PhotonAuctionManager photonAuctionManager;
    [SerializeField] private PhotonTurnSynchronizer photonTurnSynchronizer;
    [SerializeField] private PhotonPlayerSpawner photonPlayerSpawner;
    [SerializeField] private PhotonJailManager photonJailManager;
    [SerializeField] private PhotonChanceManager photonChanceManager;
    [SerializeField] private PhotonBranchManager photonBranchManager;
    public override void InstallBindings()
    {
        Container.Bind<IBoardService>().To<BoardService>().AsSingle().NonLazy();
        Container.Bind<ICellOccupancyService>().To<CellOccupancyService>().AsSingle().NonLazy();
        Container.Bind<IPlayerRepository>().To<PlayerRepository>().AsSingle().NonLazy();

        Container.Bind<IPlayerColorService>().To<UnityPlayerColorService>().AsSingle()
          .WithArguments(playerColors).NonLazy();
        Container.Bind<GameManager>().AsSingle().NonLazy();
        Container.Bind<PhotonPlayerHandler>().FromComponentInHierarchy().AsSingle();


        Container.Bind<IBoardRepository>().To<BoardRepository>().AsSingle().WithArguments(boardConfig).NonLazy();

        Container.BindInstance(parentTransform).WithId("BoardParent");
        Container.BindInstance(playerRoot).WithId("PlayerRoot").NonLazy(); 
        Container.Bind<IChanceService>().To<ChanceService>().AsSingle().NonLazy();
        Container.Bind<ICasinoService>().To<CasinoService>().AsSingle().NonLazy();
        Container.Bind<ITurnService>().To<TurnService>().AsSingle().NonLazy();
        Container.BindInstance(playerSettings).WithId("PlayerSettings").AsSingle().NonLazy();
        // Container.BindInterfacesAndSelfTo<PlayerMove>().FromComponentInHierarchy().AsTransient();
        // Container.BindInterfacesAndSelfTo<PlayerSkin>().FromComponentInHierarchy().AsTransient();

        Container.Bind<ICompanyUIService>().To<CompanyUIService>().AsSingle().NonLazy();

        // Container.Bind<UITurnWindow>().FromInstance(turnWindow).AsSingle();
        Container.Bind<ILocalPlayerService>().To<PhotonLocalPlayerService>().AsSingle();

        Container.Bind<ITurnWindow>().To<UITurnWindow>().FromInstance(uiTurnWindow).AsSingle();
        Container.BindInterfacesTo<TurnPresenter>().AsSingle().NonLazy();
        Container.Bind<IDiceService>().To<RandomDiceService>().AsSingle().NonLazy();
        Container.Bind<IPhotonDiceManager>().To<DiceManagerPhoton>().FromInstance(diceManagerPhoton).AsSingle();
        Container.Bind<IDiceManager3D>().To<DiceManager3D>().FromInstance(diceManager3D).AsSingle();

        Container.Bind<IRollDiceUseCase>().To<RollDiceUseCase>().AsSingle().NonLazy();
        Container.BindInterfacesTo<DicePresenter>().AsSingle().NonLazy();

        Container.BindInterfacesTo<PlayerMoveInitService>().AsSingle().NonLazy();
        Container.Bind<IPhotonPlayerMoveManager>().To<PhotonPlayerMoveManager>().FromInstance(playerMoveManager).AsSingle();
        Container.Bind<IPlayerMoveUseCase>().To<PlayerMoveUseCase>().AsSingle().NonLazy();

        Container.Bind<IPhotonTurnManager>().To<PhotonTurnManager>().FromInstance(photonTurnManager).AsSingle();
        Container.Bind<IPhotonCompanyManager>().To<PhotonCompanyManager>().FromInstance(photonCompanyManager).AsSingle();
        Container.Bind<IBuyWindow>().To<UIBuyWindow>().FromInstance(uiBuyWindow).AsSingle();
        Container.Bind<IAuctionWindow>().To<UIAuctionWindow>().FromInstance(uiAuctionWindow).AsSingle();


        Container.Bind<IPayRentWindow>().To<UIPayRentWindow>().FromInstance(uiPayRentWindow).AsSingle();

        Container.Bind<ICompanyRepository>().To<CompanyRepository>().AsSingle().WithArguments(boardConfig);

        Container.Bind<IBankService>().To<BankService>().AsSingle();
        Container.Bind<IEventBus>().To<EventBus>().AsSingle().NonLazy();

        Container.BindInterfacesTo<BuyCompanyPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<CellHandlerService>().AsSingle().NonLazy();

        Container.Bind<UIPlayerStats>().WithId("PlayerStatsPrefab").FromInstance(playerStatsPrefab);
        Container.Bind<Transform>().WithId("PlayerStatsContainer").FromInstance(playersStatsContainer);
        Container.Bind<IPhotonLoanManager>().To<PhotonLoanManager>().FromInstance(photonLoanManager).AsSingle();
        Container.Bind<IPhotonTradeManager>().To<PhotonTradeManager>().FromInstance(photonTradeManager).AsSingle();
        Container.Bind<IBankNotifier>().To<PhotonBankNotifier>().FromInstance(photonBankNotifier).AsSingle();
        Container.Bind<ICompanySyncService>().To<PhotonCompanySyncManager>().FromInstance(photonCompanySyncManager).AsSingle();
        Container.Bind<IPhotonAuctionManager>().To<PhotonAuctionManager>().FromInstance(photonAuctionManager).AsSingle();
        Container.Bind<IPhotonTurnSynchronizer>().To<PhotonTurnSynchronizer>().FromInstance(photonTurnSynchronizer).AsSingle();
        Container.Bind<IPlayerSpawner>().To<PhotonPlayerSpawner>().FromInstance(photonPlayerSpawner).AsSingle();
        Container.Bind<IPhotonChanceManager>().To<PhotonChanceManager>().FromInstance(photonChanceManager).AsSingle();
        Container.Bind<IPhotonBranchManager>().To<PhotonBranchManager>().FromInstance(photonBranchManager).AsSingle();

        Container.Bind<IBranchService>().To<BranchService>().AsSingle();
        Container.Bind<IBranchUseCase>().To<BranchUseCase>().AsSingle();
        Container.Bind<IAuctionService>().To<AuctionService>().AsSingle();

        Container.Bind<IGroupOwnershipService>().To<GroupOwnershipService>().AsSingle();
        Container.Bind<IBranchTurnHandler>().To<BranchTurnHandler>().AsSingle();

        Container.Bind<ITradeService>().To<TradeService>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerStatsService>()
          .FromComponentInHierarchy()
          .AsSingle();

        Container.BindInterfacesTo<UICompanyCellPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<PayRentPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<AuctionUseCase>().AsSingle().NonLazy();
        Container.BindInterfacesTo<AuctionPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<CompanyService>().AsSingle().NonLazy();

        Container.Bind<IUICompanyCellRepository>().To<UICompanyCellRepository>().AsSingle();
        Container.Bind<IGroupColors>().To<GroupColorsService>().AsSingle();
        //   Container.BindInterfacesAndSelfTo<UICompanyCell>()
        //.FromComponentsInHierarchy()
        //.AsTransient();
        foreach (var cell in companyCells)
        {
            Container.BindInterfacesAndSelfTo<UICompanyCell>().FromInstance(cell).AsCached();
        }

        Container.Bind<IPhotonJailManager>().To<PhotonJailManager>().FromInstance(photonJailManager).AsSingle();
        Container.BindInterfacesTo<JailPresenter>().AsSingle().NonLazy();

        Container.Bind<IJailWindow>().To<UIJailWindow>().FromInstance(uiJailWindow).AsSingle();
        Container.Bind<IRansomJailWindow>().To<UIRansomJailWindow>().FromInstance(uiRansomJailWindow).AsSingle();
        Container.Bind<ILoanPayWindow>().To<UILoanPayWindow>().FromInstance(uiLoanPayWindow).AsSingle();

        Container.Bind<IJailService>().To<JailService>().AsSingle();
        Container.Bind<JailRules>().AsSingle().WithArguments(jailTurnsCount, jailRansom).NonLazy();
        Container.Bind<BranchRules>().AsSingle().NonLazy();
        Container.Bind<TurnBranchAdapter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PhotonTimerUpdater>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ITimerManager>().To<TimerManager>().AsSingle().NonLazy();
        Container.BindInterfacesTo<LoanService>().AsSingle().NonLazy();

        Container.BindInterfacesTo<LoanPayPresenter>().AsSingle().NonLazy();


    }
}
