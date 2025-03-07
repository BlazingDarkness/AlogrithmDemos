﻿using AlogrithmDemos.Models;
using System;
using System.Collections;
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
        private readonly DrawingGroup m_GeometryDrawings;
        private readonly DrawingGroup m_TriominoDrawings;
        private GeometryDrawing? m_GridDrawing;
        private DrawingGroup[] m_TriominoGeometries = [];
        private const double m_CellSize = 20.0;

        private readonly DispatcherTimer m_UIUpdateTimer;

        private bool IsRunning { get; set; } = false;

        public TriominoView()
        {
            InitializeComponent();
            InitializeTriominoGeometries();

            m_GeometryDrawings = new DrawingGroup();
            m_TriominoDrawings = new DrawingGroup();
            m_GeometryDrawings.Children.Add(m_TriominoDrawings);

            DataContextChanged += TriominoView_DataContextChanged;

            m_TriominoModel = new TriominosModel(2, 6);

            RecalcGridDrawGroup(m_TriominoModel.Width, m_TriominoModel.Height);

            TriominoImage.Source = new DrawingImage(m_GeometryDrawings);

            m_UIUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            m_UIUpdateTimer.Tick += RunUIUpdate;
        }

        private void TriominoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            m_TriominoModel = DataContext as TriominosModel ?? new TriominosModel(2, 6);

            RowsSlider.Value = m_TriominoModel.Height;
            ColsSlider.Value = m_TriominoModel.Width;
            Reset();
        }

        private void InitializeTriominoGeometries()
        {
            m_TriominoGeometries = new DrawingGroup[(int)ETriomino.NumOfPieces];

            object resource;
            if ((resource = TryFindResource("TriominoVLine")) != null)
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
            
            GeometryGroup gridGeometry = new();

            gridGeometry.Children.Add(new RectangleGeometry(new Rect(new Point(0.0, 0.0), new Point(gridWidth, gridHeight))));
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

            m_GridDrawing = new GeometryDrawing(Brushes.LightGray, new Pen(Brushes.DarkGray, 1.0), gridGeometry);

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

            StepsLabel.Content = $"{m_TriominoModel.StepsTaken:n0}";
            PermutationsLabel.Content = $"{m_TriominoModel.Permutations:n0}";
            CompletedLabel.Content = m_TriominoModel.Completed;
        }

        private void ColsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int width = Convert.ToInt32(e.NewValue);
            if (m_TriominoModel != null)
            {
                Reset();
                RecalcGridDrawGroup(width, m_TriominoModel.Height);
                m_TriominoModel.Resize(width, m_TriominoModel.Height);
            }
        }

        private void RowsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int height = Convert.ToInt32(e.NewValue);
            if (m_TriominoModel != null)
            {
                Reset();
                RecalcGridDrawGroup(m_TriominoModel.Width, height);
                m_TriominoModel.Resize(m_TriominoModel.Width, height);
            }
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            m_TriominoModel.RunStep();
            RecalcTriominoDrawGroup();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
            {
                m_TriominoModel.Pause();
            }
            else
            {
                IsRunning = true;
                NextStepButton.IsEnabled = false;
                FastRunButton.IsEnabled = false;
                RunButton.Content = "Stop";
                m_UIUpdateTimer.Start();
                m_TriominoModel.Run();
            }
        }

        private void FastRunButton_Click(object sender, RoutedEventArgs e)
        {
            FastRunButton.Content = "Calculating...";
            m_TriominoModel.Reset();
            m_TriominoModel.Calculate();
            FastRunButton.Content = "Fast Run";
            RecalcTriominoDrawGroup();
        }

        private void MemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            m_TriominoModel.UseMemorization = MemCheckBox.IsChecked ?? false;
        }

        private void Reset()
        {
            m_TriominoModel.Reset();
            RecalcTriominoDrawGroup();
            ResetControls();
        }

        private void RunUIUpdate(object? sender, EventArgs e)
        {
            if(!m_TriominoModel.IsRunning)
            {
                m_UIUpdateTimer.Stop();
                ResetControls();
            }
            RecalcTriominoDrawGroup();
        }

        private void ResetControls()
        {
            IsRunning = false;
            RunButton.Content = "Run";
            NextStepButton.IsEnabled = true;
            FastRunButton.IsEnabled = true;
        }
    }
}
