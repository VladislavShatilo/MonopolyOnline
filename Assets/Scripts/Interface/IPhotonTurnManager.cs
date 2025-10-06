using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhotonTurnManager
{
    void RequestStartRandomTurn();
    void RequestEndTurn();
    void RegisterDouble(int playerId);
}
