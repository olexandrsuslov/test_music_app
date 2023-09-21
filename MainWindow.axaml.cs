using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace test_music;


public class Playlist
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string Description { get; set; }
        public List<Song> Songs { get; set; }
    }

    public class Song
    {
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string Duration { get; set; }
    }

    public class ViewModel
    {
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
    }

    public partial class MainWindow : Window
    {
        private string url;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        private async void LoadPlaylists_Click(object sender, RoutedEventArgs e)
        {
            // string playlistId = ExtractPlaylistId(url);
            url = this.FindControl<TextBox>("playlistUrlTextBox").Text;

            var playlists = await ParsePlaylistsAsync();

                if (playlists != null)
                {
                    var viewModel = (ViewModel)DataContext;
                    viewModel.Playlists.Clear();
                    viewModel.Playlists.AddRange(playlists);
                }
            
        }

        // private string ExtractPlaylistId(string url)
        // {
        //     // Extract the playlist ID from the URL using string manipulation or regex
        //     // For example, you can split the URL by '/' and get the last segment as the ID
        //     string[] segments = url.Split('/');
        //     if (segments.Length > 1)
        //     {
        //         return segments.Last();
        //     }
        //
        //     return null;
        // }

        private async Task<List<Playlist>> ParsePlaylistsAsync()
        {
            try
            {
                // var web = new HtmlWeb();
                // var doc = await web.LoadFromWebAsync(url);
                string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36";
                var options = new ChromeOptions();
                 options.AddArgument($"--user-agent={userAgent}");
                options.AddArguments("headless");
                 options.AddArgument("--enable-javascript");
                 HtmlNode headerNode;
                 HtmlDocument doc;
                 using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl(url);
                    // Thread.Sleep(5000);
                    // Wait for the page to fully load (adjust the timeout as needed)
                    // WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    // wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                
                    // Now you can access the page source
                    var pageSource = driver.PageSource;
                    // Thread.Sleep(5000);
                    doc = new HtmlDocument();
                    doc.LoadHtml(pageSource);
                    // Thread.Sleep(5000);
                     headerNode = doc.DocumentNode.SelectSingleNode("//music-detail-header");
                    //string name = headerNode.SelectNodes("./h1").First().InnerHtml;
                }
                
                
                

                // var handler = new HttpClientHandler();
                // // this is the important bit
                // handler.AutomaticDecompression = System.Net.DecompressionMethods.All;
                // var httpClient = new HttpClient(handler);
                // var html = await httpClient.GetStringAsync(url);
                // var doc = new HtmlDocument();
                // doc.LoadHtml(html);
                // var divs = doc.DocumentNode.Descendants().ToList();
                
                // var headerNode = divs.First();
                var playlistItems = new List<Playlist>();

                if (headerNode != null)
                {
                    var playlistItem = new Playlist();
                
                    playlistItem.Name = headerNode.SelectSingleNode("./h1").InnerHtml;
                        playlistItem.AvatarUrl = headerNode.Attributes["image-src"].Value;
                        playlistItem.Description = headerNode.Attributes["secondary-text"].Value;
                
                        var songNodes = doc.DocumentNode.SelectNodes("//music-image-row");
                        if (songNodes != null)
                        {
                            playlistItem.Songs = new List<Song>();
                            foreach (var songNode in songNodes)
                            {
                                var songItem = new Song();
                
                                songItem.SongName = songNode.Attributes["primary-text"].Value;
                                songItem.ArtistName = songNode.Attributes["secondary-text-1"].Value;
                                songItem.AlbumName = songNode.Attributes["secondary-text-2"].Value;
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
                // Handle exceptions or errors during parsing
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }