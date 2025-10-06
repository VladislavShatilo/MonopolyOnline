using Zenject;
using System;

public class PayRentPresenter : IInitializable, IDisposable
{
    private IPayRentWindow window;
    private IPhotonCompanyManager photonCompanyManager;
    private IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;

    private int currentPlayerId;
    private int currentCellIndex;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPayRentWindow window, IPhotonCompanyManager photonCompanyManager, IPlayerRepository playerRepository, ILocalPlayerService localPlayerService, IEventBus eventBus)
    {
        this.window = window;
        this.photonCompanyManager = photonCompanyManager;
        this.playerRepository = playerRepository;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<OfferRentEvent>(ShowRentFor);
        eventBus.Subscribe<RentPaidEvent>(OnRentPaid);

        window.SetPayAction(OnPayClicked);
    }

    void IDisposable.Dispose()
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
        var player = playerRepository.GetPlayerById(e.PlayerId);
        bool canPay = player.Money >= e.Rent;
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.PlayerId == localId)
        {
            window.Show(e.PlayerId, e.CellIndex, e.Rent, canPay);
        }
        else
        {
            window.HardHide();
        }
    }

    private void OnRentPaid(RentPaidEvent e)
    {
        if (e.PlayerId == currentPlayerId && e.CellIndex == currentCellIndex)
            window.Hide();
    }

    private void OnPayClicked()
    {
        photonCompanyManager.RequestPayRent(currentCellIndex, currentPlayerId);
    }

    #endregion CALLBACKS
}