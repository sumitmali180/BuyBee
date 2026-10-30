using EMart.Repositories.Interfaces;
using EMart.Services;
using EMart.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class AiController : Controller
    {
        private readonly IAiAssistantService _aiAssistantService;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<AiController> _logger;

        public AiController(
            IAiAssistantService aiAssistantService,
            ICategoriesRepository categoriesRepository,
            ILogger<AiController> logger)
        {
            _aiAssistantService = aiAssistantService;
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        #region Full-Screen /ai View

        [HttpGet]
        [Route("ai")]
        public IActionResult Index()
        {
            ViewData["Title"] = "AI Shopping Assistant - Buybee";
            var categories = _categoriesRepository.GetAll()?.Where(c => c.IsActive).Take(8).ToList() 
                             ?? new List<Models.Categories>();
            return View(categories);
        }

        #endregion

        #region Chat API Endpoint

        [HttpPost]
        [Route("ai/chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.Messages == null || request.Messages.Count == 0)
            {
                return Json(new AiChatResponse
                {
                    Success = false,
                    Message = "Please provide a question or request for the assistant."
                });
            }

            try
            {
                var response = await _aiAssistantService.ProcessChatAsync(request, cancellationToken);
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat in AiController");
                return Json(new AiChatResponse
                {
                    Success = false,
                    Message = "An error occurred while connecting to the AI shopping assistant. Please try again.",
                    Error = ex.Message
                });
            }
        }

        #endregion

        #region Status Endpoint

        [HttpGet]
        [Route("ai/status")]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
        {
            var status = await _aiAssistantService.GetStatusAsync(cancellationToken);
            return Json(status);
        }

        #endregion
    }
}
