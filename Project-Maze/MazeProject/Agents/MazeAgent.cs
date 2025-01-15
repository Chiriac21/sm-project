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
        public double TotalCost { get; private set; }
        public int LastDirection { get; set; } // Direcția în care s-a mișcat agentul ultima dată

        private MainForm _mainForm;
        private double[,,] _maze;
        private HashSet<string> visitedCells = new HashSet<string>(); // Celulele vizitate
        private Stack<int> pathHistory = new Stack<int>(); // Istoricul direcțiilor parcurse
        private bool wasDeadEnd = false;

        public MazeAgent(double[,,] maze, int startX, int startY, string name, MainForm mainForm, double initialCost = 100)
        {
            X = startX;
            Y = startY;
            OldX = startX;
            OldY = startY;
            TotalCost = initialCost;
            _maze = maze;
            Name = name;
            LastDirection = -1;
            _mainForm = mainForm;
        }

        public override void Setup()
        {

        }

        public override void Act(ActressMas.Message message)
        {

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
                StopAgent();
            }
            else
            { 
                MoveStrategically();
            }
        }

        public void MoveStrategically()
        {

            // Verificăm vecinii pentru a găsi direcțiile valide
            List<int> validDirections = GetValidDirections(X, Y);

            // Dacă singura direcție validă este cea pe care a venit agentul (deci se află într-un dead-end)
            if (validDirections.Count == 1 && validDirections.Contains(GetOppositeDirection(LastDirection)))
            {
                TurnBack();  // Se întoarce pe drumul anterior
            }
            else if (wasDeadEnd)
            {
                // Dacă nu există direcții valide, întoarce-te pe drumul anterior
                EvadeDeadEnd();
            }
            else if (validDirections.Count > 0)
            {
                // Alege cea mai bună direcție dintre cele valide
                int bestDirection = ChooseBestDirection(validDirections);
                MoveInDirection(bestDirection);
            }

        }

        // Funcție pentru obținerea direcțiilor valide (unde ponderea este mai mare decât 0)
        private List<int> GetValidDirections(int x, int y)
        {
            List<int> validDirections = new List<int>();

            // Verificăm fiecare direcție: sus (1), jos (2), stânga (3), dreapta (4)
            for (int direction = 1; direction <= 4; direction++)
            {
                // Dacă ponderea este mai mare decât 0 și direcția nu a fost deja vizitată (sau a fost modificată)
                if (_maze[x, y, direction] > 0 && !visitedCells.Contains($"{x},{y},{direction}"))
                {
                    validDirections.Add(direction);
                }
            }

            return validDirections;
        }

        // Funcție pentru alegerea celei mai bune direcții bazată pe ponderile vecinilor
        private int ChooseBestDirection(List<int> validDirections)
        {
            int bestDirection = -1;
            double bestWeight = -1f;

            foreach (int direction in validDirections)
            {
                int neighborX = X;
                int neighborY = Y;

                // Calculăm vecinul în funcție de direcție
                if (direction == 1) neighborY--; // Vecinul de sus
                else if (direction == 2) neighborY++; // Vecinul de jos
                else if (direction == 3) neighborX--; // Vecinul din stânga
                else if (direction == 4) neighborX++; // Vecinul din dreapta

                // Verificăm ponderea vecinului
                double weight = _maze[neighborX, neighborY, 0];

                // Dacă ponderea vecinului este mai mare decât cea mai mare pondere găsită
                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        // Funcție pentru a muta agentul într-o direcție aleasă
        private void MoveInDirection(int direction)
        {
            // Memorează direcția curentă pentru evitarea întoarcerii
            LastDirection = direction;
            OldX = X;
            OldY = Y;
            // Verificăm ce direcție trebuie să luăm
            if (direction == 1) Y--; // Mergem sus
            else if (direction == 2) Y++; // Mergem jos
            else if (direction == 3) X--; // Mergem stânga
            else if (direction == 4) X++; // Mergem dreapta

            // Adăugăm celula curentă la lista de celule vizitate
            visitedCells.Add($"{X},{Y}");

            if (_maze[X, Y, 0] != -2)
            {
                // Actualizăm ponderea celulei curente (scădem ponderea progresiv)
                _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0f); // Scădem ponderea progresiv (minim 0)
            }

            // Adăugăm această celulă la istoricul drumului
            pathHistory.Push(direction);

        }

        private void EvadeDeadEnd()
        {
            // Dacă agentul nu are direcții valabile, se întoarce înapoi
            int lastDirection = pathHistory.Pop(); // Ia ultima direcție parcursă
            LastDirection = lastDirection;
            // Dacă celula a ghidat agentul la dead-end, o vom marca cu ponderea 1
            _maze[X, Y, 0] = 0f; // Setăm ponderea celulei pe drumul de întoarcere la 1
            OldX = X;
            OldY = Y;

            // Verificăm dacă în această celulă există două sau mai multe direcții valide
            List<int> validDirections = GetValidDirections(X, Y);

            if(validDirections.Count == 1 && LastDirection != validDirections[0])
            {
                // Dacă există o singură direcție validă și aceasta nu este direcția din care a venit agentul
                LastDirection = validDirections[0];
            }


            // Mergem înapoi
            if (LastDirection == 1) Y--; // Dacă se deplasa în sus, întoarcem în jos
            else if (LastDirection == 2) Y++; // Dacă se deplasa în jos, întoarcem în sus
            else if (LastDirection == 3) X--; // Dacă se deplasa în stânga, întoarcem în dreapta
            else if (LastDirection == 4) X++; // Dacă se deplasa în dreapta, întoarcem în stânga

            _maze[X, Y, GetOppositeDirection(LastDirection)] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);
            pathHistory.Push(lastDirection);

            // Verificăm dacă în această celulă există două sau mai multe direcții valide
            validDirections = GetValidDirections(X, Y);

            // Dacă există mai multe direcții valide, agentul va opri întoarcerea și va alege o direcție validă
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
            // Dacă agentul nu are direcții valabile, se întoarce înapoi
            int lastDirection = pathHistory.Pop(); // Ia ultima direcție parcursă

            // Dacă celula a ghidat agentul la dead-end, o vom marca cu ponderea 1
            _maze[X, Y, 0] = 0f; // Setăm ponderea celulei pe drumul de întoarcere la 1
            OldX = X;
            OldY = Y;

            // Verificăm dacă în această celulă există două sau mai multe direcții valide
            List<int> validDirections = GetValidDirections(X, Y);

            // Mergem înapoi
            if (LastDirection == 1) Y++; // Dacă se deplasa în sus, întoarcem în jos
            else if (LastDirection == 2) Y--; // Dacă se deplasa în jos, întoarcem în sus
            else if (LastDirection == 3) X++; // Dacă se deplasa în stânga, întoarcem în dreapta
            else if (LastDirection == 4) X--; // Dacă se deplasa în dreapta, întoarcem în stânga

            _maze[X, Y, LastDirection] = 0f;
            _maze[X, Y, 0] = Math.Max(_maze[X, Y, 0] - 0.1f, 0.1f);
            // Redefinim ultima direcție
            LastDirection = GetOppositeDirection(lastDirection);
            pathHistory.Push(LastDirection);

            // Marcați această celulă ca fiind vizitată și modificată
            visitedCells.Add($"{X},{Y}");

            wasDeadEnd = true;

            if (_maze[X, Y, 1] == -3 || _maze[X, Y, 2] == -3 || _maze[X, Y, 3] == -3 || _maze[X, Y, 4] == -3)
                wasDeadEnd = false;
        }

        // Funcție pentru a obține direcția opusă a unei mișcări
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


        public void UpdateCellWeight(int newX, int newY)
        {
            // Creștem ponderea celulei, având grijă să nu depășim 1 (pondere maximă)
            if (_maze[newX, newY, 0] < 1)
            {
                _maze[newX, newY, 0] += 0.1;  // Creștem ponderea cu 0.1 (poate fi ajustat)
            }
        }

        private void StopAgent()
        {
            //Send("Environment", "AgentFinished");
            _mainForm.AgentAtFinish(this);
        }
    }

}
