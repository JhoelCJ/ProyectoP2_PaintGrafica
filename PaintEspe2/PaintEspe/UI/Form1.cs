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
        private Bitmap bitmapComprometido;    // snapshot del lienzo tras cada figura confirmada
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

        // ─── Bezier (flujo clic y arrastre) ─────────────────────────────────────
        private CurvaBezier curvaBezierSeleccionada;    // curva en modo edicion
        private int indiceControlBezier = -1;
        private bool arrastrandoControlBezier;          // arrastrando un manejador
        private bool bezierDefiniendoLinea;             // fase 1: MouseDown sostenido
        private Point bezierP0;                         // P0 fijado en MouseDown
        private Point bezierP3Actual;                   // P3 dinamico al arrastrar
        private const int ToleranciaControl = 8;

        // ─── Lapiz ──────────────────────────────────────────────────────────────
        private readonly List<Point> puntosLapiz = new List<Point>();

        // ─── Selección y movimiento ─────────────────────────────────────────────
        private Rectangle rectSeleccion = Rectangle.Empty;
        private bool dibujandoSeleccion;
        private Bitmap bitmapSeleccionado;
        private Point puntoInicioArrastre;
        private bool arrastandoSeleccion;
        private Bitmap bitmapSinSeleccion;   // lienzo antes de cortar la selección

        // ─── Texto ──────────────────────────────────────────────────────────────
        private bool modoTextoActivo;
        private Point puntoTexto;
        private string textoActual = "";
        private bool escribiendoTexto;

        // ─── Botones de herramientas (para resaltar el activo) ─────────────────
        private readonly Dictionary<Herramienta, Button> botonesHerramienta = new Dictionary<Herramienta, Button>();
        private Button btnColorActual;
        private Button btnColorRellenoActual;

        // ─── Colores de la paleta (2 filas × 14 cols como Paint) ───────────────
        private static readonly Color[] ColorsPaleta =
        {
            // Fila 1: colores base
            Color.Black, Color.FromArgb(127,127,127), Color.FromArgb(136,0,21),
            Color.Red, Color.FromArgb(255,127,39), Color.Yellow,
            Color.FromArgb(0,128,0), Color.FromArgb(0,200,100),
            Color.FromArgb(0,162,232), Color.FromArgb(0,0,255),
            Color.FromArgb(63,72,204), Color.FromArgb(112,48,160),
            Color.White, Color.FromArgb(195,195,195),
            // Fila 2: tonos claros / pasteles
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
        }

        // ════════════════════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN DE LA BARRA DE HERRAMIENTAS
        // ════════════════════════════════════════════════════════════════════════
        private void ConstruirToolbar()
        {
            panelToolbar.Paint += PanelToolbar_Paint;

            // ── Etiquetas de sección ────────────────────────────────────────────
            AgregarEtiquetaSeccion(panelHerramientas, "Herramientas", 0);
            AgregarEtiquetaSeccion(panelColores, "Colores", 0);
            AgregarEtiquetaSeccion(panelGrosor, "Grosor", 0);
            AgregarEtiquetaSeccion(panelAcciones, "Acciones", 0);

            // ── Botones de herramientas ─────────────────────────────────────────
            int bx = 6, by = 18;
            AgregarBtnHerramienta(Herramienta.Seleccion, "⊹", "Seleccionar y mover", ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Texto,     "A", "Escribir texto",        ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Relleno,   "◈", "Rellenar área",         ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Borrador,  "⌫", "Borrador",              ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Lapiz,     "✏", "Lápiz (dibujo libre)",  ref bx, ref by);

            // segunda fila
            bx = 6; by = 48;
            AgregarBtnHerramienta(Herramienta.Linea,      "╱",  "Línea",     ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Rectangulo, "▭",  "Rectángulo",ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Circulo,    "◯",  "Círculo",   ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Poligono,   "△",  "Polígono",  ref bx, ref by);
            AgregarBtnHerramienta(Herramienta.Bezier,     "∿",  "Curva Bézier", ref bx, ref by);

            // ── Paleta de colores ───────────────────────────────────────────────
            ConstruirPaleta();

            // ── Selección actual (2 cuadros color activo / relleno) ─────────────
            var lblC = new Label { Text = "Línea", AutoSize = false,
                Width = 36, Height = 13, Location = new Point(4, 20),
                ForeColor = Color.FromArgb(80,80,80), Font = new Font("Segoe UI", 7f),
                TextAlign = ContentAlignment.MiddleLeft };
            panelGrosor.Controls.Add(lblC);

            btnColorActual = CrearCuadroColor(8, 32, colorActual, "Color de línea");
            btnColorActual.Click += (s,e) => ElegirColor(ref colorActual, btnColorActual);
            panelGrosor.Controls.Add(btnColorActual);

            var lblR = new Label { Text = "Relleno", AutoSize = false,
                Width = 46, Height = 13, Location = new Point(4, 5),
                ForeColor = Color.FromArgb(80,80,80), Font = new Font("Segoe UI", 7f),
                TextAlign = ContentAlignment.MiddleLeft };

            // reubicamos nudGrosor y chkRelleno
            nudGrosor.Location = new Point(62, 28);
            var lblGrosor = new Label { Text = "Grosor:", AutoSize = true,
                Location = new Point(4, 31), ForeColor = Color.FromArgb(60,60,60),
                Font = new Font("Segoe UI", 8f) };
            panelGrosor.Controls.Add(lblGrosor);

            btnColorRellenoActual = CrearCuadroColor(8, 52, colorRellenoActual, "Color de relleno");
            btnColorRellenoActual.Click += (s,e) => ElegirColor(ref colorRellenoActual, btnColorRellenoActual);
            panelGrosor.Controls.Add(btnColorRellenoActual);

            // ── Acciones ────────────────────────────────────────────────────────
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
            var lbl = new Label
            {
                Text = texto,
                AutoSize = false, Width = panel.Width > 0 ? panel.Width - 4 : 120,
                Height = 14, Location = new Point(x, 2),
                ForeColor = Color.FromArgb(100,100,100),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(lbl);
        }

        private void AgregarBtnHerramienta(Herramienta h, string icono, string tooltip, ref int x, ref int y)
        {
            var btn = new Button
            {
                Text = icono, Size = new Size(40, 24),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Symbol", 10f),
                ForeColor = Color.FromArgb(50,50,50),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TabStop = false
            };
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
                int col = i % cols;
                int row = i / cols;
                int x = offX + col * cellW;
                int y = row == 0 ? offY1 : offY2;

                var color = ColorsPaleta[i];
                var btn = new Button
                {
                    Size = new Size(cellW - 1, cellH - 1),
                    Location = new Point(x, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = color,
                    TabStop = false,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(160,160,160);
                new ToolTip().SetToolTip(btn, $"R:{color.R} G:{color.G} B:{color.B}");

                btn.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        colorActual = color;
                        btnColorActual.BackColor = color;
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        colorRellenoActual = color;
                        btnColorRellenoActual.BackColor = color;
                    }
                };
                panelColores.Controls.Add(btn);
            }

            // etiqueta ayuda
            var lbl = new Label
            {
                Text = "Clic izq: color línea  |  Clic der: color relleno",
                AutoSize = false, Width = 260, Height = 13,
                Location = new Point(offX, 57),
                ForeColor = Color.FromArgb(120,120,120),
                Font = new Font("Segoe UI", 7f)
            };
            panelColores.Controls.Add(lbl);
        }

        private Button CrearCuadroColor(int x, int y, Color color, string tip)
        {
            var btn = new Button
            {
                Size = new Size(22, 16), Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat, BackColor = color,
                TabStop = false, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(120,120,120);
            new ToolTip().SetToolTip(btn, tip);
            return btn;
        }

        private void AgregarBtnAccion(Panel panel, string texto, int x, int y, EventHandler handler)
        {
            var btn = new Button
            {
                Text = texto, Size = new Size(82, 24),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(50,50,50),
                BackColor = Color.FromArgb(225,225,225),
                TabStop = false, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(180,180,180);
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += handler;
            panel.Controls.Add(btn);
        }

        private void PanelToolbar_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Líneas separadoras entre secciones
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
                kvp.Value.BackColor = activo
                    ? Color.FromArgb(180, 215, 243)
                    : Color.Transparent;
                kvp.Value.FlatAppearance.BorderSize = activo ? 1 : 0;
                kvp.Value.FlatAppearance.BorderColor = Color.FromArgb(0,120,215);
            }
        }

        private void ElegirColor(ref Color campo, Button boton)
        {
            using (var dlg = new ColorDialog { Color = campo })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    campo = dlg.Color;
                    boton.BackColor = dlg.Color;
                }
            }
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
            // Si había texto en edición, confirmarlo
            if (escribiendoTexto) ConfirmarTexto();

            // Si había selección activa, confirmarla
            if (rectSeleccion != Rectangle.Empty && bitmapSeleccionado != null)
                ConfirmarSeleccion();

            herramientaActual = h;
            estaDibujando = false;
            arrastrandoControlBezier = false;
            bezierDefiniendoLinea = false;
            indiceControlBezier = -1;
            puntosPoligono.Clear();
            puntosLapiz.Clear();
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
            string nombre = herramientaActual.ToString();
            switch (herramientaActual)
            {
                case Herramienta.Seleccion:  nombre = "Selección y movimiento"; break;
                case Herramienta.Texto:      nombre = "Texto (A)"; break;
                case Herramienta.Linea:      nombre = "Línea"; break;
                case Herramienta.Rectangulo: nombre = "Rectángulo"; break;
                case Herramienta.Circulo:    nombre = "Círculo/Elipse"; break;
                case Herramienta.Poligono:   nombre = "Polígono (clic der o Enter para cerrar)"; break;
                case Herramienta.Bezier:     nombre = "Curva Bézier (clic y arrastre para crear, luego ajusta manejadores)"; break;
                case Herramienta.Relleno:    nombre = "Relleno de área"; break;
                case Herramienta.Borrador:   nombre = "Borrador"; break;
                case Herramienta.Lapiz:      nombre = "Lápiz (dibujo libre)"; break;
            }
            lblHerramientaActual.Text = "  " + nombre;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  EVENTOS DE RATÓN
        // ════════════════════════════════════════════════════════════════════════
        private void pbLienzo_MouseDown(object sender, MouseEventArgs e)
        {
            if (mapaBits == null) return;

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
                relleno.Dibujar(mapaBits);           // aplicar sobre estado actual
                ActualizarBitmapComprometido();       // snapshot post-relleno
                ImagenLienzoActual = mapaBits;
                return;
            }

            // ── Borrador ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Borrador && e.Button == MouseButtons.Left)
            {
                GuardarEstadoParaDeshacer();
                estaDibujando = true;
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
                // Si hay curva en modo edición: intentar agarrar un manejador
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
                    // Clic fuera de manejadores: confirmar y empezar nueva curva
                    ConfirmarBezier();
                }

                // Fase 1: fijar P0, comenzar arrastre para P3
                bezierDefiniendoLinea = true;
                bezierP0 = e.Location;
                bezierP3Actual = e.Location;
                return;
            }

            // ── Polígono ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Poligono)
            {
                if (e.Button == MouseButtons.Right) { FinalizarPoligono(); return; }
                if (e.Button == MouseButtons.Left)
                {
                    puntosPoligono.Add(e.Location);
                    MostrarVistaPreviaPoligono(e.Location);
                    return;
                }
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

            // ── Dibujar rectángulo de selección ──────────────────────────────────
            if (herramientaActual == Herramienta.Seleccion && dibujandoSeleccion)
            {
                rectSeleccion = NormalizarRect(puntoInicio, e.Location);
                using (var bmp = new Bitmap(mapaBits))
                using (var g = Graphics.FromImage(bmp))
                {
                    using (var p = new Pen(Color.FromArgb(0, 120, 215), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                        g.DrawRectangle(p, rectSeleccion);
                    ImagenLienzoActual = (Bitmap)bmp.Clone();
                }
                return;
            }

            // ── Arrastrar selección ──────────────────────────────────────────────
            if (herramientaActual == Herramienta.Seleccion && arrastandoSeleccion && bitmapSeleccionado != null)
            {
                int dx = e.X - puntoInicioArrastre.X;
                int dy = e.Y - puntoInicioArrastre.Y;
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

            // ── Borrador ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Borrador && estaDibujando) { BorrarEnPunto(e.Location); return; }

            // ── Lápiz ────────────────────────────────────────────────────────────
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

            // ── Bezier: arrastrando manejador de edición ──────────────────────────
            if (arrastrandoControlBezier && curvaBezierSeleccionada != null)
            {
                curvaBezierSeleccionada.MoverPuntoControl(indiceControlBezier, e.Location);
                MostrarVistaPreviaBezierEdicion();
                return;
            }

            // ── Bezier: fase 1 - definiendo línea inicial ─────────────────────────
            if (herramientaActual == Herramienta.Bezier && bezierDefiniendoLinea)
            {
                bezierP3Actual = e.Location;
                // Mostrar línea recta entre P0 y P3
                using (var tmp = new Bitmap(bitmapComprometido ?? mapaBits))
                {
                    new Linea(bezierP0, bezierP3Actual, colorActual, grosorActual).Dibujar(tmp);
                    // Marcadores P0 y P3
                    PintarMarcadores(tmp, new[] { bezierP0, bezierP3Actual }, Color.CornflowerBlue);
                    ImagenLienzoActual = (Bitmap)tmp.Clone();
                }
                return;
            }

            // ── Polígono ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Poligono && puntosPoligono.Count > 0) { MostrarVistaPreviaPoligono(e.Location); return; }

            // ── Figuras simples ──────────────────────────────────────────────────
            if (estaDibujando) MostrarVistaPreviaFigura(e.Location);
        }

        private void pbLienzo_MouseUp(object sender, MouseEventArgs e)
        {
            // ── Fin dibujo selección ─────────────────────────────────────────────
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
                return;
            }

            // ── Fin arrastre manejador Bezier ─────────────────────────────────────
            if (arrastrandoControlBezier)
            {
                arrastrandoControlBezier = false;
                indiceControlBezier = -1;
                // Redibujar todas las figuras (la curva editada ya fue modificada en su objeto)
                using (var g = Graphics.FromImage(mapaBits))
                    g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                MostrarVistaPreviaBezierEdicion();
                return;
            }

            // ── Fin borrador ──────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Borrador)
            {
                estaDibujando = false;
                ActualizarBitmapComprometido();
                return;
            }

            // ── Fin lápiz ─────────────────────────────────────────────────────────
            if (herramientaActual == Herramienta.Lapiz && estaDibujando)
            {
                estaDibujando = false;
                // Guardar trazo como figura Lapiz (lista de segmentos ya dibujados en mapaBits)
                if (puntosLapiz.Count > 1)
                {
                    var trazo = new TrazoLapiz(new List<Point>(puntosLapiz), colorActual, grosorActual);
                    historialFiguras.Add(trazo);
                    // El trazo ya está dibujado en mapaBits directamente
                    ActualizarBitmapComprometido();
                }
                puntosLapiz.Clear();
                ImagenLienzoActual = mapaBits;
                return;
            }

            // ── Fin Bezier fase 1: soltar → entrar a modo edición ────────────────
            if (herramientaActual == Herramienta.Bezier && bezierDefiniendoLinea)
            {
                bezierDefiniendoLinea = false;
                // Calcular P1 y P2 por defecto (1/3 y 2/3 de la línea)
                int p1x = bezierP0.X + (bezierP3Actual.X - bezierP0.X) / 3;
                int p1y = bezierP0.Y + (bezierP3Actual.Y - bezierP0.Y) / 3;
                int p2x = bezierP0.X + 2 * (bezierP3Actual.X - bezierP0.X) / 3;
                int p2y = bezierP0.Y + 2 * (bezierP3Actual.Y - bezierP0.Y) / 3;
                var nuevaCurva = new CurvaBezier(
                    bezierP0,
                    new Point(p1x, p1y),
                    new Point(p2x, p2y),
                    bezierP3Actual,
                    colorActual, grosorActual);
                GuardarEstadoParaDeshacer();
                historialFiguras.Add(nuevaCurva);
                nuevaCurva.Dibujar(mapaBits);
                ActualizarBitmapComprometido();
                curvaBezierSeleccionada = nuevaCurva;
                // Entrar a modo edición: mostrar manejadores
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
            }
            ImagenLienzoActual = mapaBits;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  SELECCIÓN
        // ════════════════════════════════════════════════════════════════════════
        private void IniciarSeleccion()
        {
            GuardarEstadoParaDeshacer();

            // Guardar copia del lienzo antes de "levantar" la zona
            bitmapSinSeleccion = new Bitmap(mapaBits);

            // Recortar región seleccionada
            var clipped = new Rectangle(
                Math.Max(0, rectSeleccion.X), Math.Max(0, rectSeleccion.Y),
                Math.Min(rectSeleccion.Width,  mapaBits.Width  - rectSeleccion.X),
                Math.Min(rectSeleccion.Height, mapaBits.Height - rectSeleccion.Y));

            bitmapSeleccionado = new Bitmap(clipped.Width, clipped.Height);
            using (var g = Graphics.FromImage(bitmapSeleccionado))
                g.DrawImage(mapaBits, new Rectangle(0,0,clipped.Width,clipped.Height), clipped, GraphicsUnit.Pixel);

            // Borrar zona del lienzo original (dejar blanco)
            using (var g = Graphics.FromImage(mapaBits))
                g.FillRectangle(Brushes.White, clipped);

            bitmapSinSeleccion = new Bitmap(mapaBits);

            // Mostrar
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
            using (var g = Graphics.FromImage(mapaBits))
                g.DrawImage(bitmapSeleccionado, rectSeleccion.Location);

            bitmapSeleccionado?.Dispose();
            bitmapSeleccionado = null;
            bitmapSinSeleccion?.Dispose();
            bitmapSinSeleccion = null;
            rectSeleccion = Rectangle.Empty;
            ImagenLienzoActual = mapaBits;
        }

        private static Rectangle NormalizarRect(Point a, Point b)
        {
            return new Rectangle(
                Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
                Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        // ════════════════════════════════════════════════════════════════════════
        //  TEXTO
        // ════════════════════════════════════════════════════════════════════════
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (escribiendoTexto)
            {
                if (e.KeyChar == '\r') { ConfirmarTexto(); e.Handled = true; return; }
                if (e.KeyChar == '\b')
                {
                    if (textoActual.Length > 0) textoActual = textoActual.Substring(0, textoActual.Length - 1);
                }
                else if (e.KeyChar >= 32)
                {
                    textoActual += e.KeyChar;
                }
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
                var textoPlusCursor = textoActual + "|";
                g.DrawString(textoPlusCursor, font, new SolidBrush(colorActual), puntoTexto);
                ImagenLienzoActual = (Bitmap)bmp.Clone();
            }
        }

        private void ConfirmarTexto()
        {
            if (!escribiendoTexto || textoActual.Length == 0) { escribiendoTexto = false; textoActual = ""; return; }
            GuardarEstadoParaDeshacer();
            using (var g = Graphics.FromImage(mapaBits))
            {
                var font = new Font("Segoe UI", 16f);
                g.DrawString(textoActual, font, new SolidBrush(colorActual), puntoTexto);
            }
            escribiendoTexto = false;
            textoActual = "";
            ImagenLienzoActual = mapaBits;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  FIGURAS
        // ════════════════════════════════════════════════════════════════════════
        private Figura CrearFigura(Point inicio, Point fin)
        {
            switch (herramientaActual)
            {
                case Herramienta.Linea:
                    return new Linea(inicio, fin, colorActual, grosorActual);
                case Herramienta.Rectangulo:
                    return new Rectangulo(inicio, fin, colorActual, grosorActual)
                        { EstaRelleno = chkRelleno.Checked, ColorRelleno = colorRellenoActual };
                case Herramienta.Circulo:
                    return new Circulo(inicio, fin, colorActual, grosorActual)
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
            ImagenLienzoActual = mapaBits;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  VISTAS PREVIAS
        // ════════════════════════════════════════════════════════════════════════
        private void MostrarLienzoBase()
        {
            if (mapaBits == null) return;
            // Redibujar todo desde cero en un bitmap temporal, luego copiar a mapaBits
            using (var tmp = new Bitmap(mapaBits.Width, mapaBits.Height))
            {
                using (var g = Graphics.FromImage(tmp)) g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(tmp);
                using (var g = Graphics.FromImage(mapaBits))
                    g.DrawImage(tmp, 0, 0);
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
            // Redibujar todo (la curva editada ya tiene sus puntos actualizados)
            using (var tmp = new Bitmap(mapaBits.Width, mapaBits.Height))
            {
                using (var g = Graphics.FromImage(tmp)) g.Clear(Color.White);
                foreach (var fig in historialFiguras) fig.Dibujar(tmp);
                var cp = new[] { curvaBezierSeleccionada.P0, curvaBezierSeleccionada.P1,
                                 curvaBezierSeleccionada.P2, curvaBezierSeleccionada.P3 };
                // Líneas de control
                DibujarPolilineaControl(tmp, new[] { cp[0], cp[1] }, Color.DarkGray);
                DibujarPolilineaControl(tmp, new[] { cp[3], cp[2] }, Color.DarkGray);
                // Manejadores
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
            if (escribiendoTexto) { escribiendoTexto = false; textoActual = ""; }
            ConfirmarSeleccion();
            historialFiguras.Clear();
            historialFiguras.AddRange(pilaDeshacer.Pop());
            puntosPoligono.Clear(); puntosLapiz.Clear();
            curvaBezierSeleccionada = null; indiceControlBezier = -1;
            arrastrandoControlBezier = false; bezierDefiniendoLinea = false;
            estaDibujando = false;
            MostrarLienzoBase();
        }

        private void LimpiarLienzo()
        {
            if (DialogBox.Show(this, "¿Limpiar el lienzo?", "Confirmar") != DialogResult.OK) return;
            GuardarEstadoParaDeshacer();
            historialFiguras.Clear(); puntosPoligono.Clear(); puntosLapiz.Clear();
            curvaBezierSeleccionada = null; indiceControlBezier = -1;
            arrastrandoControlBezier = false; bezierDefiniendoLinea = false;
            estaDibujando = false;
            escribiendoTexto = false; textoActual = "";
            using (var g = Graphics.FromImage(mapaBits)) g.Clear(Color.White);
            ActualizarBitmapComprometido();
            ImagenLienzoActual = mapaBits;
        }

        private void Guardar()
        {
            ConfirmarTexto(); ConfirmarSeleccion();
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
        //  BORRADOR / HELPERS
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
        //  TECLADO
        // ════════════════════════════════════════════════════════════════════════
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (escribiendoTexto) return; // KeyPress lo maneja

            if (e.Control && e.KeyCode == Keys.Z) { Deshacer(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter && herramientaActual == Herramienta.Poligono) { FinalizarPoligono(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape)
            {
                puntosPoligono.Clear(); puntosLapiz.Clear();
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

    // ── Helper de confirmación simple ────────────────────────────────────────
    internal static class DialogBox
    {
        public static DialogResult Show(Form owner, string msg, string title)
            => MessageBox.Show(owner, msg, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
    }
}
