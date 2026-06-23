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

        public override Rectangle ObtenerBounds()
        {
            int x = Math.Min(Inicio.X, Fin.X);
            int y = Math.Min(Inicio.Y, Fin.Y);
            int w = Math.Abs(Fin.X - Inicio.X);
            int h = Math.Abs(Fin.Y - Inicio.Y);
            return new Rectangle(x, y, Math.Max(w, 1), Math.Max(h, 1));
        }

        public override void Dibujar(Bitmap lienzo)
        {
            if (Grosor <= 1)
            {
                TrazarBresenham(lienzo, Inicio.X, Inicio.Y, Fin.X, Fin.Y, ColorLinea);
                return;
            }

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

        internal static void TrazarBresenham(Bitmap lienzo, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;

            while (true)
            {
                if (x0 >= 0 && x0 < lienzo.Width && y0 >= 0 && y0 < lienzo.Height)
                    lienzo.SetPixel(x0, y0, color);

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

        public override void Rotar(double anguloDeg, Point centro)
        {
            Inicio = RotarPunto(Inicio, anguloDeg, centro);
            Fin = RotarPunto(Fin, anguloDeg, centro);
        }

        public override void Escalar(double factorX, double factorY, Point centro)
        {
            Inicio = EscalarPunto(Inicio, factorX, factorY, centro);
            Fin = EscalarPunto(Fin, factorX, factorY, centro);
        }
    }
}
