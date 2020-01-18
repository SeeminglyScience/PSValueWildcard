namespace PSValueWildcard
{
    internal ref partial struct WildcardInterpreter
    {
        private struct Frame
        {
            public Range Position;

            public bool CanBacktrackTo;

            public bool PreceededByWildcard;
        }
    }
}
