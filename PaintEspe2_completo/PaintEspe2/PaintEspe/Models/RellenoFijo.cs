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

        public override Rectangle ObtenerBounds() => new Rectangle(Semilla.X, Semilla.Y, 1, 1);

        public override void Dibujar(Bitmap lienzo)
        {
            AlgoritmosRelleno.FloodFill(lienzo, Semilla, ColorLinea);
        }

        public override void Trasladar(int dx, int dy)
        {
            Semilla = new Point(Semilla.X + dx, Semilla.Y + dy);
        }

        public override void Rotar(double anguloDeg, Point centro)
        {
            Semilla = RotarPunto(Semilla, anguloDeg, centro);
        }

        public override void Escalar(double factorX, double factorY, Point centro)
        {
            Semilla = EscalarPunto(Semilla, factorX, factorY, centro);
        }
    }
}
