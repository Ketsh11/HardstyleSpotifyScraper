using HtmlAgilityPack;
using SpotifyScavenger.Interfaces;
using SpotifyScavenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpotifyScavenger.TrackSources
{
    public class HardtunesScavenger : IScavenge
    {
        public List<TrackData> ScavengeForTracks()
        {
            List<TrackData> trackData = new List<TrackData>();
            HtmlWeb web = new HtmlWeb();
            for (int i = 1; i < 2; i++)
            {
                Thread.Sleep(1000);
                //Loop over a couple of pages (just in case a ton of music gets dropped in a single day)
                HtmlDocument document = web.Load(@"https://www.hardtunes.com/hardcore/page/" + i.ToString());

                IEnumerable<HtmlNode> nodes =
                    document.DocumentNode.Descendants(0)
                        .Where(n => n.HasClass("release-list-item"));

                bool zzz = false;

                foreach (var node in nodes)
                {


                    HtmlNode? infoNodeMainScreen = node.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info").FirstOrDefault();
                    HtmlNode? infoNode2MainScreen = infoNodeMainScreen.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info-secondary").FirstOrDefault();
                    HtmlNode? almostThereNode = infoNode2MainScreen.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-type").FirstOrDefault();
                    string albumType = almostThereNode.ChildNodes.FirstOrDefault().InnerHtml;

                    if (albumType.ToLower() != "single tune")
                    {
                        var child = node.ChildNodes.FirstOrDefault().ChildNodes.FirstOrDefault();
                        string url = child.Attributes["href"].Value;

                        Thread.Sleep(500);
                        HtmlDocument documentDetailed = web.Load(url);

                        IEnumerable<HtmlNode> nodesDetailed =
                       documentDetailed.DocumentNode.Descendants(0)
                           .Where(n => n.HasClass("col-xs-6") && n.HasClass("release-list-item"));

                        foreach (var node2 in nodesDetailed)
                        {
                            HtmlNode? infoNode = node2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info").FirstOrDefault();
                            HtmlNode? infoNode2 = infoNode.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info-secondary").FirstOrDefault();
                            var date = infoNode2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-release-date").FirstOrDefault().InnerHtml;

                            HtmlNode? infoNode3 = infoNode.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info-primary").FirstOrDefault();
                            var titleName = infoNode3.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-title").FirstOrDefault().ChildNodes[0].InnerHtml;
                            var artistName = infoNode3.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-artist").FirstOrDefault().ChildNodes[0].InnerHtml;

                            if (date.ToLower() == "today")
                            {
                                trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now });
                            }
                            else if (date.ToLower() == "yesterday")
                            {
                                trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-1) });
                            }
                            else if (date.ToLower() == "this week")
                            {
                                trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-4) });
                            }
                            else if (date.ToLower() == "last week")
                            {
                                trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-8) });
                            }
                            else
                            {
                                //trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.ParseExact(date, "dd.MM.yyyy", null) });
                            }

                        }
                    }
                    else
                    {
                        HtmlNode? infoNodeMainScreen2 = node.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info").FirstOrDefault();
                        HtmlNode? infoNode2MainScreen2 = infoNodeMainScreen2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info-primary").FirstOrDefault();
                        var titleName = infoNode2MainScreen2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-title").FirstOrDefault().InnerHtml;
                        var artistName = infoNode2MainScreen2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-artist").FirstOrDefault().InnerHtml;

                        HtmlNode? infoNode2 = infoNodeMainScreen2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-info-secondary").FirstOrDefault();
                        var date = infoNode2.ChildNodes.Where(item => item.Attributes["class"].Value == "release-list-item-release-date").FirstOrDefault().InnerHtml;

                        if (date.ToLower() == "today")
                        {
                            trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now });
                        }
                        else if (date.ToLower() == "yesterday")
                        {
                            trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-1) });
                        }
                        else if(date.ToLower() == "this week")
                        {
                            trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-4) });
                        }
                        else if (date.ToLower() == "last week")
                        {
                            trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.Now.AddDays(-8) });
                        }
                        else
                        {
                            //trackData.Add(new TrackData { ArtistName = artistName.Replace("&nbsp;", " ").Trim(), TrackName = titleName.Replace("&nbsp;", " ").Trim(), Genre = Genre.Hardcore, TrackDate = DateTime.ParseExact(date, "dd.MM.yyyy", null) });
                        }
                    }

                }
            }
            foreach(TrackData item in trackData.ToList())
            {
                item.TrackName = item.TrackName.ToLower().Replace("(original mix)", "");
                item.TrackName = item.TrackName.ToLower().Replace("(radio edit)", "");
                item.TrackName = item.TrackName.ToLower().Replace("(extended mix)", "");
                item.TrackName = item.TrackName.ToLower().Replace("(extented)", "");

                if(item.TrackName.Contains("href="))
                {
                    trackData.Remove(item);
                }    
            }

            trackData = trackData.DistinctBy(item => item.TrackName).ToList();
            trackData = trackData.Where(item => item.TrackDate.Year >= 2023).ToList();

            Console.WriteLine($"retrieved a total of { trackData.Count() } entries from hardtunes");

            return trackData;
        }
    }
}
