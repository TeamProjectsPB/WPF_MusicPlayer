using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using WMPLib;
using Song = TagLib.File;

namespace Wpf_MusicPlayer.Model
{
    public class MediaPlayer
    {
        #region Members

        private static readonly string[] mediaExtensions =
        {
            ".WAV", ".WMA", ".MP3"
        };


        private static WindowsMediaPlayer mPlayer = new WindowsMediaPlayer();
        private List<string> currentPlaylistSongUrl;
        private IWMPPlaylist allLibrariesPlaylist;
        private string allLibrariesPlaylistName;

        private bool libraryCurrentlyPlaying;
        private bool randomPlay;
        private bool repeatAll;

        #endregion

        #region Properties

        public WindowsMediaPlayer MPlayer
        {
            get { return mPlayer; }
        }

        public IWMPPlaylist CurrentPlaylist
        {
            get { return mPlayer.currentPlaylist; }
            set { mPlayer.currentPlaylist = value; }
        }

        public static Dictionary<string, Song> SongInfo { get; set; }
        public List<IWMPPlaylist> Playlists { get; set; }

        public Dictionary<string, List<string>> PlaylistsUrl { get; set; }
        public List<Library> Libraries { get; set; }

        public bool LibraryCurrentlyPlaying
        {
            get { return libraryCurrentlyPlaying; }
            set
            {
                if (libraryCurrentlyPlaying)
                {
                    mPlayer.playlistCollection.remove(CurrentPlaylist);
                }
                libraryCurrentlyPlaying = value;
            }
        }

        public int CurrentVolume
        {
            get { return mPlayer.settings.volume; }
            set { mPlayer.settings.volume = value; }
        }

        public string CurrentPositionToString
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentPositionString : "00:00"; }
        }

        public double CurrentPosition
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentPosition : 0; }
            set
            {
                if (mPlayer.controls.currentItem != null)
                {
                    mPlayer.controls.currentPosition = value;
                }
            }
        }
        

        public string DurationToString
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentItem.durationString : "00:00"; }
        }

        public double Duration
        {
            get { return mPlayer.controls.currentItem != null ? mPlayer.controls.currentItem.duration : 0; }
        }


        public List<string> CurrentSongsToString
        {
            get
            {
                var currentSongs = new List<string>();
                currentPlaylistSongUrl.ForEach(x =>
                {
                    var song = SongInfo[x.ToLower()];
                    currentSongs.Add(FormatedViewSong(song));
                });
                return currentSongs;
            }
        }

        public List<Song> CurrentSongs
        {
            get
            {
                var currentSongs = new List<Song>();
                currentPlaylistSongUrl.ForEach(x =>
                {
                    var song = SongInfo[x.ToLower()];
                    currentSongs.Add(song);
                });
                return currentSongs;
            }
        } 

        public List<string> PlaylistsToString
        {
            get
            {
                var playlistName = new List<string>();
                Playlists.ForEach(x => playlistName.Add(x.name));
                return playlistName;
            }
        }

        public List<string> LibrariesToString
        {
            get
            {
                var libraryName = new List<string>();
                libraryName.Add("Wszystkie biblioteki");
                Libraries.ForEach(x => libraryName.Add(x.Name));
                return libraryName;
            }
        }

        public string CurrentSong
        {
            get
            {
                try
                {
                    return Path.GetFileNameWithoutExtension(mPlayer.currentMedia.sourceURL);
                }
                catch
                {
                    return "Aktualna piosenka";
                }
            }
        }

        #endregion

        #region  Constructors

        public MediaPlayer()
        {
            SongInfo = new Dictionary<string, Song>();
            LoadMediaInfo();
            currentPlaylistSongUrl = new List<string>();
            PlaylistsUrl = new Dictionary<string, List<string>>();
            Playlists = new List<IWMPPlaylist>();
            Libraries = new List<Library>();
            allLibrariesPlaylistName = "allLibrariesPlaylist";
            allLibrariesPlaylist = GetPlaylistFromMediaCollection(allLibrariesPlaylistName);
        }

        ~MediaPlayer()
        {
            //var temporaryPlaylists = mPlayer.playlistCollection.getByName("temporaryPlaylist");
            /*for (int i = 0; i < temporaryPlaylists.count; i++)
            {
                mPlayer.playlistCollection.remove(temporaryPlaylists.Item(i));
            }*/
        }

        #endregion

        #region MediaPlayerControls

        public void Play()
        {
            mPlayer.controls.play();
        }

        public void Pause()
        {
            mPlayer.controls.pause();
        }

        public void Stop()
        {
            mPlayer.controls.stop();
        }

        public void NextTrack()
        {
            mPlayer.controls.next();
        }

        public void PreviousTrack()
        {
            mPlayer.controls.previous();
        }

        public int VolumeUp()
        {
            if (MPlayer.settings.volume < 100)
            {
                MPlayer.settings.volume = MPlayer.settings.volume + 10;
            }
            return mPlayer.settings.volume;
        }

        public int VolumeDown()
        {
            if (MPlayer.settings.volume > 0)
            {
                MPlayer.settings.volume = MPlayer.settings.volume - 10;
            }
            return mPlayer.settings.volume;
        }

        public bool ChangeRandomPlayStatement()
        {
            randomPlay = !randomPlay;
            mPlayer.settings.setMode("shuffle", randomPlay);
            return randomPlay;
        }

        public bool ChangeRepeatAllStatement()
        {
            repeatAll = !repeatAll;
            mPlayer.settings.setMode("loop", repeatAll);
            return repeatAll;
        }

        public void MoveTrack(int songIndex, int newIndex)
        {
            mPlayer.currentPlaylist.moveItem(songIndex, newIndex);
        }

        #endregion

        #region SortPlaylist

        public void Sort(string attribute)
        {
            bool sortAsc = bool.Parse(CurrentPlaylist.getItemInfo("SortAscending"));
            if (CurrentPlaylist.getItemInfo("SortAttribute").Equals(attribute))
            {
                CurrentPlaylist.setItemInfo("SortAscending", (!sortAsc).ToString());
            }
            else
            {
                CurrentPlaylist.setItemInfo("SortAttribute", attribute);
                CurrentPlaylist.setItemInfo("SortAscending", "true");
            }
            SetCurrentPlaylistSongUrl(CurrentPlaylist);
            currentPlaylistSongUrl = PlaylistsUrl[CurrentPlaylist.name];
        }

        #endregion

        #region Getters

        public string FormatedViewSong(Song s)
        {
            var spaces = 21;
            var artist = "";
            var title = "";
            var album = "";
            var white_spaces = "";

            try
            {
                if (s.Tag.FirstPerformer.Length < spaces - 1)
                {
                    for (var i = 0; i < spaces - 1 - s.Tag.FirstPerformer.Length; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    artist = s.Tag.FirstPerformer + white_spaces + " ";
                    white_spaces = "";
                }
                else if (s.Tag.FirstPerformer.Length <= 2)
                {
                    for (var i = 0; i < spaces; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    artist = white_spaces;
                    white_spaces = "";
                }
                else
                {
                    artist = s.Tag.FirstPerformer.Substring(0, spaces - 1) + " ";
                }
            }
            catch
            {
                artist = String.Empty;
            }
            try
            {
                if (s.Tag.Title.Length < 39)
                {
                    for (var i = 0; i < 39 - s.Tag.Title.Length; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    title = s.Tag.Title + white_spaces + " ";
                    white_spaces = "";
                }
                else if (s.Tag.Title.Length <= 1)
                {
                    for (var i = 0; i < 40; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    title = white_spaces;
                    white_spaces = "";
                }
                else
                {
                    title = s.Tag.Title.Substring(0, 39) + " ";
                }
            }
            catch
            {
                title = string.Empty;
            }
            try
            {
                if (s.Tag.Album.Length < spaces + 4)
                {
                    for (var i = 0; i < spaces + 4 - s.Tag.Album.Length; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    album = s.Tag.Album + white_spaces + " ";
                    white_spaces = "";
                }
                else if (s.Tag.Album.Length <= 2)
                {
                    for (var i = 0; i < spaces; i++)
                    {
                        white_spaces = white_spaces + " ";
                    }
                    album = white_spaces;
                    white_spaces = "";
                }
                else
                {
                    album = s.Tag.Album.Substring(0, spaces - 1) + " ";
                }
            }
            catch
            {
                album = string.Empty;
            }
            if (String.IsNullOrWhiteSpace(title))
            {
                return Path.GetFileNameWithoutExtension(s.Name);
            }
            else
            {
                return artist + title + album;
            }
        }

        public IWMPPlaylist GetPlaylistFromMediaCollection(string playlistName)
        {
            IWMPPlaylist playlist;
            if (mPlayer.mediaCollection.getByName(playlistName).count > 0)
            {
                playlist = mPlayer.mediaCollection.getByName(playlistName);
                playlist.clear();
            }
            else
            {
                playlist = mPlayer.playlistCollection.newPlaylist(playlistName);
            }
            return playlist;
        }

        #endregion

        #region Setters

        public void LoadCurrentPlaylist(string newCurrentPlaylist)
        {
            libraryCurrentlyPlaying = false;
            var temporaryPlaylist = Playlists.Find(x => x.name.Equals(newCurrentPlaylist));
            if (!PlaylistsUrl.ContainsKey(newCurrentPlaylist))
            {
                SetCurrentPlaylistSongUrl(temporaryPlaylist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[newCurrentPlaylist];
            CurrentPlaylist = temporaryPlaylist;
        }

        private void SetCurrentPlaylistSongUrl(IWMPPlaylist playlist)
        {
            var urls = new List<string>();
            for (int i = 0; i < playlist.count; i++)
            {
                urls.Add(playlist.Item[i].sourceURL);
            }
            if (PlaylistsUrl.ContainsKey(playlist.name))
            {
                PlaylistsUrl[playlist.name] = urls;
            }
            else
            {
                PlaylistsUrl.Add(playlist.name, urls);
            }
            //currentPlaylistSongUrl = PlaylistsUrl[playlist.name];
        }

        #endregion

        #region Loaders

        public void LoadLibraryMediaPlaylist()
        {
            if (!PlaylistsUrl.ContainsKey(allLibrariesPlaylistName))
            {
                SetCurrentPlaylistSongUrl(allLibrariesPlaylist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[allLibrariesPlaylistName];
            CurrentPlaylist = allLibrariesPlaylist;
        }

        public void LoadCurrentLibrary(string newCurrentLibrary)
        {
            libraryCurrentlyPlaying = true;
            var newCurrentLib = Libraries.Find(x => x.Name.Equals(newCurrentLibrary));
            var playlist = newCurrentLib.Playlist;
            if (!PlaylistsUrl.ContainsKey(playlist.name))
            {
                SetCurrentPlaylistSongUrl(playlist);
            }
            currentPlaylistSongUrl = PlaylistsUrl[playlist.name];
            CurrentPlaylist = playlist;
        }

        public void LoadCurrentSong(int index)
        {
            mPlayer.controls.playItem(mPlayer.currentPlaylist.Item[index]);
        }

        #endregion

        #region Remove

        public void RemoveTrack(int index)
        {
            var media = mPlayer.currentPlaylist.Item[index];
            var count = CurrentPlaylist.count;
            CurrentPlaylist.removeItem(media);
            var countafter = CurrentPlaylist.count;
            SetCurrentPlaylistSongUrl(CurrentPlaylist);
        }

        //return: true - current playlist was removed;
        public bool RemovePlaylist(string name)
        {
            bool removedCurrentPlaylist = CurrentPlaylist.name.Equals(name);
            if (removedCurrentPlaylist)
            {
                LoadLibraryMediaPlaylist();
            }
            var playlists = new List<IWMPPlaylist>();
            for (int i = 0; i < mPlayer.playlistCollection.getByName(name).count; i++)
            {
                playlists.Add(mPlayer.playlistCollection.getByName(name).Item(i));
            }
            playlists.ForEach(x => mPlayer.playlistCollection.remove(x));
            Playlists.Remove(Playlists.Find(x => x.name.Equals(name)));
            return removedCurrentPlaylist;
        }

        public bool RemoveLibrary(string name)
        {
            var library = Libraries.Find(x => x.Name.Equals(name));
            var libraryPlaylistName = library.Playlist.name;
            bool removedCurrentLibrary = CurrentPlaylist.name.Equals(libraryPlaylistName);

            var removePlaylists = new List<IWMPPlaylist>();
            for (int i = 0; i < mPlayer.playlistCollection.getByName(libraryPlaylistName).count; i++)
            {
                var media = mPlayer.playlistCollection.getByName(libraryPlaylistName).Item(i);
                removePlaylists.Add(media);
            }
            removePlaylists.ForEach(x => mPlayer.playlistCollection.remove(x));
            Playlists.Remove(Playlists.Find(x => x.name.Equals(name)));
            Libraries.Remove(library);
            RemoveLibraryTracksFromAllLibrariesPlaylist(library.Url);
            return removedCurrentLibrary;
        }

        private void RemoveLibraryTracksFromAllLibrariesPlaylist(string libUrl)
        {
            for (int i = allLibrariesPlaylist.count - 1; i > 0; i--)
            {
                var item = allLibrariesPlaylist.Item[i];
                if (item.sourceURL.ToLower().Contains(libUrl.ToLower()))
                {
                    allLibrariesPlaylist.removeItem(item);
                }
            }
            SetCurrentPlaylistSongUrl(allLibrariesPlaylist);
        }

        #endregion

        #region Library

        public void AddLibrary(string name, string url)
        {
            if (Directory.Exists(url))
            {
                LoadMediaInfoFromNewLibrary(url);
                var playlistname = "lib_" + name;
                var playlist = GetPlaylistFromMediaCollection(playlistname);

                var audio = mPlayer.mediaCollection.getByAttribute("MediaType", "audio");
                for (var i = 0; i < audio.count; i++)
                {
                    var media = audio.Item[i];
                    if (media.sourceURL.ToLower().Contains(url.ToLower()))
                    {
                        var count = mPlayer.mediaCollection.getAll().count;
                        playlist.appendItem(media);
                        allLibrariesPlaylist.appendItem(media);
                    }
                }
                Libraries.Add(new Library(name, url, playlist));
            }
        }

        private static void LoadMediaInfoFromNewLibrary(string url)
        {
            var songsUrl = new List<string>(Directory.EnumerateFiles(url, "*.*", SearchOption.AllDirectories).
                Where(
                    s => mediaExtensions.Contains(Path.GetExtension(s), StringComparer.OrdinalIgnoreCase)));
            songsUrl.ForEach(x =>
            {
                if (!SongInfo.ContainsKey(x.ToLower()))
                {
                    mPlayer.mediaCollection.add(x.ToLower());
                    var song = Song.Create(x.ToLower());
                    SongInfo.Add(x.ToLower(), song);
                }
            });
        }

        public void AddTrackToPlaylist(int trackIndex, string playlistName)
        {
            var song = mPlayer.currentPlaylist.Item[trackIndex];
            var playlists = mPlayer.playlistCollection.getByName(playlistName);
            for (int i = 0; i < playlists.count; i++)
            {
                mPlayer.playlistCollection.getByName(playlistName).Item(i).appendItem(song);
            }
            SetCurrentPlaylistSongUrl(playlists.Item(0));
        }

        public void CreatePlaylist(string name)
        {
            Playlists.Add(mPlayer.playlistCollection.newPlaylist(name));
        }

        public void AddPlaylist(string name)
        {
            if (mPlayer.playlistCollection.getByName(name).count > 0)
            {
                Playlists.Add(mPlayer.playlistCollection.getByName(name).Item(0));
            }
        }

        private void LoadMediaInfo()
        {
            var mediaCollection = mPlayer.mediaCollection.getByAttribute("MediaType", "audio");
            var count = mediaCollection.count;
            for (var i = 0; i < mediaCollection.count; i++)
            {
                var media = mediaCollection.Item[i];
                var url = media.sourceURL;
                var mediaInfo = Song.Create(media.sourceURL);
                SongInfo.Add(media.sourceURL.ToLower(), mediaInfo);
            }
        }

        #endregion
    }
}