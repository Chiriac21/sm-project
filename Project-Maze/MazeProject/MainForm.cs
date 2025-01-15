using ActressMas;
using MazeProject.Agents;
using MazeProject.Utils;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MazeProject
{
    public partial class MainForm : Form
    {
        private Bitmap? _mazeImage;
        private Bitmap? _agentsImage;
        private double[,,]? _maze;
        private int _startX, _startY;

        private static int previousGreenBlue = 255; // Valoarea inițială pentru green și blue
        private List<MazeAgent> _agents = new();
        private MazeEnvironment? _environment;
        private Thread? _simulationThread;
        private Bitmap? _wallImage;
        private Bitmap? _pathwayImage;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomControls();
            CustomizeUI();
        }

        private void InitializeCustomControls()
        {

            // Manually load images using ResourceManager
            try
            {
                var rm = new ResourceManager("MazeProject.Properties.Resources", Assembly.GetExecutingAssembly());
                _wallImage = (Bitmap)rm.GetObject("walls");      // Access 'wall' resource
                _pathwayImage = (Bitmap)rm.GetObject("pathway"); // Access 'pathway' resource
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading resources: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomizeUI()
        {
            this.Text = "Maze Simulator";
            this.BackColor = Color.LightGray;

            // Style PictureBox
            pictureBox.BackColor = Color.WhiteSmoke;
            pictureBox.BorderStyle = BorderStyle.Fixed3D;

            // Style Buttons
            buttonGenerateMaze.BackColor = Color.CornflowerBlue;
            buttonGenerateMaze.ForeColor = Color.White;
            buttonGenerateMaze.Font = new Font("Arial", 10, FontStyle.Bold);

            buttonStartSimulation.BackColor = Color.ForestGreen;
            buttonStartSimulation.ForeColor = Color.White;
            buttonStartSimulation.Font = new Font("Arial", 10, FontStyle.Bold);

            buttonStopSimulation.BackColor = Color.Firebrick;
            buttonStopSimulation.ForeColor = Color.White;
            buttonStopSimulation.Font = new Font("Arial", 10, FontStyle.Bold);

            // Labels and Progress Bar
            progressBarSimulation.Visible = false;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (_mazeImage != null)
            {
                e.Graphics.DrawImage(new Bitmap(_mazeImage), 0, 0);
            }

            if (_agentsImage != null)
            {
                e.Graphics.DrawImage(new Bitmap(_agentsImage), 0, 0);
            }
        }

        private async void buttonGenerateMaze_Click(object sender, EventArgs e)
        {
            if (_environment != null)
            {
                ShowMessage("Simulation in progress. Please stop it before generating a new maze.", MessageBoxIcon.Warning);
                return;
            }

            DisposeImages();

            var mazeWidth = (int)numericMazeWidth.Value;
            var mazeHeight = (int)numericMazeHeight.Value;
            var mazeSeed = (int)numericMazeSeed.Value;

            progressBarSimulation.Maximum = 100;
            progressBarSimulation.Value = 0;
            progressBarSimulation.Visible = true; 

            try
            {
                await Task.Run(() =>
                {
                    _maze = MazeGenerator.GenerateMaze(mazeWidth, mazeHeight, out _startX, out _startY, UpdateProgressBar, mazeSeed);
                });

                _mazeImage = new Bitmap(pictureBox.Width, pictureBox.Height);

                using Graphics g = Graphics.FromImage(_mazeImage);
                DrawMaze(g);

                pictureBox.Invalidate(); // Force redraw
            }
            catch (Exception ex)
            {
                ShowMessage($"Error generating maze: {ex.Message}", MessageBoxIcon.Error);
            }
            progressBarSimulation.Visible = false;
        }

        private void buttonStartSimulation_Click(object sender, EventArgs e)
        {
            if (_maze == null)
            {
                ShowMessage("Please generate a maze first.", MessageBoxIcon.Warning);
                return;
            }

            int noAgents = (int)numericNoAgents.Value;
            if (noAgents <= 0)
            {
                ShowMessage("At least one agent is required to start simulation.", MessageBoxIcon.Warning);
                return;
            }

            if (_environment != null)
            {
                if (ShowConfirmation("A simulation is already in progress. Would you like to abort and start a new one?"))
                {
                    StopSimulation();
                    StartSimulation(noAgents, _maze);
                }
            }
            else
            {
                StartSimulation(noAgents, _maze);
            }
        }

        private void buttonStopSimulation_Click(object sender, EventArgs e)
        {
            StopSimulation();
        }

        private void StartSimulation(int noAgents, double[,,] maze)
        {
            try
            {
                progressBarSimulation.Visible = true;
                progressBarSimulation.Style = ProgressBarStyle.Marquee;

                _environment = new MazeEnvironment(0, 1000);
                _agents.Clear();

                for (int i = 0; i < noAgents; i++)
                {
                    var agent = new MazeAgent(maze, _startX, _startY, $"Agent_{i}", this);
                    _environment.Add(agent);
                    _agents.Add(agent);
                }

                _environment.OnAgentMoveEvent += Agent_OnMoveEvent;

                _simulationThread = new Thread(() => _environment.Start());
                _simulationThread.Start();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error starting simulation: {ex.Message}", MessageBoxIcon.Error);
            }
        }

        private void StopSimulation()
        {
            if (_environment == null) return;

            foreach (var agent in _environment.AllAgents())
                _environment.Remove(agent);

            _environment = null;

            progressBarSimulation.Visible = false;
            ShowMessage("Simulation stopped successfully.", MessageBoxIcon.Information);
        }

        public void AgentAtFinish(MazeAgent agent)
        {
            if (_environment == null) return;

            _environment.Remove(agent);

            ShowMessage("Agent escaped the maze.", MessageBoxIcon.Information);

            if (_environment.NoAgents == 0)
            {
                _environment = null;
                ShowMessage("Simulation stopped successfully.", MessageBoxIcon.Information);
            }

        }

        private Color GetColorByWeight(double weight)
        {
            int greenBlue;
            int red = 255;  // Pe măsură ce ponderea crește, roșul crește
            if (weight != 0)
            { 
                greenBlue = (int)(255 * weight) - 20;
                if (greenBlue <= 0)
                    greenBlue = 0;
            }
            else
                greenBlue = (int)(255 * weight);
            return Color.FromArgb(red, greenBlue, greenBlue);
        }

        private void DrawMaze(Graphics g)
        {
            int minXY = Math.Min(pictureBox.Width, pictureBox.Height);
            int maxMazeXY = Math.Max(_maze.GetLength(0), _maze.GetLength(1));
            int cellSize = minXY / maxMazeXY;

            for (int x = 0; x < _maze.GetLength(0); x++)
            {
                for (int y = 0; y < _maze.GetLength(1); y++)
                {
                    switch (_maze[x, y, 0])
                    {
                        case (int)MazeCell.Wall:
                            g.FillRectangle(Brushes.Black, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;

                        case (int)MazeCell.Start:
                            g.FillRectangle(Brushes.DarkRed, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;

                        case (int)MazeCell.Exit:
                            g.FillRectangle(Brushes.Green, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;

                        default: // Pathway
                            g.FillRectangle(Brushes.White, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;
                    }
                }
            }
        }

        private void DrawAgents(Graphics g)
        {
            if (_maze == null) return;

            int minDimension = Math.Min(pictureBox.Width, pictureBox.Height);
            int maxMazeDimension = Math.Max(_maze.GetLength(0), _maze.GetLength(1));
            int cellSize = minDimension / maxMazeDimension;
            int agentSize = (int)(cellSize * 0.9);

            foreach (var agent in _agents)
            {
                g.FillEllipse(Brushes.Blue, agent.X * cellSize, agent.Y * cellSize, agentSize, agentSize);
            }
        }

        private void DisposeImages()
        {
            _agentsImage?.Dispose();
            _agentsImage = null;

            _mazeImage?.Dispose();
            _mazeImage = null;

            GC.Collect();
        }

        private void Agent_OnMoveEvent()
        {
            // Actualizează imaginea agentului pe bază de mișcare
            if (_agentsImage != null)
            {
                _agentsImage.Dispose();
                GC.Collect(); // Previne scurgerile de memorie
            }

            int minXY = Math.Min(pictureBox.Width, pictureBox.Height);
            int maxMazeXY = Math.Max(_maze.GetLength(0), _maze.GetLength(1));
            int cellSize = minXY / maxMazeXY;

            foreach (var agent in _agents)
            { 
            Graphics g = Graphics.FromImage(_mazeImage);

            //=====Test purpose - show weight of the cell=====
            //using Font font = new Font("Arial", cellSize / 3);
            //using Brush textBrush = new SolidBrush(Color.Black);
            //double result = Math.Round(_maze[agent.OldX, agent.OldY, 0], 1, MidpointRounding.ToEven);
            //string weightText = result.ToString();
            //SizeF textSize = g.MeasureString(weightText, font);
            //float textX = agent.OldX * cellSize + (cellSize - textSize.Width) / 2;
            //float textY = agent.OldY * cellSize + (cellSize - textSize.Height) / 2;

            // Calculăm ponderea celulei curente
            double weight = _maze[agent.OldX, agent.OldY, 0];

            if (weight != -2 && weight != -3)
            {
                // Obținem culoarea bazată pe pondere
                Color cellColor = GetColorByWeight(weight);
                Brush cellBrush = new SolidBrush(cellColor);
                g.FillRectangle(cellBrush, agent.OldX * cellSize, agent.OldY * cellSize, cellSize, cellSize);
            }

            //=====Test purpose - show weight of the cell=====
            //g.DrawString(weightText, font, textBrush, textX, textY);

            }
            _agentsImage = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics ga = Graphics.FromImage(_agentsImage);
            DrawAgents(ga); // Redesenăm agenții pe harta

            // Folosim Invoke pentru a ne asigura că actualizările sunt făcute pe thread-ul principal
            this.Invoke((MethodInvoker)(() =>
            {
                pictureBox.Refresh(); // Reîmprospătăm imaginea
            }));
        }

        private void ShowMessage(string message, MessageBoxIcon icon)
        {
            MessageBox.Show(message, "Maze Simulator", MessageBoxButtons.OK, icon);
        }

        private bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Maze Simulator", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void UpdateProgressBar(int progress)
        {
            // Ensure we update the progress bar on the UI thread
            if (this.InvokeRequired)
            {
                // Use Invoke to call the method on the UI thread if necessary
                this.Invoke(new Action<int>(UpdateProgressBar), progress);
            }
            else
            {
                // Update the ProgressBar value
                progressBarSimulation.Value = progress;
            }
        }

    }

}