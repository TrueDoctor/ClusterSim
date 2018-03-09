namespace ClusterSim.ClusterLib.Visualization
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    using ClusterSim.ClusterLib.Calculation;

    public static class StarExtension
    {
        private static readonly double[] RedCoe =
            {
                1.62098281e-82, -5.03110845e-77, 6.66758278e-72, -4.71441850e-67, 1.66429493e-62, -1.50701672e-59,
                -2.42533006e-53, 8.42586475e-49, 7.94816523e-45, -1.68655179e-39, 7.25404556e-35, -1.85559350e-30,
                3.23793430e-26, -4.00670131e-22, 3.53445102e-18, -2.19200432e-14, 9.27939743e-11, -2.56131914e-07,
                4.29917840e-04, -3.88866019e-01, 3.97307766e+02
            };

        private static readonly double[] GreenCoe =
            {
                1.21775217e-82, -3.79265302e-77, 5.04300808e-72, -3.57741292e-67, 1.26763387e-62, -1.28724846e-59,
                -1.84618419e-53, 6.43113038e-49, 6.05135293e-45, -1.28642374e-39, 5.52273817e-35, -1.40682723e-30,
                2.43659251e-26, -2.97762151e-22, 2.57295370e-18, -1.54137817e-14, 6.14141996e-11, -1.50922703e-07,
                1.90667190e-04, -1.23973583e-02, -1.33464366e+01
            };

        private static readonly double[] BlueCoe =
            {
                2.17374683e-82, -6.82574350e-77, 9.17262316e-72, -6.60390151e-67, 2.40324203e-62, -5.77694976e-59,
                -3.42234361e-53, 1.26662864e-48, 8.75794575e-45, -2.45089758e-39, 1.10698770e-34, -2.95752654e-30,
                5.41656027e-26, -7.10396545e-22, 6.74083578e-18, -4.59335728e-14, 2.20051751e-10, -7.14068799e-07,
                1.46622559e-03, -1.60740964e+00, 6.85200095e+02
            };

        public static Bitmap CratePic(this Star star, double scale = 1, double distance = 1e2)
        {
            if (star.Mass > 15)
            {
                star.Mass = 15;
            }

            var lum = Luminosity(star);
            var size = Size(lum, distance);
            var color = Color(star);
            var radius = Radius(size, size) * 2;
            int xy = radius * 2 + 1;

            var pic = new Bitmap(xy, xy, PixelFormat.Format32bppArgb);
            
            for (int x = 0; x < xy; x++)
            {
                for (int y = 0; y < xy; y++)
                {
                    var dist = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2));
                    var attenuation = Math.Round(Normal(dist, radius * 0.4) * 655 * size);
                    var aColor = System.Drawing.Color.FromArgb(FitByte(attenuation), color[0], color[1], color[2]);
                    pic.SetPixel(x, y, aColor);
                }
            }

            return pic;
        }

        private static double Luminosity(Star star, double coe = 1)
        {
            return Math.Pow(star.Mass, 3.5) * coe;
        }

        private static double Size(double lum, double distance)
        {
            return Math.Log10(lum / distance) + 8;
        }

        private static byte[] Color(Star star)
        {
            var temp = Math.Pow(5778 * 5778 * star.Mass, 0.5);
            var rgb = new byte[3];

            rgb[0] = FitByte(Poly(temp, RedCoe));
            rgb[1] = FitByte(Poly(temp, GreenCoe));
            rgb[2] = FitByte(Poly(temp, BlueCoe));

            return rgb;
        }

        private static int Radius(double size, double lum)
        {
            int i;
            for (i = 0; (int)Math.Round(Normal(i, Math.Log(size, 50)) * lum) > 0; i++)
            {
            }

            return i;
        }

        private static double Poly(double x, double[] coe)
        {
            double value = 0;

            for (int i = coe.Length - 1; i >= 0; i--)
            {
                value += coe[coe.Length - i - 1] * Math.Pow(x, i);
            }

            return value;
        }

        private static byte FitByte(double value)
        {
            int val = (int)Math.Round(value);
            val = val < 0 ? 0 : (val > 255 ? 255 : val);
            return Convert.ToByte(val);
        }

        private static double Normal(double x, double sigma = 1)
        {
            var density = 1 / Math.Sqrt(2 * Math.PI * sigma * sigma) * Math.Exp(x * x / (-2 * sigma * sigma));
            return density;
        }
    }
}
