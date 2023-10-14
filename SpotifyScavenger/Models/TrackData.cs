using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyScavenger.Models
{
    public class TrackData
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public DateTime TrackDate { get; set; }
        public Genre Genre { get; set; }
    }

    public enum Genre
    {
        Unknown = 0,
        Hardstyle = 1,
        Hardcore = 2,
    }
}
