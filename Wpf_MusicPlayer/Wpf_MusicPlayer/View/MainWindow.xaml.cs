using System;
using System.Windows;
using Wpf_MusicPlayer.Model;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Timers;
using Song = TagLib.File;
using System.Windows.Threading;

namespace Wpf_MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        static DispatcherTimer timer; 
        public static MediaPlayer player = new MediaPlayer();
        private readonly string fileUrl = Directory.GetCurrentDirectory() + "\\config.dat";
        #endregion
        #region Properties
        public bool RepeatAll { get; set; }
        public bool RandomPlay { get; set; }
        public string CurrentVolumeToString
        {
            get { return player.CurrentVolume + "%"; }
        }
        public string CurrentPositionToString
        {
            get { return player.CurrentPositionToString; }
        }
        public double CurrentPosition
        {
            get { return player.CurrentPosition; }
            set { player.CurrentPosition = value; }
        }
        public string DurationToString
        {
            get { return player.DurationToString; }
        }
        public double Duration
        {
            get { return player.Duration; }
        }
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

        public int SongId { get; set; }
        #endregion
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
            else { ConfigFile.CreateNewFile(fileUrl); }

            MainGrid.DataContext = player;
            PlaylistsListView.ItemsSource = PlaylistsToString;
            LibraryListView.ItemsSource = Libraries;
            CurrentPlayingListView.ItemsSource = CurrentSongs;

            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            PositionLabel.Content = CurrentPositionToString;
            DurationLabel.Content = DurationToString;
            SeekBarSlider.Maximum = Duration;
            SeekBarSlider.Value = CurrentPosition;
        }
        #region ReloadItemsSource

        private void ReloadPlaylistsListViewItemsSource()
        {
            PlaylistsListView.ItemsSource = null;
            PlaylistsListView.ItemsSource = PlaylistsToString;
        }
        private void ReloadLibraryListViewItemsSource()
        {
            LibraryListView.ItemsSource = null;
            LibraryListView.ItemsSource = Libraries;
        }
        private void ReloadCurrentPlayingListViewItemsSource()
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
        }

        public void AddPlaylist(string name)
        {
            player.AddPlaylist(name);
        }
        
        #endregion
        #region Create
        public void CreatePlaylist(string name)
        {
            ConfigFile.SaveNewPlaylist(fileUrl, name);
            player.CreatePlaylist(name);
        }
        public void CreateLibrary(string name, string url)
        {
            ConfigFile.SaveNewLibrary(fileUrl, name, url);
            AddLibrary(name, url);
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
            player.Play();
        }
        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ConfigFile.SaveVolume(fileUrl, player.CurrentVolume);
        }

        private void AddNewLibraryItem_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RemoveLibrary_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var library = item.DataContext as Library;
            bool removedCurrentLib = player.RemoveLibrary(library.Name);
            ConfigFile.RemoveLibrary(fileUrl, library.Name);
            ReloadLibraryListViewItemsSource();
            if (removedCurrentLib)
            {
                ReloadCurrentPlayingListViewItemsSource();
            }
        }

        private void AddTrackToPlaylistItem_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RemoveTrackItem_OnClick(object sender, RoutedEventArgs e)
        {
            player.RemoveTrack(CurrentPlayingListView.SelectedIndex);
            ReloadCurrentPlayingListViewItemsSource();
        }

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
            int index = player.CurrentSongs.FindIndex(x => x == track);
            player.LoadCurrentSong(index);
        }
        #endregion

        private void SeekBarSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {            
            if (Math.Round(Math.Abs(e.NewValue - e.OldValue)) > 2)
            {
                //timer.Stop();
                CurrentPosition = e.NewValue;
                //timer.Start();
            }
            
            //throw new NotImplementedException();
        }
    }
}
