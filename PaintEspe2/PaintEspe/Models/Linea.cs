using System;
using System.Drawing;

namespace PaintEspe.Models
{
    public class Linea : Figura
    {
        public Point Inicio { get; set; }
        public Point Fin { get; set; }

        public Linea(Point inicio, Point fin, Color colorLinea, int grosor) : base(colorLinea, grosor)
        {
            Inicio = inicio;
            Fin = fin;
        }

        public override void Dibujar(Bitmap lienzo)
        {
            if (Grosor <= 1)
            {
                TrazarBresenham(lienzo, Inicio.X, Inicio.Y, Fin.X, Fin.Y, ColorLinea);
                return;
            }

            // Para líneas gruesas se trazan varias rectas paralelas (perpendiculares a la
            // dirección original) usando Bresenham puro en cada una, simulando el "pincel"
            // de grosor N que usa MS Paint.
            double dx = Fin.X - Inicio.X;
            double dy = Fin.Y - Inicio.Y;
            double longitud = Math.Sqrt(dx * dx + dy * dy);

            double nx = 0, ny = 0;
            if (longitud > 0.0001)
            {
                nx = -dy / longitud;
                ny = dx / longitud;
            }

            int mitad = Grosor / 2;
            for (int offset = -mitad; offset <= mitad; offset++)
            {
                int ox = (int)Math.Round(nx * offset);
                int oy = (int)Math.Round(ny * offset);
                TrazarBresenham(lienzo, Inicio.X + ox, Inicio.Y + oy, Fin.X + ox, Fin.Y + oy, ColorLinea);
            }
        }

        /// <summary>
        /// Algoritmo de Bresenham puro para rasterización de líneas (control manual de píxeles).
        /// </summary>
        internal static void TrazarBresenham(Bitmap lienzo, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;

            while (true)
            {
                if (x0 >= 0 && x0 < lienzo.Width && y0 >= 0 && y0 < lienzo.Height)
                {
                    lienzo.SetPixel(x0, y0, color);
                }

                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }

        public override void Trasladar(int dx, int dy)
        {
            Inicio = new Point(Inicio.X + dx, Inicio.Y + dy);
            Fin = new Point(Fin.X + dx, Fin.Y + dy);
        }
    }
}
