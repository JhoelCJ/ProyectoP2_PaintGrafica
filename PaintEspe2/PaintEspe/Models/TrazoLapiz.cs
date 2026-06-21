using System.Collections.Generic;
using System.Drawing;

namespace PaintEspe.Models
{
    public class TrazoLapiz : Figura
    {
        private readonly List<Point> _puntos;

        public TrazoLapiz(List<Point> puntos, Color colorLinea, int grosor)
            : base(colorLinea, grosor)
        {
            _puntos = new List<Point>(puntos);
        }

        public override void Dibujar(Bitmap lienzo)
        {
            for (int i = 0; i < _puntos.Count - 1; i++)
                new Linea(_puntos[i], _puntos[i + 1], ColorLinea, Grosor).Dibujar(lienzo);
        }

        public override void Trasladar(int dx, int dy)
        {
            for (int i = 0; i < _puntos.Count; i++)
                _puntos[i] = new Point(_puntos[i].X + dx, _puntos[i].Y + dy);
        }
    }
}
