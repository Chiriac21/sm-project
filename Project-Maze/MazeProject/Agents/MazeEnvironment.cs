using ActressMas;

namespace MazeProject.Agents
{
    public class MazeEnvironment : EnvironmentMas
    {
        public delegate void HandleMove();
        public event HandleMove? OnAgentMoveEvent;

        public MazeEnvironment(int noTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random? rand = null, bool parallel = true)
            : base(noTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
        }

        public override void TurnFinished(int turn)
        {
            OnAgentMoveEvent?.Invoke();
        }
    }
}
