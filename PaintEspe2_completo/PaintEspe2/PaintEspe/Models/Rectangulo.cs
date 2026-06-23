using System;
using System.Collections.Generic;
using System.Drawing;
using PaintEspe.GraphicsCore;

namespace PaintEspe.Models
{
    public class Rectangulo : Figura
    {
        public Point Inicio { get; set; }
        public Point Fin { get; set; }

        public Rectangulo(Point inicio, Point fin, Color colorLinea, int grosor) : base(colorLinea, grosor)
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
            if (EstaRelleno)
            {
                List<Point> vertices = new List<Point>
                {
                    new Point(Inicio.X, Inicio.Y),
                    new Point(Fin.X, Inicio.Y),
                    new Point(Fin.X, Fin.Y),
                    new Point(Inicio.X, Fin.Y)
                };
                AlgoritmosRelleno.ScanlineFill(lienzo, vertices, ColorRelleno);
            }

            new Linea(new Point(Inicio.X, Inicio.Y), new Point(Fin.X, Inicio.Y), ColorLinea, Grosor).Dibujar(lienzo);
            new Linea(new Point(Fin.X, Inicio.Y), new Point(Fin.X, Fin.Y), ColorLinea, Grosor).Dibujar(lienzo);
            new Linea(new Point(Fin.X, Fin.Y), new Point(Inicio.X, Fin.Y), ColorLinea, Grosor).Dibujar(lienzo);
            new Linea(new Point(Inicio.X, Fin.Y), new Point(Inicio.X, Inicio.Y), ColorLinea, Grosor).Dibujar(lienzo);
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
