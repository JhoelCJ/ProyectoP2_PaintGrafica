namespace PaintEspe
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbLienzo = new System.Windows.Forms.PictureBox();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLinea = new System.Windows.Forms.Button();
            this.btnRectangulo = new System.Windows.Forms.Button();
            this.btnCirculo = new System.Windows.Forms.Button();
            this.btnColor = new System.Windows.Forms.Button();
            this.btnPoligono = new System.Windows.Forms.Button();
            this.btnBezier = new System.Windows.Forms.Button();
            this.btnRelleno = new System.Windows.Forms.Button();
            this.labelGrosor = new System.Windows.Forms.Label();
            this.nudGrosor = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pbLienzo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrosor)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbLienzo
            // 
            this.pbLienzo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbLienzo.Location = new System.Drawing.Point(0, 0);
            this.pbLienzo.Name = "pbLienzo";
            this.pbLienzo.Size = new System.Drawing.Size(800, 450);
            this.pbLienzo.TabIndex = 0;
            this.pbLienzo.TabStop = false;
            this.pbLienzo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseDown);
            this.pbLienzo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseMove);
            this.pbLienzo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbLienzo_MouseUp);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Location = new System.Drawing.Point(661, 10);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(49, 23);
            this.btnLimpiar.TabIndex = 1;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(716, 10);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(55, 23);
            this.btnGuardar.TabIndex = 2;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel1.Controls.Add(this.btnRelleno);
            this.panel1.Controls.Add(this.nudGrosor);
            this.panel1.Controls.Add(this.labelGrosor);
            this.panel1.Controls.Add(this.btnBezier);
            this.panel1.Controls.Add(this.btnPoligono);
            this.panel1.Controls.Add(this.btnColor);
            this.panel1.Controls.Add(this.btnCirculo);
            this.panel1.Controls.Add(this.btnRectangulo);
            this.panel1.Controls.Add(this.btnLinea);
            this.panel1.Controls.Add(this.btnGuardar);
            this.panel1.Controls.Add(this.btnLimpiar);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 43);
            this.panel1.TabIndex = 3;
            // 
            // btnLinea
            // 
            this.btnLinea.Location = new System.Drawing.Point(27, 11);
            this.btnLinea.Name = "btnLinea";
            this.btnLinea.Size = new System.Drawing.Size(44, 23);
            this.btnLinea.TabIndex = 3;
            this.btnLinea.Text = "Linea";
            this.btnLinea.UseVisualStyleBackColor = true;
            this.btnLinea.Click += new System.EventHandler(this.btnLinea_Click);
            // 
            // btnRectangulo
            // 
            this.btnRectangulo.Location = new System.Drawing.Point(77, 11);
            this.btnRectangulo.Name = "btnRectangulo";
            this.btnRectangulo.Size = new System.Drawing.Size(75, 23);
            this.btnRectangulo.TabIndex = 4;
            this.btnRectangulo.Text = "Rectangulo";
            this.btnRectangulo.UseVisualStyleBackColor = true;
            this.btnRectangulo.Click += new System.EventHandler(this.btnRectangulo_Click);
            // 
            // btnCirculo
            // 
            this.btnCirculo.Location = new System.Drawing.Point(158, 12);
            this.btnCirculo.Name = "btnCirculo";
            this.btnCirculo.Size = new System.Drawing.Size(47, 23);
            this.btnCirculo.TabIndex = 5;
            this.btnCirculo.Text = "Circulo";
            this.btnCirculo.UseVisualStyleBackColor = true;
            this.btnCirculo.Click += new System.EventHandler(this.btnCirculo_Click);
            // 
            // btnColor
            // 
            this.btnColor.Location = new System.Drawing.Point(611, 10);
            this.btnColor.Name = "btnColor";
            this.btnColor.Size = new System.Drawing.Size(44, 23);
            this.btnColor.TabIndex = 6;
            this.btnColor.Text = "Color";
            this.btnColor.UseVisualStyleBackColor = true;
            this.btnColor.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // btnPoligono
            // 
            this.btnPoligono.Location = new System.Drawing.Point(212, 10);
            this.btnPoligono.Name = "btnPoligono";
            this.btnPoligono.Size = new System.Drawing.Size(56, 23);
            this.btnPoligono.TabIndex = 7;
            this.btnPoligono.Text = "Poligono";
            this.btnPoligono.UseVisualStyleBackColor = true;
            this.btnPoligono.Click += new System.EventHandler(this.btnPoligono_Click);
            // 
            // btnBezier
            // 
            this.btnBezier.Location = new System.Drawing.Point(274, 10);
            this.btnBezier.Name = "btnBezier";
            this.btnBezier.Size = new System.Drawing.Size(43, 23);
            this.btnBezier.TabIndex = 8;
            this.btnBezier.Text = "Curva";
            this.btnBezier.UseVisualStyleBackColor = true;
            this.btnBezier.Click += new System.EventHandler(this.btnBezier_Click);
            // 
            // btnRelleno
            // 
            this.btnRelleno.Location = new System.Drawing.Point(551, 10);
            this.btnRelleno.Name = "btnRelleno";
            this.btnRelleno.Size = new System.Drawing.Size(54, 23);
            this.btnRelleno.TabIndex = 9;
            this.btnRelleno.Text = "Rellenar";
            this.btnRelleno.UseVisualStyleBackColor = true;
            this.btnRelleno.Click += new System.EventHandler(this.btnRelleno_Click);
            // 
            // labelGrosor
            // 
            this.labelGrosor.AutoSize = true;
            this.labelGrosor.ForeColor = System.Drawing.Color.White;
            this.labelGrosor.Location = new System.Drawing.Point(336, 15);
            this.labelGrosor.Name = "labelGrosor";
            this.labelGrosor.Size = new System.Drawing.Size(41, 13);
            this.labelGrosor.TabIndex = 10;
            this.labelGrosor.Text = "Grosor:";
            // 
            // nudGrosor
            // 
            this.nudGrosor.Location = new System.Drawing.Point(383, 12);
            this.nudGrosor.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGrosor.Name = "nudGrosor";
            this.nudGrosor.Size = new System.Drawing.Size(47, 20);
            this.nudGrosor.TabIndex = 11;
            this.nudGrosor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudGrosor.ValueChanged += new System.EventHandler(this.nudGrosor_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pbLienzo);
            this.Name = "Form1";
            this.Text = "Mi Paint ESPE";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.pbLienzo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrosor)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLienzo;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Button btnCirculo;
        private System.Windows.Forms.Button btnRectangulo;
        private System.Windows.Forms.Button btnLinea;
        private System.Windows.Forms.Button btnBezier;
        private System.Windows.Forms.Button btnPoligono;
        private System.Windows.Forms.Button btnRelleno;
        private System.Windows.Forms.Label labelGrosor;
        private System.Windows.Forms.NumericUpDown nudGrosor;
    }
}

