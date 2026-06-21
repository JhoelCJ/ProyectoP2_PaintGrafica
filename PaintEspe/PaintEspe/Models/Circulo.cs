using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintEspe.Models
{
    public class Circulo : Figura
    {
        public Point Centro { get; set; }
        public int Radio { get; set; }

        public Circulo(Point inicio, Point fin, Color colorLinea, int grosor) : base(colorLinea, grosor)
        {
            Centro = inicio;
            // Calculamos el radio usando el teorema de Pitágoras (distancia entre inicio y fin)
            Radio = (int)Math.Sqrt(Math.Pow(fin.X - inicio.X, 2) + Math.Pow(fin.Y - inicio.Y, 2));
        }

        public override void Dibujar(Bitmap lienzo)
        {
            // Algoritmo de Punto Medio para Círculos
            int x = 0;
            int y = Radio;
            int d = 3 - 2 * Radio;

            DibujarSimetria(lienzo, Centro.X, Centro.Y, x, y);

            while (y >= x)
            {
                x++;
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }
                DibujarSimetria(lienzo, Centro.X, Centro.Y, x, y);
            }
        }

        private void DibujarSimetria(Bitmap bmp, int xc, int yc, int x, int y)
        {
            // Pinta los 8 octantes del círculo
            PintarPixelSeguro(bmp, xc + x, yc + y);
            PintarPixelSeguro(bmp, xc - x, yc + y);
            PintarPixelSeguro(bmp, xc + x, yc - y);
            PintarPixelSeguro(bmp, xc - x, yc - y);
            PintarPixelSeguro(bmp, xc + y, yc + x);
            PintarPixelSeguro(bmp, xc - y, yc + x);
            PintarPixelSeguro(bmp, xc + y, yc - x);
            PintarPixelSeguro(bmp, xc - y, yc - x);
        }

        private void PintarPixelSeguro(Bitmap bmp, int x, int y)
        {
            if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
            {
                bmp.SetPixel(x, y, ColorLinea);
            }
        }

        public override void Trasladar(int dx, int dy)
        {
            Centro = new Point(Centro.X + dx, Centro.Y + dy);
        }
    }
}
