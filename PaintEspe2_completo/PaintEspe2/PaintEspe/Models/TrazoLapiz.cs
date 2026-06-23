using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public override Rectangle ObtenerBounds()
        {
            if (_puntos == null || _puntos.Count == 0) return Rectangle.Empty;
            int minX = _puntos.Min(p => p.X), maxX = _puntos.Max(p => p.X);
            int minY = _puntos.Min(p => p.Y), maxY = _puntos.Max(p => p.Y);
            return new Rectangle(minX, minY, Math.Max(maxX - minX, 1), Math.Max(maxY - minY, 1));
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

        public override void Rotar(double anguloDeg, Point centro)
        {
            for (int i = 0; i < _puntos.Count; i++)
                _puntos[i] = RotarPunto(_puntos[i], anguloDeg, centro);
        }

        public override void Escalar(double factorX, double factorY, Point centro)
        {
            for (int i = 0; i < _puntos.Count; i++)
                _puntos[i] = EscalarPunto(_puntos[i], factorX, factorY, centro);
        }
    }
}
