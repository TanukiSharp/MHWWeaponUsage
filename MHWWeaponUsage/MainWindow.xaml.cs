using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHWWeaponUsage.ViewModels;

namespace MHWWeaponUsage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RootViewModel rootViewModel = new RootViewModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = rootViewModel;

            rootViewModel.Reload();
        }

        private Point originMousePosition;
        private Point originWindowPosition;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (rootViewModel.IsMiniMode == false)
                return;

            originMousePosition = PointToScreen(e.GetPosition(this));
            originWindowPosition = new Point(Left, Top);

            // Very important to capture AFTER acquiring the position
            CaptureMouse();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (rootViewModel.IsMiniMode == false)
                return;

            if (IsMouseCaptured)
            {
                Point currentPosition = PointToScreen(e.GetPosition(this));

                Left = originWindowPosition.X + (currentPosition.X - originMousePosition.X);
                Top = originWindowPosition.Y + (currentPosition.Y - originMousePosition.Y);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (rootViewModel.IsMiniMode == false)
                return;

            ReleaseMouseCapture();
        }
    }
}
