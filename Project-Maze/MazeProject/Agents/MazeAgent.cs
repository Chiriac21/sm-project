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
        public int LastDirection { get; set; } // Directia agentului ultima data

        private MainForm _mainForm;
        private double[,,] _maze;
        private Stack<int> pathHistory = new Stack<int>(); // Istoricul directiilor parcurse
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

            // agentul se afla in dead-end
            if (validDirections.Count == 1 && validDirections.Contains(GetOppositeDirection(LastDirection)))
            {
                TurnBack(); 
            }
            else if (wasDeadEnd)
            {
                // Daca nu există directii valide, ne intoarcem
                EvadeDeadEnd();
            }
            else if (validDirections.Count > 0)
            {
                // Alegem cea mai buna directie dintre cele valide
                int bestDirection = ChooseBestDirection(validDirections);
                MoveInDirection(bestDirection);
            }

        }

        private List<int> GetValidDirections(int x, int y)
        {
            List<int> validDirections = new List<int>();

            // parcurgem directiile
            for (int direction = 1; direction <= 4; direction++)
            {
                // daca ponderea > 0 si celula n-a fost vizitata
                if (_maze[x, y, direction] > 0)
                {
                    validDirections.Add(direction);
                }
            }

            return validDirections;
        }

        // Calculeaza directia in functie de ponderile vecinilor
        private int ChooseBestDirection(List<int> validDirections)
        {
            int bestDirection = -1;
            double bestWeight = 0f;

            foreach (int direction in validDirections)
            {
                int neighborX = X;
                int neighborY = Y;

                // Calculam vecinul in functie de directie
                if (direction == 1) neighborY--; // Vecinul de sus
                else if (direction == 2) neighborY++; // Vecinul de jos
                else if (direction == 3) neighborX--; // Vecinul din stanga
                else if (direction == 4) neighborX++; // Vecinul din dreapta

                double weight = _maze[neighborX, neighborY, 0];

                // Comparam ponderea gasita pana o gasim pe cea maxima (bestDirection)
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        // Muta agentul intr-o directie
        private void MoveInDirection(int direction)
        {
            // ca sa evitam intoarcerea
            LastDirection = direction;
            OldX = X;
            OldY = Y;
            if (direction == 1) Y--; // sus
            else if (direction == 2) Y++; // jos
            else if (direction == 3) X--; // stanga
            else if (direction == 4) X++; // dreapta

            if (_maze[X, Y, 0] != -2)
            {
                // Actualizam ponderea
                _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0f); 
            }

            pathHistory.Push(direction);

        }

        private void EvadeDeadEnd()
        {
            int lastDirection = pathHistory.Pop();
            LastDirection = lastDirection;
            // In momentul asta, suntem in dead-end deci marcam celula cu ponderea 0
            _maze[X, Y, 0] = 0f;
            OldX = X;
            OldY = Y;

            // in caz ca exista doua sau mai multe directii valide
            List<int> validDirections = GetValidDirections(X, Y);

            // daca singura directie valida e diferita de directia de unde a venit el initial (LastDirection)
            if(validDirections.Count == 1 && LastDirection != validDirections[0])
            {
                // mergem in directia noua
                LastDirection = validDirections[0];
            }


            // actualizam coordonatele (mergem inapoi)
            if (LastDirection == 1) Y--;
            else if (LastDirection == 2) Y++; 
            else if (LastDirection == 3) X--; 
            else if (LastDirection == 4) X++; 

            // actualizam gridul cu ponderea celulei
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

            _maze[X, Y, 0] = 0f; // suntem in dead-end
            OldX = X;
            OldY = Y;

            // in caz ca exista una sau mai multe directii valide
            List<int> validDirections = GetValidDirections(X, Y);

            // o luam inapoi (actualizam coord.)
            if (LastDirection == 1) Y++;  
            else if (LastDirection == 2) Y--; 
            else if (LastDirection == 3) X++; 
            else if (LastDirection == 4) X--;

            _maze[X, Y, LastDirection] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);

            // Redefinim ultima directie
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
                case 1: return 2; // Sus <-> Jos
                case 2: return 1;
                case 3: return 4; // Stânga <-> Dreapta
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
