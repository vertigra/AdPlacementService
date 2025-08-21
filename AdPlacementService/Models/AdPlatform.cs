namespace AdPlacementService.Models
{
    public class AdPlatform
    {
        public string Name { get; set; } = string.Empty;
        public HashSet<string> Locations { get; set; } = [];
    }
}
