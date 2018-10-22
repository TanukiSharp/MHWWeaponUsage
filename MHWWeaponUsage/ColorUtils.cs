using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MHWWeaponUsage
{
    public static class ColorUtils
    {
        public static Color Interpolate(double ratio, params Color[] colors)
        {
            if (colors == null || colors.Length == 0)
                return Colors.Black;

            if (colors.Length == 1)
                return colors[0];

            if (colors.Length == 2)
                return Interpolate(ratio, colors[0], colors[1]);

            double step = 1.0 / (colors.Length - 1);

            int interval;
            double totalSteps = step;
            for (interval = 0; interval < colors.Length; interval++)
            {
                if (ratio <= totalSteps)
                    break;
                totalSteps += step;
            }

            return Interpolate(Interpolate(totalSteps - step, totalSteps, ratio), colors[interval], colors[interval + 1]);
        }

        private static double Interpolate(double min, double max, double value)
        {
            double divisor = max - min;
            if (Math.Abs(divisor) < 1e-6)
                return 0.0;
            return (value - min) / divisor;
        }

        public static Color Interpolate(double ratio, Color color1, Color color2)
        {
            double invRatio = 1.0 - ratio;
            return Color.FromArgb(
                (byte)(color1.A * invRatio + color2.A * ratio),
                (byte)(color1.R * invRatio + color2.R * ratio),
                (byte)(color1.G * invRatio + color2.G * ratio),
                (byte)(color1.B * invRatio + color2.B * ratio)
            );
        }
    }
}
