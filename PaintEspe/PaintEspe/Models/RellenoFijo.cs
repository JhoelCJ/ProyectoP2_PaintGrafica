using System.Drawing;
using PaintEspe.GraphicsCore;

namespace PaintEspe.Models
{
    public class RellenoFijo : Figura
    {
        public Point Semilla { get; private set; }

        public RellenoFijo(Point semilla, Color colorRelleno) : base(colorRelleno, 1)
        {
            Semilla = semilla;
        }

        public override void Dibujar(Bitmap lienzo)
        {
            AlgoritmosRelleno.FloodFill(lienzo, Semilla, ColorLinea);
        }

        public override void Trasladar(int dx, int dy)
        {
            Semilla = new Point(Semilla.X + dx, Semilla.Y + dy);
        }
    }
}
