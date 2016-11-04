namespace ZaloCommunityDev.Shared
{
    public enum ZaloMessageType
    {
        Text = 0,
        Image = 1
    }

    public class ZaloMessage
    {
        public ZaloMessageType Type { get; set; }

        public string Value { get; set; }
    }
}