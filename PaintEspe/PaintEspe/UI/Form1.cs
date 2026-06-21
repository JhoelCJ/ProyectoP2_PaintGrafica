using PaintEspe.GraphicsCore;
using PaintEspe.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace PaintEspe
{
    public partial class Form1 : Form
    {
        private enum Herramienta
        {
            Ninguna,
            Linea,
            Rectangulo,
            Circulo,
            Poligono,
            Bezier,
            Relleno
        }

        private Bitmap mapaBits;
        private readonly List<Figura> historialFiguras = new List<Figura>();
        private readonly List<Point> puntosPoligono = new List<Point>();
        private readonly List<Point> puntosBezier = new List<Point>();

        private Herramienta herramientaActual = Herramienta.Linea;
        private Color colorActual = Color.Black;
        private int grosorActual = 1;

        private Point puntoInicio;
        private bool estaDibujando;
        private CurvaBezier curvaBezierSeleccionada;
        private int indiceControlBezier = -1;
        private bool arrastrandoControlBezier;

        private const int ToleranciaControl = 8;

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            grosorActual = (int)nudGrosor.Value;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InicializarLienzo();
        }

        private void InicializarLienzo()
        {
            if (pbLienzo.Width <= 0 || pbLienzo.Height <= 0)
            {
                return;
            }

            mapaBits?.Dispose();
            mapaBits = new Bitmap(pbLienzo.Width, pbLienzo.Height);
            using (Graphics g = Graphics.FromImage(mapaBits))
            {
                g.Clear(Color.White);
            }

            ImagenLienzoActual = mapaBits;
        }

        private Image ImagenLienzoActual
        {
            set
            {
                Image anterior = pbLienzo.Image;
                pbLienzo.Image = value;

                if (anterior != null && anterior != mapaBits && anterior != value)
                {
                    anterior.Dispose();
                }
            }
        }

        private void ActivarHerramienta(Herramienta herramienta)
        {
            herramientaActual = herramienta;
            estaDibujando = false;
            arrastrandoControlBezier = false;
            indiceControlBezier = -1;
            puntosPoligono.Clear();
            puntosBezier.Clear();
            curvaBezierSeleccionada = null;
            MostrarLienzoBase();
        }

        private void btnLinea_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Linea);

        private void btnRectangulo_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Rectangulo);

        private void btnCirculo_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Circulo);

        private void btnPoligono_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Poligono);

        private void btnBezier_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Bezier);

        private void btnRelleno_Click(object sender, EventArgs e) => ActivarHerramienta(Herramienta.Relleno);

        private void btnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dialogoColor = new ColorDialog())
            {
                dialogoColor.Color = colorActual;
                if (dialogoColor.ShowDialog() == DialogResult.OK)
                {
                    colorActual = dialogoColor.Color;
                    btnColor.BackColor = colorActual;
                }
            }
        }

        private void nudGrosor_ValueChanged(object sender, EventArgs e)
        {
            grosorActual = (int)nudGrosor.Value;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            historialFiguras.Clear();
            puntosPoligono.Clear();
            puntosBezier.Clear();
            curvaBezierSeleccionada = null;
            indiceControlBezier = -1;
            arrastrandoControlBezier = false;
            estaDibujando = false;

            using (Graphics g = Graphics.FromImage(mapaBits))
            {
                g.Clear(Color.White);
            }

            ImagenLienzoActual = mapaBits;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Imagen PNG|*.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    mapaBits.Save(sfd.FileName, ImageFormat.Png);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && herramientaActual == Herramienta.Poligono)
            {
                FinalizarPoligono();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                puntosPoligono.Clear();
                puntosBezier.Clear();
                arrastrandoControlBezier = false;
                indiceControlBezier = -1;
                curvaBezierSeleccionada = null;
                estaDibujando = false;
                MostrarLienzoBase();
                e.Handled = true;
            }
        }

        private void pbLienzo_MouseDown(object sender, MouseEventArgs e)
        {
            if (mapaBits == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Right && herramientaActual == Herramienta.Poligono)
            {
                FinalizarPoligono();
                return;
            }

            if (herramientaActual == Herramienta.Relleno && e.Button == MouseButtons.Left)
            {
                historialFiguras.Add(new RellenoFijo(e.Location, colorActual));
                historialFiguras[historialFiguras.Count - 1].Dibujar(mapaBits);
                ImagenLienzoActual = mapaBits;
                return;
            }

            if (herramientaActual == Herramienta.Bezier && e.Button == MouseButtons.Left)
            {
                CurvaBezier curvaCercana = ObtenerCurvaBezierEnPunto(e.Location, out indiceControlBezier);
                if (curvaCercana != null)
                {
                    curvaBezierSeleccionada = curvaCercana;
                    arrastrandoControlBezier = true;
                    return;
                }

                if (puntosBezier.Count < 4)
                {
                    puntosBezier.Add(e.Location);
                    MostrarVistaPreviaBezier(e.Location);

                    if (puntosBezier.Count == 4)
                    {
                        CurvaBezier curva = new CurvaBezier(puntosBezier[0], puntosBezier[1], puntosBezier[2], puntosBezier[3], colorActual, grosorActual);
                        historialFiguras.Add(curva);
                        curva.Dibujar(mapaBits);
                        curvaBezierSeleccionada = curva;
                        puntosBezier.Clear();
                        ImagenLienzoActual = mapaBits;
                    }
                }

                return;
            }

            if (herramientaActual == Herramienta.Poligono && e.Button == MouseButtons.Left)
            {
                puntosPoligono.Add(e.Location);
                MostrarVistaPreviaPoligono(e.Location);
                return;
            }

            if (herramientaActual == Herramienta.Linea ||
                herramientaActual == Herramienta.Rectangulo ||
                herramientaActual == Herramienta.Circulo)
            {
                estaDibujando = true;
                puntoInicio = e.Location;
            }
        }

        private void pbLienzo_MouseMove(object sender, MouseEventArgs e)
        {
            if (mapaBits == null)
            {
                return;
            }

            if (arrastrandoControlBezier && curvaBezierSeleccionada != null)
            {
                curvaBezierSeleccionada.MoverPuntoControl(indiceControlBezier, e.Location);
                MostrarLienzoBase();
                MostrarVistaPreviaBezier();
                return;
            }

            if (herramientaActual == Herramienta.Poligono && puntosPoligono.Count > 0)
            {
                MostrarVistaPreviaPoligono(e.Location);
                return;
            }

            if (herramientaActual == Herramienta.Bezier && puntosBezier.Count > 0 && puntosBezier.Count < 4)
            {
                MostrarVistaPreviaBezier(e.Location);
                return;
            }

            if (estaDibujando)
            {
                MostrarVistaPreviaFigura(e.Location);
            }
        }

        private void pbLienzo_MouseUp(object sender, MouseEventArgs e)
        {
            if (arrastrandoControlBezier)
            {
                arrastrandoControlBezier = false;
                indiceControlBezier = -1;
                MostrarLienzoBase();
                MostrarVistaPreviaBezier();
                return;
            }

            if (!estaDibujando)
            {
                return;
            }

            estaDibujando = false;

            Figura nuevaFigura = CrearFigura(puntoInicio, e.Location);
            if (nuevaFigura != null)
            {
                historialFiguras.Add(nuevaFigura);
                nuevaFigura.Dibujar(mapaBits);
            }

            ImagenLienzoActual = mapaBits;
        }

        private Figura CrearFigura(Point inicio, Point fin)
        {
            switch (herramientaActual)
            {
                case Herramienta.Linea:
                    return new Linea(inicio, fin, colorActual, grosorActual);
                case Herramienta.Rectangulo:
                    return new Rectangulo(inicio, fin, colorActual, grosorActual);
                case Herramienta.Circulo:
                    return new Circulo(inicio, fin, colorActual, grosorActual);
                default:
                    return null;
            }
        }

        private CurvaBezier ObtenerCurvaBezierEnPunto(Point punto, out int indiceControl)
        {
            for (int i = historialFiguras.Count - 1; i >= 0; i--)
            {
                CurvaBezier curva = historialFiguras[i] as CurvaBezier;
                if (curva != null && curva.ContienePuntoDeControl(punto, ToleranciaControl, out indiceControl))
                {
                    return curva;
                }
            }

            indiceControl = -1;
            return null;
        }

        private void FinalizarPoligono()
        {
            if (puntosPoligono.Count < 3)
            {
                return;
            }

            Poligono poligono = new Poligono(puntosPoligono, colorActual, grosorActual);
            historialFiguras.Add(poligono);
            poligono.Dibujar(mapaBits);
            puntosPoligono.Clear();
            ImagenLienzoActual = mapaBits;
        }

        private void MostrarLienzoBase()
        {
            if (mapaBits == null)
            {
                return;
            }

            using (Graphics g = Graphics.FromImage(mapaBits))
            {
                g.Clear(Color.White);
            }

            foreach (Figura figura in historialFiguras)
            {
                figura.Dibujar(mapaBits);
            }

            ImagenLienzoActual = mapaBits;
        }

        private void MostrarVistaPreviaFigura(Point puntoFin)
        {
            using (Bitmap temporal = new Bitmap(mapaBits))
            {
                Figura figuraTemp = CrearFigura(puntoInicio, puntoFin);
                figuraTemp?.Dibujar(temporal);
                ImagenLienzoActual = (Bitmap)temporal.Clone();
            }
        }

        private void MostrarVistaPreviaPoligono(Point cursor)
        {
            using (Bitmap temporal = new Bitmap(mapaBits))
            {
                DibujarPoligonoTemporal(temporal, cursor);
                ImagenLienzoActual = (Bitmap)temporal.Clone();
            }
        }

        private void MostrarVistaPreviaBezier(Point? cursor = null)
        {
            using (Bitmap temporal = new Bitmap(mapaBits))
            {
                DibujarBezierTemporal(temporal, cursor);
                ImagenLienzoActual = (Bitmap)temporal.Clone();
            }
        }

        private void DibujarPoligonoTemporal(Bitmap lienzo, Point cursor)
        {
            if (puntosPoligono.Count == 0)
            {
                return;
            }

            List<Point> vertices = new List<Point>(puntosPoligono);
            if (cursor != Point.Empty)
            {
                vertices.Add(cursor);
            }

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                new Linea(vertices[i], vertices[i + 1], colorActual, grosorActual).Dibujar(lienzo);
            }

            PintarMarcadores(lienzo, puntosPoligono, Color.OrangeRed);
        }

        private void DibujarBezierTemporal(Bitmap lienzo, Point? cursor)
        {
            if (puntosBezier.Count == 0 && curvaBezierSeleccionada == null)
            {
                return;
            }

            List<Point> puntos = new List<Point>(puntosBezier);
            if (cursor.HasValue && puntos.Count < 4)
            {
                puntos.Add(cursor.Value);
            }

            if (puntos.Count >= 4)
            {
                CurvaBezier curvaTemp = new CurvaBezier(puntos[0], puntos[1], puntos[2], puntos[3], colorActual, grosorActual);
                DibujarPolilineaControl(lienzo, puntos, Color.DarkGray);
                curvaTemp.Dibujar(lienzo);
                PintarMarcadores(lienzo, new[] { puntos[0], puntos[1], puntos[2], puntos[3] }, Color.Gold);
            }
            else
            {
                DibujarPolilineaControl(lienzo, puntos, Color.DarkGray);
                PintarMarcadores(lienzo, puntos, Color.Gold);
            }

            if (puntosBezier.Count == 0 && curvaBezierSeleccionada != null)
            {
                DibujarPolilineaControl(lienzo,
                    new[] { curvaBezierSeleccionada.P0, curvaBezierSeleccionada.P1, curvaBezierSeleccionada.P2, curvaBezierSeleccionada.P3 },
                    Color.DarkGray);
                PintarMarcadores(lienzo,
                    new[] { curvaBezierSeleccionada.P0, curvaBezierSeleccionada.P1, curvaBezierSeleccionada.P2, curvaBezierSeleccionada.P3 },
                    Color.Gold);
            }
        }

        private void DibujarPolilineaControl(Bitmap lienzo, IEnumerable<Point> puntos, Color color)
        {
            List<Point> lista = new List<Point>(puntos);
            if (lista.Count < 2)
            {
                return;
            }

            for (int i = 0; i < lista.Count - 1; i++)
            {
                new Linea(lista[i], lista[i + 1], color, 1).Dibujar(lienzo);
            }
        }

        private void PintarMarcadores(Bitmap lienzo, IEnumerable<Point> puntos, Color color)
        {
            using (Graphics g = Graphics.FromImage(lienzo))
            using (Brush brush = new SolidBrush(color))
            using (Pen pen = new Pen(Color.Black, 1))
            {
                foreach (Point punto in puntos)
                {
                    Rectangle rect = new Rectangle(punto.X - 3, punto.Y - 3, 6, 6);
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }
            }
        }

        private void PintarPuntosBezier(Bitmap lienzo, CurvaBezier curva)
        {
            if (curva == null)
            {
                return;
            }

            PintarMarcadores(lienzo, new[] { curva.P0, curva.P1, curva.P2, curva.P3 }, Color.Gold);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            mapaBits?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
