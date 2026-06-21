using System;
using System.Drawing;

namespace PaintEspe.Models
{
    public class CurvaBezier : Figura
    {
        public Point P0 { get; set; }
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }

        public CurvaBezier(Point p0, Point p1, Point p2, Point p3, Color colorLinea, int grosor) : base(colorLinea, grosor)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public override void Dibujar(Bitmap lienzo)
        {
            Point puntoAnterior = P0;
            const double paso = 0.01;

            for (double t = 0; t <= 1.0; t += paso)
            {
                Point puntoActual = ObtenerPunto(t);
                new Linea(puntoAnterior, puntoActual, ColorLinea, Grosor).Dibujar(lienzo);
                puntoAnterior = puntoActual;
            }

            new Linea(puntoAnterior, P3, ColorLinea, Grosor).Dibujar(lienzo);
        }

        public Point ObtenerPunto(double t)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;

            double x = uuu * P0.X + 3 * uu * t * P1.X + 3 * u * tt * P2.X + ttt * P3.X;
            double y = uuu * P0.Y + 3 * uu * t * P1.Y + 3 * u * tt * P2.Y + ttt * P3.Y;

            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }

        public bool ContienePuntoDeControl(Point punto, int tolerancia, out int indice)
        {
            Point[] puntos = { P0, P1, P2, P3 };
            for (int i = 0; i < puntos.Length; i++)
            {
                if (Math.Abs(puntos[i].X - punto.X) <= tolerancia && Math.Abs(puntos[i].Y - punto.Y) <= tolerancia)
                {
                    indice = i;
                    return true;
                }
            }

            indice = -1;
            return false;
        }

        public void MoverPuntoControl(int indice, Point nuevoPunto)
        {
            switch (indice)
            {
                case 0: P0 = nuevoPunto; break;
                case 1: P1 = nuevoPunto; break;
                case 2: P2 = nuevoPunto; break;
                case 3: P3 = nuevoPunto; break;
            }
        }

        public override void Trasladar(int dx, int dy)
        {
            P0 = new Point(P0.X + dx, P0.Y + dy);
            P1 = new Point(P1.X + dx, P1.Y + dy);
            P2 = new Point(P2.X + dx, P2.Y + dy);
            P3 = new Point(P3.X + dx, P3.Y + dy);
        }
    }
}
