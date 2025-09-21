public interface IGroupOwnershipService
{
    bool PlayerOwnsWholeGroup(CompanyGroup group, int playerId);
}
