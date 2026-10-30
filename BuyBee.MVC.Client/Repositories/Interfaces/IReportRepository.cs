namespace EMart.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<byte[]> GetProductsReportPdfAsync();
    }
}
