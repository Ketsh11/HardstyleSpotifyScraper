using SpotifyScavenger.Models;

namespace SpotifyScavenger.Tests
{
    [TestClass]
    public class UnitTest1
    {
        RetrieveTrackBot program;
        [TestInitialize]
        public void SetupTests()
        {
            program = new RetrieveTrackBot();
        }

        [TestMethod]
        public void CheckForWildCardMatchAdditionalData()
        {
            List<TrackData> trackData = new List<TrackData>();
            trackData.Add(new TrackData { ArtistName = "Anderex, Imperatorz", TrackName = "Stuck In My Head!" });
            trackData.Add(new TrackData { ArtistName = "Dual Damage, Bmberjck, MC Siqnal", TrackName = "Total Demolition (Official Get Wrecked Anthem 2023)" });

            TrackData trackFound = program.CheckForWildCardMatch("Total Demolition", trackData);

            Assert.IsNotNull(trackFound);
        }

        [TestMethod]
        public void CheckForWildCardMatchMatchingData()
        {
            List<TrackData> trackData = new List<TrackData>();
            trackData.Add(new TrackData { ArtistName = "Anderex, Imperatorz", TrackName = "Stuck In My Head!" });
            trackData.Add(new TrackData { ArtistName = "Dual Damage, Bmberjck, MC Siqnal", TrackName = "Total Demolition (Official Get Wrecked Anthem 2023)" });

            TrackData trackFound = program.CheckForWildCardMatch("Stuck in my head!", trackData);

            Assert.IsNotNull(trackFound);
        }
    }
}