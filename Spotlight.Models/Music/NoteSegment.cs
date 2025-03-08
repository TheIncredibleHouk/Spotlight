using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Music
{
    public class NoteSegment
    {
        public List<NoteData> NoteData { get; set; }

        public NoteSegment()
        {
            NoteData = new List<NoteData>();
        }

        public bool IsMaxNoteCount { get => NoteWeight >= MAX_NOTE_WEIGHT; }

        public const int MAX_NOTE_WEIGHT = 48;

        private int NoteWeight
        {
            get
            {
                int weight = 0;
                NoteLength currentNoteLength = NoteLength.Invalid;

                foreach (var noteData in NoteData)
                {
                    if (noteData is NotePropertyCommand)
                    {
                        currentNoteLength = ((NotePropertyCommand)noteData).NoteLength;
                    }
                    else if (noteData is NoteValue)
                    {
                        weight += GetNoteWeight(currentNoteLength);
                    }
                }

                return weight;
            }
        }

        public static int GetNoteWeight(NoteLength length)
        {
            switch (length)
            {
                case NoteLength.Whole:
                    return MAX_NOTE_WEIGHT;

                case NoteLength.Half:
                    return MAX_NOTE_WEIGHT / 2;

                case NoteLength.Quarter:
                    return MAX_NOTE_WEIGHT / 4;

                case NoteLength.Eighth:
                    return MAX_NOTE_WEIGHT / 8;

                case NoteLength.Sixteenth:
                    return MAX_NOTE_WEIGHT / 16;

                case NoteLength.EigthTriplet:
                    return MAX_NOTE_WEIGHT / 12;

                case NoteLength.SixteenthTriplet:
                    return MAX_NOTE_WEIGHT / 24;
            }

            return 0;
        }

        public static List<NoteSegment> Parse(Channel channel, List<int> rawData)
        {
            List<NoteSegment> noteSegments = new List<NoteSegment>();
            NoteSegment currentNoteSegment = null;

            foreach (int rawValue in rawData)
            {
                NoteData noteData = new NoteData(rawValue);

                if (noteData.IsEnd())
                {
                    return noteSegments;
                }
                else if (noteData.IsPortmanto(channel))
                {
                    ((NoteValue)currentNoteSegment.NoteData[currentNoteSegment.NoteData.Count - 1]).Portmanto = true;
                }
                else if (noteData.IsProperty())
                {
                    if(currentNoteSegment == null)
                    {
                        currentNoteSegment = new NoteSegment();
                        noteSegments.Add(currentNoteSegment);
                    }

                    NotePropertyCommand notePropertyCommand = noteData.AsPropertyCommand();
                    currentNoteSegment.NoteData.Add(notePropertyCommand);
                }
                else
                {
                    if(currentNoteSegment == null)
                    {
                        currentNoteSegment = new NoteSegment();
                        noteSegments.Add(currentNoteSegment);
                    }

                    currentNoteSegment.NoteData.Add(noteData.AsNoteValue(channel));

                    if (currentNoteSegment.IsMaxNoteCount)
                    {
                        currentNoteSegment = null;
                    }
                }

            }

            return noteSegments;
        }
    }
}
