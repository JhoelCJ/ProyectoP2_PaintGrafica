using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Algoritmo de Bresenham puro para rasterización de líneas
            int x0 = Inicio.X, y0 = Inicio.Y;
            int x1 = Fin.X, y1 = Fin.Y;

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;

            while (true)
            {
                if (x0 >= 0 && x0 < lienzo.Width && y0 >= 0 && y0 < lienzo.Height)
                {
                    lienzo.SetPixel(x0, y0, ColorLinea); // Control de píxeles manual
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
