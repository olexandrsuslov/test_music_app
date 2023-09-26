using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ReactiveUI;
using test_music_mvvm.Models;

namespace test_music_mvvm.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    ObservableCollection<Playlist> items;
    private string url;
    public string Url { get; set; }
    public ObservableCollection<Playlist> Items
    {
        get => items;
        private set => this.RaiseAndSetIfChanged(ref items, value);
    }
    
    public PlaylistViewModel(IEnumerable<Playlist> items)
    {
        Items = new ObservableCollection<Playlist>(items);
    }
    
    
    
    public async void LoadPlaylists_Click()
    { 
        var playlists = await ParsePlaylistsAsync();

        if (playlists != null)
        {
            Items = new ObservableCollection<Playlist>(playlists);
        }
            
    }

        private async Task<List<Playlist>> ParsePlaylistsAsync()
        {
            try
            {
                //https://music.amazon.com/playlists/B08BWK8W15
                //https://music.amazon.com/albums/B001230JXC
                var playlistItems = new List<Playlist>();
                //https://music.amazon.com/playlists/B0861VPZ6D
                string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36";
                var options = new ChromeOptions();
                options.AddArgument("headless");
                options.AddArgument($"--user-agent={userAgent}");
                options.AddArgument("--disable-web-security");
                options.AddArgument("--disable-features=IsolateOrigins,site-per-process");
                options.AddArgument("--enable-javascript");
                
                using (var driver = new ChromeDriver(options))
                {
                    HtmlNode headerNode;
                    HtmlDocument doc;
                    driver.Navigate().GoToUrl(Url);
                    Thread.Sleep(5000);
                    
                    int scrollIncrement = 1000;
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    long scrollHeight = (long)js.ExecuteScript("return document.body.scrollHeight;");
                    long currentPosition = 0;

                    doc = new HtmlDocument();
                    doc.LoadHtml(driver.PageSource);
                    headerNode = doc.DocumentNode.SelectNodes("//music-detail-header").Last();
                    var playlistItem = new Playlist();
                    playlistItem.Name = headerNode.Attributes["headline"].Value;
                    playlistItem.AvatarUrl = headerNode.Attributes["image-src"].Value;
                    playlistItem.Description = IsRequestForPlaylist() ? headerNode.Attributes["secondary-text"].Value : "album has no description";
                    playlistItem.Songs = new List<Song>();
                    while(currentPosition < scrollHeight)
                    {
                        if (headerNode != null)
                        {
                            var songNodes = IsRequestForPlaylist() ? doc.DocumentNode.SelectNodes("//music-image-row").Where(x => !x.HasClass("disabled-hover")) :
                                doc.DocumentNode.SelectNodes("//music-text-row");
                            if (songNodes != null)
                            {
                                
                                foreach (var songNode in songNodes)
                                {
                                    if (playlistItem.Songs.Exists(s =>
                                            s.SongName == songNode.Attributes["primary-text"].Value))
                                    {
                                        continue;
                                    }
                                    var songItem = new Song();
                                    songItem.SongName = songNode.Attributes["primary-text"].Value;
                                    songItem.ArtistName = IsRequestForPlaylist() ? songNode.Attributes["secondary-text-1"].Value : headerNode.Attributes["primary-text"].Value;;
                                    songItem.AlbumName = IsRequestForPlaylist() ? songNode.Attributes["secondary-text-2"].Value : headerNode.Attributes["headline"].Value;
                                    songItem.Duration = songNode
                                        .SelectSingleNode("./div[@class='content']/div[@class='col4']/music-link")
                                        .Attributes["title"].Value;
                                    playlistItem.Songs.Add(songItem);
                                }
                            }
                            
                        }
                        
                        
                        js.ExecuteScript($"window.scrollBy(0, {scrollIncrement});");
                        currentPosition += scrollIncrement;
                        Thread.Sleep(2000); 
                        doc = new HtmlDocument();
                        doc.LoadHtml(driver.PageSource);
                        
                    }
                    playlistItems.Add(playlistItem);

                }
                
                return playlistItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private bool IsRequestForPlaylist()
        {
            return Url.Contains("playlists");
        }
}