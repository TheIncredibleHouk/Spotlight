namespace Spotlight.Models
{
    public struct CompressionPoint
    {
        public int XPointer, YPointer, PagePointer;
        public bool EOD;

        public CompressionPoint(int x, int y, int p, bool e)
        {
            XPointer = x;
            YPointer = y;
            PagePointer = p;
            EOD = e;
        }
    }
}