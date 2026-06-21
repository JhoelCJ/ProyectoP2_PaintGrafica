using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Reutilizamos el algoritmo de Bresenham de la clase Linea para dibujar los 4 lados
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
