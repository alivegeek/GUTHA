namespace Gutha.Models
{
    public class VoiceOption
    {
        public string Name { get; set; }
        public string ServiceType { get; set; }
        public string Identifier { get; set; } // For FakeYou model token

        public VoiceOption(string name, string serviceType, string identifier = "")
        {
            Name = name;
            ServiceType = serviceType;
            Identifier = identifier;
        }
    }
}
