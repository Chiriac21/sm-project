namespace MazeProject.Utils
{
    public static class MazeGenerator
    {
        /*
         3D Matrix index meaning:
         Index 0 - state of current cell (wall, path, start, exit)
         Index 1 - "Up" neighbor
         Index 2 - "Down" neighbor
         Index 3 - "Left" neighbor
         Index 4 - "Right" neighbor
         */
        public static int[,,] GenerateMaze(int width, int height, out int startX, out int startY, Action<int> progressCallback, int seed = 0)
        {
            int actualWidth = width;
            int actualHeight = height;

            int totalSteps = width * height / 2;
            int currentStep = 0;

            // verify dimensions
            if (actualWidth < 3)
                actualWidth = 3;
            if (actualHeight < 3)
                actualHeight = 3;
            if (actualWidth % 2 == 0)
                actualWidth++;
            if (actualHeight % 2 == 0)
                actualHeight++;

            int[,] maze = new int[actualWidth, actualHeight];

            Random random = new Random(seed);
            if (seed == 0)
                random = new Random();
            List<int[]> frontierCells = new List<int[]>();

            // initialize the maze with walls
            for (int i = 0; i < actualWidth; i++)
                for (int j = 0; j < actualHeight; j++)
                { 
                    maze[i, j] = (int)MazeCell.Wall;
                }

            // choose a random starting point and add it as a frontier cell
            int initX = random.Next(1, actualWidth);
            int initY = random.Next(1, actualHeight);
            while (initX % 2 == 0)
                initX = random.Next(1, actualWidth);
            while (initY % 2 == 0)
                initY = random.Next(1, actualHeight);

            frontierCells.Add(new int[] { initX, initY, initX, initY });

            // compute all frontier cells
            while (frontierCells.Any())
            {
                int[] frontierCell = frontierCells[random.Next(frontierCells.Count)];
                frontierCells.Remove(frontierCell);
                int x = frontierCell[2];
                int y = frontierCell[3];
                int ix = frontierCell[0];
                int iy = frontierCell[1];

                if (maze[x, y] == (int)MazeCell.Wall)
                {
                    maze[x, y] = (int)MazeCell.Path;
                    maze[ix, iy] = (int)MazeCell.Path;

                    if (x > 2 && maze[x - 2, y] == (int)MazeCell.Wall)
                        frontierCells.Add(new int[] { x - 1, y, x - 2, y });

                    if (y > 2 && maze[x, y - 2] == (int)MazeCell.Wall)
                        frontierCells.Add(new int[] { x, y - 1, x, y - 2 });

                    if (x < actualWidth - 3 && maze[x + 2, y] == (int)MazeCell.Wall)
                        frontierCells.Add(new int[] { x + 1, y, x + 2, y });

                    if (y < actualHeight - 3 && maze[x, y + 2] == (int)MazeCell.Wall)
                        frontierCells.Add(new int[] { x, y + 1, x, y + 2 });
                }

                currentStep++;
                int progress = (int)((currentStep / (float)totalSteps) * 100);
                // Call the progress callback to update the progress bar
                progressCallback?.Invoke(progress);

                // Optionally, introduce a small delay to visualize the progress (remove in production)
                System.Threading.Thread.Sleep(1);
            }

            int exitX, exitY;
            int randomSide = random.Next(3);
            switch (randomSide)
            {
                case 0:
                    exitY = 0;
                    exitX = random.Next(actualWidth);
                    while (maze[exitX, exitY + 1] != (int)MazeCell.Path)
                        exitX = random.Next(1, actualWidth);

                    startY = actualHeight - 1;
                    startX = random.Next(actualWidth);
                    while (maze[startX, startY - 1] != (int)MazeCell.Path)
                        startX = random.Next(1, actualWidth);
                    break;

                case 1:
                    exitX = actualWidth - 1;
                    exitY = random.Next(actualHeight);
                    while (maze[exitX - 1, exitY] != (int)MazeCell.Path)
                        exitY = random.Next(actualHeight);

                    startX = 0;
                    startY = random.Next(actualHeight);
                    while (maze[startX + 1, startY] != (int)MazeCell.Path)
                        startY = random.Next(actualHeight);
                    break;

                case 2:
                    exitY = actualHeight - 1;
                    exitX = random.Next(actualWidth);
                    while (maze[exitX, exitY - 1] != (int)MazeCell.Path)
                        exitX = random.Next(1, actualWidth);

                    startY = 0;
                    startX = random.Next(actualWidth);
                    while (maze[startX, startY + 1] != (int)MazeCell.Path)
                        startX = random.Next(1, actualWidth);
                    break;

                default:
                    exitX = 0;
                    exitY = random.Next(actualHeight);
                    while (maze[exitX + 1, exitY] != (int)MazeCell.Path)
                        exitY = random.Next(actualHeight);

                    startX = actualWidth - 1;
                    startY = random.Next(actualHeight);
                    while (maze[startX - 1, startY] != (int)MazeCell.Path)
                        startY = random.Next(actualHeight);
                    break;

            }

            maze[exitX, exitY] = (int)MazeCell.Exit;
            maze[startX, startY] = (int)MazeCell.Start;

            // Create 3D matrix for neighbors
            int[,,] mazeWithNeighbors = new int[actualWidth, actualHeight, 5];
            for (int i = 0; i < actualWidth; i++)
            {
                for (int j = 0; j < actualHeight; j++)
                {
                    // Cell value
                    mazeWithNeighbors[i, j, 0] = maze[i, j];

                    // Neighbors: Carefully handle boundaries
                    mazeWithNeighbors[i, j, 1] = (j > 0) ? maze[i, j - 1] : (int)MazeCell.Wall;                // Up
                    mazeWithNeighbors[i, j, 2] = (j < actualHeight - 1) ? maze[i, j + 1] : (int)MazeCell.Wall; // Down
                    mazeWithNeighbors[i, j, 3] = (i > 0) ? maze[i - 1, j] : (int)MazeCell.Wall;                // Left
                    mazeWithNeighbors[i, j, 4] = (i < actualWidth - 1) ? maze[i + 1, j] : (int)MazeCell.Wall;  // Right
                }
            }

            return mazeWithNeighbors;
        }

    }

    public enum MazeCell
    {
        Path = 0, 
        Wall = 1, 
        Exit = 2,
        Start = 3
    }
}
