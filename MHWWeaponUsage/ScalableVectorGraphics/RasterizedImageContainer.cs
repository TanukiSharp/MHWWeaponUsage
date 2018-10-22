using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Resources;

namespace MHWWeaponUsage.ScalableVectorGraphics
{
    public static class RasterizedImageContainer
    {
        private static readonly Dictionary<string, ImageSource> renders = new Dictionary<string, ImageSource>();

        public static ImageSource GetRasterizedImage(int size, string imageResourceLocation)
        {
            size *= 2;

            string key = $"{size}|{imageResourceLocation}";

            if (renders.TryGetValue(key, out ImageSource result) == false)
            {
                StreamResourceInfo streamResourceInfo;

                try
                {
                    streamResourceInfo = App.GetResourceStream(new Uri($"pack://application:,,,/{imageResourceLocation}"));

                    if (streamResourceInfo != null)
                    {
                        var svgLoader = new Loader();
                        VectorGraphicsInfo vectorGraphicsInfo = svgLoader.LoadFromStream(streamResourceInfo.Stream);

                        result = Rasterizer.Render(size, size, vectorGraphicsInfo);
                    }

                    renders.Add(key, result);
                }
                catch
                {
                    renders.Add(key, null);
                }
            }

            return result;
        }
    }
}
