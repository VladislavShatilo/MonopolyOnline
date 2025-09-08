public interface IMoveService
{
    MoveResult Move(int currentCellIndex, int steps, bool forward, int totalCells);
}