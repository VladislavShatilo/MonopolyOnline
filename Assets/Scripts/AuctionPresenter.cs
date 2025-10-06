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
        this.auctionWindow = auctionWindow ?? throw new ArgumentNullException(nameof(auctionWindow));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.photonAuctionManager = photonAuctionManager ?? throw new ArgumentNullException(nameof(photonAuctionManager));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void Initialize()
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

            if (player == null)
                throw new InvalidOperationException($"Player with ID {localId} not found in repository.");
            if (company == null)
                throw new InvalidOperationException($"Company with ID {e.CompanyId} not found in repository.");

            auctionWindow.Show(e.PlayerId, company.Name, e.Bid, player.Money);
        }
        else
        {
            auctionWindow.Hide();
        }
    }
    #endregion CALLBAKS
}