using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CompanyUIService : ICompanyUIService
{
    private IBoardService boardService;
    private Dictionary<int, UICompanyCell> companyUIs = new();

    [Inject]
    public void Construct(IBoardService boardService)
    {
        this.boardService = boardService;
    }

    public void InitializeUI()
    {
        var allCells = boardService.GetAllCellData();
        for (int i = 0; i < allCells.Count; i++)
        {
            var cellTransform = boardService.GetCellRectTransform(i);
            if (cellTransform.TryGetComponent(out UICompanyCell companyUI))
            {
                companyUI.Init(i);
                companyUIs[i] = companyUI;
            }
        }
    }

    public UICompanyCell GetCompanyUI(int index)
    {
        companyUIs.TryGetValue(index, out var ui);
        return ui;
    }
}