namespace Gobang.Model
{
    public class ValuedPoint
    {
        public Point Point { get; set; }

        public int Score { get; set; }
    }

    public struct Point
    {
        public int Row { get; set; }
        public int Cell { get; set; }
    }
}