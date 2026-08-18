namespace TripEngine.Core
{
    public interface IPatternMatcher
    {
        bool IsMatch(string tripcode);
    }
}
