using Spotlight.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class SegmentData
    {
        public List<NoteSegment> Square1Segments { get; set; }
        public List<NoteSegment> Square2Segments { get; set; }
        public List<NoteSegment> TriangleSegments { get; set; }
        public List<NoteSegment> NoiseSegments { get; set; }
        public List<NoteSegment> DMCSegments { get; set; }

        public SegmentData()
        {
            Square1Segments = new List<NoteSegment>();
            Square2Segments = new List<NoteSegment>();
            TriangleSegments = new List<NoteSegment>();
            NoiseSegments = new List<NoteSegment>();
            DMCSegments = new List<NoteSegment>();
        }

        public static SegmentData Parse(SegmentHeader segmentHeader, string text)
        {

            List<string> segmentParts = text.Split('\n').Select(part => part.Trim()).Where(part => part.StartsWith(".byte")).Select(part => part.Replace(".byte", "")).ToList();
            if (segmentParts.Count == 0) return null;

            List<int> rawData = new List<int>();
            SegmentData segmentData = new SegmentData();

            foreach (string line in segmentParts)
            {
                rawData.AddRange(line.Split(',').Select(part => part.Trim().Trim('$'))
                                                    .Where(part => !string.IsNullOrEmpty(part))
                                                    .Select(part => int.Parse(part, System.Globalization.NumberStyles.HexNumber)).ToList());

            }


            segmentData.Square2Segments = NoteSegment.Parse(Channel.Square2, rawData);
            segmentData.Square1Segments = NoteSegment.Parse(Channel.Square1, rawData.Skip(segmentHeader.Square1ChannelIndex).ToList());
            segmentData.TriangleSegments = NoteSegment.Parse(Channel.Triangle, rawData.Skip(segmentHeader.TriangleChannelIndex).ToList());
            segmentData.NoiseSegments = NoteSegment.Parse(Channel.Noise, rawData.Skip(segmentHeader.NoiseChannelIndex).ToList());
            segmentData.DMCSegments = NoteSegment.Parse(Channel.Noise, rawData.Skip(segmentHeader.DMCChannelIndex).ToList());

            return segmentData;
        }
    }
}
