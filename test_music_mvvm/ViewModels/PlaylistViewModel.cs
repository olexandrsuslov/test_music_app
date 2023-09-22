using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using ReactiveUI;
using test_music_mvvm.Models;

namespace test_music_mvvm.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    ObservableCollection<Playlist> items;
    private string url;
    private const int TagsToSkip = 13;
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
                string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36";
                var options = new ChromeOptions();
                options.AddArgument($"--user-agent={userAgent}");
                options.AddArgument("--disable-web-security");
                options.AddArgument("--disable-features=IsolateOrigins,site-per-process");
                options.AddArgument("--enable-javascript");
                HtmlNode headerNode;
                HtmlDocument doc;
                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl(Url);
                    Thread.Sleep(5000);
                    var pageSource = driver.PageSource;
                    doc = new HtmlDocument();
                    doc.LoadHtml(pageSource);
                    headerNode = doc.DocumentNode.SelectNodes("//music-detail-header").Last();
                }
                 
                var playlistItems = new List<Playlist>();

                if (headerNode != null)
                {
                    var playlistItem = new Playlist();
                
                    playlistItem.Name = headerNode.Attributes["headline"].Value;
                    playlistItem.AvatarUrl = headerNode.Attributes["image-src"].Value;
                    playlistItem.Description = IsRequestForPlaylist() ? headerNode.Attributes["secondary-text"].Value : "album has no description";
                    var songNodes = IsRequestForPlaylist() ? doc.DocumentNode.SelectNodes("//music-image-row").Skip(TagsToSkip) : 
                            doc.DocumentNode.SelectNodes("//music-text-row");
                        if (songNodes != null)
                        {
                            playlistItem.Songs = new List<Song>();
                            foreach (var songNode in songNodes)
                            {
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