namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftResult
    {
        public ShiftResult(ShiftState state, int score, int coins, int correctSorts, int mistakes)
        {
            State = state;
            Score = score;
            Coins = coins;
            CorrectSorts = correctSorts;
            Mistakes = mistakes;
        }

        public ShiftState State { get; }

        public int Score { get; }

        public int Coins { get; }

        public int CorrectSorts { get; }

        public int Mistakes { get; }
    }
}

