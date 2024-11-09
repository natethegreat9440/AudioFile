using AudioFile.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFile.Model
{
    class TrackIDRegistry
    {
        //TODO: Make sure this class saves to memory

        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackIDRegistry> _instance = new Lazy<TrackIDRegistry>(() => new TrackIDRegistry());

        // Private constructor to prevent instantiation
        private TrackIDRegistry() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackIDRegistry Instance => _instance.Value;

        private static HashSet<string> trackIDs = new HashSet<string>();

        public IEnumerable<string> TrackIDs => trackIDs;

        public string GenerateNewTrackID()
        {
            string newID;
            if (trackIDs.Count == 0)
            {
                newID = "000001";
            }
            else
            {
                var lastID = trackIDs.Max();
                int nextID = int.Parse(lastID) + 1;
                newID = nextID.ToString("D6");
            }
            trackIDs.Add(newID);
            return newID;
        }

        public bool RemoveTrackID(string id)
        {
            return trackIDs.Remove(id);
        }
    }
}
