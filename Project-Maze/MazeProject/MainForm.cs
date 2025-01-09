using MazeProject.Agents;
using MazeProject.Utils;
using System.Reflection;
using System.Resources;

namespace MazeProject
{
    public partial class MainForm : Form
    {
        private Bitmap? _mazeImage;
        private Bitmap? _agentsImage;
        private int[,,]? _maze;
        private int _startX, _startY;

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
                DrawMaze(g, _maze);

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

        private void StartSimulation(int noAgents, int[,,] maze)
        {
            try
            {
                progressBarSimulation.Visible = true;
                progressBarSimulation.Style = ProgressBarStyle.Marquee;

                _environment = new MazeEnvironment(0, 1000);
                _agents.Clear();

                for (int i = 0; i < noAgents; i++)
                {
                    var agent = new MazeAgent(maze, _startX, _startY, $"Agent_{i}");
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

        private void DrawMaze(Graphics g, int[,,] maze)
        {
            int minXY = Math.Min(pictureBox.Width, pictureBox.Height);
            int maxMazeXY = Math.Max(maze.GetLength(0), maze.GetLength(1));
            int cellSize = minXY / maxMazeXY;

            for (int x = 0; x < maze.GetLength(0); x++)
            {
                for (int y = 0; y < maze.GetLength(1); y++)
                {
                    switch (maze[x, y, 0])
                    {
                        case (int)MazeCell.Wall:
                            if (_wallImage != null)
                            {
                                g.DrawImage(_wallImage, new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                            }
                            else
                            {
                                g.FillRectangle(Brushes.Black, x * cellSize, y * cellSize, cellSize, cellSize);
                            }
                            break;

                        case (int)MazeCell.Start:
                            g.FillRectangle(Brushes.Red, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;

                        case (int)MazeCell.Exit:
                            g.FillRectangle(Brushes.LimeGreen, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;

                        default: // Pathway
                            if (_pathwayImage != null)
                            {
                                g.DrawImage(_pathwayImage, x * cellSize, y * cellSize, cellSize, cellSize);
                            }
                            else
                            {
                                g.FillRectangle(Brushes.White, x * cellSize, y * cellSize, cellSize, cellSize);
                            }
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
            // Avoid invoking if the form handle is not created yet
            if (!this.IsHandleCreated)
                return;

            // Safely update the UI via Invoke
            if (_agentsImage != null)
            {
                _agentsImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _agentsImage = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics g = Graphics.FromImage(_agentsImage);
            DrawAgents(g);

            // Use Invoke to ensure thread-safety when updating the UI
            this.Invoke((MethodInvoker)(() =>
            {
                pictureBox.Refresh();
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