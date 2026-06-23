namespace PaintEspe
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pbLienzo = new System.Windows.Forms.PictureBox();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.panelHerramientas = new System.Windows.Forms.Panel();
            this.panelColores = new System.Windows.Forms.Panel();
            this.panelGrosor = new System.Windows.Forms.Panel();
            this.panelAcciones = new System.Windows.Forms.Panel();
            this.nudGrosor = new System.Windows.Forms.NumericUpDown();
            this.chkRelleno = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblHerramientaActual = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblPosicion = new System.Windows.Forms.ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)(this.pbLienzo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrosor)).BeginInit();
            this.panelToolbar.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // pbLienzo
            this.pbLienzo.BackColor = System.Drawing.Color.White;
            this.pbLienzo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbLienzo.Location = new System.Drawing.Point(0, 80);
            this.pbLienzo.Name = "pbLienzo";
            this.pbLienzo.TabIndex = 0;
            this.pbLienzo.TabStop = false;
            this.pbLienzo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseDown);
            this.pbLienzo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseMove);
            this.pbLienzo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseUp);

            // panelToolbar
            this.panelToolbar.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Height = 80;
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Controls.Add(this.panelHerramientas);
            this.panelToolbar.Controls.Add(this.panelColores);
            this.panelToolbar.Controls.Add(this.panelGrosor);
            this.panelToolbar.Controls.Add(this.panelAcciones);

            // panelHerramientas
            this.panelHerramientas.Location = new System.Drawing.Point(0, 0);
            this.panelHerramientas.Size = new System.Drawing.Size(290, 80);
            this.panelHerramientas.Name = "panelHerramientas";
            this.panelHerramientas.BackColor = System.Drawing.Color.Transparent;

            // panelColores
            this.panelColores.Location = new System.Drawing.Point(295, 0);
            this.panelColores.Size = new System.Drawing.Size(560, 80);
            this.panelColores.Name = "panelColores";
            this.panelColores.BackColor = System.Drawing.Color.Transparent;

            // panelGrosor
            this.panelGrosor.Location = new System.Drawing.Point(860, 0);
            this.panelGrosor.Size = new System.Drawing.Size(160, 80);
            this.panelGrosor.Name = "panelGrosor";
            this.panelGrosor.BackColor = System.Drawing.Color.Transparent;
            this.panelGrosor.Controls.Add(this.nudGrosor);
            this.panelGrosor.Controls.Add(this.chkRelleno);

            // panelAcciones
            this.panelAcciones.Location = new System.Drawing.Point(1025, 0);
            this.panelAcciones.Size = new System.Drawing.Size(200, 80);
            this.panelAcciones.Name = "panelAcciones";
            this.panelAcciones.BackColor = System.Drawing.Color.Transparent;

            // nudGrosor
            this.nudGrosor.Location = new System.Drawing.Point(60, 28);
            this.nudGrosor.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudGrosor.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudGrosor.Name = "nudGrosor";
            this.nudGrosor.Size = new System.Drawing.Size(50, 22);
            this.nudGrosor.TabIndex = 11;
            this.nudGrosor.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudGrosor.ValueChanged += new System.EventHandler(this.nudGrosor_ValueChanged);

            // chkRelleno
            this.chkRelleno.AutoSize = true;
            this.chkRelleno.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.chkRelleno.Location = new System.Drawing.Point(8, 55);
            this.chkRelleno.Name = "chkRelleno";
            this.chkRelleno.Size = new System.Drawing.Size(70, 17);
            this.chkRelleno.TabIndex = 14;
            this.chkRelleno.Text = "Rellenar";
            this.chkRelleno.Font = new System.Drawing.Font("Segoe UI", 8f);

            // statusStrip1
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblHerramientaActual, this.lblPosicion });
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1044, 22);
            this.statusStrip1.TabIndex = 16;

            // lblHerramientaActual
            this.lblHerramientaActual.Name = "lblHerramientaActual";
            this.lblHerramientaActual.ForeColor = System.Drawing.Color.White;
            this.lblHerramientaActual.Spring = true;
            this.lblHerramientaActual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHerramientaActual.Text = "Herramienta: Línea";

            // lblPosicion
            this.lblPosicion.Name = "lblPosicion";
            this.lblPosicion.ForeColor = System.Drawing.Color.White;
            this.lblPosicion.Size = new System.Drawing.Size(80, 17);
            this.lblPosicion.Text = "X: 0  Y: 0";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.pbLienzo);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelToolbar);
            this.Name = "Form1";
            this.Text = "PaintEspe";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            ((System.ComponentModel.ISupportInitialize)(this.pbLienzo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrosor)).EndInit();
            this.panelToolbar.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pbLienzo;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.Panel panelHerramientas;
        private System.Windows.Forms.Panel panelColores;
        private System.Windows.Forms.Panel panelGrosor;
        private System.Windows.Forms.Panel panelAcciones;
        private System.Windows.Forms.NumericUpDown nudGrosor;
        private System.Windows.Forms.CheckBox chkRelleno;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblPosicion;
        private System.Windows.Forms.ToolStripStatusLabel lblHerramientaActual;
    }
}
