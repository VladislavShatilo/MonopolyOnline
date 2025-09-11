public interface IUICompanyCellRepository
{
    void Register(IUICompanyCellView view);
    void Unregister(IUICompanyCellView view);
    IUICompanyCellView GetByCompanyId(int companyId);
}