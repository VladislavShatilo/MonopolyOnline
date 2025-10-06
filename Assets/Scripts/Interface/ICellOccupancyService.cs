using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICellOccupancyService
{
    void InitializePlayer();
    void UpdatePositions(int cellIndex);
}
