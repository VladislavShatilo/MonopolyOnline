using Zenject;
using System;

public class PayRentPresenter : IPayRentPresenter, IInitializable, IDisposable
{
    private  IPayRentWindow window;
    private  IPhotonCompanyManager photonCompanyManager;
    private  IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;

    private int currentPlayerId;
    private int currentCellIndex;

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
        window.SetPayAction(OnPayClicked);
        eventBus.Subscribe<OfferRentEvent>(ShowRentFor);

        eventBus.Subscribe<RentPaidEvent>(OnRentPaid);
        //EventBus.Subscribe<RentFailedEvent>(OnRentFailed);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<RentPaidEvent>(OnRentPaid);
        eventBus.Unsubscribe<OfferRentEvent>(ShowRentFor);

        //EventBus.Unsubscribe<RentFailedEvent>(OnRentFailed);
    }

    public void ShowRentFor(OfferRentEvent e)
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

    public void HideRent() => window.Hide();

    private void OnPayClicked()
    {
        photonCompanyManager.RequestPayRent( currentCellIndex, currentPlayerId);
    }

    private void OnRentPaid(RentPaidEvent e)
    {
        if (e.PlayerId == currentPlayerId && e.CellIndex == currentCellIndex)
            window.Hide();
    }

    //private void OnRentFailed(RentFailedEvent e)
    //{
    //    if (e.PlayerId == currentPlayerId && e.CellIndex == currentCellIndex)
    //    {
    //        Debug.Log("Игрок не смог оплатить аренду");
    //        window.Hide();
    //    }
    //}
}
