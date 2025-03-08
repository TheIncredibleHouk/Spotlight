using Spotlight.Models;
using Spotlight.Models.Music;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for MusicEditor.xaml
    /// </summary>
    public partial class MusicEditor : Window
    {
        public MusicEditor()
        {
            InitializeComponent();

            TempoComboBox.ItemsSource = Tempo.All;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ParseMusic_Click(object sender, RoutedEventArgs e)
        {
            SegmentHeader segmentHeader = SegmentHeader.Parse(SegmentHeaderText.Text);
            if (segmentHeader == null)
            {
                SegmentHeaderText.Foreground = Brushes.Red;
                return;
            }

            SegmentHeaderText.Foreground = Brushes.White;

            SegmentName.Text = segmentHeader.Name;
            SegmentStart.Text = segmentHeader.Square2ChannelStart;
            SegmentSquare1.Text = $"0x{segmentHeader.Square1ChannelIndex.ToString("X")}";
            SegmentTriangle.Text = $"0x{segmentHeader.TriangleChannelIndex.ToString("X")}";
            SegmentNoise.Text = $"0x{segmentHeader.NoiseChannelIndex.ToString("X")}";
            SegmentDMC.Text = $"0x{segmentHeader.DMCChannelIndex.ToString("X")}";
            TempoComboBox.SelectedValue = segmentHeader.Tempo.Value;

            SegmentData segmentData = SegmentData.Parse(segmentHeader, SegmentDataText.Text);

            if (segmentData == null)
            {
                SegmentHeaderText.Foreground = Brushes.Red;
                return;
            }

            SegmentHeaderText.Foreground = Brushes.White;


            int square2ChannelCount = 0;
            Square1ChannelControls.Children.Clear();
            Square2ChannelControls.Children.Clear();
            TriangleChannelControls.Children.Clear();
            NoiseChannelControls.Children.Clear();
            DMCChannelControls.Children.Clear();

            foreach (NoteSegment square1Data in segmentData.Square1Segments)
            {
                MusicMeasure musicMeasure = new MusicMeasure();
                musicMeasure.Channel = Channel.Square1;
                musicMeasure.NoteSegment = square1Data;
                Square1ChannelControls.Children.Add(musicMeasure);
            }

            foreach (NoteSegment square2Data in segmentData.Square2Segments)
            {
                MusicMeasure musicMeasure = new MusicMeasure();
                musicMeasure.Channel = Channel.Square2;
                musicMeasure.NoteSegment = square2Data;
                Square2ChannelControls.Children.Add(musicMeasure);
            }

            foreach (NoteSegment triangleData in segmentData.TriangleSegments)
            {
                MusicMeasure musicMeasure = new MusicMeasure();
                musicMeasure.Channel = Channel.Triangle;
                musicMeasure.NoteSegment = triangleData;
                TriangleChannelControls.Children.Add(musicMeasure);
            }

            foreach (NoteSegment noiseData in segmentData.NoiseSegments)
            {
                MusicMeasure musicMeasure = new MusicMeasure();
                musicMeasure.Channel = Channel.Noise;
                musicMeasure.NoteSegment = noiseData;
                NoiseChannelControls.Children.Add(musicMeasure);
            }

            foreach (NoteSegment dmcData in segmentData.DMCSegments)
            {
                MusicMeasure musicMeasure = new MusicMeasure();
                musicMeasure.Channel = Channel.DMC;
                musicMeasure.NoteSegment = dmcData;
                DMCChannelControls.Children.Add(musicMeasure);
            }
        }
    }
}
