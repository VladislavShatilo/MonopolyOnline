using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyService : ICompanyService
{
    public void HandleCompanyCell(int cellIndex, int playerId)
    {
        // Тут мы просто делегируем в существующий менеджер
        CompanyManager.Instance.HandleCell(cellIndex, playerId);
    }
}
