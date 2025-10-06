using System.Collections.Generic;
using UnityEngine;

public class UICompanyCellRepository : IUICompanyCellRepository
{
    private readonly Dictionary<int, IUICompanyCellView> views = new();

    #region PUBLIC_METHODS

    public void Register(IUICompanyCellView view)
    {
        if (views.ContainsKey(view.CompanyId()))
        {
            Debug.LogWarning($"UICompanyCell с id {view.CompanyId()} уже зарегистрирован");
            return;
        }

        views[view.CompanyId()] = view;
    }

    public void Unregister(IUICompanyCellView view)
    {
        views.Remove(view.CompanyId());
    }

    public IUICompanyCellView GetByCompanyId(int companyId)
    {
        return views.TryGetValue(companyId, out var view) ? view : null;
    }

    #endregion PUBLIC_METHODS

}
