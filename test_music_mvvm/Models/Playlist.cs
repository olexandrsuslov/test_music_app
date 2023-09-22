using System.Collections.Generic;

namespace test_music_mvvm.Models;

public class Playlist
{
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string Description { get; set; }
    public List<Song> Songs { get; set; }
}