using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WMPLib;
using Wpf_MusicPlayer.Model;
using Wpf_MusicPlayer.View;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MediaPlayer = Wpf_MusicPlayer.Model.MediaPlayer;
using MenuItem = System.Windows.Controls.MenuItem;
using Song = TagLib.File;

namespace Wpf_MusicPlayer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Members

        private static DispatcherTimer timer;
        public static MediaPlayer player = new MediaPlayer();
        private readonly string fileUrl = Directory.GetCurrentDirectory() + "\\config.dat";
        private bool repeatAll;
        private bool randomPlay;

        #endregion
        #region Properties

        public bool RepeatAll
        {
            get { return repeatAll; }
            set
            {
                repeatAll = value;
                OnPropertyChanged("RepeatAll");
                OnPropertyChanged("RepeatAllButtonBackground");
            }
        }

        public bool RandomPlay
        {
            get { return randomPlay; }
            set
            {
                randomPlay = value;
                OnPropertyChanged("RandomPlay");
                OnPropertyChanged("ShuffleButtonBackground");
            }
        }

        public Brush ShuffleButtonBackground
        {
            get { return randomPlay ? Brushes.Yellow : Brushes.White; }
        }

        public Brush RepeatAllButtonBackground
        {
            get { return repeatAll ? Brushes.Yellow : Brushes.White; }
        }
        #region CurrentPosition
        public double CurrentPosition
        {
            get { return player.CurrentPosition; }
            set { player.CurrentPosition = value; }
        }
        public string CurrentPositionToString
        {
            get { return player.CurrentPositionToString; }
        }
        #endregion

        #region Duration
        public double Duration
        {
            get { return player.Duration; }
        }

        public string DurationToString
        {
            get { return player.DurationToString; }
        }
        #endregion




        public List<Song> CurrentSongs
        {
            get { return player.CurrentSongs; }
        }

        public List<string> CurrentSongsToString
        {
            get { return player.CurrentSongsToString; }
        }

        public List<string> PlaylistsToString
        {
            get { return player.PlaylistsToString; }
        }

        public List<string> LibrariesToString
        {
            get { return player.LibrariesToString; }
        }

        public string CurrentSong
        {
            get { return player.CurrentSong; }
        }

        public List<Library> Libraries
        {
            get { return player.Libraries; }
        }

        #endregion
        #region Constructors
        public MainWindow()
        {
            InitializeComponent();


            if (File.Exists(fileUrl))
            {
                LoadCurrentVolume(fileUrl);
                LoadLibraries(fileUrl);
                LoadPlaylists(fileUrl);
                LoadLibraryMediaPlaylist();
            }
            else
            {
                ConfigFile.CreateNewFile(fileUrl);
            }

            MainGrid.DataContext = player;
            ControlPanelGrid.DataContext = this;
            VolumeGrid.DataContext = player;
            PlaylistsListView.ItemsSource = PlaylistsToString;
            LibraryListView.ItemsSource = Libraries;
            CurrentPlayingListView.ItemsSource = CurrentSongs;

            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        #endregion
        #region Timer
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("CurrentPositionToString");
            OnPropertyChanged("DurationToString");
            OnPropertyChanged("Duration");
            OnPropertyChanged("CurrentPosition");
            OnPropertyChanged("CurrentSong");
        }
        #endregion
        #region Sliders

        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ConfigFile.SaveVolume(fileUrl, player.CurrentVolume);
        }
        private void SeekBarSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(Math.Abs(e.NewValue - e.OldValue)) > 2)
            {
                //timer.Stop();
                CurrentPosition = e.NewValue;
                //timer.Start();
            }
        }
        #endregion
        #region LibraryListViewClicks

        private void AddNewLibraryItem_OnClick(object sender, RoutedEventArgs e)
        {
            AddLibraryWindow addLibraryWindow = new AddLibraryWindow(this);
            addLibraryWindow.ShowDialog();
        }

        private void RemoveLibrary_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var library = item.DataContext as Library;
            RemoveLibrary(library);
        }

        public void RemoveLibrary(Library library)
        {
            var removedCurrentLib = player.RemoveLibrary(library.Name);
            ConfigFile.RemoveLibrary(fileUrl, library.Name);
            ReloadLibraryListViewItemsSource();
            if (removedCurrentLib)
            {
                ReloadCurrentPlayingListViewItemsSource();
            }
        }

        #endregion
        #region CurrentTrackListViewClicks

        private void AddTrackToPlaylistItem_OnClick(object sender, RoutedEventArgs e)
        {
            var song = sender.ToString();
            AddTrackToPlaylistWindow dialog = new AddTrackToPlaylistWindow(PlaylistsToString);
            if (dialog.ShowDialog() == true)
            {
                var item = (Song)CurrentPlayingListView.SelectedItem;
                var index = CurrentSongs.FindIndex(x => x.Name.Equals(item.Name));
                var playlistName = dialog.SelectedPlaylistName;
                player.AddTrackToPlaylist(index, playlistName);
                ReloadPlaylistsListViewItemsSource();
                ReloadCurrentPlayingListViewItemsSource();
            }
        }

        private void RemoveTrackItem_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (Song) CurrentPlayingListView.SelectedItem;
            var index = CurrentSongs.FindIndex(x => x.Name.Equals(item.Name));
            player.RemoveTrack(index);
            ReloadCurrentPlayingListViewItemsSource();
        }

        #endregion       
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion      
        #region ReloadItemsSource

        public void ReloadPlaylistsListViewItemsSource()
        {
            PlaylistsListView.ItemsSource = null;
            PlaylistsListView.ItemsSource = PlaylistsToString;
        }

        public void ReloadLibraryListViewItemsSource()
        {
            LibraryListView.ItemsSource = null;
            LibraryListView.ItemsSource = Libraries;
        }

        public void ReloadCurrentPlayingListViewItemsSource()
        {
            CurrentPlayingListView.ItemsSource = null;
            CurrentPlayingListView.ItemsSource = CurrentSongs;
        }

        #endregion
        #region FileLoaders
        public void LoadCurrentVolume(string url)
        {
            player.CurrentVolume = ConfigFile.GetVolume(url);
        }

        private void LoadLibraries(string url)
        {
            var libraries = ConfigFile.GetLibraries(url);
            libraries.ForEach(x => AddLibrary(x.Item1, x.Item2));
        }

        private void LoadPlaylists(string url)
        {
            var playlists = ConfigFile.GetPlaylists(url);
            playlists.ForEach(x => AddPlaylist(x));
        }

        public void LoadLibraryMediaPlaylist()
        {
            player.LoadLibraryMediaPlaylist();
        }
        #endregion
        #region Add

        public void AddLibrary(string name, string url)
        {
            player.AddLibrary(name, url);
            ReloadLibraryListViewItemsSource();
        }

        public void AddPlaylist(string name)
        {
            player.AddPlaylist(name);
            ReloadPlaylistsListViewItemsSource();            
        }

        #endregion
        #region Create

        public void CreatePlaylist(string name)
        {
            ConfigFile.SaveNewPlaylist(fileUrl, name);
            player.CreatePlaylist(name);
            ReloadPlaylistsListViewItemsSource();
        }

        public void CreateLibrary(string name, string url)
        {
            AddLibrary(name, url);
            ConfigFile.SaveNewLibrary(fileUrl, name, url);
        }

        #endregion
        #region ButtonEvents

        private void RepeatAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            RepeatAll = player.ChangeRepeatAllStatement();
        }


        private void ShufflePlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            RandomPlay = player.ChangeRandomPlayStatement();
        }

        #endregion
        #region PlayerButtons

        private void StopTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }

        private void NextTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            player.NextTrack();
        }

        private void PreviousTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            player.PreviousTrack();
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (player.MPlayer.playState.Equals(WMPPlayState.wmppsPaused))
            {
                player.Play();
            }
            else if (player.MPlayer.playState.Equals(WMPPlayState.wmppsPlaying))
            {
                player.Pause();
            }
        }

        #endregion
        #region DoubleClicks

        private void LibraryItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            var library = item.DataContext as Library;
            player.LoadCurrentLibrary(library.Name);
            ReloadCurrentPlayingListViewItemsSource();
        }

        private void PlaylistItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            player.LoadCurrentPlaylist(item.DataContext.ToString());
            ReloadCurrentPlayingListViewItemsSource();
        }

        private void CurrentTrackItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            var track = item.DataContext as Song;
            var index = player.CurrentSongs.FindIndex(x => x == track);
            player.LoadCurrentSong(index);
        }

        #endregion

        private void ManageLibrary_Click(object sender, RoutedEventArgs e)
        {
            AddLibraryWindow addLibraryWindow = new AddLibraryWindow(this);
            addLibraryWindow.ShowDialog();
        }

        #region PlaylistManager

        private void AddPlaylistWindow()
        {
            AddNewPlaylistWindow dialog = new AddNewPlaylistWindow();
            if (dialog.ShowDialog() == true)
            {
                CreatePlaylist(dialog.PlaylistName);
            }
        }
        private void ManagePlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            AddPlaylistWindow();
        }

        private void AddNewPlaylistItem_OnClick(object sender, RoutedEventArgs e)
        {
            AddPlaylistWindow();
        }
        #endregion

        
    }
}