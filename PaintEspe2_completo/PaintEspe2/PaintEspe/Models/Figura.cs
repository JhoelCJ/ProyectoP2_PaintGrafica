using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintEspe.Models
{
    public abstract class Figura
    {
        public Color ColorLinea { get; set; }
        public int Grosor { get; set; }
        public bool EstaRelleno { get; set; }
        public Color ColorRelleno { get; set; }

        public Figura(Color colorLinea, int grosor)
        {
            ColorLinea = colorLinea;
            Grosor = grosor;
        }

        // Método polimórfico que cada figura implementará con sus propios algoritmos
        public abstract void Dibujar(Bitmap lienzo);

        // Obtener bounding-box para calcular centro de transformaciones
        public abstract Rectangle ObtenerBounds();

        // Método para aplicar transformaciones geométricas
        public abstract void Trasladar(int dx, int dy);

        // Rotar en grados alrededor de un centro
        public abstract void Rotar(double anguloDeg, Point centro);

        // Escalar con respecto a un centro (factorX, factorY > 0)
        public abstract void Escalar(double factorX, double factorY, Point centro);

        // Helper estático: rotar un punto alrededor de un centro
        protected static Point RotarPunto(Point p, double anguloDeg, Point centro)
        {
            double rad = anguloDeg * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double dx = p.X - centro.X;
            double dy = p.Y - centro.Y;
            return new Point(
                (int)Math.Round(centro.X + dx * cos - dy * sin),
                (int)Math.Round(centro.Y + dx * sin + dy * cos));
        }

        // Helper estático: escalar un punto alrededor de un centro
        protected static Point EscalarPunto(Point p, double fx, double fy, Point centro)
        {
            return new Point(
                (int)Math.Round(centro.X + (p.X - centro.X) * fx),
                (int)Math.Round(centro.Y + (p.Y - centro.Y) * fy));
        }
    }
}
