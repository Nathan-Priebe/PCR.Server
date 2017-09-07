namespace PCR.Server.Models
{
    public class Message
    {
        public uint ProcessId { get; set; }
        public int Volume { get; set; }
        public bool Mute { get; set; }
    }
}
