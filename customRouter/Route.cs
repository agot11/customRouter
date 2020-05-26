using System;

namespace customRouter
{
    public class Route
    {
        public string[] UriSegments { get; set; }
        public Type Handler { get; set; }

        public MatchResult Match(string[] segments)
        {
            if (segments.Length != UriSegments.Length)
            {
                return MatchResult.NoMatch();
            }

            for (int i = 0; i < UriSegments.Length; i++)
            {
                if (!string.Equals(segments[i], UriSegments[i], StringComparison.OrdinalIgnoreCase))
                {
                    return MatchResult.NoMatch();
                }
            }

            return MatchResult.Match(this);
        }
    }
}