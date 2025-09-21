using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBranchTurnHandler 
{
    void OnTurnStart(int playerId, int localPlayerId);
}
