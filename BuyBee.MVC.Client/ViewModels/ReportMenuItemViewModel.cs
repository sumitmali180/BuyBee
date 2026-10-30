namespace EMart.ViewModels
{
    public class ReportMenuItemViewModel
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Icon { get; set; } = "bi-file-earmark-text";

        public string Controller { get; set; } = "Reports";

        public string Action { get; set; } = string.Empty;

        public bool RequiresParameter { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
