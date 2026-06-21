using System;
using System.Collections.Generic;
using System.Drawing;

namespace PaintEspe.GraphicsCore
{
    /// <summary>
    /// Algoritmos de relleno de áreas.
    /// </summary>
    internal static class AlgoritmosRelleno
    {
        /// <summary>
        /// Relleno por línea de barrido (Scanline) para polígonos genéricos (convexos o cóncavos).
        /// Construye, para cada fila Y, la lista de intersecciones de los bordes del polígono
        /// con esa fila, las ordena en X y pinta por pares (dentro/fuera), tal como hace
        /// el algoritmo clásico de Scanline Fill visto en Gráficos por Computadora.
        /// </summary>
        public static void ScanlineFill(Bitmap lienzo, IList<Point> vertices, Color colorRelleno)
        {
            if (lienzo == null || vertices == null || vertices.Count < 3)
            {
                return;
            }

            int n = vertices.Count;

            int yMin = vertices[0].Y;
            int yMax = vertices[0].Y;
            for (int i = 1; i < n; i++)
            {
                if (vertices[i].Y < yMin) yMin = vertices[i].Y;
                if (vertices[i].Y > yMax) yMax = vertices[i].Y;
            }

            yMin = Math.Max(yMin, 0);
            yMax = Math.Min(yMax, lienzo.Height - 1);

            // Bloqueamos el bitmap una sola vez para pintar miles de píxeles de forma rápida.
            System.Drawing.Imaging.BitmapData datos = lienzo.LockBits(
                new Rectangle(0, 0, lienzo.Width, lienzo.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                int bytesPorPixel = 4;
                int stride = datos.Stride;
                int ancho = lienzo.Width;
                int alto = lienzo.Height;

                unsafe
                {
                    byte* puntero0 = (byte*)datos.Scan0;
                    int argb = colorRelleno.ToArgb();
                    byte b = (byte)(argb & 0xFF);
                    byte g = (byte)((argb >> 8) & 0xFF);
                    byte r = (byte)((argb >> 16) & 0xFF);
                    byte a = (byte)((argb >> 24) & 0xFF);

                    for (int y = yMin; y <= yMax; y++)
                    {
                        List<double> intersecciones = new List<double>();

                        // Recorremos cada arista (borde) del polígono.
                        for (int i = 0; i < n; i++)
                        {
                            Point p1 = vertices[i];
                            Point p2 = vertices[(i + 1) % n];

                            if (p1.Y == p2.Y)
                            {
                                continue; // arista horizontal: no aporta intersección de scanline
                            }

                            // Regla "punto inferior incluido, superior excluido" para no duplicar
                            // intersecciones exactamente en los vértices compartidos por dos aristas.
                            int yMenor = Math.Min(p1.Y, p2.Y);
                            int yMayor = Math.Max(p1.Y, p2.Y);

                            if (y >= yMenor && y < yMayor)
                            {
                                double x = p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y);
                                intersecciones.Add(x);
                            }
                        }

                        if (intersecciones.Count < 2)
                        {
                            continue;
                        }

                        intersecciones.Sort();

                        // Pintamos por pares: (x0,x1), (x2,x3), ... (regla par-impar)
                        for (int k = 0; k + 1 < intersecciones.Count; k += 2)
                        {
                            int xIni = (int)Math.Round(intersecciones[k]);
                            int xFin = (int)Math.Round(intersecciones[k + 1]);

                            xIni = Math.Max(xIni, 0);
                            xFin = Math.Min(xFin, ancho - 1);

                            byte* fila = puntero0 + y * stride;
                            for (int x = xIni; x <= xFin; x++)
                            {
                                int offset = x * bytesPorPixel;
                                fila[offset] = b;
                                fila[offset + 1] = g;
                                fila[offset + 2] = r;
                                fila[offset + 3] = a;
                            }
                        }
                    }
                }
            }
            finally
            {
                lienzo.UnlockBits(datos);
            }
        }

        /// <summary>
        /// Relleno de un círculo combinando el Algoritmo del Punto Medio (para hallar, por cada
        /// fila, los bordes izquierdo y derecho del círculo) con un barrido horizontal (Scanline)
        /// entre esos bordes.
        /// </summary>
        public static void RellenarCirculoScanline(Bitmap lienzo, Point centro, int radio, Color colorRelleno)
        {
            if (lienzo == null || radio <= 0)
            {
                return;
            }

            int x = 0;
            int y = radio;
            int d = 3 - 2 * radio;

            while (y >= x)
            {
                TrazarFilaHorizontal(lienzo, centro.X - x, centro.X + x, centro.Y + y, colorRelleno);
                TrazarFilaHorizontal(lienzo, centro.X - x, centro.X + x, centro.Y - y, colorRelleno);
                TrazarFilaHorizontal(lienzo, centro.X - y, centro.X + y, centro.Y + x, colorRelleno);
                TrazarFilaHorizontal(lienzo, centro.X - y, centro.X + y, centro.Y - x, colorRelleno);

                x++;
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }
            }
        }

        private static void TrazarFilaHorizontal(Bitmap lienzo, int xIni, int xFin, int fila, Color color)
        {
            if (fila < 0 || fila >= lienzo.Height)
            {
                return;
            }

            xIni = Math.Max(xIni, 0);
            xFin = Math.Min(xFin, lienzo.Width - 1);

            for (int x = xIni; x <= xFin; x++)
            {
                lienzo.SetPixel(x, fila, color);
            }
        }

        /// <summary>
        /// Relleno por inundación (flood fill / "cubeta de pintura"), usado por la herramienta
        /// de balde de pintura sobre áreas delimitadas a mano alzada.
        /// </summary>
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
