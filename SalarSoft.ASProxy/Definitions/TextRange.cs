namespace SalarSoft.ASProxy
{
    /// <summary>
    /// Range of a string in main text
    /// </summary>
    public struct TextRange
    {
        public int Start;
        public int End;
        public TextRange(int start, int end)
        {
            Start = start;
            End = end;
        }
        public TextRange(int start)
        {
            Start = start;
            End = -1;
        }
        public bool IsEqual(TextRange txtRange)
        {
            return (txtRange.End == End && txtRange.Start == Start);
        }
    }

}