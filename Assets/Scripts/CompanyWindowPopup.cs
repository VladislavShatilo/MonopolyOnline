using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public enum StatsWindowPosition
{
    Up,
    LeftUp,
    LeftDown,
    Down,
    RightDown,
    RightUp
}

public class CompanyWindowPopup : MonoBehaviour
{
    [SerializeField] private Button showWindowButton;
    private ITradeService tradeService;
    private ICompanyRepository companyRepository;

    private CompanyOfferInteractor interactor;
    private int companyId;

    public event Action<int> OnCompanyClicked; // если не в трейде

    [Inject]
    public void Construct(ITradeService tradeService, ICompanyRepository companyRepository)
    {
        this.tradeService = tradeService;
        this.companyRepository = companyRepository;
    }

    public void Init(int id)
    {
        companyId = id;
        showWindowButton.onClick.AddListener(OnClick);
        interactor = new CompanyOfferInteractor(tradeService, companyRepository);
    }

    private void OnClick()
    {
        bool handled = interactor.TryToggleCompanyInOffer(companyId);

        if (!handled)
        {
            OnCompanyClicked?.Invoke(companyId);
        }
    }
}