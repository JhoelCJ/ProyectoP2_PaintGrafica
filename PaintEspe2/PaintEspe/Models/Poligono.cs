using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PaintEspe.GraphicsCore;

namespace PaintEspe.Models
{
    public class Poligono : Figura
    {
        public List<Point> Vertices { get; private set; }

        public Poligono(IEnumerable<Point> vertices, Color colorLinea, int grosor) : base(colorLinea, grosor)
        {
            Vertices = vertices != null ? new List<Point>(vertices) : new List<Point>();
        }

        public void AgregarVertice(Point punto)
        {
            Vertices.Add(punto);
        }

        public override void Dibujar(Bitmap lienzo)
        {
            if (Vertices == null || Vertices.Count < 2)
            {
                return;
            }

            if (EstaRelleno && TieneFormaCerrada())
            {
                // Relleno con el algoritmo de línea de barrido (Scanline), válido para
                // polígonos convexos y cóncavos.
                AlgoritmosRelleno.ScanlineFill(lienzo, Vertices, ColorRelleno);
            }

            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                new Linea(Vertices[i], Vertices[i + 1], ColorLinea, Grosor).Dibujar(lienzo);
            }

            if (Vertices.Count >= 3)
            {
                new Linea(Vertices[Vertices.Count - 1], Vertices[0], ColorLinea, Grosor).Dibujar(lienzo);
            }
        }

        public bool TieneFormaCerrada()
        {
            return Vertices != null && Vertices.Count >= 3;
        }

        public override void Trasladar(int dx, int dy)
        {
            if (Vertices == null)
            {
                return;
            }

            Vertices = Vertices.Select(p => new Point(p.X + dx, p.Y + dy)).ToList();
        }
    }
}
