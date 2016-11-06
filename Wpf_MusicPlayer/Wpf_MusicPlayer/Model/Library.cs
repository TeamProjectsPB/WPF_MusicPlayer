using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;
using Song = TagLib.File;

namespace Wpf_MusicPlayer.Model
{
    public class Library
    {
        #region Members
        private string name;
        private string url;
        private IWMPPlaylist playlist;
        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public IWMPPlaylist Playlist
        {
            get { return playlist; }
            set { playlist = value; }
        }

        #endregion
        #region Constructor

        public Library(string name, string url)
        {
            this.name = name;
            this.url = url;
        }

        public Library(string name, string url, IWMPPlaylist playlist)
        {
            this.name = name;
            this.url = url;
            this.playlist = playlist;
        }

        #endregion


    }
}
