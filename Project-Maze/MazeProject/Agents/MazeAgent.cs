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
        public int LastDirection { get; set; } // Last direction agent went to

        private MainForm _mainForm;
        private double[,,] _maze;
        private Stack<int> pathHistory = new Stack<int>(); // directions history
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

            // agent is in dead-end
            if (validDirections.Count == 1 && validDirections.Contains(GetOppositeDirection(LastDirection)))
            {
                TurnBack(); 
            }
            else if (wasDeadEnd)
            {
                // if there are no valid directions, turn back
                EvadeDeadEnd();
            }
            else if (validDirections.Count > 0)
            {
                // choose the best direction for valid ones
                int bestDirection = ChooseBestDirection(validDirections);
                MoveInDirection(bestDirection);
            }

        }

        private List<int> GetValidDirections(int x, int y)
        {
            List<int> validDirections = new List<int>();

            // all directions
            for (int direction = 1; direction <= 4; direction++)
            {
                // if weight > 0 and cell is not visited
                if (_maze[x, y, direction] > 0)
                {
                    validDirections.Add(direction);
                }
            }

            return validDirections;
        }

        // find direction knowing neightbours weights
        private int ChooseBestDirection(List<int> validDirections)
        {
            int bestDirection = -1;
            double bestWeight = 0f;

            foreach (int direction in validDirections)
            {
                int neighborX = X;
                int neighborY = Y;

                // Find the neighbour 
                if (direction == 1) neighborY--; // up
                else if (direction == 2) neighborY++; // down
                else if (direction == 3) neighborX--; // left
                else if (direction == 4) neighborX++; // right

                double weight = _maze[neighborX, neighborY, 0];

                // compare the weights until we find the best one (bestDirection)
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        // move the agent to direction x
        private void MoveInDirection(int direction)
        {
            // used to avoid turn
            LastDirection = direction;
            OldX = X;
            OldY = Y;
            if (direction == 1) Y--; // up
            else if (direction == 2) Y++; // down
            else if (direction == 3) X--; // left
            else if (direction == 4) X++; // right

            if (_maze[X, Y, 0] != -2)
            {
                // update the weight
                _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0f); 
            }

            pathHistory.Push(direction);

        }

        private void EvadeDeadEnd()
        {
            int lastDirection = pathHistory.Pop();
            LastDirection = lastDirection;
            // we're in dead-end, mark the cell with 0 weight
            _maze[X, Y, 0] = 0f;
            OldX = X;
            OldY = Y;

            // get a valid direction in case there are one or two
            List<int> validDirections = GetValidDirections(X, Y);

            // if the new direction is different from the one which comes from (LastDirection)
            if(validDirections.Count == 1 && LastDirection != validDirections[0])
            {
                // to with the new one
                LastDirection = validDirections[0];
            }


            // update coord. (go backwards)
            if (LastDirection == 1) Y--;
            else if (LastDirection == 2) Y++; 
            else if (LastDirection == 3) X--; 
            else if (LastDirection == 4) X++; 

            // update the grid with the cell's weight
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

            _maze[X, Y, 0] = 0f; // dead-end
            OldX = X;
            OldY = Y;

            // in case there are one or more valid directions
            List<int> validDirections = GetValidDirections(X, Y);

            // go backward and also update coord.
            if (LastDirection == 1) Y++;  
            else if (LastDirection == 2) Y--; 
            else if (LastDirection == 3) X++; 
            else if (LastDirection == 4) X--;

            _maze[X, Y, LastDirection] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);

            // update last direction
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
                case 1: return 2; // up <-> down
                case 2: return 1;
                case 3: return 4; // left <-> right
                case 4: return 3;
                default: return -1;
            }
        }

        private void StopAgent()
        {
            //Send("Environment", "AgentFinished");
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
