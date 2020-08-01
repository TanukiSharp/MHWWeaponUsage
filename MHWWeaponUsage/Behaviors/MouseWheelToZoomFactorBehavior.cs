using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MHWWeaponUsage.ScalableVectorGraphics;
using MHWWeaponUsage.ViewModels;

namespace MHWWeaponUsage.Behaviors
{
    public static class MouseWheelToZoomFactorBehavior
    {
        public static bool GetIsAttached(DependencyObject target)
        {
            return (bool)target.GetValue(IsAttachedProperty);
        }

        public static void SetIsAttached(DependencyObject target, bool value)
        {
            target.SetValue(IsAttachedProperty, value);
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(MouseWheelToZoomFactorBehavior),
            new PropertyMetadata(OnIsAttachedPropertyChanged)
        );

        private static void OnIsAttachedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Control control)
            {
                if (e.NewValue == null)
                    control.MouseWheel -= ControlMouseWheel;
                else
                    control.MouseWheel += ControlMouseWheel;
            }
            else
            {
                var element = (FrameworkElement)sender;
                if (element == null)
                    return;

                if (e.NewValue == null)
                    element.MouseWheel -= ControlMouseWheel;
                else
                    element.MouseWheel += ControlMouseWheel;
            }
        }

        private static void ControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is IZoomFactorController zoomFactorController)
            {
                double newZoomFactor = Math.Round(zoomFactorController.ZoomFactor + 0.1 * Math.Sign(e.Delta), 1);

                zoomFactorController.ZoomFactor = Math.Max(0.5, Math.Min(newZoomFactor, 5.0));
            }
        }
    }
}
