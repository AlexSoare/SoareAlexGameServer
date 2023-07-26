using SoareAlexGameServer.Infrastructure.Entities.DB;

namespace SoareAlexGameServer.Infrastructure.WebSockets.Events
{
    public class GiftEvent
    {
        public string SenderId { get; set; }
        public ResourceType ResourceType { get; set; }
        public double ResourceValue { get; set; }
    }
}
