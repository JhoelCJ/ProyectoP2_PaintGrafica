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
        // ─── Enumeracion herramientas ───────────────────────────────────────────
        private enum Herramienta
        {
            Ninguna, Seleccion,
            Linea, Rectangulo, Circulo, Poligono, Bezier,
            Relleno, Borrador, Texto, Lapiz
        }

        // ─── Estado del lienzo ──────────────────────────────────────────────────
        private Bitmap mapaBits;
        private Bitmap bitmapComprometido;
        private readonly List<Figura> historialFiguras = new List<Figura>();
        private readonly List<Point> puntosPoligono = new List<Point>();
        private readonly Stack<List<Figura>> pilaDeshacer = new Stack<List<Figura>>();
        private const int LimiteDeshacer = 30;

        // ─── Herramienta activa ─────────────────────────────────────────────────
        private Herramienta herramientaActual = Herramienta.Linea;
        private Color colorActual = Color.Black;
        private Color colorRellenoActual = Color.White;
        private int grosorActual = 1;
        private const int TamanoBorrador = 16;

        // ─── Dibujo básico ──────────────────────────────────────────────────────
        private Point puntoInicio;
        private bool estaDibujando;

        // ─── Bezier ─────────────────────────────────────────────────────────────
        private CurvaBezier curvaBezierSeleccionada;
        private int indiceControlBezier = -1;
        private bool arrastrandoControlBezier;
        private bool bezierDefiniendoLinea;
        private Point bezierP0;
        private Point bezierP3Actual;
        private const int ToleranciaControl = 8;

        // ─── Lapiz ──────────────────────────────────────────────────────────────
        private readonly List<Point> puntosLapiz = new List<Point>();

        // ─── Borrador ────────────────────────────────────────────────────────────
        // Trazos de borrador se guardan como figuras "BorradorTrazo" en el historial
        private List<Point> puntosBorrador = new List<Point>();

        // ─── Selección y movimiento ─────────────────────────────────────────────
        private Rectangle rectSeleccion = Rectangle.Empty;
        private bool dibujandoSeleccion;
        private Bitmap bitmapSeleccionado;
        private Point puntoInicioArrastre;
        private bool arrastandoSeleccion;
        private Bitmap bitmapSinSeleccion;

        // ─── Modo Edición (rotar/escalar) ────────────────────────────────────────
        // figuraEnEdicion: la última figura dibujada entra en modo edición
        // figuraSeleccionadaEdicion: figura elegida con la herramienta Selección
        private Figura figuraEnEdicion = null;
        private bool modoEdicionActivo = false;

        // Para rotar/escalar con la barra de herramientas de edición
        private Panel panelEdicion = null;
        private TrackBar trackRotacion = null;
        private TrackBar trackEscala = null;
        private double anguloAcumulado = 0;
        private double escalaAcumulada = 1.0;
        // Snapshot de la figura original al entrar en modo edición (para preview en tiempo real)
        private Figura figuraOriginalEdicion = null;

        // ─── Texto ──────────────────────────────────────────────────────────────
        private bool modoTextoActivo;
        private Point puntoTexto;
        private string textoActual = "";
        private bool escribiendoTexto;

        // ─── Botones de herramientas ────────────────────────────────────────────
        private readonly Dictionary<Herramienta, Button> botonesHerramienta = new Dictionary<Herramienta, Button>();
        private Button btnColorActual;
        private Button btnColorRellenoActual;

        // ─── Paleta ─────────────────────────────────────────────────────────────
        private static readonly Color[] ColorsPaleta =
        {
            Color.Black, Color.FromArgb(127,127,127), Color.FromArgb(136,0,21),
            Color.Red, Color.FromArgb(255,127,39), Color.Yellow,
            Color.FromArgb(0,128,0), Color.FromArgb(0,200,100),
            Color.FromArgb(0,162,232), Color.FromArgb(0,0,255),
            Color.FromArgb(63,72,204), Color.FromArgb(112,48,160),
            Color.White, Color.FromArgb(195,195,195),
            Color.FromArgb(39,39,39), Color.FromArgb(195,195,195),
            Color.FromArgb(185,122,87), Color.FromArgb(255,174,201),
            Color.FromArgb(255,201,14), Color.FromArgb(239,228,176),
            Color.FromArgb(181,230,29), Color.FromArgb(153,217,234),
            Color.FromArgb(112,146,190), Color.FromArgb(84,109,142),
            Color.FromArgb(101,140,160), Color.FromArgb(177,122,162),
            Color.FromArgb(240,240,240), Color.FromArgb(80,80,80),
        };

        // ═══════════════════════════════════════════════════════════════════════
        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            grosorActual = (int)nudGrosor.Value;
            ConstruirToolbar();
            ActualizarBotonActivo();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InicializarLienzo();
            ConstruirPanelEdicion();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PANEL DE EDICIÓN (Rotar / Escalar) — se muestra sobre el lienzo
        // ════════════════════════════════════════════════════════════════════════
        private void ConstruirPanelEdicion()
        {
            panelEdicion = new Panel
            {
                Size = new Size(340, 72),
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            // Etiqueta Rotación
            var lblRot = new Label { Text = "Rotar (°):", Location = new Point(6, 8), AutoSize = true,
                Font = new Font("Segoe UI", 8f) };
            panelEdicion.Controls.Add(lblRot);

            trackRotacion = new TrackBar
            {
                Minimum = -180, Maximum = 180, Value = 0, TickFrequency = 45,
                Location = new Point(70, 4), Size = new Size(140, 28), SmallChange = 1, LargeChange = 15
            };
            trackRotacion.Scroll += TrackRotacion_Scroll;
            panelEdicion.Controls.Add(trackRotacion);

            var lblRotVal = new Label { Name = "lblRotVal", Text = "0°", Location = new Point(215, 8),
                AutoSize = true, Font = new Font("Segoe UI", 8f) };
            panelEdicion.Controls.Add(lblRotVal);

            // Etiqueta Escala
            var lblEsc = new Label { Text = "Escalar (%):", Location = new Point(6, 42), AutoSize = true,
                Font = new Font("Segoe UI", 8f) };
            panelEdicion.Controls.Add(lblEsc);

            trackEscala = new TrackBar
            {
                Minimum = 10, Maximum = 300, Value = 100, TickFrequency = 50,
                Location = new Point(70, 38), Size = new Size(140, 28), SmallChange = 5, LargeChange = 20
            };
            trackEscala.Scroll += TrackEscala_Scroll;
            panelEdicion.Controls.Add(trackEscala);

            var lblEscVal = new Label { Name = "lblEscVal", Text = "100%", Location = new Point(215, 42),
                AutoSize = true, Font = new Font("Segoe UI", 8f) };
            panelEdicion.Controls.Add(lblEscVal);

            // Botón Confirmar
            var btnOk = new Button
            {
                Text = "✔ OK", Size = new Size(54, 50), Location = new Point(260, 8),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f),
                BackColor = Color.FromArgb(198,225,198), ForeColor = Color.FromArgb(0,100,0)
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(100,180,100);
            btnOk.Click += (s, e) => ConfirmarEdicion();
            panelEdicion.Controls.Add(btnOk);

            pbLienzo.Controls.Add(panelEdicion);
        }

        private void MostrarPanelEdicion(Figura fig)
        {
            if (fig == null) return;
            figuraEnEdicion = fig;
            // Clonar figura original para poder re-aplicar transformaciones
            figuraOriginalEdicion = CloneFigura(fig);
            anguloAcumulado = 0;
            escalaAcumulada = 1.0;

            trackRotacion.Value = 0;
            trackEscala.Value = 100;
            ActualizarLabelEdicion("lblRotVal", "0°");
            ActualizarLabelEdicion("lblEscVal", "100%");

            // Posicionar panel cerca de la figura
            var bounds = fig.ObtenerBounds();
            int px = bounds.Right + 8;
            int py = bounds.Top;
            if (px + panelEdicion.Width > pbLienzo.Width) px = bounds.Left - panelEdicion.Width - 8;
            if (px < 0) px = 4;
            if (py + panelEdicion.Height > pbLienzo.Height) py = pbLienzo.Height - panelEdicion.Height - 4;
            if (py < 0) py = 4;
            panelEdicion.Location = new Point(px, py);
            panelEdicion.Visible = true;
            panelEdicion.BringToFront();
            modoEdicionActivo = true;

            ActualizarStatusHerramienta();
            DibujarConDestacado(fig);
        }

        private void ActualizarLabelEdicion(string name, string text)
        {
            var lbl = panelEdicion.Controls[name] as Label;
            if (lbl != null) lbl.Text = text;
        }

        private void TrackRotacion_Scroll(object sender, EventArgs e)
        {
            if (figuraEnEdicion == null || figuraOriginalEdicion == null) return;
            // Restaurar figura original y aplicar nueva rotación
            CopiarEstadoFigura(figuraOriginalEdicion, figuraEnEdicion);
            double angulo = trackRotacion.Value;
            anguloAcumulado = angulo;
            var bounds = figuraOriginalEdicion.ObtenerBounds();
            var centro = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            figuraEnEdicion.Rotar(angulo, centro);
            // También aplicar la escala actual
            double factor = trackEscala.Value / 100.0;
            if (Math.Abs(factor - 1.0) > 0.001)
                figuraEnEdicion.Escalar(factor, factor, centro);
            ActualizarLabelEdicion("lblRotVal", $"{trackRotacion.Value}°");
            RedibujarConEdicion();
        }

        private void TrackEscala_Scroll(object sender, EventArgs e)
        {
            if (figuraEnEdicion == null || figuraOriginalEdicion == null) return;
            CopiarEstadoFigura(figuraOriginalEdicion, figuraEnEdicion);
            var bounds = figuraOriginalEdicion.ObtenerBounds();
            var centro = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            // Aplicar rotación primero, luego escala
            double angulo = trackRotacion.Value;
            if (Math.Abs(angulo) > 0.001)
                figuraEnEdicion.Rotar(angulo, centro);
            double factor = trackEscala.Value / 100.0;
            escalaAcumulada = factor;
            figuraEnEdicion.Escalar(factor, factor, centro);
            ActualizarLabelEdicion("lblEscVal", $"{trackEscala.Value}%");
            RedibujarConEdicion();
        }

        private void RedibujarConEdicion()
        {
            using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
            foreach (var fig in historialFiguras) fig.Dibujar(mapaBits);
            ActualizarBitmapComprometido();
            DibujarConDestacado(figuraEnEdicion);
        }

        private void DibujarConDestacado(Figura fig)
        {
            if (fig == null) { ImagenLienzoActual = mapaBits; return; }
            using (var tmp = new Bitmap(mapaBits))
            using (var g = Graphics.FromImage(tmp))
            {
                var bounds = fig.ObtenerBounds();
                var r = Rectangle.Inflate(bounds, 4, 4);
                using (var p = new Pen(Color.FromArgb(0, 120, 215), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    g.DrawRectangle(p, r);
                // Marcadores de esquina
                foreach (var corner in new[] {
                    new Point(r.Left, r.Top), new Point(r.Right, r.Top),
                    new Point(r.Left, r.Bottom), new Point(r.Right, r.Bottom) })
                {
                    g.FillRectangle(Brushes.White, corner.X - 3, corner.Y - 3, 6, 6);
                    g.DrawRectangle(new Pen(Color.FromArgb(0, 120, 215)), corner.X - 3, corner.Y - 3, 6, 6);
                }
                ImagenLienzoActual = (Bitmap)tmp.Clone();
            }
        }

        private void ConfirmarEdicion()
        {
            modoEdicionActivo = false;
            figuraEnEdicion = null;
            figuraOriginalEdicion = null;
            panelEdicion.Visible = false;
            // Redibujar todo definitivamente
            using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
            foreach (var fig in historialFiguras) fig.Dibujar(mapaBits);
            ActualizarBitmapComprometido();
            ImagenLienzoActual = mapaBits;
            ActualizarStatusHerramienta();
        }

        // Clonar una figura (copia superficial de propiedades clave)
        private Figura CloneFigura(Figura fig)
        {
            if (fig is Linea l) return new Linea(l.Inicio, l.Fin, l.ColorLinea, l.Grosor);
            if (fig is Rectangulo rect) return new Rectangulo(rect.Inicio, rect.Fin, rect.ColorLinea, rect.Grosor)
                { EstaRelleno = rect.EstaRelleno, ColorRelleno = rect.ColorRelleno };
            if (fig is Circulo c)
            {
                var nc = new Circulo(c.Centro, new Point(c.Centro.X + c.Radio, c.Centro.Y), c.ColorLinea, c.Grosor)
                    { EstaRelleno = c.EstaRelleno, ColorRelleno = c.ColorRelleno };
                // Fix radio manually
                return new CirculoClone(c.Centro, c.Radio, c.ColorLinea, c.Grosor)
                    { EstaRelleno = c.EstaRelleno, ColorRelleno = c.ColorRelleno };
            }
            if (fig is Poligono poly)
                return new Poligono(new List<Point>(poly.Vertices), poly.ColorLinea, poly.Grosor)
                    { EstaRelleno = poly.EstaRelleno, ColorRelleno = poly.ColorRelleno };
            if (fig is CurvaBezier bez)
                return new CurvaBezier(bez.P0, bez.P1, bez.P2, bez.P3, bez.ColorLinea, bez.Grosor);
            if (fig is TrazoLapiz tl)
            {
                // Use reflection to get private _puntos
                var field = typeof(TrazoLapiz).GetField("_puntos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var pts = field?.GetValue(tl) as List<Point>;
                return new TrazoLapiz(pts != null ? new List<Point>(pts) : new List<Point>(), tl.ColorLinea, tl.Grosor);
            }
            return fig; // fallback
        }

        // Copiar estado de figOrigen a figDestino (mismos tipos)
        private void CopiarEstadoFigura(Figura origen, Figura destino)
        {
            if (origen is Linea lo && destino is Linea ld) { ld.Inicio = lo.Inicio; ld.Fin = lo.Fin; return; }
            if (origen is Rectangulo ro && destino is Rectangulo rd) { rd.Inicio = ro.Inicio; rd.Fin = ro.Fin; return; }
            if (origen is CirculoClone cc && destino is Circulo cd) { cd.Centro = cc.Centro; cd.Radio = cc.Radio; return; }
            if (origen is Circulo co && destino is Circulo cdd) { cdd.Centro = co.Centro; cdd.Radio = co.Radio; return; }
            if (origen is Poligono poo && destino is Poligono pod)
            {
                pod.Vertices.Clear(); pod.Vertices.AddRange(poo.Vertices); return;
            }
            if (origen is CurvaBezier bo && destino is CurvaBezier bd)
            {
                bd.P0 = bo.P0; bd.P1 = bo.P1; bd.P2 = bo.P2; bd.P3 = bo.P3; return;
            }
            if (origen is TrazoLapiz tlo && destino is TrazoLapiz tld)
            {
                var field = typeof(TrazoLapiz).GetField("_puntos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var ptsOrig = field?.GetValue(tlo) as List<Point>;
                var ptsDest = field?.GetValue(tld) as List<Point>;
                if (ptsOrig != null && ptsDest != null) { ptsDest.Clear(); ptsDest.AddRange(ptsOrig); }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN DE LA BARRA DE HERRAMIENTAS
        // ════════════════════════════════════════════════════════════════════════
        private void ConstruirToolbar()
        {
            panelToolbar.Paint += PanelToolbar_Paint;

            AgregarEtiquetaSeccion(panelHerramientas, "Herramientas", 0);
            AgregarEtiquetaSeccion(panelColores, "Colores", 0);
            AgregarEtiquetaSeccion(panelGrosor, "Grosor", 0);
            AgregarEtiquetaSeccion(panelAcciones, "Acciones", 0);

            int bx = 6, by = 18;
            AgregarBtnHerramienta(Herramienta.Seleccion, "⊹", "Seleccionar y mover", ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Texto,     "A", "Escribir texto",        ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Relleno,   "◈", "Rellenar área",         ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Borrador,  "⌫", "Borrador",              ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Lapiz,     "✏", "Lápiz (dibujo libre)",  ref bx, ref by);

            bx = 6; by = 48;
            AgregarBtnHerramienta(Herramienta.Linea,      "╱",  "Línea",        ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Rectangulo, "▭",  "Rectángulo",   ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Circulo,    "◯",  "Círculo",      ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Poligono,   "△",  "Polígono",     ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Bezier,     "∿",  "Curva Bézier", ref bx, ref by);

            ConstruirPaleta();

            var lblC = new Label { Text = "Línea", AutoSize = false, Width = 36, Height = 13,
                Location = new Point(4, 20), ForeColor = Color.FromArgb(80,80,80),
                Font = new Font("Segoe UI", 7f), TextAlign = ContentAlignment.MiddleLeft };
            panelGrosor.Controls.Add(lblC);

            btnColorActual = CrearCuadroColor(8, 32, colorActual, "Color de línea");
            btnColorActual.Click += (s,e) => ElegirColor(ref colorActual, btnColorActual);
            panelGrosor.Controls.Add(btnColorActual);

            nudGrosor.Location = new Point(62, 28);
            var lblGrosor = new Label { Text = "Grosor:", AutoSize = true, Location = new Point(4, 31),
                ForeColor = Color.FromArgb(60,60,60), Font = new Font("Segoe UI", 8f) };
            panelGrosor.Controls.Add(lblGrosor);

            btnColorRellenoActual = CrearCuadroColor(8, 52, colorRellenoActual, "Color de relleno");
            btnColorRellenoActual.Click += (s,e) => ElegirColor(ref colorRellenoActual, btnColorRellenoActual);
            panelGrosor.Controls.Add(btnColorRellenoActual);

            int ax = 6;
            AgregarBtnAccion(panelAcciones, "↩ Deshacer", ax, 18, (s,e) => Deshacer()); ax += 80;
            ax = 6;
            AgregarBtnAccion(panelAcciones, "🗑 Limpiar",  ax, 48, (s,e) => LimpiarLienzo()); ax += 80;
            ax = 88;
            AgregarBtnAccion(panelAcciones, "💾 Guardar",  ax, 18, (s,e) => Guardar()); ax += 80;
            ax = 88;
            AgregarBtnAccion(panelAcciones, "📂 Abrir",   ax, 48, (s,e) => Abrir());
        }

        private void AgregarEtiquetaSeccion(Panel panel, string texto, int x)
        {
            var lbl = new Label { Text = texto, AutoSize = false,
                Width = panel.Width > 0 ? panel.Width - 4 : 120,
                Height = 14, Location = new Point(x, 2),
                ForeColor = Color.FromArgb(100,100,100),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.Add(lbl);
        }

        private void AgregarBtnHerramienta(Herramienta h, string icono, string tooltip, ref int x, ref int y)
        {
            var btn = new Button { Text = icono, Size = new Size(40, 24), Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Symbol", 10f),
                ForeColor = Color.FromArgb(50,50,50), BackColor = Color.Transparent,
                Cursor = Cursors.Hand, TabStop = false };
            btn.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(btn, tooltip);
            btn.Click += (s, e) => { ActivarHerramienta(h); ActualizarBotonActivo(); };
            botonesHerramienta[h] = btn;
            panelHerramientas.Controls.Add(btn);
            x += 44;
        }

        private void ConstruirPaleta()
        {
            int cols = 14, cellW = 19, cellH = 17, offX = 6, offY1 = 18, offY2 = 37;
            for (int i = 0; i < ColorsPaleta.Length; i++)
            {
                int col = i % cols, row = i / cols;
                int x = offX + col * cellW, y = row == 0 ? offY1 : offY2;
                var color = ColorsPaleta[i];
                var btn = new Button { Size = new Size(cellW - 1, cellH - 1), Location = new Point(x, y),
                    FlatStyle = FlatStyle.Flat, BackColor = color, TabStop = false, Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(160,160,160);
                new ToolTip().SetToolTip(btn, $"R:{color.R} G:{color.G} B:{color.B}");
                btn.MouseDown += (s, e) => {
                    if (e.Button == MouseButtons.Left) { colorActual = color; btnColorActual.BackColor = color; }
                    else if (e.Button == MouseButtons.Right) { colorRellenoActual = color; btnColorRellenoActual.BackColor = color; }
                };
                panelColores.Controls.Add(btn);
            }
            var lbl = new Label { Text = "Clic izq: color línea  |  Clic der: color relleno",
                AutoSize = false, Width = 260, Height = 13, Location = new Point(offX, 57),
                ForeColor = Color.FromArgb(120,120,120), Font = new Font("Segoe UI", 7f) };
            panelColores.Controls.Add(lbl);
        }

        private Button CrearCuadroColor(int x, int y, Color color, string tip)
        {
            var btn = new Button { Size = new Size(22, 16), Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat, BackColor = color, TabStop = false, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(120,120,120);
            new ToolTip().SetToolTip(btn, tip);
            return btn;
        }

        private void AgregarBtnAccion(Panel panel, string texto, int x, int y, EventHandler handler)
        {
            var btn = new Button { Text = texto, Size = new Size(82, 24), Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(50,50,50), BackColor = Color.FromArgb(225,225,225),
                TabStop = false, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderColor = Color.FromArgb(180,180,180);
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += handler;
            panel.Controls.Add(btn);
        }

        private void PanelToolbar_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(200,200,200), 1))
            {
                e.Graphics.DrawLine(pen, 292, 6, 292, 70);
                e.Graphics.DrawLine(pen, 858, 6, 858, 70);
                e.Graphics.DrawLine(pen, 1022, 6, 1022, 70);
            }
        }

        private void ActualizarBotonActivo()
        {
            foreach (var kvp in botonesHerramienta)
            {
                bool activo = kvp.Key == herramientaActual;
                kvp.Value.BackColor = activo ? Color.FromArgb(180,215,243) : Color.Transparent;
                kvp.Value.FlatAppearance.BorderSize = activo ? 1 : 0;
                kvp.Value.FlatAppearance.BorderColor = Color.FromArgb(0,120,215);
            }
        }

        private void ElegirColor(ref Color campo, Button boton)
        {
            using (var dlg = new ColorDialog { Color = campo })
                if (dlg.ShowDialog() == DialogResult.OK) { campo = dlg.Color; boton.BackColor = dlg.Color; }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  LIENZO
        // ════════════════════════════════════════════════════════════════════════
        private void InicializarLienzo()
        {
            if (pbLienzo.Width <= 0 || pbLienzo.Height <= 0) return;
            mapaBits?.Dispose();
            bitmapComprometido?.Dispose();
            mapaBits = new Bitmap(pbLienzo.Width, pbLienzo.Height);
            using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
            bitmapComprometido = new Bitmap(mapaBits);
            pbLienzo.Image = mapaBits;
        }

        private Image ImagenLienzoActual
        {
            set
            {
                var anterior = pbLienzo.Image;
                pbLienzo.Image = value;
                if (anterior != null && anterior != mapaBits && anterior != value)
                    anterior.Dispose();
            }
        }

        private void ActivarHerramienta(Herramienta h)
        {
            if (escribiendoTexto) ConfirmarTexto();
            if (rectSeleccion != Rectangle.Empty && bitmapSeleccionado != null) ConfirmarSeleccion();
            if (modoEdicionActivo) ConfirmarEdicion();

            herramientaActual = h;
            estaDibujando = false;
            arrastrandoControlBezier = false;
            bezierDefiniendoLinea = false;
            indiceControlBezier = -1;
            puntosPoligono.Clear();
            puntosLapiz.Clear();
            puntosBorrador.Clear();
            curvaBezierSeleccionada = null;
            modoTextoActivo = (h == Herramienta.Texto);
            escribiendoTexto = false;
            textoActual = "";

            pbLienzo.Cursor = h == Herramienta.Texto ? Cursors.IBeam :
                              h == Herramienta.Seleccion ? Cursors.Default :
                              Cursors.Cross;

            MostrarLienzoBase();
            ActualizarStatusHerramienta();
        }

        private void ActualizarStatusHerramienta()
        {
            string nombre;
            if (modoEdicionActivo)
            {
                nombre = "Modo Edición — Rotar/Escalar con los controles. ✔ OK para confirmar.";
            }
            else
            {
                switch (herramientaActual)
                {
                    case Herramienta.Seleccion:  nombre = "Selección y movimiento (selecciona para rotar/escalar)"; break;
                    case Herramienta.Texto:      nombre = "Texto (A)"; break;
                    case Herramienta.Linea:      nombre = "Línea"; break;
                    case Herramienta.Rectangulo: nombre = "Rectángulo"; break;
                    case Herramienta.Circulo:    nombre = "Círculo/Elipse"; break;
                    case Herramienta.Poligono:   nombre = "Polígono (clic der o Enter para cerrar)"; break;
                    case Herramienta.Bezier:     nombre = "Curva Bézier (clic y arrastre para crear, luego ajusta manejadores)"; break;
                    case Herramienta.Relleno:    nombre = "Relleno de área"; break;
                    case Herramienta.Borrador:   nombre = "Borrador"; break;
                    case Herramienta.Lapiz:      nombre = "Lápiz (dibujo libre)"; break;
                    default:                     nombre = herramientaActual.ToString(); break;
                }
            }
            lblHerramientaActual.Text = "  " + nombre;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  EVENTOS DE RATÓN
        // ════════════════════════════════════════════════════════════════════════
        private void pbLienzo_MouseDown(object sender, MouseEventArgs e)
        {
            if (mapaBits == null) return;

            // Si estamos en modo edición y hacen clic fuera del panel, confirmar
            if (modoEdicionActivo && !panelEdicion.Bounds.Contains(e.Location))
            {
                ConfirmarEdicion();
                return;
            }
            if (modoEdicionActivo) return;

            // ── Texto ────────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Texto && e.Button == MouseButtons.Left)
            {
                if (escribiendoTexto) ConfirmarTexto();
                puntoTexto = e.Location;
                textoActual = "";
                escribiendoTexto = true;
                pbLienzo.Invalidate();
                return;
            }

            // ── Selección ────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Seleccion && e.Button == MouseButtons.Left)
            {
                if (rectSeleccion != Rectangle.Empty && rectSeleccion.Contains(e.Location) && bitmapSeleccionado != null)
                {
                    arrastandoSeleccion = true;
                    puntoInicioArrastre = e.Location;
                }
                else
                {
                    if (rectSeleccion != Rectangle.Empty && bitmapSeleccionado != null)
                        ConfirmarSeleccion();
                    dibujandoSeleccion = true;
                    puntoInicio = e.Location;
                    rectSeleccion = Rectangle.Empty;
                }
                return;
            }

            // ── Relleno ──────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Relleno && e.Button == MouseButtons.Left)
            {
                GuardarEstadoParaDeshacer();
                var relleno = new RellenoFijo(e.Location, colorActual);
                historialFiguras.Add(relleno);
                relleno.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                ImagenLienzoActual = mapaBits;
                return;
            }

            // ── Borrador ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Borrador && e.Button == MouseButtons.Left)
            {
                GuardarEstadoParaDeshacer();
                estaDibujando = true;
                puntosBorrador.Clear();
                puntosBorrador.Add(e.Location);
                BorrarEnPunto(e.Location);
                return;
            }

            // ── Lápiz ────────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Lapiz && e.Button == MouseButtons.Left)
            {
                GuardarEstadoParaDeshacer();
                estaDibujando = true;
                puntosLapiz.Clear();
                puntosLapiz.Add(e.Location);
                return;
            }

            // ── Bezier ───────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Bezier && e.Button == MouseButtons.Left)
            {
                if (curvaBezierSeleccionada != null)
                {
                    var curvaCercana = ObtenerCurvaBezierEnPunto(e.Location, out indiceControlBezier);
                    if (curvaCercana != null)
                    {
                        curvaBezierSeleccionada = curvaCercana;
                        arrastrandoControlBezier = true;
                        GuardarEstadoParaDeshacer();
                        return;
                    }
                    ConfirmarBezier();
                }
                bezierDefiniendoLinea = true;
                bezierP0 = e.Location;
                bezierP3Actual = e.Location;
                return;
            }

            // ── Polígono ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Poligono)
            {
                if (e.Button == MouseButtons.Right) { FinalizarPoligono(); return; }
                if (e.Button == MouseButtons.Left) { puntosPoligono.Add(e.Location); MostrarVistaPreviaPoligono(e.Location); return; }
            }

            // ── Figuras simples ──────────────────────────────────────────────────
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
            if (mapaBits == null) return;
            lblPosicion.Text = $"X: {e.X}  Y: {e.Y}";
            if (modoEdicionActivo) return;

            if (herramientaActual == Herramienta.Seleccion && dibujandoSeleccion)
            {
                rectSeleccion = NormalizarRect(puntoInicio, e.Location);
                using (var bmp = new Bitmap(mapaBits))
                using (var g = Graphics.FromImage(bmp))
                {
                    using (var p = new Pen(Color.FromArgb(0,120,215), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                        g.DrawRectangle(p, rectSeleccion);
                    ImagenLienzoActual = (Bitmap)bmp.Clone();
                }
                return;
            }

            if (herramientaActual == Herramienta.Seleccion && arrastandoSeleccion && bitmapSeleccionado != null)
            {
                int dx = e.X - puntoInicioArrastre.X, dy = e.Y - puntoInicioArrastre.Y;
                puntoInicioArrastre = e.Location;
                rectSeleccion = new Rectangle(rectSeleccion.X + dx, rectSeleccion.Y + dy, rectSeleccion.Width, rectSeleccion.Height);
                var bmp = new Bitmap(bitmapSinSeleccion);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(bitmapSeleccionado, rectSeleccion.Location);
                    using (var p = new Pen(Color.FromArgb(0,120,215), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                        g.DrawRectangle(p, rectSeleccion);
                }
                ImagenLienzoActual = bmp;
                return;
            }

            if (herramientaActual == Herramienta.Borrador && estaDibujando)
            {
                puntosBorrador.Add(e.Location);
                BorrarEnPunto(e.Location);
                return;
            }

            if (herramientaActual == Herramienta.Lapiz && estaDibujando)
            {
                if (puntosLapiz.Count > 0)
                {
                    var ultimo = puntosLapiz[puntosLapiz.Count - 1];
                    new Linea(ultimo, e.Location, colorActual, grosorActual).Dibujar(mapaBits);
                }
                puntosLapiz.Add(e.Location);
                ImagenLienzoActual = mapaBits;
                return;
            }

            if (arrastrandoControlBezier && curvaBezierSeleccionada != null)
            {
                curvaBezierSeleccionada.MoverPuntoControl(indiceControlBezier, e.Location);
                MostrarVistaPreviaBezierEdicion();
                return;
            }

            if (herramientaActual == Herramienta.Bezier && bezierDefiniendoLinea)
            {
                bezierP3Actual = e.Location;
                using (var tmp = new Bitmap(bitmapComprometido ?? mapaBits))
                {
                    new Linea(bezierP0, bezierP3Actual, colorActual, grosorActual).Dibujar(tmp);
                    PintarMarcadores(tmp, new[] { bezierP0, bezierP3Actual }, Color.CornflowerBlue);
                    ImagenLienzoActual = (Bitmap)tmp.Clone();
                }
                return;
            }

            if (herramientaActual == Herramienta.Poligono && puntosPoligono.Count > 0) { MostrarVistaPreviaPoligono(e.Location); return; }
            if (estaDibujando) MostrarVistaPreviaFigura(e.Location);
        }

        private void pbLienzo_MouseUp(object sender, MouseEventArgs e)
        {
            if (modoEdicionActivo) return;

            if (herramientaActual == Herramienta.Seleccion && dibujandoSeleccion)
            {
                dibujandoSeleccion = false;
                rectSeleccion = NormalizarRect(puntoInicio, e.Location);
                if (rectSeleccion.Width > 2 && rectSeleccion.Height > 2)
                    IniciarSeleccion();
                else
                    rectSeleccion = Rectangle.Empty;
                return;
            }

            if (herramientaActual == Herramienta.Seleccion && arrastandoSeleccion)
            {
                arrastandoSeleccion = false;
                // Entrar en modo edición para la selección movida
                if (bitmapSeleccionado != null)
                    MostrarBotonesEdicionSeleccion();
                return;
            }

            if (arrastrandoControlBezier)
            {
                arrastrandoControlBezier = false;
                indiceControlBezier = -1;
                using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                MostrarVistaPreviaBezierEdicion();
                return;
            }

            if (herramientaActual == Herramienta.Borrador)
            {
                estaDibujando = false;
                // ── BUG FIX: guardar el trazo de borrador como figura en el historial ──
                // Así cuando se redibuja el historial, el borrado queda incluido
                if (puntosBorrador.Count > 0)
                {
                    var trazoErase = new TrazoErase(new List<Point>(puntosBorrador), TamanoBorrador);
                    historialFiguras.Add(trazoErase);
                    puntosBorrador.Clear();
                    ActualizarBitmapComprometido();
                }
                return;
            }

            if (herramientaActual == Herramienta.Lapiz && estaDibujando)
            {
                estaDibujando = false;
                if (puntosLapiz.Count > 1)
                {
                    var trazo = new TrazoLapiz(new List<Point>(puntosLapiz), colorActual, grosorActual);
                    historialFiguras.Add(trazo);
                    ActualizarBitmapComprometido();
                    // Entrar en modo edición para el trazo recién dibujado
                    MostrarPanelEdicion(trazo);
                }
                puntosLapiz.Clear();
                if (!modoEdicionActivo) ImagenLienzoActual = mapaBits;
                return;
            }

            if (herramientaActual == Herramienta.Bezier && bezierDefiniendoLinea)
            {
                bezierDefiniendoLinea = false;
                int p1x = bezierP0.X + (bezierP3Actual.X - bezierP0.X) / 3;
                int p1y = bezierP0.Y + (bezierP3Actual.Y - bezierP0.Y) / 3;
                int p2x = bezierP0.X + 2 * (bezierP3Actual.X - bezierP0.X) / 3;
                int p2y = bezierP0.Y + 2 * (bezierP3Actual.Y - bezierP0.Y) / 3;
                var nuevaCurva = new CurvaBezier(bezierP0, new Point(p1x, p1y), new Point(p2x, p2y), bezierP3Actual, colorActual, grosorActual);
                GuardarEstadoParaDeshacer();
                historialFiguras.Add(nuevaCurva);
                nuevaCurva.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                curvaBezierSeleccionada = nuevaCurva;
                MostrarVistaPreviaBezierEdicion();
                return;
            }

            if (!estaDibujando) return;
            estaDibujando = false;
            var nuevaFigura = CrearFigura(puntoInicio, e.Location);
            if (nuevaFigura != null)
            {
                GuardarEstadoParaDeshacer();
                historialFiguras.Add(nuevaFigura);
                nuevaFigura.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                // Entrar en modo edición automáticamente
                MostrarPanelEdicion(nuevaFigura);
            }
            if (!modoEdicionActivo) ImagenLienzoActual = mapaBits;
        }

        // Muestra panel de edición para una selección ya confirmada/movida
        private void MostrarBotonesEdicionSeleccion()
        {
            // Buscar la figura que cae dentro del rectSeleccion
            Figura figSel = null;
            for (int i = historialFiguras.Count - 1; i >= 0; i--)
            {
                var b = historialFiguras[i].ObtenerBounds();
                if (rectSeleccion.IntersectsWith(b)) { figSel = historialFiguras[i]; break; }
            }
            if (figSel != null)
            {
                ConfirmarSeleccion();
                MostrarPanelEdicion(figSel);
            }
            else
            {
                // No hay figura lógica dentro — sólo confirmar selección bitmap
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  SELECCIÓN
        // ════════════════════════════════════════════════════════════════════════
        private void IniciarSeleccion()
        {
            GuardarEstadoParaDeshacer();
            bitmapSinSeleccion = new Bitmap(mapaBits);
            var clipped = new Rectangle(
                Math.Max(0, rectSeleccion.X), Math.Max(0, rectSeleccion.Y),
                Math.Min(rectSeleccion.Width,  mapaBits.Width  - rectSeleccion.X),
                Math.Min(rectSeleccion.Height, mapaBits.Height - rectSeleccion.Y));
            bitmapSeleccionado = new Bitmap(clipped.Width, clipped.Height);
            using (var g = Graphics.FromImage(bitmapSeleccionado))
                g.DrawImage(mapaBits, new Rectangle(0,0,clipped.Width,clipped.Height), clipped, GraphicsUnit.Pixel);
            using (var g = Graphics.FromImage(mapaBits)) g.FillRectangle(Brushes.White, clipped);
            bitmapSinSeleccion = new Bitmap(mapaBits);
            var bmp = new Bitmap(mapaBits);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(bitmapSeleccionado, rectSeleccion.Location);
                using (var p = new Pen(Color.FromArgb(0,120,215), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                    g.DrawRectangle(p, rectSeleccion);
            }
            ImagenLienzoActual = bmp;
        }

        private void ConfirmarSeleccion()
        {
            if (bitmapSeleccionado == null) return;
            using (var g = Graphics.FromImage(mapaBits)) g.DrawImage(bitmapSeleccionado, rectSeleccion.Location);
            bitmapSeleccionado?.Dispose(); bitmapSeleccionado = null;
            bitmapSinSeleccion?.Dispose(); bitmapSinSeleccion = null;
            rectSeleccion = Rectangle.Empty;
            ActualizarBitmapComprometido();
            ImagenLienzoActual = mapaBits;
        }

        private static Rectangle NormalizarRect(Point a, Point b)
            => new Rectangle(Math.Min(a.X,b.X), Math.Min(a.Y,b.Y), Math.Abs(a.X-b.X), Math.Abs(a.Y-b.Y));

        // ════════════════════════════════════════════════════════════════════════
        //  TEXTO
        // ════════════════════════════════════════════════════════════════════════
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (escribiendoTexto)
            {
                if (e.KeyChar == '\r') { ConfirmarTexto(); e.Handled = true; return; }
                if (e.KeyChar == '\b') { if (textoActual.Length > 0) textoActual = textoActual.Substring(0, textoActual.Length - 1); }
                else if (e.KeyChar >= 32) textoActual += e.KeyChar;
                DibujarTextoCursor();
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        private void DibujarTextoCursor()
        {
            using (var bmp = new Bitmap(mapaBits))
            using (var g = Graphics.FromImage(bmp))
            {
                var font = new Font("Segoe UI", 16f);
                g.DrawString(textoActual + "|", font, new SolidBrush(colorActual), puntoTexto);
                ImagenLienzoActual = (Bitmap)bmp.Clone();
            }
        }

        private void ConfirmarTexto()
        {
            if (!escribiendoTexto || textoActual.Length == 0) { escribiendoTexto = false; textoActual = ""; return; }
            GuardarEstadoParaDeshacer();
            using (var g = Graphics.FromImage(mapaBits))
                g.DrawString(textoActual, new Font("Segoe UI", 16f), new SolidBrush(colorActual), puntoTexto);
            escribiendoTexto = false; textoActual = "";
            ImagenLienzoActual = mapaBits;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  FIGURAS
        // ════════════════════════════════════════════════════════════════════════
        private Figura CrearFigura(Point inicio, Point fin)
        {
            switch (herramientaActual)
            {
                case Herramienta.Linea: return new Linea(inicio, fin, colorActual, grosorActual);
                case Herramienta.Rectangulo: return new Rectangulo(inicio, fin, colorActual, grosorActual)
                    { EstaRelleno = chkRelleno.Checked, ColorRelleno = colorRellenoActual };
                case Herramienta.Circulo: return new Circulo(inicio, fin, colorActual, grosorActual)
                    { EstaRelleno = chkRelleno.Checked, ColorRelleno = colorRellenoActual };
                default: return null;
            }
        }

        private CurvaBezier ObtenerCurvaBezierEnPunto(Point punto, out int indice)
        {
            for (int i = historialFiguras.Count - 1; i >= 0; i--)
            {
                var curva = historialFiguras[i] as CurvaBezier;
                if (curva != null && curva.ContienePuntoDeControl(punto, ToleranciaControl, out indice))
                    return curva;
            }
            indice = -1; return null;
        }

        private void FinalizarPoligono()
        {
            if (puntosPoligono.Count < 3) return;
            GuardarEstadoParaDeshacer();
            var poly = new Poligono(puntosPoligono, colorActual, grosorActual)
                { EstaRelleno = chkRelleno.Checked, ColorRelleno = colorRellenoActual };
            historialFiguras.Add(poly);
            poly.Dibujar(mapaBits);
            puntosPoligono.Clear();
            ActualizarBitmapComprometido();
            // Modo edición tras dibujar polígono
            MostrarPanelEdicion(poly);
        }

        // ════════════════════════════════════════════════════════════════════════
        //  VISTAS PREVIAS
        // ════════════════════════════════════════════════════════════════════════
        private void MostrarLienzoBase()
        {
            if (mapaBits == null) return;
            using (var tmp = new Bitmap(mapaBits.Width, mapaBits.Height))
            {
                using (var g = Graphics.FromImage(tmp)) g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(tmp);
                using (var g = Graphics.FromImage(mapaBits)) g.DrawImage(tmp, 0, 0);
            }
            ActualizarBitmapComprometido();
            ImagenLienzoActual = mapaBits;
        }

        private void ActualizarBitmapComprometido()
        {
            bitmapComprometido?.Dispose();
            bitmapComprometido = new Bitmap(mapaBits);
        }

        private void ConfirmarBezier()
        {
            curvaBezierSeleccionada = null;
            indiceControlBezier = -1;
            arrastrandoControlBezier = false;
        }

        private void MostrarVistaPreviaBezierEdicion()
        {
            if (curvaBezierSeleccionada == null) { ImagenLienzoActual = mapaBits; return; }
            using (var tmp = new Bitmap(mapaBits.Width, mapaBits.Height))
            {
                using (var g = Graphics.FromImage(tmp)) g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(tmp);
                var cp = new[] { curvaBezierSeleccionada.P0, curvaBezierSeleccionada.P1,
                                 curvaBezierSeleccionada.P2, curvaBezierSeleccionada.P3 };
                DibujarPolilineaControl(tmp, new[] { cp[0], cp[1] }, Color.DarkGray);
                DibujarPolilineaControl(tmp, new[] { cp[3], cp[2] }, Color.DarkGray);
                PintarMarcadores(tmp, cp, Color.Gold);
                ImagenLienzoActual = (Bitmap)tmp.Clone();
            }
        }

        private void MostrarVistaPreviaFigura(Point fin)
        {
            using (var tmp = new Bitmap(bitmapComprometido ?? mapaBits))
            {
                CrearFigura(puntoInicio, fin)?.Dibujar(tmp);
                ImagenLienzoActual = (Bitmap)tmp.Clone();
            }
        }

        private void MostrarVistaPreviaPoligono(Point cursor)
        {
            using (var tmp = new Bitmap(bitmapComprometido ?? mapaBits))
            {
                DibujarPoligonoTemporal(tmp, cursor);
                ImagenLienzoActual = (Bitmap)tmp.Clone();
            }
        }

        private void DibujarPoligonoTemporal(Bitmap lienzo, Point cursor)
        {
            if (puntosPoligono.Count == 0) return;
            var verts = new List<Point>(puntosPoligono);
            if (cursor != Point.Empty) verts.Add(cursor);
            if (chkRelleno.Checked && verts.Count >= 3)
                AlgoritmosRelleno.ScanlineFill(lienzo, verts, colorRellenoActual);
            for (int i = 0; i < verts.Count - 1; i++)
                new Linea(verts[i], verts[i+1], colorActual, grosorActual).Dibujar(lienzo);
            PintarMarcadores(lienzo, puntosPoligono, Color.OrangeRed);
        }

        private void DibujarPolilineaControl(Bitmap lienzo, IEnumerable<Point> pts, Color color)
        {
            var lst = new List<Point>(pts);
            for (int i = 0; i < lst.Count - 1; i++)
                new Linea(lst[i], lst[i+1], color, 1).Dibujar(lienzo);
        }

        private void PintarMarcadores(Bitmap lienzo, IEnumerable<Point> pts, Color color)
        {
            using (var g = Graphics.FromImage(lienzo))
            using (var br = new SolidBrush(color))
            using (var p = new Pen(Color.Black, 1))
            {
                foreach (var pt in pts)
                {
                    var r = new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
                    g.FillEllipse(br, r); g.DrawEllipse(p, r);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  BORRADOR ─ helper
        // ════════════════════════════════════════════════════════════════════════
        private void BorrarEnPunto(Point p)
        {
            using (var g = Graphics.FromImage(mapaBits))
            using (var br = new SolidBrush(Color.White))
            {
                int m = TamanoBorrador / 2;
                g.FillRectangle(br, p.X - m, p.Y - m, TamanoBorrador, TamanoBorrador);
            }
            ImagenLienzoActual = mapaBits;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  DESHACER / LIMPIAR / GUARDAR / ABRIR
        // ════════════════════════════════════════════════════════════════════════
        private void GuardarEstadoParaDeshacer()
        {
            if (pilaDeshacer.Count >= LimiteDeshacer)
            {
                var arr = pilaDeshacer.ToArray();
                pilaDeshacer.Clear();
                for (int i = arr.Length - 2; i >= 0; i--) pilaDeshacer.Push(arr[i]);
            }
            pilaDeshacer.Push(new List<Figura>(historialFiguras));
        }

        private void Deshacer()
        {
            if (pilaDeshacer.Count == 0) return;
            if (modoEdicionActivo) ConfirmarEdicion();
            if (escribiendoTexto) { escribiendoTexto = false; textoActual = ""; }
            ConfirmarSeleccion();
            historialFiguras.Clear();
            historialFiguras.AddRange(pilaDeshacer.Pop());
            puntosPoligono.Clear(); puntosLapiz.Clear(); puntosBorrador.Clear();
            curvaBezierSeleccionada = null; indiceControlBezier = -1;
            arrastrandoControlBezier = false; bezierDefiniendoLinea = false;
            estaDibujando = false;
            MostrarLienzoBase();
        }

        private void LimpiarLienzo()
        {
            if (DialogBox.Show(this, "¿Limpiar el lienzo?", "Confirmar") != DialogResult.OK) return;
            if (modoEdicionActivo) ConfirmarEdicion();
            GuardarEstadoParaDeshacer();
            historialFiguras.Clear(); puntosPoligono.Clear(); puntosLapiz.Clear(); puntosBorrador.Clear();
            curvaBezierSeleccionada = null; indiceControlBezier = -1;
            arrastrandoControlBezier = false; bezierDefiniendoLinea = false;
            estaDibujando = false; escribiendoTexto = false; textoActual = "";
            using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
            ActualizarBitmapComprometido();
            ImagenLienzoActual = mapaBits;
        }

        private void Guardar()
        {
            ConfirmarTexto(); ConfirmarSeleccion();
            if (modoEdicionActivo) ConfirmarEdicion();
            using (var sfd = new SaveFileDialog { Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var fmt = sfd.FilterIndex == 2 ? ImageFormat.Jpeg :
                              sfd.FilterIndex == 3 ? ImageFormat.Bmp : ImageFormat.Png;
                    mapaBits.Save(sfd.FileName, fmt);
                }
            }
        }

        private void Abrir()
        {
            using (var ofd = new OpenFileDialog { Filter = "Imágenes|*.png;*.jpg;*.bmp;*.gif" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (modoEdicionActivo) ConfirmarEdicion();
                    GuardarEstadoParaDeshacer();
                    historialFiguras.Clear();
                    using (var img = Image.FromFile(ofd.FileName))
                    using (var g = Graphics.FromImage(mapaBits))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(img, 0, 0, mapaBits.Width, mapaBits.Height);
                    }
                    ImagenLienzoActual = mapaBits;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  TECLADO
        // ════════════════════════════════════════════════════════════════════════
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (escribiendoTexto) return;
            if (e.Control && e.KeyCode == Keys.Z) { Deshacer(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter && herramientaActual == Herramienta.Poligono) { FinalizarPoligono(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter && modoEdicionActivo) { ConfirmarEdicion(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape)
            {
                if (modoEdicionActivo) { ConfirmarEdicion(); e.Handled = true; return; }
                puntosPoligono.Clear(); puntosLapiz.Clear(); puntosBorrador.Clear();
                bezierDefiniendoLinea = false;
                arrastrandoControlBezier = false; indiceControlBezier = -1;
                curvaBezierSeleccionada = null; estaDibujando = false;
                if (rectSeleccion != Rectangle.Empty && bitmapSeleccionado != null) ConfirmarSeleccion();
                MostrarLienzoBase(); e.Handled = true;
            }
        }

        private void nudGrosor_ValueChanged(object sender, EventArgs e) => grosorActual = (int)nudGrosor.Value;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            mapaBits?.Dispose();
            bitmapComprometido?.Dispose();
            base.OnFormClosed(e);
        }
    }

    // ── CirculoClone: permite clonar un círculo con radio explícito ──────────
    internal class CirculoClone : Circulo
    {
        public CirculoClone(Point centro, int radio, Color colorLinea, int grosor)
            : base(centro, new Point(centro.X + radio, centro.Y), colorLinea, grosor)
        {
            Centro = centro;
            Radio = radio;
        }
    }

    // ── TrazoErase: borrador como figura para que el historial lo recuerde ───
    internal class TrazoErase : Figura
    {
        private readonly List<Point> _puntos;
        private readonly int _tamano;

        public TrazoErase(List<Point> puntos, int tamano) : base(Color.White, 1)
        {
            _puntos = new List<Point>(puntos);
            _tamano = tamano;
        }

        public override Rectangle ObtenerBounds() => Rectangle.Empty;

        public override void Dibujar(Bitmap lienzo)
        {
            using (var g = Graphics.FromImage(lienzo))
            using (var br = new SolidBrush(Color.White))
            {
                int m = _tamano / 2;
                foreach (var p in _puntos)
                    g.FillRectangle(br, p.X - m, p.Y - m, _tamano, _tamano);
            }
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

    // ── Helper de confirmación simple ────────────────────────────────────────
    internal static class DialogBox
    {
        public static DialogResult Show(Form owner, string msg, string title)
            => MessageBox.Show(owner, msg, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
    }
}
