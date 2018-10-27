using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using MHWWeaponUsage.ViewModels;
using MHWSaveUtils;

namespace MHWWeaponUsage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public MainWindow()
        {
            InitializeComponent();

            rootViewModel = new RootViewModel(OnBeginMiniMode);

            DataContext = rootViewModel;

            rootViewModel.Reload().Forget();
        }

        private Task<WeaponUsageSaveSlotInfo> OnBeginMiniMode()
        {
            var selectorWindow = new SaveSlotSelectorWindow(rootViewModel.SelectorViewModel);

            if (selectorWindow.ShowDialog() != true)
                return Task.FromResult<WeaponUsageSaveSlotInfo>(null);

            return Task.FromResult(rootViewModel.SelectorViewModel.SelectedWeaponUsage);
        }

        #region Moving window

        private Point originMousePosition;
        private Point originWindowPosition;

        private static T FindParentOfType<T>(Visual current) where T : FrameworkElement
        {
            if (current == null)
                return null;

            if (current is T typed)
                return typed;

            current = VisualTreeHelper.GetParent(current) as Visual;
            if (current == null)
                return null;

            return FindParentOfType<T>(current);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (FindParentOfType<ComboBox>(e.OriginalSource as Visual) != null ||
                FindParentOfType<ComboBoxItem>(e.OriginalSource as Visual) != null ||
                FindParentOfType<Button>(e.OriginalSource as Visual) != null)
                return;

            originMousePosition = PointToScreen(e.GetPosition(this));
            originWindowPosition = new Point(Left, Top);

            // Very important to capture AFTER acquiring the position
            CaptureMouse();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

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

            ReleaseMouseCapture();
        }

        #endregion
    }
}
