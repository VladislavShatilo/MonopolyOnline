
public class GameManagerPlayerRepository : IPlayerRepository
{
    public PlayerData GetPlayerById(int id)
    {
        return GameManager.Instance.GetPlayerById(id);
    }
}