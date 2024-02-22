using System;

namespace Kz.Liero.Utilities
{
    public static class Utils
    {
        private static Random _random = new Random();
        public static int[] GenerateNoiseMap(int width, int height)
        {
            var noise = new int[width * height];

            // top-left corner
            noise[0] = _random.Next(7);

            // left
            for (var y = 1; y < height; y++)
            {
                var x = 0;
                var top = noise[(y - 1) * width];
                var val = ((_random.Next(7)) + top) >> 1;
                noise[x + y * width] = val;
            }

            // top
            for (var x = 1; x < width; x++)
            {
                var y = 0;
                var left = noise[x - 1];
                var val = ((_random.Next(7)) + left) >> 1;
                noise[x + y * width] = val;
            }

            // everything else
            for (var y = 1; y < height; y++)
            {
                for (var x = 1; x < width; x++)
                {
                    var left = noise[(x - 1) + y * width];
                    var top = noise[x + (y - 1) * width];
                    var val = (left + top + _random.Next(8)) / 3;
                    noise[x + y * width] = val;
                }
            }

            return noise;
        }
    }
}