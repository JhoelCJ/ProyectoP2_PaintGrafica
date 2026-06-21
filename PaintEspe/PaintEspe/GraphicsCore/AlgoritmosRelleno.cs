using System.Collections.Generic;
using System.Drawing;

namespace PaintEspe.GraphicsCore
{
    internal static class AlgoritmosRelleno
    {
        public static void FloodFill(Bitmap bmp, Point pt, Color colorReemplazo)
        {
            if (bmp == null || pt.X < 0 || pt.X >= bmp.Width || pt.Y < 0 || pt.Y >= bmp.Height)
            {
                return;
            }

            int colorObjetivo = bmp.GetPixel(pt.X, pt.Y).ToArgb();
            int colorNuevo = colorReemplazo.ToArgb();

            if (colorObjetivo == colorNuevo)
            {
                return;
            }

            Queue<Point> cola = new Queue<Point>();
            cola.Enqueue(pt);

            while (cola.Count > 0)
            {
                Point actual = cola.Dequeue();
                if (actual.X < 0 || actual.X >= bmp.Width || actual.Y < 0 || actual.Y >= bmp.Height)
                {
                    continue;
                }

                if (bmp.GetPixel(actual.X, actual.Y).ToArgb() != colorObjetivo)
                {
                    continue;
                }

                int izquierda = actual.X;
                while (izquierda >= 0 && bmp.GetPixel(izquierda, actual.Y).ToArgb() == colorObjetivo)
                {
                    izquierda--;
                }
                izquierda++;

                int derecha = actual.X;
                while (derecha < bmp.Width && bmp.GetPixel(derecha, actual.Y).ToArgb() == colorObjetivo)
                {
                    derecha++;
                }
                derecha--;

                for (int x = izquierda; x <= derecha; x++)
                {
                    bmp.SetPixel(x, actual.Y, colorReemplazo);

                    if (actual.Y > 0 && bmp.GetPixel(x, actual.Y - 1).ToArgb() == colorObjetivo)
                    {
                        cola.Enqueue(new Point(x, actual.Y - 1));
                    }

                    if (actual.Y < bmp.Height - 1 && bmp.GetPixel(x, actual.Y + 1).ToArgb() == colorObjetivo)
                    {
                        cola.Enqueue(new Point(x, actual.Y + 1));
                    }
                }
            }
        }
    }
}
