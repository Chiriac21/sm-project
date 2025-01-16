using ActressMas;
using MazeProject.Utils;

namespace MazeProject.Agents
{
    public class MazeAgent : Agent
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int OldX { get; set; }
        public int OldY { get; set; }
        public int LastDirection { get; set; } // Agent's last moving direction

        private MainForm _mainForm;
        private double[,,] _maze;
        private Stack<int> pathHistory = new Stack<int>(); // Agent's path history
        private bool wasDeadEnd;
        private bool _hasExitCoordinates;
        private int _exitX;
        private int _exitY;

        public MazeAgent(double[,,] maze, int startX, int startY, string name, MainForm mainForm, double initialCost = 100)
        {
            X = startX;
            Y = startY;
            OldX = startX;
            OldY = startY;
            _maze = maze;
            Name = name;
            LastDirection = -1;
            _mainForm = mainForm;
            wasDeadEnd= false;
            _hasExitCoordinates = false;
        }

        public override void Setup()
        {

        }

        public override void Act(ActressMas.Message message)
        {
            if (message.Content.StartsWith("Exit:"))
            {
                string[] parts = message.Content.Split(':')[1].Split(',');
                _exitX = int.Parse(parts[0]);
                _exitY = int.Parse(parts[1]);
                _hasExitCoordinates = true;
            }
        }

        public override void ActDefault()
        {
            if (_maze[X, Y, 1] == -2)
            {
                MoveInDirection(1);
            }
            else if (_maze[X, Y, 2] == -2)
            {
                MoveInDirection(2);
            }
            else if (_maze[X, Y, 3] == -2)
            {
                MoveInDirection(3);
            }
            else if (_maze[X, Y, 4] == -2)
            {
                MoveInDirection(4);
            }
            else if (_maze[X, Y, 0] == -2)
            {
                BroadcastExitFound();
                StopAgent();
            }
            else
            {
                MoveStrategically();
            }
        }

        public void MoveStrategically()
        {

            List<int> validDirections = GetValidDirections(X, Y);

            // Agent reched a dead-end
            if (validDirections.Count == 1 && validDirections.Contains(GetOppositeDirection(LastDirection)))
            {
                TurnBack(); 
            }
            else if (wasDeadEnd)
            {
                // If there are no valid directions, the agent should evade the dead-end
                EvadeDeadEnd();
            }
            else if (validDirections.Count > 0)
            {
                // We choose the best direction to move in
                int bestDirection = ChooseBestDirection(validDirections);
                MoveInDirection(bestDirection);
            }

        }

        private List<int> GetValidDirections(int x, int y)
        {
            List<int> validDirections = new List<int>();

            // Iterate trough all directions
            for (int direction = 1; direction <= 4; direction++)
            {
                // If the cell's weight is greater than 0, it means it's a valid direction
                if (_maze[x, y, direction] > 0)
                {
                    validDirections.Add(direction);
                }
            }

            return validDirections;
        }

        // Calculates the direction based on the weights of neighbors
        private int ChooseBestDirection(List<int> validDirections)
        {
            int bestDirection = -1;
            double bestWeight = 0f;

            foreach (int direction in validDirections)
            {
                int neighborX = X;
                int neighborY = Y;

                // Find the neighbor's coordinates based on the direction
                if (direction == 1) neighborY--; // Vecinul de sus
                else if (direction == 2) neighborY++; // Vecinul de jos
                else if (direction == 3) neighborX--; // Vecinul din stanga
                else if (direction == 4) neighborX++; // Vecinul din dreapta

                double weight = _maze[neighborX, neighborY, 0];

                // Compare the found weight with the best weight
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        // Moves the agent in a certain direction
        private void MoveInDirection(int direction)
        {
         
            LastDirection = direction;
            OldX = X;
            OldY = Y;
            if (direction == 1) Y--; // Up
            else if (direction == 2) Y++; // Down
            else if (direction == 3) X--; // Left
            else if (direction == 4) X++; // Right

            if (_maze[X, Y, 0] != -2)
            {
                // Update the cell's weight
                _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0f); 
            }

            pathHistory.Push(direction);

        }

        private void EvadeDeadEnd()
        {
            int lastDirection = pathHistory.Pop();
            LastDirection = lastDirection;

            // Mark the cell as a dead-end
            _maze[X, Y, 0] = 0f;
            OldX = X;
            OldY = Y;

            // Get the valid directions
            List<int> validDirections = GetValidDirections(X, Y);

            // If there is only one valid direction and it's not the opposite of the last direction
            if (validDirections.Count == 1 && LastDirection != validDirections[0])
            {
                // Update the direction
                LastDirection = validDirections[0];
            }


            // Go the new direction
            if (LastDirection == 1) Y--;
            else if (LastDirection == 2) Y++; 
            else if (LastDirection == 3) X--; 
            else if (LastDirection == 4) X++; 

            // Set the last visited cell as dead-end
            _maze[X, Y, GetOppositeDirection(LastDirection)] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);
            pathHistory.Push(lastDirection);

            validDirections = GetValidDirections(X, Y);

            if (validDirections.Count == 1)
            {
                LastDirection = ChooseBestDirection(validDirections);
            }
            else
            {
                wasDeadEnd = false;
            }

            if (_maze[X, Y, 1] == -3 || _maze[X, Y, 2] == -3 || _maze[X, Y, 3] == -3 || _maze[X, Y, 4] == -3)
                wasDeadEnd = false;
        }

        private void TurnBack()
        {
            int lastDirection = pathHistory.Pop();

            // Mark the cell as a dead-end
            _maze[X, Y, 0] = 0f;
            OldX = X;
            OldY = Y;

            // Get the valid directions
            List<int> validDirections = GetValidDirections(X, Y);

            // Go opposite direction
            if (LastDirection == 1) Y++;  
            else if (LastDirection == 2) Y--; 
            else if (LastDirection == 3) X++; 
            else if (LastDirection == 4) X--;

            _maze[X, Y, LastDirection] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);

            // Update the last direction
            LastDirection = GetOppositeDirection(lastDirection);
            pathHistory.Push(LastDirection);

            wasDeadEnd = true;

            if (_maze[X, Y, 1] == -3 || _maze[X, Y, 2] == -3 || _maze[X, Y, 3] == -3 || _maze[X, Y, 4] == -3)
                wasDeadEnd = false;
        }


        private int GetOppositeDirection(int direction)
        {
            switch (direction)
            {
                case 1: return 2; // Up <-> Down
                case 2: return 1;
                case 3: return 4; // Left <-> Right
                case 4: return 3;
                default: return -1;
            }
        }

        private void StopAgent()
        {
            _mainForm.AgentAtFinish(this);
        }

        private void BroadcastExitFound()
        {
            string message = $"Exit:{X},{Y}";
            Broadcast(message);
            _hasExitCoordinates = true;
        }
    }

}
