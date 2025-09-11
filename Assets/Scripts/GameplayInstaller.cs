
using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private PlayerSettings playerSettings;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Color[] playerColors;
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private UITurnWindow uiTurnWindow;
    [SerializeField] private UIBuyWindow uiBuyWindow;
    [SerializeField] private UIPayRentWindow uiPayRentWindow;

    [SerializeField] private DiceManagerPhoton diceManagerPhoton;
    [SerializeField] private DiceManager3D diceManager3D;
    [SerializeField] private PhotonPlayerMoveManager playerMoveManager;
    [SerializeField] private PhotonTurnManager photonTurnManager;
    [SerializeField] private PhotonCompanyManager photonCompanyManager;
    [SerializeField] private PhotonLoanManager photonLoanManager;
    [SerializeField] private PhotonTradeManager photonTradeManager;
    [SerializeField] private PhotonBankNotifier photonBankNotifier;
    [SerializeField] private PhotonCompanySyncManager photonCompanySyncManager;

    [Header("Player Stats UI")]
    [SerializeField] private UIPlayerStats playerStatsPrefab;
    [SerializeField] private Transform playersStatsContainer;

    [SerializeField] private UICompanyCell[] companyCells; // сюда в инспекторе закинешь все 28 view


    public override void InstallBindings()
    {
        Container.Bind<IBoardService>().To<BoardService>().AsSingle().NonLazy();
        Container.Bind<ICellOccupancyService>().To<CellOccupancyService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PlayerViewService>().AsSingle().NonLazy();
        Container.Bind<IPlayerRepository>().To<PlayerRepository>().AsSingle().NonLazy();

        Container.Bind<IPlayerColorService>().To<UnityPlayerColorService>().AsSingle()
          .WithArguments(playerColors).NonLazy();
        Container.Bind<GameManager>().AsSingle().NonLazy();
        Container.Bind<PhotonPlayerHandler>().FromComponentInHierarchy().AsSingle();


        Container.Bind<IBoardRepository>().To<BoardRepository>().AsSingle().WithArguments(boardConfig).NonLazy();

        Container.BindInstance(parentTransform).WithId("BoardParent");
        Container.BindInstance(playerRoot).WithId("PlayerRoot").NonLazy(); // сцена передает Transform


        Container.Bind<ICompanyService>().To<CompanyService>().AsSingle().NonLazy();
        Container.Bind<IChanceService>().To<ChanceService>().AsSingle().NonLazy();
        Container.Bind<IJailService>().To<JailService>().AsSingle().NonLazy();
        Container.Bind<ICasinoService>().To<CasinoService>().AsSingle().NonLazy();
        Container.Bind<ITurnService>().To<TurnService>().AsSingle().NonLazy();
        Container.BindInstance(playerSettings).WithId("PlayerSettings").AsSingle().NonLazy();
        Container.Bind<IPlayerSpawner>().To<PhotonPlayerSpawner>().AsSingle().NonLazy();
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
        Container.Bind<IPayRentWindow>().To<UIPayRentWindow>().FromInstance(uiPayRentWindow).AsSingle();

        Container.Bind<ICompanyRepository>().To<CompanyRepository>().AsSingle().WithArguments(boardConfig);

        Container.Bind<IBankService>().To<BankService>().AsSingle();


        Container.BindInterfacesTo<BuyCompanyPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<CellHandlerService>().AsSingle().NonLazy();

        Container.Bind<UIPlayerStats>().WithId("PlayerStatsPrefab").FromInstance(playerStatsPrefab);
        Container.Bind<Transform>().WithId("PlayerStatsContainer").FromInstance(playersStatsContainer);
        Container.Bind<IPhotonLoanManager>().To<PhotonLoanManager>().FromInstance(photonLoanManager).AsSingle();
        Container.Bind<IPhotonTradeManager>().To<PhotonTradeManager>().FromInstance(photonTradeManager).AsSingle();
        Container.Bind<IBankNotifier>().To<PhotonBankNotifier>().FromInstance(photonBankNotifier).AsSingle();
        Container.Bind<ICompanySyncService>().To<PhotonCompanySyncManager>().FromInstance(photonCompanySyncManager).AsSingle();

        Container.Bind<ILoanService>().To<LoanService>().AsSingle();

        Container.Bind<ITradeService>().To<TradeService>().AsSingle();
        Container.BindInterfacesTo<PlayerStatsService>()
                 .FromComponentInHierarchy()
                 .AsSingle();

        Container.BindInterfacesTo<UICompanyCellPresenter>().AsSingle().NonLazy();
        Container.BindInterfacesTo<PayRentPresenter>().AsSingle().NonLazy();

        Container.Bind<IUICompanyCellRepository>().To<UICompanyCellRepository>().AsSingle();

        Container.BindInterfacesAndSelfTo<UICompanyCell>()
     .FromComponentsInHierarchy()
     .AsTransient();
    }
}
