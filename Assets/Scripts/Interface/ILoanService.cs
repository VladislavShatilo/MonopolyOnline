using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoanService
{
    // Методы, которые вызываются извне (UI, игроки, события)
    void RequestTakeLoan(int playerId);
    void RequestPayLoan(int playerId);

    // Методы, которые вызываются сетью (RPC)
    void TakeLoanConfirmed(int playerId);
    void PayLoanConfirmed(int playerId);
}
