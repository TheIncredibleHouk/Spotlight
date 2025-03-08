using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class SegmentHeader
    {
        public string Name { get; set; }
        public Tempo Tempo { get; set; }
        public string Square2ChannelStart { get; set; }
        public int Square1ChannelIndex { get; set; }
        public int TriangleChannelIndex { get; set; }
        public int NoiseChannelIndex { get; set; }
        public int DMCChannelIndex { get; set; }

        public static SegmentHeader Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            List<string> segmentHeaderParts = text.Split(':').Select(part => part.Trim()).ToList();

            if (segmentHeaderParts.Count != 2)
            {
                return null;
            }
            string segmentHeaderName = segmentHeaderParts[0];
            List<string> segmentHeaderValues = segmentHeaderParts[1].Split(',').Select(part => part.Trim().Trim('$')).ToList();

            if (segmentHeaderValues.Count != 6)
            {
                return null;
            }

            List<string> tempoParts = segmentHeaderValues[0].Split(' ').Select(part => part.Trim().Trim('$')).ToList();

            if (tempoParts.Count != 2)
            {
                return null;
            }

            SegmentHeader segmentHeader = new SegmentHeader();
            segmentHeader.Name = segmentHeaderName;

            int tempoValue = int.Parse(tempoParts[1], System.Globalization.NumberStyles.HexNumber);
            segmentHeader.Tempo = Tempo.All.Where(t => t.Value == tempoValue).FirstOrDefault();

            if (segmentHeader.Tempo == null)
            {
                return null;
            }

            segmentHeader.Square2ChannelStart = segmentHeaderValues[1];

            segmentHeader.TriangleChannelIndex = int.Parse(segmentHeaderValues[2], System.Globalization.NumberStyles.HexNumber);
            segmentHeader.Square1ChannelIndex = int.Parse(segmentHeaderValues[3], System.Globalization.NumberStyles.HexNumber);
            segmentHeader.NoiseChannelIndex = int.Parse(segmentHeaderValues[4], System.Globalization.NumberStyles.HexNumber);
            segmentHeader.DMCChannelIndex = int.Parse(segmentHeaderValues[5], System.Globalization.NumberStyles.HexNumber);

            return segmentHeader;
        }
    }
}
