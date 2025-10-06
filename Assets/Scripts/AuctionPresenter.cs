using System;
using Zenject;

public class AuctionPresenter : IInitializable, IDisposable
{
    private IAuctionWindow auctionWindow;
    private ILocalPlayerService localPlayerService;
    private IPhotonAuctionManager photonAuctionManager;
    private IPlayerRepository playerRepository;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IAuctionWindow auctionWindow, ILocalPlayerService localPlayerService, IPhotonAuctionManager photonAuctionManager, IPlayerRepository playerRepository,
        ICompanyRepository companyRepository, IEventBus eventBus)
    {
        this.auctionWindow = auctionWindow;
        this.localPlayerService = localPlayerService;
        this.photonAuctionManager = photonAuctionManager;
        this.playerRepository = playerRepository;
        this.companyRepository = companyRepository;
        this.eventBus = eventBus;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<AuctionPromptBidEvent>(OnAuctionPromptBid);
        eventBus.Subscribe<AuctionEndEvent>(_ => auctionWindow.Hide());

        auctionWindow.SetPlayAction(OnPlayClicked);
        auctionWindow.SetPassAction(OnPassClicked);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<AuctionPromptBidEvent>(OnAuctionPromptBid);
        eventBus.Unsubscribe<AuctionEndEvent>(_ => auctionWindow.Hide());
    }
    #endregion LIFE_CYCLE
   

    #region CALLBAKS

    private void OnPlayClicked(int playerId)
    {
        photonAuctionManager.PlayerBidRequest(playerId);
    }

    private void OnPassClicked(int playerId)
    {
        photonAuctionManager.PlayerPassRequest(playerId);
    }
    private void OnAuctionPromptBid(AuctionPromptBidEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();
        if (e.PlayerId == localId)
        {
            PlayerData player = playerRepository.GetPlayerById(localId);
            Company company = companyRepository.GetCompanyById(e.CompanyId);
            auctionWindow.Show(e.PlayerId, company.Name, e.Bid, player.Money);
        }
        else
        {
            auctionWindow.Hide();
        }
    }
    #endregion CALLBAKS
}