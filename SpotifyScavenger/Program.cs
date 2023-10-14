using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyScavenger.Interfaces;
using SpotifyScavenger.Models;
using SpotifyScavenger.TrackSources;
using System.Text.RegularExpressions;
using static SpotifyAPI.Web.PlaylistRemoveItemsRequest;

namespace SpotifyScavenger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                RetrieveTrackBot mainLoop = new RetrieveTrackBot();
                mainLoop.MainLoop().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
    public class RetrieveTrackBot
    {

        //private string playlistCode = "1AoQkljjWiQpySZCdkBIoX"; //test playlist
        private string playlistCode = "HARDSTYLE PLAYLIST CODE HERE";
        private string hardcorePlaylistCode = "HARDCORE PLAYLIST CODE HERE";
        private SpotifyClient spotify;

        DateTime currentDate;


        bool checkForDate = true;

        public async Task<bool> MainLoop()
        {
            await Authenticate();

            List<TrackData> data = GetDataFromSources();

            if (!data.Any())
            {
                throw new Exception("No data retrieved from sources");
            }



            List<TrackData> hardcoreData = new List<TrackData>();
            hardcoreData.AddRange(data.Where(item => item.Genre == Genre.Hardcore));
            hardcoreData = hardcoreData.DistinctBy(item => item.TrackName).ToList();

            List<TrackData> hardstyleData = new List<TrackData>();
            hardstyleData.AddRange(data.Where(item => item.Genre == Genre.Hardstyle));
            hardstyleData = hardstyleData.DistinctBy(item => item.TrackName).ToList();

            #region clean hardcore list
            List<PlaylistTrack<FullTrack>> currentItemsInPlaylistHardcore = await GetListOfCurrentItemsInPlaylistHardcore();

            hardcoreData = await DeleteTracksWeAlreadyHave(currentItemsInPlaylistHardcore, hardcoreData);

            List<PlaylistRemoveItemsRequest.Item> itemsToDeleteHardcore = new List<Item>();
            foreach (PlaylistTrack<FullTrack> item in currentItemsInPlaylistHardcore)
            {
                var result = await DeleteTracksOlderThanXDays(item);
                if (result != null)
                {
                    itemsToDeleteHardcore.Add(result);
                }
            }

            await DeleteEntriesFromSpotifyPlaylistHardcore(itemsToDeleteHardcore);
            #endregion

            #region clean hardstyle list
            List<PlaylistTrack<FullTrack>> currentItemsInPlaylistHardstyle = await GetListOfCurrentItemsInPlaylistHardstyle();

            hardstyleData = await DeleteTracksWeAlreadyHave(currentItemsInPlaylistHardstyle, hardstyleData);

            List<PlaylistRemoveItemsRequest.Item> itemsToDeleteHardstyle = new List<Item>();
            foreach (PlaylistTrack<FullTrack> item in currentItemsInPlaylistHardstyle)
            {
                var result = await DeleteTracksOlderThanXDays(item);
                if (result != null)
                {
                    itemsToDeleteHardstyle.Add(result);
                }
            }

            await DeleteEntriesFromSpotifyPlaylistHardstyle(itemsToDeleteHardstyle);
            #endregion

            currentDate = GetProperTimeZone();
            Console.WriteLine($"Searching for tracks with the following date: {currentDate.Day}-{currentDate.Month}-{currentDate.Year}");

            data = new List<TrackData>();
            data.AddRange(hardcoreData);
            data.AddRange(hardstyleData);

            //data.Reverse();
            int maximumAmountOfFails = 30;
            int currentlyFailed = 0;

            foreach (TrackData item in data)
            {

                try
                {



                    bool trackFound = false;
                    List<string> artists = GetListOfArtistsFromString(item);

                    Thread.Sleep(1000);
                    var result222 = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, item.TrackName + " " + item.ArtistName));

                    if (result222.Tracks.Items.Any())
                    {
                        FullTrack foundTrack = null;
                        if (checkForDate)
                        {
                            int year = currentDate.Year;
                            int day = currentDate.Day;
                            int month = currentDate.Month;


                            foreach (var track in result222.Tracks.Items)
                            {
                                if (DateTime.TryParse(track.Album.ReleaseDate, out DateTime value))
                                {
                                    if (value.Year == year && value.Month == month && value.Day == day)
                                    {
                                        foundTrack = track;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foundTrack = result222.Tracks.Items.FirstOrDefault();
                        }




                        if (foundTrack != null)
                        {
                            foreach (string artist in artists)
                            {
                                SimpleArtist bestMatch = null;
                                if (foundTrack != null)
                                {
                                    bestMatch = foundTrack.Artists.FirstOrDefault(item => item.Name.ToLower().Trim() == artist.ToLower().Trim());
                                }
                                if (bestMatch == null)
                                {
                                    bestMatch = CheckForWildCardMatch(artist, foundTrack.Artists);
                                }

                                if (bestMatch != null)
                                {
                                    Console.WriteLine($"Found a track! {foundTrack.Name}");

                                   
                                    await AddTrackToPlaylist(foundTrack.Uri, item.Genre);
                                      

                                    trackFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (trackFound)
                    {
                        continue;
                    }


                    Thread.Sleep(1000);
                    var result223 = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, item.TrackName + " " + artists.FirstOrDefault()));

                    if (result223.Tracks.Items.Any())
                    {
                        FullTrack foundTrack = null;
                        if (checkForDate)
                        {
                            int year = currentDate.Year;
                            int day = currentDate.Day;
                            int month = currentDate.Month;


                            foreach (var track in result223.Tracks.Items)
                            {
                                if (DateTime.TryParse(track.Album.ReleaseDate, out DateTime value))
                                {
                                    if (value.Year == year && value.Month == month && value.Day == day)
                                    {
                                        foundTrack = track;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foundTrack = result223.Tracks.Items.FirstOrDefault();
                        }




                        if (foundTrack != null)
                        {
                            foreach (string artist in artists)
                            {
                                SimpleArtist bestMatch = null;
                                if (foundTrack != null)
                                {
                                    bestMatch = foundTrack.Artists.FirstOrDefault(item => item.Name.ToLower().Trim() == artist.ToLower().Trim());
                                }
                                if (bestMatch == null)
                                {
                                    bestMatch = CheckForWildCardMatch(artist, foundTrack.Artists);
                                }

                                if (bestMatch != null)
                                {
                                    Console.WriteLine($"Found a track! {foundTrack.Name}");


                                    await AddTrackToPlaylist(foundTrack.Uri, item.Genre);


                                    trackFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (trackFound)
                    {
                        continue;
                    }


                    foreach (string artist in artists)
                    {
                        FullArtist artistFound = await SearchForArtist(artist);
                        Thread.Sleep(1000);
                        if (artistFound == null) continue;
                        List<SimpleAlbum> listOfAlbumsFromArtist = await GetListOfAlbumsFromArtist(artistFound.Id);
                        Thread.Sleep(1000);
                        SimpleAlbum albumOfToday = await GetAlbumOfTodayFromListOfAlbums(listOfAlbumsFromArtist);

                        if (albumOfToday == null) continue;
                        Thread.Sleep(1000);

                        var tracksOfAlbum = await spotify.Albums.GetTracks(albumOfToday.Id);

                        foreach (var track in tracksOfAlbum.Items)
                        {
                            if (track.Name.ToLower().Trim() == item.TrackName.ToLower().Trim())
                            {

                               
                                await AddTrackToPlaylist(track.Uri, item.Genre);
                                  

                                Console.WriteLine($"Found a match: {track.Name} with {item.TrackName}");
                                trackFound = true;
                                break;
                            }
                            else
                            {
                                string result = await GetTrackUriWildCatchMatch(item.TrackName, track);
                                if (result != null)
                                {
                                    await AddTrackToPlaylist(track.Uri, item.Genre);
                                    Console.WriteLine($"Found a match: {track.Name} with {item.TrackName}");
                                    trackFound = true;
                                    break;
                                }
                            }
                        }


                        if (trackFound)
                        {
                            break;
                        }

                    }

                    if (trackFound)
                    {
                        continue;
                    }

                    if (!trackFound)
                    {
                        FullArtist artistFound = await SearchForArtist(item.ArtistName);
                        Thread.Sleep(1000);
                        if (artistFound == null)
                        {
                            Console.WriteLine($"Track not found! {item.TrackName} - {item.ArtistName} for date {currentDate.Day}-{currentDate.Month}-{currentDate.Year}");
                            continue;
                        }
                        List<SimpleAlbum> listOfAlbumsFromArtist = await GetListOfAlbumsFromArtist(artistFound.Id);
                        Thread.Sleep(1000);
                        SimpleAlbum albumOfToday = await GetAlbumOfTodayFromListOfAlbums(listOfAlbumsFromArtist);

                        if (albumOfToday == null)
                        {
                            Console.WriteLine($"Track not found! {item.TrackName} - {item.ArtistName} for date {currentDate.Day}-{currentDate.Month}-{currentDate.Year}");
                            continue;
                        }
                        Thread.Sleep(1000);

                        var tracksOfAlbum = await spotify.Albums.GetTracks(albumOfToday.Id);

                        foreach (var track in tracksOfAlbum.Items)
                        {
                            if (track.Name.ToLower().Trim() == item.TrackName.ToLower().Trim())
                            {

                              
                                await AddTrackToPlaylist(track.Uri, item.Genre);
  

                                Console.WriteLine($"Found a match: {track.Name} with {item.TrackName}");
                                trackFound = true;
                                break;
                            }
                            else
                            {
                                string result = await GetTrackUriWildCatchMatch(item.TrackName, track);
                                if (result != null)
                                {
                                  
                                    await AddTrackToPlaylist(track.Uri, item.Genre);
                                       

                                    Console.WriteLine($"Found a match: {track.Name} with {item.TrackName}");
                                    trackFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!trackFound)
                    {
                        Console.WriteLine($"Track not found! {item.TrackName} - {item.ArtistName}");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Got an error, gonna retry in 60 seconds: {ex.Message}");
                    Thread.Sleep(60000);
                    currentlyFailed += 1;
                    if(currentlyFailed >= maximumAmountOfFails)
                    {
                        throw new Exception($"Failed a total of {currentlyFailed} times, stopping");
                    }
                }
            }

            return false;
        }

        public List<TrackData> GetDataFromSources()
        {
            List<IScavenge> listOfSources = new List<IScavenge>();
            //listOfSources.Add(new HardtunesScavenger());
            //listOfSources.Add(new HardstyleDotComScavengerHardcore());
            listOfSources.Add(new HardstyleDotComScavengerHardstyle());

            List<TrackData> trackData = new List<TrackData>();
            foreach (IScavenge scavengeSource in listOfSources)
            {
                trackData.AddRange(scavengeSource.ScavengeForTracks());
            }

            return trackData;
        }

        public async Task AddTrackToPlaylist(string trackUri, Genre genre)
        {
            var request = new PlaylistAddItemsRequest(new List<string> { trackUri });
            request.Position = 0;

            if (genre == Genre.Hardstyle)
            {
                var bbb = await spotify.Playlists.AddItems(playlistCode, request);
            }
            else if (genre == Genre.Hardcore)
            {
                var bbb = await spotify.Playlists.AddItems(hardcorePlaylistCode, request);
            }

        }

        public async Task Authenticate()
        {
            string clientId = "CLIENT ID HERE";
            string clientSecret = "CLIENT SECRET HERE";
            string refreshToken = "REFRESH TOKEN HERE, VALID FOREVER!";

            AuthorizationCodeRefreshResponse newResponse = await new OAuthClient().RequestToken(new AuthorizationCodeRefreshRequest(clientId, clientSecret, refreshToken));

            spotify = new SpotifyClient(newResponse.AccessToken);
        }

        public async Task<List<PlaylistTrack<FullTrack>>> GetListOfCurrentItemsInPlaylistHardstyle()
        {
            Paging<PlaylistTrack<IPlayableItem>>? currPlaylist = await spotify.Playlists.GetItems(playlistCode);
            List<PlaylistTrack<IPlayableItem>> itemsToAppend = new List<PlaylistTrack<IPlayableItem>>();
            await foreach (var item in spotify.Paginate(currPlaylist))
            {
                itemsToAppend.Add(item);
                // you can use "break" here!
            }


            var serializeMe = JsonConvert.SerializeObject(itemsToAppend);
            List<PlaylistTrack<FullTrack>> itemz = JsonConvert.DeserializeObject<List<PlaylistTrack<FullTrack>>>(serializeMe);
            return itemz;
        }

        public async Task<List<PlaylistTrack<FullTrack>>> GetListOfCurrentItemsInPlaylistHardcore()
        {
            Paging<PlaylistTrack<IPlayableItem>>? currPlaylist = await spotify.Playlists.GetItems(hardcorePlaylistCode);

            var serializeMe = JsonConvert.SerializeObject(currPlaylist.Items);
            List<PlaylistTrack<FullTrack>> itemz = JsonConvert.DeserializeObject<List<PlaylistTrack<FullTrack>>>(serializeMe);
            return itemz;
        }

        public async Task<List<TrackData>> DeleteTracksWeAlreadyHave(List<PlaylistTrack<FullTrack>> itemsToPossiblyDelete, List<TrackData> itemsRetrievedFromSources)
        {
            foreach (PlaylistTrack<FullTrack> itemToPossiblyDelete in itemsToPossiblyDelete)
            {
                TrackData itemAlreadyExists = itemsRetrievedFromSources.FirstOrDefault(item => itemToPossiblyDelete.Track?.Name.ToLower().Trim().Contains(item.TrackName.ToLower().Trim()) == true);
                if (itemAlreadyExists != null)
                {
                    itemsRetrievedFromSources.Remove(itemAlreadyExists);
                    continue;
                }
                else
                {
                    //double check!
                    TrackData possibleMatch = CheckForWildCardMatch(itemToPossiblyDelete.Track?.Name, itemsRetrievedFromSources);
                    if (possibleMatch != null)
                    {
                        itemsRetrievedFromSources.Remove(possibleMatch);
                        continue;
                    }
                }
                Console.WriteLine($"Unable to find track in playlist: {itemToPossiblyDelete.Track.Name} - {itemToPossiblyDelete.Track.Artists.FirstOrDefault().Name}");
            }

            return itemsRetrievedFromSources;
        }

        public async Task<PlaylistRemoveItemsRequest.Item> DeleteTracksOlderThanXDays(PlaylistTrack<FullTrack> item)
        {
            if (item.AddedAt < DateTime.Now.AddDays(-7))
            {
                return new PlaylistRemoveItemsRequest.Item { Uri = item.Track.Uri };
            }
            return null;
        }

        public async Task DeleteEntriesFromSpotifyPlaylistHardcore(List<PlaylistRemoveItemsRequest.Item> itemsToRemove)
        {
            await spotify.Playlists.RemoveItems(hardcorePlaylistCode, new PlaylistRemoveItemsRequest { Tracks = itemsToRemove });
        }

        public async Task DeleteEntriesFromSpotifyPlaylistHardstyle(List<PlaylistRemoveItemsRequest.Item> itemsToRemove)
        {
            await spotify.Playlists.RemoveItems(playlistCode, new PlaylistRemoveItemsRequest { Tracks = itemsToRemove });
        }

        public DateTime GetProperTimeZone()
        {
            TimeZoneInfo NLtimezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime dateNow = DateTime.UtcNow;

            return TimeZoneInfo.ConvertTimeFromUtc(dateNow, NLtimezone);
        }


        public List<string> GetListOfArtistsFromString(TrackData track)
        {
            //List of strings to split by
            String[] delimiters = { " & ", " and ", " featuring ", " features ", ",", " ft. " };

            List<string> artists = track.ArtistName.Split(delimiters, StringSplitOptions.None).ToList();

            return artists;
        }


        public async Task<FullArtist> SearchForArtist(string artistName)
        {
            var searchRequest = new SearchRequest(SearchRequest.Types.Artist, artistName);
            var response = await spotify.Search.Item(searchRequest);

            FullArtist bestMatch = null;
            if (response != null)
            {
                bestMatch = response.Artists.Items.FirstOrDefault(item => item.Name.ToLower().Trim() == artistName.ToLower().Trim());
            }
            if (bestMatch == null)
            {
                bestMatch = CheckForWildCardMatch(artistName, response.Artists.Items);
            }

            return bestMatch;
        }

        public async Task<List<SimpleAlbum>> GetListOfAlbumsFromArtist(string artistId)
        {
            List<SimpleAlbum> albumsRetrieved = new List<SimpleAlbum>();

            var albums = await spotify.Artists.GetAlbums(artistId);
            await foreach (var item in spotify.Paginate(albums))
            {
                albumsRetrieved.Add(item);
                // you can use "break" here!
            }

            return albumsRetrieved;
        }

        public async Task<SimpleAlbum> GetAlbumOfTodayFromListOfAlbums(List<SimpleAlbum> albums)
        {
            SimpleAlbum foundAlbum = null;
            if (checkForDate)
            {
                int year = currentDate.Year;
                int month = currentDate.Month;
                int day = currentDate.Day;

                foreach (var album in albums)
                {
                    if (DateTime.TryParse(album.ReleaseDate, out DateTime releasedate))
                    {
                        if (releasedate.Year == year && releasedate.Month == month && releasedate.Day == day)
                        {
                            foundAlbum = album;
                            break;
                        }
                    }
                }
            }
            else
            {
                foundAlbum = albums.FirstOrDefault();
            }



            return foundAlbum;
        }

        public SimpleArtist CheckForWildCardMatch(string artistName, List<SimpleArtist> artistsRetrieved)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -']");
            var newString = rgx.Replace(artistName.ToLower().Trim(), "");
            newString = newString.Replace("  ", " ");
            List<string> indivialWordsSpotifyTrack = newString.Split(" ").ToList();
            foreach (SimpleArtist artistResult in artistsRetrieved.ToList())
            {
                string possibleItem = rgx.Replace(artistResult.Name.ToLower().Trim(), " ");
                List<string> individualWords = possibleItem.Split(" ").ToList();
                if (indivialWordsSpotifyTrack.Intersect(individualWords).Count() == indivialWordsSpotifyTrack.Count())
                {
                    //perfect match
                    return artistResult;
                }
            }

            return null;
        }

        public FullArtist CheckForWildCardMatch(string artistName, List<FullArtist> artistsRetrieved)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -']");
            var newString = rgx.Replace(artistName.ToLower().Trim(), "");
            newString = newString.Replace("  ", " ");
            List<string> indivialWordsSpotifyTrack = newString.Split(" ").Distinct().ToList();
            foreach (FullArtist artistResult in artistsRetrieved.ToList())
            {
                string possibleItem = rgx.Replace(artistResult.Name.ToLower().Trim(), "");
                List<string> individualWords = possibleItem.Split(" ").Distinct().ToList();
                if (indivialWordsSpotifyTrack.Intersect(individualWords).Count() == indivialWordsSpotifyTrack.Count())
                {
                    //perfect match
                    return artistResult;
                }
            }

            return null;
        }

        public async Task<string> GetTrackUriWildCatchMatch(string nameOfTrack, SimpleTrack item)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var itemName = item.Name.ToLower().Trim().Replace('´', '\'');
            var newString = rgx.Replace(item.Name.ToLower().Trim(), "");
            newString = newString.Replace("  ", " ");
            List<string> indivialWordsSpotifyTrack = newString.Split(" ").Distinct().ToList();

            string possibleItem = rgx.Replace(nameOfTrack.ToLower().Trim(), "");
            List<string> individualWords = possibleItem.Split(" ").Distinct().ToList();
            if (indivialWordsSpotifyTrack.Intersect(individualWords).Count() == indivialWordsSpotifyTrack.Count())
            {
                return item.Uri;
            }

            return null;
        }


        public TrackData CheckForWildCardMatch(string trackName, List<TrackData> itemsWeRetrieved)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -']");
            var newString = rgx.Replace(trackName.ToLower().Trim(), "");
            newString = newString.Replace("  ", " ");
            List<string> indivialWordsSpotifyTrack = newString.Split(" ").Distinct().ToList();

            indivialWordsSpotifyTrack = indivialWordsSpotifyTrack.Select(s => s.Trim()).ToList();

            foreach (TrackData trackDataItem in itemsWeRetrieved.ToList())
            {
                string possibleItem = rgx.Replace(trackDataItem.TrackName.ToLower().Trim(), "");
                List<string> individualWords = possibleItem.Split(" ").Distinct().ToList();

                individualWords = individualWords.Select(s => s.Trim()).ToList();

                if (indivialWordsSpotifyTrack.Intersect(individualWords).Count() == indivialWordsSpotifyTrack.Count())
                {
                    //perfect match
                    return trackDataItem;
                }
            }

            return null;
        }
    }
}