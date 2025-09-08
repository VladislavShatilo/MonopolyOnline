using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveService : MonoBehaviour
{
    private const int JailCellIndex = 10;
    private const int StartCellIndex = 0;

    public MoveResult Move(int currentCellIndex, int steps, bool forward, int totalCells)
    {
        if (totalCells <= 0) throw new ArgumentException("Board must have cells");

        int targetIndex;
        if (forward)
        {
            targetIndex = (currentCellIndex + steps) % totalCells;
        }
        else
        {
            targetIndex = (currentCellIndex - steps + totalCells) % totalCells;
        }

        bool passedStart = forward && (currentCellIndex + steps) >= totalCells;

        return new MoveResult(currentCellIndex, targetIndex, passedStart);
    }
}
