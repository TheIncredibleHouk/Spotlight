namespace Spotlight.Models
{
    public enum CompressionCommandType
    {
        RepeatTile = 0x00,
        SkipTile = 0x40,
        RepeatPattern = 0x80,
        WriteRaw = 0xC0,
    }
}