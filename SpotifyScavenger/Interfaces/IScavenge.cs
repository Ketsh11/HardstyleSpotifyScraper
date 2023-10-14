using SpotifyScavenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyScavenger.Interfaces
{
    public interface IScavenge
    {
        public List<TrackData> ScavengeForTracks();
    }
}
