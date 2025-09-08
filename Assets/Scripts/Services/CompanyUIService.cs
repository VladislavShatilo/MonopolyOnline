using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyUIService : ICompanyUIService
{
    private readonly IBoardService boardService;
    private readonly Dictionary<int, UICompanyCell> companyUIs = new();

    public CompanyUIService(IBoardService boardService)
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