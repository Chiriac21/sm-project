using ActressMas;
using MazeProject.Utils;

namespace MazeProject.Agents
{
    public class MazeAgent : Agent
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int OldX { get; private set; }
        public int OldY { get; private set; }

        private int[,] _maze;
        private readonly Random _random;

        public MazeAgent(int[,] maze, int startX, int startY, string name)
        {
            X = startX;
            Y = startY;
            OldX = startX;
            OldY = startY;
            _maze = maze;
            Name = name;
            _random = new Random();
        }

        public override void Setup()
        {

        }

        public override void Act(ActressMas.Message message)
        {

        }

        public override void ActDefault()
        {
            MoveRandomly();
        }

        private void MoveRandomly()
        {
            bool hasMoved = false;
            while (!hasMoved)
            {
                int d = _random.Next(4);
                switch (d)
                {
                    case 0: // up
                        hasMoved = MoveUp();
                        break;
                    
                    case 1: // right
                        hasMoved = MoveRight();
                        break;
                    
                    case 2: // down
                        hasMoved = MoveDown();
                        break;
                    
                    case 3: // left
                        hasMoved = MoveLeft();
                        break;
                }
            }
        }

        private bool MoveUp()
        {
            if (Y - 1 < 0)
                return false;

            if (_maze[X, Y - 1] != (int)MazeCell.Path)
                return false;

            OldX = X;
            OldY = Y;
            Y--;
            return true;
        }

        private bool MoveDown()
        {
            if (Y + 1 >= _maze.GetLength(0))
                return false;
            if (_maze[X, Y + 1] != (int)MazeCell.Path)
                return false;

            OldX = X;
            OldY = Y;
            Y++;
            return true;
        }

        private bool MoveRight()
        {
            if (X + 1 >= _maze.GetLength(1))
                return false;
            if (_maze[X + 1, Y] != (int)MazeCell.Path)
                return false;

            OldX = X;
            OldY = Y;
            X++;
            return true;
        }

        private bool MoveLeft()
        {
            if (X - 1 < 0)
                return false;
            if (_maze[X - 1, Y] != (int)MazeCell.Path)
                return false;

            OldX = X;
            OldY = Y;
            X--;
            return true;
        }
    }
}
