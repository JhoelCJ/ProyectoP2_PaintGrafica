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

                // Relleno con el algoritmo de línea de barrido (Scanline).
                AlgoritmosRelleno.ScanlineFill(lienzo, vertices, ColorRelleno);
            }

            // Bordes con Bresenham (reutilizando la clase Linea).
            Linea arriba = new Linea(new Point(Inicio.X, Inicio.Y), new Point(Fin.X, Inicio.Y), ColorLinea, Grosor);
            Linea derecha = new Linea(new Point(Fin.X, Inicio.Y), new Point(Fin.X, Fin.Y), ColorLinea, Grosor);
            Linea abajo = new Linea(new Point(Fin.X, Fin.Y), new Point(Inicio.X, Fin.Y), ColorLinea, Grosor);
            Linea izquierda = new Linea(new Point(Inicio.X, Fin.Y), new Point(Inicio.X, Inicio.Y), ColorLinea, Grosor);

            arriba.Dibujar(lienzo);
            derecha.Dibujar(lienzo);
            abajo.Dibujar(lienzo);
            izquierda.Dibujar(lienzo);
        }

        public override void Trasladar(int dx, int dy)
        {
            Inicio = new Point(Inicio.X + dx, Inicio.Y + dy);
            Fin = new Point(Fin.X + dx, Fin.Y + dy);
        }
    }
}
