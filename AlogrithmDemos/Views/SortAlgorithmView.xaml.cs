using AlogrithmDemos.Models.Sorting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace AlogrithmDemos.Views
{
    /// <summary>
    /// Interaction logic for SortAlgorithmView.xaml
    /// </summary>
    public partial class SortAlgorithmView : UserControl
    {
        public delegate void NextStepDelegate();

        public SortAlgorithm SortModel { private set; get; }
        private DrawingGroup m_GeometryDrawings;
        private DrawingGroup m_DataDrawings;
        private GeometryDrawing m_BackgroundDrawing;

        private RectangleGeometry[] m_RectangleGeometryCache;
        private RectangleGeometry[] m_RectangleBinGeometryCache;

        private const double m_CellSize = 20.0;

        private Stopwatch m_Stopwatch = new Stopwatch();
        private long m_PrevElapsedTime = 0;

        private DispatcherTimer m_UIUpdateTimer;
        private IEnumerator m_Iterator;

        private bool IsRunning { get; set; } = false;
        private bool ShouldRun { get; set; } = false;
        private int StepDelay { get; set; } = 8;

        public SortAlgorithmView()
        {
            InitializeComponent();

            m_RectangleGeometryCache = new RectangleGeometry[(int)DataSetSizeSlider.Maximum + 1];
            m_RectangleBinGeometryCache = new RectangleGeometry[(int)DataSetSizeSlider.Maximum + 1];

            for (int i = 0; i < m_RectangleGeometryCache.Length; ++i)
            {
                m_RectangleGeometryCache[i] = new RectangleGeometry(new Rect(0, 0, 3, i + 1));
                m_RectangleBinGeometryCache[i] = new RectangleGeometry(new Rect(0, 0, 3, i + 1));
            }

            m_GeometryDrawings = new DrawingGroup();
            m_DataDrawings = new DrawingGroup();
            m_GeometryDrawings.Children.Add(m_DataDrawings);

            DataContextChanged += SortAlgorithmView_DataContextChanged;
            
            SortModel = new BubbleSort(128);
            m_Iterator = SortModel.CalculateCoroutine();

            RecalcGridDrawGroup(SortModel.Data.Length);

            DataImage.Source = new DrawingImage(m_GeometryDrawings);

            m_UIUpdateTimer = new DispatcherTimer();
            m_UIUpdateTimer.Interval = TimeSpan.FromMilliseconds(30);
            m_UIUpdateTimer.Tick += RunUIUpdate;
        }

        private void SortAlgorithmView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is SortAlgorithm)
            {
                SortModel = DataContext as SortAlgorithm;
            }
            else if (SortModel == null)
            {
                SortModel = new BubbleSort(128);
            }
            DataSetSizeSlider.Value = SortModel.Data.Length;
            Reset();
        }

        private void RecalcGridDrawGroup(int size)
        {
            m_DataDrawings.Children.Clear();
            m_GeometryDrawings.Children.Remove(m_BackgroundDrawing);

            double height = 20 + size;
            double width = 20 + 4 * size - 1;

            GeometryGroup backgroundGeometry = new GeometryGroup();

            backgroundGeometry.Children.Add(new RectangleGeometry(new Rect(0, 0, width, height)));

            m_BackgroundDrawing = new GeometryDrawing(Brushes.DimGray, null, backgroundGeometry);

            m_GeometryDrawings.Children.Insert(0, m_BackgroundDrawing);
        }

        private void RecalcDataDrawGroup()
        {
            m_GeometryDrawings.Children.Remove(m_DataDrawings);

            long StepHistoryCutoff = SortModel.StepsTaken - SortModel.StepHistoryDisplayed;

            var iterator = m_DataDrawings.Children.GetEnumerator();

            for (int i = 0; i < SortModel.Data.Length; ++i)
            {
                int size = SortModel.Data.Length;
                Geometry geo = SortModel.Data[i].action == SortAlgorithm.SortAction.Bin ? m_RectangleBinGeometryCache[SortModel.Data[i].data - 1] : m_RectangleGeometryCache[SortModel.Data[i].data - 1];
                geo.Transform = new TranslateTransform(10 + 4 * i, 10 + size - SortModel.Data[i].data);

                SolidColorBrush theBrush = Brushes.WhiteSmoke;

                switch (SortModel.Data[i].action)
                {
                    case SortAlgorithm.SortAction.Bin:
                        double binMultiplier = 0.25 + ((double)SortModel.Data[i].bin / (double)SortModel.BinCount) * 0.75;
                        theBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, (byte)((double)245 * binMultiplier)));
                        break;

                    case SortAlgorithm.SortAction.Compare:
                    case SortAlgorithm.SortAction.Swap:
                        if (SortModel.Data[i].step > StepHistoryCutoff - 1)
                        {
                            double multiplier = ((double)(SortModel.StepsTaken - SortModel.Data[i].step) / (double)SortModel.StepHistoryDisplayed);

                            theBrush = new SolidColorBrush(Color.FromArgb(255,
                                (SortModel.Data[i].action == SortAlgorithm.SortAction.Compare) ? (byte)((double)245 * multiplier) : (byte)245,
                                (SortModel.Data[i].action == SortAlgorithm.SortAction.Swap) ? (byte)((double)245 * multiplier) : (byte)245,
                                (byte)((double)245 * multiplier)));
                        }
                        break;

                    case SortAlgorithm.SortAction.None:
                    default:
                        break;
                }

                if (i < m_DataDrawings.Children.Count)
                {
                    //if (SortModel.Data[i].step > StepHistoryCutoff - 2)
                    {
                        var geoDrawingCollection = m_DataDrawings.Children[i];
                        if (geoDrawingCollection is GeometryDrawing)
                        {
                            GeometryDrawing geoDrawing = geoDrawingCollection as GeometryDrawing;
                            geoDrawing.Brush = theBrush;
                            geoDrawing.Geometry = geo;
                        }
                    }
                }
                else
                {
                    GeometryDrawing geoDrawing = new GeometryDrawing(theBrush, null, geo);

                    m_DataDrawings.Children.Add(geoDrawing);
                }
            }
            m_GeometryDrawings.Children.Add(m_DataDrawings);

            StepsLabel.Content = $"{SortModel.StepsTaken:n0}";
            SwapsLabel.Content = $"{SortModel.Swaps:n0}";
            ComparisonsLabel.Content = $"{SortModel.Comparisons:n0}";
            ElementCopiesLabel.Content = $"{SortModel.ElementCopies:n0}";
            CompletedLabel.Content = SortModel.Completed;
        }

        private void DataSetSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int size = Convert.ToInt32(e.NewValue);
            if (SortModel != null)
            {
                Reset();
                SortModel.Resize(size);
                RecalcGridDrawGroup(size);
                RecalcDataDrawGroup();
            }
        }

        private void StepDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StepDelay = Convert.ToInt32(e.NewValue);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldRun = false;
            SortModel.Shuffle(100);
            RecalcDataDrawGroup();
            m_Iterator = SortModel.CalculateCoroutine();
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            m_Iterator.MoveNext();
            RecalcDataDrawGroup();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
            {
                ShouldRun = false;
                m_Stopwatch.Stop();
            }
            else
            {
                IsRunning = true;
                ShouldRun = true;
                ShuffleButton.IsEnabled = false;
                NextStepButton.IsEnabled = false;
                FastRunButton.IsEnabled = false;
                RunButton.Content = "Stop";
                m_UIUpdateTimer.Start();
                m_Stopwatch.Start();
            }
        }

        private void FastRunButton_Click(object sender, RoutedEventArgs e)
        {
            FastRunButton.Content = "Calculating...";
            SortModel.Reset();
            SortModel.Shuffle(100);
            SortModel.Calculate();
            FastRunButton.Content = "Fast Run";
            RecalcDataDrawGroup();
        }

        private void Reset()
        {
            ShouldRun = false;
            SortModel.Reset();
            RecalcDataDrawGroup();
            m_Iterator = SortModel.CalculateCoroutine();
        }

        private void Stop()
        {
            m_UIUpdateTimer.Stop();
            m_Stopwatch.Reset();
            m_PrevElapsedTime = 0;
            IsRunning = false;
            RunButton.Content = "Run";
            ShuffleButton.IsEnabled = true;
            NextStepButton.IsEnabled = true;
            FastRunButton.IsEnabled = true;
            RecalcDataDrawGroup();
        }

        private void RunUIUpdate(object sender, EventArgs e)
        {
            if(ShouldRun)
            {
                long newElapsedTime = m_Stopwatch.ElapsedMilliseconds;
                long timeDiff = newElapsedTime - m_PrevElapsedTime;
                m_PrevElapsedTime = newElapsedTime;

                while(timeDiff > StepDelay)
                {
                    if(!m_Iterator.MoveNext())
                    {
                        Stop();
                        return;
                    }
                    timeDiff -= StepDelay;
                }

                if (timeDiff > 0)
                    m_PrevElapsedTime -= timeDiff;
            }
            else
            {
                Stop();
            }
            RecalcDataDrawGroup();
        }

    }

}
