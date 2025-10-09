using System.Collections.Generic;

public interface ITurnCompanyUIUseCase
{
    IEnumerable<CompanyUIAction> GetAvailableActions(int currentPlayerId);
}