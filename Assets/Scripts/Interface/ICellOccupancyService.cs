using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICellOccupancyService
{
    void InitializePlayer();

    void RegisterPlayerOnCell(PlayerOccupancyRegisterEvent e);
    void UnregisterPlayerFromCell(PlayerOccupancyUnregisterEvent e);
    void UpdatePositions(int cellIndex);
}
