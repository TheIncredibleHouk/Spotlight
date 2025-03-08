using Spotlight.Models;
using Spotlight.Models.Music;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spotlight.Controls
{
    /// <summary>
    /// Interaction logic for MusicMeasure.xaml
    /// </summary>
    public partial class MusicMeasure : UserControl
    {
        public MusicMeasure()
        {
            InitializeComponent();

            Bar0.X1 =
            Bar1.X1 =
            Bar2.X1 =
            Bar3.X1 =
            Bar4.X1 =
            Bar5.X1 =
            Bar6.X1 =
            Bar7.X1 =
            Bar8.X1 =
            Bar9.X1 =
            Bar10.X1 =
            Bar11.X1 =
            Bar12.X1 =
            Bar13.X1 =
            Bar14.X1 =
            Bar15.X1 = 0;

            Bar0.X2 =
            Bar1.X2 =
            Bar2.X2 =
            Bar3.X2 =
            Bar4.X2 =
            Bar5.X2 =
            Bar6.X2 =
            Bar7.X2 =
            Bar8.X2 =
            Bar9.X2 =
            Bar10.X2 =
            Bar11.X2 =
            Bar12.X2 =
            Bar13.X2 =
            Bar14.X2 =
            Bar15.X2 = 125;

            Bar0.Y1 = BAR_DISTANCE * 0 + BAR_OFFSET;
            Bar1.Y1 = BAR_DISTANCE * 1 + BAR_OFFSET;
            Bar2.Y1 = BAR_DISTANCE * 2 + BAR_OFFSET;
            Bar3.Y1 = BAR_DISTANCE * 3 + BAR_OFFSET;
            Bar4.Y1 = BAR_DISTANCE * 4 + BAR_OFFSET;
            Bar5.Y1 = BAR_DISTANCE * 5 + BAR_OFFSET;
            Bar6.Y1 = BAR_DISTANCE * 6 + BAR_OFFSET;
            Bar7.Y1 = BAR_DISTANCE * 7 + BAR_OFFSET;
            Bar8.Y1 = BAR_DISTANCE * 8 + BAR_OFFSET;
            Bar9.Y1 = BAR_DISTANCE * 9 + BAR_OFFSET;
            Bar10.Y1 = BAR_DISTANCE * 10 + BAR_OFFSET;
            Bar11.Y1 = BAR_DISTANCE * 11 + BAR_OFFSET;
            Bar12.Y1 = BAR_DISTANCE * 12 + BAR_OFFSET;
            Bar13.Y1 = BAR_DISTANCE * 13 + BAR_OFFSET;
            Bar14.Y1 = BAR_DISTANCE * 14 + BAR_OFFSET;
            Bar15.Y1 = BAR_DISTANCE * 15 + BAR_OFFSET;

            Bar0.Y2 = BAR_DISTANCE * 0 + BAR_OFFSET;
            Bar1.Y2 = BAR_DISTANCE * 1 + BAR_OFFSET;
            Bar2.Y2 = BAR_DISTANCE * 2 + BAR_OFFSET;
            Bar3.Y2 = BAR_DISTANCE * 3 + BAR_OFFSET;
            Bar4.Y2 = BAR_DISTANCE * 4 + BAR_OFFSET;
            Bar5.Y2 = BAR_DISTANCE * 5 + BAR_OFFSET;
            Bar6.Y2 = BAR_DISTANCE * 6 + BAR_OFFSET;
            Bar7.Y2 = BAR_DISTANCE * 7 + BAR_OFFSET;
            Bar8.Y2 = BAR_DISTANCE * 8 + BAR_OFFSET;
            Bar9.Y2 = BAR_DISTANCE * 9 + BAR_OFFSET;
            Bar10.Y2 = BAR_DISTANCE * 10 + BAR_OFFSET;
            Bar11.Y2 = BAR_DISTANCE * 11 + BAR_OFFSET;
            Bar12.Y2 = BAR_DISTANCE * 12 + BAR_OFFSET;
            Bar13.Y2 = BAR_DISTANCE * 13 + BAR_OFFSET;
            Bar14.Y2 = BAR_DISTANCE * 14 + BAR_OFFSET;
            Bar15.Y2 = BAR_DISTANCE * 15 + BAR_OFFSET;

            Canvas.SetLeft(Note0, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note1, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note2, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note3, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note4, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note5, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note6, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note7, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note8, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note9, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note10, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note11, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note12, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note13, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note14, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);
            Canvas.SetLeft(Note15, (MEASURE_WIDTH / 2) - NOTE_HORZ_OFFSET);

            Canvas.SetTop(Note0, BAR_DISTANCE * 0 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note1, BAR_DISTANCE * 1 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note2, BAR_DISTANCE * 2 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note3, BAR_DISTANCE * 3 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note4, BAR_DISTANCE * 4 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note5, BAR_DISTANCE * 5 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note6, BAR_DISTANCE * 6 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note7, BAR_DISTANCE * 7 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note8, BAR_DISTANCE * 8 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note9, BAR_DISTANCE * 9 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note10, BAR_DISTANCE * 10 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note11, BAR_DISTANCE * 11 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note12, BAR_DISTANCE * 12 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note13, BAR_DISTANCE * 13 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note14, BAR_DISTANCE * 14 + BAR_OFFSET - NOTE_VERT_OFFSET);
            Canvas.SetTop(Note15, BAR_DISTANCE * 15 + BAR_OFFSET - NOTE_VERT_OFFSET);
        }

        public Channel Channel
        {
            get
            {
                return _channel;
            }
            set
            {
                _channel = value;
                if(_channel == Channel.DMC)
                {
                    MeasureCanvas.Width = 150;
                } else
                {
                    MeasureCanvas.Width = 100;
                }
                UpdateItemSources();
            }
        }

        public void UpdateItemSources()
        {

            switch (Channel)
            {
                case Channel.Square1:
                    Note0.ItemsSource =
                    Note1.ItemsSource =
                    Note2.ItemsSource =
                    Note3.ItemsSource =
                    Note4.ItemsSource =
                    Note5.ItemsSource =
                    Note6.ItemsSource =
                    Note7.ItemsSource =
                    Note8.ItemsSource =
                    Note9.ItemsSource =
                    Note10.ItemsSource =
                    Note11.ItemsSource =
                    Note12.ItemsSource =
                    Note13.ItemsSource =
                    Note14.ItemsSource =
                    Note15.ItemsSource = Note.SquareChannel1;
                    break;

                case Channel.Square2:
                    Note0.ItemsSource =
                    Note1.ItemsSource =
                    Note2.ItemsSource =
                    Note3.ItemsSource =
                    Note4.ItemsSource =
                    Note5.ItemsSource =
                    Note6.ItemsSource =
                    Note7.ItemsSource =
                    Note8.ItemsSource =
                    Note9.ItemsSource =
                    Note10.ItemsSource =
                    Note11.ItemsSource =
                    Note12.ItemsSource =
                    Note13.ItemsSource =
                    Note14.ItemsSource =
                    Note15.ItemsSource = Note.SquareChannel2;
                    break;

                case Channel.Triangle:
                    Note0.ItemsSource =
                    Note1.ItemsSource =
                    Note2.ItemsSource =
                    Note3.ItemsSource =
                    Note4.ItemsSource =
                    Note5.ItemsSource =
                    Note6.ItemsSource =
                    Note7.ItemsSource =
                    Note8.ItemsSource =
                    Note9.ItemsSource =
                    Note10.ItemsSource =
                    Note11.ItemsSource =
                    Note12.ItemsSource =
                    Note13.ItemsSource =
                    Note14.ItemsSource =
                    Note15.ItemsSource = Note.Triangle;
                    break;

                case Channel.Noise:
                    Note0.ItemsSource =
                    Note1.ItemsSource =
                    Note2.ItemsSource =
                    Note3.ItemsSource =
                    Note4.ItemsSource =
                    Note5.ItemsSource =
                    Note6.ItemsSource =
                    Note7.ItemsSource =
                    Note8.ItemsSource =
                    Note9.ItemsSource =
                    Note10.ItemsSource =
                    Note11.ItemsSource =
                    Note12.ItemsSource =
                    Note13.ItemsSource =
                    Note14.ItemsSource =
                    Note15.ItemsSource = Note.Noise;
                    break;

                case Channel.DMC:
                    Note0.ItemsSource =
                    Note1.ItemsSource =
                    Note2.ItemsSource =
                    Note3.ItemsSource =
                    Note4.ItemsSource =
                    Note5.ItemsSource =
                    Note6.ItemsSource =
                    Note7.ItemsSource =
                    Note8.ItemsSource =
                    Note9.ItemsSource =
                    Note10.ItemsSource =
                    Note11.ItemsSource =
                    Note12.ItemsSource =
                    Note13.ItemsSource =
                    Note14.ItemsSource =
                    Note15.ItemsSource = Note.DMC;
                    break;
            }
        }

        public NoteSegment NoteSegment
        {
            get
            {
                return _noteSegment;
            }
            set
            {
                _noteSegment = value;
                UpdateNotes();
            }
        }

        private void UpdateNotes()
        {
            int currentNoteValue = 0;
            int currentNoteWeight = 0;

            foreach (NoteData noteData in _noteSegment.NoteData)
            {
                if (noteData.IsProperty())
                {
                    currentNoteWeight = NoteSegment.GetNoteWeight(((NotePropertyCommand)noteData).NoteLength);
                }
                else
                {
                    NoteValue noteValue = ((NoteValue)noteData);
                    ComboBox noteComboBox = GetNoteControl(currentNoteValue);

                    if (noteComboBox != null)
                    {
                        noteComboBox.SelectedValue = noteValue.Value;
                    }
                    currentNoteValue += currentNoteWeight;
                }
            }
        }

        private ComboBox GetNoteControl(int currentNoteValue)
        {
            switch (currentNoteValue)
            {
                case 0:
                    return Note0;

                case 3:
                    return Note1;

                case 6:
                    return Note2;

                case 9:
                    return Note3;

                case 12:
                    return Note4;

                case 15:
                    return Note5;

                case 18:
                    return Note6;

                case 21:
                    return Note7;

                case 24:
                    return Note8;

                case 27:
                    return Note9;

                case 30:
                    return Note10;

                case 33:
                    return Note11;

                case 36:
                    return Note12;

                case 39:
                    return Note13;

                case 42:
                    return Note14;

                case 45:
                    return Note15;
            }

            return null;
        }
    }

    public class ComboBoxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var width = ((double)value) - 10;
            return width < 0 ? 50 : width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) + 10;
        }
    }
}
