using AlogrithmDemos.Combinatorics;
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
using System.Windows.Threading;

namespace AlogrithmDemos.Views
{
    /// <summary>
    /// Interaction logic for TriominoView.xaml
    /// </summary>
    public partial class TriominoView : UserControl
    {
        public delegate void NextStepDelegate();

        private TriominosModel m_TriominoModel;
        private DrawingGroup m_GeometryDrawings;
        private DrawingGroup m_TriominoDrawings;
        private GeometryDrawing m_GridDrawing;
        private DrawingGroup[] m_TriominoGeometries;
        private const double m_CellSize = 20.0;

        DispatcherTimer m_UIUpdateTimer;

        private bool IsRunning { get; set; } = false;
        private bool ShouldRun { get; set; } = false;

        public TriominoView()
        {
            InitializeComponent();
            InitializeTriominoGeometries();

            m_GeometryDrawings = new DrawingGroup();
            m_TriominoDrawings = new DrawingGroup();
            m_GeometryDrawings.Children.Add(m_TriominoDrawings);

            m_TriominoModel = new TriominosModel(2, 6);

            RecalcGridDrawGroup(m_TriominoModel.Width, m_TriominoModel.Height);

            TriominoImage.Source = new DrawingImage(m_GeometryDrawings);

            m_UIUpdateTimer = new DispatcherTimer();
            m_UIUpdateTimer.Interval = TimeSpan.FromMilliseconds(15);
            m_UIUpdateTimer.Tick += RunUIUpdate;
        }

        private void InitializeTriominoGeometries()
        {
            m_TriominoGeometries = new DrawingGroup[(int)ETriomino.NumOfPieces];
            
            object resource = null; ;
            if((resource = TryFindResource("TriominoVLine")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.VLine] = (DrawingGroup)resource;
            }
            if ((resource = TryFindResource("TriominoTopLeft")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.TopLeft] = (DrawingGroup)resource;
            }
            if ((resource = TryFindResource("TriominoTopRight")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.TopRight] = (DrawingGroup)resource;
            }
            if ((resource = TryFindResource("TriominoBottomLeft")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.BottomLeft] = (DrawingGroup)resource;
            }
            if ((resource = TryFindResource("TriominoBottomRight")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.BottomRight] = (DrawingGroup)resource;
            }
            if ((resource = TryFindResource("TriominoHLine")) != null)
            {
                m_TriominoGeometries[(int)ETriomino.HLine] = (DrawingGroup)resource;
            }
        }

        private void RecalcGridDrawGroup(int width, int height)
        {
            m_GeometryDrawings.Children.Remove(m_GridDrawing);

            double gridHeight = m_CellSize * height;
            double gridWidth = m_CellSize * width;
            
            GeometryGroup gridGeometry = new GeometryGroup();

            //Create vertical lines
            for(double x = 0; x < gridWidth; x += m_CellSize)
            {
                gridGeometry.Children.Add(new LineGeometry(new Point(x, 0.0), new Point(x, gridHeight)));
            }

            //Create horizontal lines
            for (double y = 0; y < gridHeight; y += m_CellSize)
            {
                gridGeometry.Children.Add(new LineGeometry(new Point(0.0, y), new Point(gridWidth, y)));
            }
            gridGeometry.Children.Add(new RectangleGeometry(new Rect(new Point(0.0, 0.0), new Point(gridWidth, gridHeight))));

            m_GridDrawing = new GeometryDrawing(Brushes.LightGray, new Pen(Brushes.DarkGray, 1.0), gridGeometry);
            m_GridDrawing.Freeze();

            m_GeometryDrawings.Children.Insert(0, m_GridDrawing);
        }

        private void RecalcTriominoDrawGroup()
        {
            m_TriominoDrawings.Children.Clear();

            foreach(var step in m_TriominoModel.Steps)
            {
                if(step.placed)
                {
                    DrawingGroup clonedTriomino = m_TriominoGeometries[(int)step.triomino].Clone();
                    clonedTriomino.Transform = new TranslateTransform(step.x * m_CellSize, step.y * m_CellSize);
                    clonedTriomino.Freeze();
                    m_TriominoDrawings.Children.Add(clonedTriomino);
                }
            }

            StepsLabel.Content = m_TriominoModel.StepsTaken;
            PermutationsLabel.Content = m_TriominoModel.Permutations;
            CompletedLabel.Content = m_TriominoModel.Completed;
        }

        private void ColsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int width = Convert.ToInt32(e.NewValue);
            if (m_TriominoModel != null)
            {
                ShouldRun = false;
                RecalcGridDrawGroup(width, m_TriominoModel.Height);
                m_TriominoModel.Resize(width, m_TriominoModel.Height);
                RecalcTriominoDrawGroup();
            }
        }

        private void RowsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int height = Convert.ToInt32(e.NewValue);
            if (m_TriominoModel != null)
            {
                ShouldRun = false;
                RecalcGridDrawGroup(m_TriominoModel.Width, height);
                m_TriominoModel.Resize(m_TriominoModel.Width, height);
                RecalcTriominoDrawGroup();
            }
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            m_TriominoModel.NextStep();
            RecalcTriominoDrawGroup();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldRun = false;
            m_TriominoModel.Reset();
            RecalcTriominoDrawGroup();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
            {
                ShouldRun = false;
            }
            else
            {
                IsRunning = true;
                ShouldRun = true;
                RunButton.Content = "Stop";
                m_UIUpdateTimer.Start();
                Run();
            }
        }

        private void MemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ShouldRun = false;
            m_TriominoModel.UseMemorization = MemCheckBox.IsChecked ?? false;
            RecalcTriominoDrawGroup();
        }

        private void Run()
        {
            if(ShouldRun)
            {
                m_TriominoModel.NextStep();
                if(!m_TriominoModel.Completed)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new NextStepDelegate(this.Run));
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            m_UIUpdateTimer.Stop();
            IsRunning = false;
            RunButton.Content = "Run";
            RecalcTriominoDrawGroup();
        }

        private void RunUIUpdate(object sender, EventArgs e)
        {
            RecalcTriominoDrawGroup();
        }
    }
}
