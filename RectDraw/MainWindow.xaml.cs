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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RectDraw
{
    public partial class MainWindow : Window
    {
        private bool _isMouseDown = false;
        private Point _drawStart;
        private Rectangle _rect;
        private ScaleTransform _rectTransform;

        public MainWindow()
        {
            InitializeComponent();
            _rectTransform = new ScaleTransform(1, 1);
            _rect = new Rectangle()
            {
                Stroke = Brushes.White,
                StrokeThickness = 2.0,
                RenderTransform = _rectTransform
            };
            drawLayer.Children.Add(_rect);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _drawStart = e.GetPosition(drawLayer);
            _rect.Width = 0;
            _rect.Height = 0;
            _rectTransform.ScaleX = 1;
            _rectTransform.ScaleY = 1;
            Canvas.SetLeft(_rect, _drawStart.X);
            Canvas.SetTop(_rect, _drawStart.Y);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var pos = e.GetPosition(drawLayer);
                var offset = pos - _drawStart;
                _rect.Width = Math.Abs(offset.X);
                _rect.Height = Math.Abs(offset.Y);
                _rectTransform.ScaleX = offset.X >= 0 ? 1 : -1;
                _rectTransform.ScaleY = offset.Y >= 0 ? 1 : -1;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }
    }
}
