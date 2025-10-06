using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoanService
{
     void TakeLoanConfirmed(int playerId);
    void PayLoanConfirmed(int playerId);
}
