namespace ClusterSim.ClusterLib.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    static class EnumerableExtension
    {
        public static Bitmap ToBitmap(this IEnumerable<Star> stars, int x = 1920, int y = 1080)
        {
            var bitmap = new Bitmap(x, y);
            var canvas = Graphics.FromImage(bitmap);

            if (stars != null)
                foreach (Star s in stars)
                {
                        var bit = s.CratePic();
                        canvas.DrawImage(bit, new Point((int)s.Pos.vec[0] - bit.Height / 2, (int)s.Pos.vec[1] - bit.Height / 2));
                }
            return bitmap;
        }
    }
}
