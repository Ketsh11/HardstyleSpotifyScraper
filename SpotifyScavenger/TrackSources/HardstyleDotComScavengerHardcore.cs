using HtmlAgilityPack;
using SpotifyScavenger.Interfaces;
using SpotifyScavenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyScavenger.TrackSources
{
    public class HardstyleDotComScavengerHardcore : IScavenge
    {
        public List<TrackData> ScavengeForTracks()
        {
            List<TrackData> trackData = new List<TrackData>();
            HtmlWeb web = new HtmlWeb();

            HtmlDocument documentAlbums = web.Load(@"https://music.hardstyle.com/hardcore-releases/albums");
            IEnumerable<HtmlNode> nodes2 =
                    documentAlbums.DocumentNode.Descendants("td")
                        .Where(n => n.HasClass("text-1"));

            int maxAmountOfAlbums = 10;
            int currAmountOfAlbums = 0;
            foreach (var node in nodes2)
            {
                currAmountOfAlbums += 1;
                if(maxAmountOfAlbums == currAmountOfAlbums)
                {
                    break;
                }


                string z = Between(node.InnerHtml, "href=\"", "\">");

                Thread.Sleep(1000);
                HtmlDocument albumDetail = web.Load(z);

                IEnumerable<HtmlNode> nodes3 =
                   albumDetail.DocumentNode.Descendants("td")
                       .Where(n => n.HasClass("text-1"));

                var listOfNodes2 = nodes3.Select(item => item.FirstChild).ToList();

                long firstValue = 0;
                foreach (var node2 in listOfNodes2)
                {
                    if (firstValue == 0)
                    {
                        string compareMe = Between(node2.OuterHtml, "href=\"", "\"><b class");

                        int indexCompareMe = compareMe.LastIndexOf('/');
                        compareMe = compareMe.Substring(indexCompareMe + 1, compareMe.Length - indexCompareMe - 1);
                        long value1 = long.Parse(compareMe);
                        firstValue = value1;
                    }
                    else
                    {
                        string compareMe = Between(node2.OuterHtml, "href=\"", "\"><b class");

                        int indexCompareMe = compareMe.LastIndexOf('/');
                        compareMe = compareMe.Substring(indexCompareMe + 1, compareMe.Length - indexCompareMe - 1);
                        long value1 = long.Parse(compareMe);

                        if (firstValue != value1)
                        {
                            continue;
                        }

                    }


                    firstValue += 1;

                    int nodeElement = 0;
                    TrackData track = new TrackData();
                    track.Genre = Genre.Hardcore;
                    foreach (var htmlNode in node2.ChildNodes)
                    {

                        nodeElement++;
                        if (htmlNode.Name == "b" && nodeElement % 2 == 1)
                        {
                            track.TrackName = htmlNode.InnerHtml;
                            Console.WriteLine($"Title: {htmlNode.InnerHtml}");
                        }
                        else if (htmlNode.Name == "span")
                        {
                            track.ArtistName = htmlNode.InnerHtml;
                            Console.WriteLine($"Artist: {htmlNode.InnerHtml}");
                        }


                    }
                    trackData.Add(track);
                }

            }


            for (int i = 1; i < 3; i++)
            {
                Thread.Sleep(1000);
                //Loop over a couple of pages (just in case a ton of music gets dropped in a single day)
                HtmlDocument document = web.Load(@"https://music.hardstyle.com/hardcore-releases/tracks/page/" + i.ToString());

                IEnumerable<HtmlNode> nodes =
                    document.DocumentNode.Descendants("td")
                        .Where(n => n.HasClass("text-1"));

                var listOfNodes = nodes.Select(item => item.FirstChild).ToList();

                foreach (var node in listOfNodes)
                {
                    int nodeElement = 0;
                    TrackData track = new TrackData();
                    track.Genre = Genre.Hardcore;
                    foreach (var htmlNode in node.ChildNodes)
                    {

                        nodeElement++;
                        if (htmlNode.Name == "b" && nodeElement % 2 == 1)
                        {
                            track.TrackName = htmlNode.InnerHtml;
                            Console.WriteLine($"Title: {htmlNode.InnerHtml}");
                        }
                        else if (htmlNode.Name == "span")
                        {
                            track.ArtistName = htmlNode.InnerHtml;
                            Console.WriteLine($"Artist: {htmlNode.InnerHtml}");
                        }


                    }
                    trackData.Add(track);
                }
            }

            Thread.Sleep(1000);


            trackData = trackData.DistinctBy(item => item.TrackName).ToList();

            Console.WriteLine($"retrieved a total of {trackData.Count()} entries from hardstyle.com");


            //Distinct, so we don't accidently get double tracks
            return trackData;
        }

        public string Between(string Text, string FirstString, string LastString)

        {

            string STR = Text;

            string STRFirst = FirstString;

            string STRLast = LastString;

            string FinalString;

            string TempString;



            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;

            int Pos2 = STR.IndexOf(LastString);

            FinalString = STR.Substring(Pos1, Pos2 - Pos1);

            return FinalString;
        }
    }
}
