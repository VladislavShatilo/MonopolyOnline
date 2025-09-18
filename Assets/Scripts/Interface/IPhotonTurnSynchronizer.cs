using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhotonTurnSynchronizer 
{
    void RequestStartTurn(int playerId, bool isNext);

}
