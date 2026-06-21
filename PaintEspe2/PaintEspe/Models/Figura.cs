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

        // Método para aplicar transformaciones geométricas
        public abstract void Trasladar(int dx, int dy);
    }
}
