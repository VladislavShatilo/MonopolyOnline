using Zenject;
using System;

public class PayRentPresenter : IInitializable, IDisposable
{
    private IPayRentWindow payRentWindow;
    private IPhotonCompanyManager photonCompanyManager;
    private IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;

    private int currentPlayerId;
    private int currentCellIndex;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPayRentWindow payRentWindow, IPhotonCompanyManager photonCompanyManager, IPlayerRepository playerRepository, ILocalPlayerService localPlayerService, IEventBus eventBus)
    {
        this.payRentWindow = payRentWindow ?? throw new ArgumentNullException(nameof(payRentWindow));
        this.photonCompanyManager = photonCompanyManager ?? throw new ArgumentNullException(nameof(photonCompanyManager));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.eventBus = eventBus;
    }

    public void Initialize()
    {
        eventBus.Subscribe<OfferRentEvent>(ShowRentFor);
        eventBus.Subscribe<RentPaidEvent>(OnRentPaid);

        payRentWindow.SetPayAction(OnPayClicked);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<RentPaidEvent>(OnRentPaid);
        eventBus.Unsubscribe<OfferRentEvent>(ShowRentFor);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void ShowRentFor(OfferRentEvent e)
    {
        currentPlayerId = e.PlayerId;
        currentCellIndex = e.CellIndex;
        var player = playerRepository.GetPlayerById(e.PlayerId) ?? throw new InvalidOperationException(nameof(ShowRentFor));
        bool canPay = player.Money >= e.Rent;
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.PlayerId == localId)
        {
            payRentWindow.Show(e.PlayerId, e.CellIndex, e.Rent, canPay);
        }
        else
        {
            payRentWindow.HardHide();
        }
    }

    private void OnRentPaid(RentPaidEvent e)
    {
        if (e.PlayerId == currentPlayerId && e.CellIndex == currentCellIndex)
            payRentWindow.Hide();
    }

    private void OnPayClicked()
    {
        photonCompanyManager.RequestPayRent(currentCellIndex, currentPlayerId);
    }

    #endregion CALLBACKS
}