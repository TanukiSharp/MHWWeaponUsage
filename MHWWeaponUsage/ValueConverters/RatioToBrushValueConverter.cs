using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MHWWeaponUsage.ValueConverters
{
    public class RatioToBrushValueConverter : IValueConverter
    {
        private static readonly Color[] colors = new Color[]
        {
            Colors.Red,
            Colors.Gold,
            Colors.LimeGreen
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ratio)
            {
                var brush = new SolidColorBrush(ColorUtils.Interpolate(ratio, colors));
                if (brush.CanFreeze)
                    brush.Freeze();
                return brush;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
