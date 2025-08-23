public class JailActionsService
{
    public bool TryPayFine(PlayerData player, int fineAmount = 500)
    {
        if (player.Money < fineAmount) return false;
        Bank.Instance.RemoveMoney(player, fineAmount);
        JailManager.Instance.ReleaseFromJail(player);
        return true;
    }

    public bool TryRollDouble(PlayerData player, int dice1, int dice2)
    {
        if (dice1 != dice2) return false;
        JailManager.Instance.ReleaseFromJail(player);
        return true;
    }
}
