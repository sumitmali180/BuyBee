using EMart.Repositories.Interfaces;

namespace EMart.Repositories.Implementation
{
    public class ReportRepository : IReportRepository
    {
        private readonly HttpClient _httpClient;

        public ReportRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> GetProductsReportPdfAsync()
        {
            var response = await _httpClient.GetAsync(
                "https://localhost/ReportServer?/EmartTesting/OrderInvoice&rs:Format=PDF");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}