using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveResult
{
    public int StartCell { get; }
    public int EndCell { get; }
    public bool PassedStart { get; }
    public bool ToJail { get; }

    public MoveResult(int startCell, int endCell, bool passedStart, bool toJail = false)
    {
        StartCell = startCell;
        EndCell = endCell;
        PassedStart = passedStart;
        ToJail = toJail;
    }
}
