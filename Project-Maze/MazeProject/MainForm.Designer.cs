namespace MazeProject
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wallImage?.Dispose();
                _pathwayImage?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox = new PictureBox();
            buttonStartSimulation = new Button();
            buttonGenerateMaze = new Button();
            numericMazeWidth = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            numericMazeHeight = new NumericUpDown();
            label3 = new Label();
            numericNoAgents = new NumericUpDown();
            label4 = new Label();
            numericMazeSeed = new NumericUpDown();
            buttonStopSimulation = new Button();
            progressBarSimulation = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericNoAgents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeSeed).BeginInit();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.Location = new Point(12, 72);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(560, 560);
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            pictureBox.Paint += pictureBox_Paint;
            // 
            // buttonStartSimulation
            // 
            buttonStartSimulation.Location = new Point(27, 12);
            buttonStartSimulation.Name = "buttonStartSimulation";
            buttonStartSimulation.Size = new Size(100, 23);
            buttonStartSimulation.TabIndex = 1;
            buttonStartSimulation.Text = "Start Simulation";
            buttonStartSimulation.UseVisualStyleBackColor = true;
            buttonStartSimulation.Click += buttonStartSimulation_Click;
            // 
            // buttonGenerateMaze
            // 
            buttonGenerateMaze.Location = new Point(12, 645);
            buttonGenerateMaze.Name = "buttonGenerateMaze";
            buttonGenerateMaze.Size = new Size(100, 23);
            buttonGenerateMaze.TabIndex = 2;
            buttonGenerateMaze.Text = "Generate Maze";
            buttonGenerateMaze.UseVisualStyleBackColor = true;
            buttonGenerateMaze.Click += buttonGenerateMaze_Click;
            // 
            // numericMazeWidth
            // 
            numericMazeWidth.Location = new Point(264, 10);
            numericMazeWidth.Margin = new Padding(0, 3, 3, 3);
            numericMazeWidth.Name = "numericMazeWidth";
            numericMazeWidth.Size = new Size(57, 23);
            numericMazeWidth.TabIndex = 3;
            numericMazeWidth.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(222, 14);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 4;
            label1.Text = "Width";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(218, 45);
            label2.Name = "label2";
            label2.Size = new Size(43, 15);
            label2.TabIndex = 6;
            label2.Text = "Height";
            // 
            // numericMazeHeight
            // 
            numericMazeHeight.Location = new Point(264, 41);
            numericMazeHeight.Margin = new Padding(0, 3, 3, 3);
            numericMazeHeight.Name = "numericMazeHeight";
            numericMazeHeight.Size = new Size(57, 23);
            numericMazeHeight.TabIndex = 5;
            numericMazeHeight.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(377, 12);
            label3.Name = "label3";
            label3.Size = new Size(105, 15);
            label3.TabIndex = 8;
            label3.Text = "Number of Agents";
            // 
            // numericNoAgents
            // 
            numericNoAgents.Location = new Point(488, 10);
            numericNoAgents.Name = "numericNoAgents";
            numericNoAgents.Size = new Size(57, 23);
            numericNoAgents.TabIndex = 7;
            numericNoAgents.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(399, 47);
            label4.Name = "label4";
            label4.Size = new Size(63, 15);
            label4.TabIndex = 10;
            label4.Text = "Maze Seed";
            // 
            // numericMazeSeed
            // 
            numericMazeSeed.Location = new Point(488, 43);
            numericMazeSeed.Margin = new Padding(0, 3, 3, 3);
            numericMazeSeed.Name = "numericMazeSeed";
            numericMazeSeed.Size = new Size(57, 23);
            numericMazeSeed.TabIndex = 9;
            // 
            // buttonStopSimulation
            // 
            buttonStopSimulation.Location = new Point(27, 41);
            buttonStopSimulation.Name = "buttonStopSimulation";
            buttonStopSimulation.Size = new Size(100, 23);
            buttonStopSimulation.TabIndex = 11;
            buttonStopSimulation.Text = "Stop Simulation";
            buttonStopSimulation.UseVisualStyleBackColor = true;
            buttonStopSimulation.Click += buttonStopSimulation_Click;
            // 
            // progressBarSimulation
            // 
            progressBarSimulation.Location = new Point(488, 645);
            progressBarSimulation.Name = "progressBarSimulation";
            progressBarSimulation.Size = new Size(84, 23);
            progressBarSimulation.TabIndex = 12;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 680);
            Controls.Add(progressBarSimulation);
            Controls.Add(buttonStopSimulation);
            Controls.Add(label4);
            Controls.Add(numericMazeSeed);
            Controls.Add(label3);
            Controls.Add(numericNoAgents);
            Controls.Add(label2);
            Controls.Add(numericMazeHeight);
            Controls.Add(label1);
            Controls.Add(numericMazeWidth);
            Controls.Add(buttonGenerateMaze);
            Controls.Add(buttonStartSimulation);
            Controls.Add(pictureBox);
            Name = "MainForm";
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericNoAgents).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMazeSeed).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox;
        private Button buttonStartSimulation;
        private Button buttonGenerateMaze;
        private NumericUpDown numericMazeWidth;
        private Label label1;
        private Label label2;
        private NumericUpDown numericMazeHeight;
        private Label label3;
        private NumericUpDown numericNoAgents;
        private Label label4;
        private NumericUpDown numericMazeSeed;
        private Button buttonStopSimulation;
        private ProgressBar progressBarSimulation;
    }
}
