using System.Collections.Generic;
using test_music_mvvm.Models;

namespace test_music_mvvm.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public PlaylistViewModel List { get; }
    public MainWindowViewModel()
    {
        List = new PlaylistViewModel(new List<Playlist>());
    }

    
}