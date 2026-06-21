using System;
using System.Drawing;
using PaintEspe.GraphicsCore;

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
            if (EstaRelleno && Radio > 0)
            {
                // Relleno: Punto Medio para hallar los bordes de cada fila + barrido horizontal.
                AlgoritmosRelleno.RellenarCirculoScanline(lienzo, Centro, Radio, ColorRelleno);
            }

            // Algoritmo de Punto Medio para Círculos (contorno)
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
            // Pinta los 8 octantes del círculo, respetando el grosor de trazo.
            PintarPuntoGrueso(bmp, xc + x, yc + y);
            PintarPuntoGrueso(bmp, xc - x, yc + y);
            PintarPuntoGrueso(bmp, xc + x, yc - y);
            PintarPuntoGrueso(bmp, xc - x, yc - y);
            PintarPuntoGrueso(bmp, xc + y, yc + x);
            PintarPuntoGrueso(bmp, xc - y, yc + x);
            PintarPuntoGrueso(bmp, xc + y, yc - x);
            PintarPuntoGrueso(bmp, xc - y, yc - x);
        }

        private void PintarPuntoGrueso(Bitmap bmp, int x, int y)
        {
            if (Grosor <= 1)
            {
                PintarPixelSeguro(bmp, x, y);
                return;
            }

            int mitad = Grosor / 2;
            for (int oy = -mitad; oy <= mitad; oy++)
            {
                for (int ox = -mitad; ox <= mitad; ox++)
                {
                    PintarPixelSeguro(bmp, x + ox, y + oy);
                }
            }
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
