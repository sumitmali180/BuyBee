using EMart.ViewModels;

namespace EMart.Services
{
    public interface IAiAssistantService
    {
        Task<AiChatResponse> ProcessChatAsync(AiChatRequest request, CancellationToken cancellationToken = default);
        Task<AiStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
    }
}
