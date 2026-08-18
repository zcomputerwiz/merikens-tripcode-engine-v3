using System;

namespace TripEngine.Core
{
    public interface ITripcodeBackend
    {
        string GenerateTripcode(string key, bool isSha1);
    }
}
