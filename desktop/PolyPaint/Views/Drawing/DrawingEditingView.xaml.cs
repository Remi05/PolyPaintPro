using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Unity.Attributes;
using PolyPaint.ViewModels;

namespace PolyPaint.Views.Drawing
{
    public partial class DrawingEditingView : UserControl
    {
        private IDrawingEditingViewModel viewModel;
        [Dependency]
        public IDrawingEditingViewModel ViewModel
        {
            get => viewModel;
            set
            {
                DataContext = viewModel = value;
                viewModel.DrawingCanvas = DrawingCanvas;
                viewModel.SelectionChanged += SelectStrokes;
            }
        }

        public DrawingEditingView()
        {
            InitializeComponent();

            // This hack is needed because of InkCanvas weird behavior with strokes selection.
            Copier.Click += (_, __) =>
            {
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                (DataContext as DrawingEditingViewModel).Editor.SelectTool("lasso");
            };

            Couper.Click += (_, __) =>
            {
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                (DataContext as DrawingEditingViewModel).Editor.SelectTool("lasso");
            };
        }

        // Pour gérer les points de contrôles.
        private void GlisserCommence(object sender, DragStartedEventArgs e) => (sender as Thumb).Background = Brushes.Black;
        private void GlisserTermine(object sender, DragCompletedEventArgs e) => (sender as Thumb).Background = Brushes.White;
        private void GlisserMouvementRecu(object sender, DragDeltaEventArgs e)
        {
            String nom = (sender as Thumb).Name;
            if (nom == "horizontal" || nom == "diagonal") DrawingCanvas.Width = Math.Max(32, DrawingCanvas.Width + e.HorizontalChange);
            if (nom == "vertical" || nom == "diagonal") DrawingCanvas.Height = Math.Max(32, DrawingCanvas.Height + e.VerticalChange);
        }

        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            CursorPosition.Text = "";
            CursorIcon.Visibility = Visibility.Collapsed;
        }
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(DrawingCanvas);
            CursorPosition.Text = Math.Round(p.X) + ", " + Math.Round(p.Y) + "px";
            CursorIcon.Visibility = Visibility.Visible;
        }

        private void SelectStrokes(StrokeCollection strokes)
        {
            DrawingCanvas.Select(new StrokeCollection(strokes));
        }
    }
}