using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wpf_MusicPlayer.View
{
    /// <summary>
    /// Interaction logic for AddNewPlaylistWindow.xaml
    /// </summary>
    public partial class AddNewPlaylistWindow : Window, INotifyPropertyChanged
    {
        private string playlistName;

        public string PlaylistName
        {
            get { return playlistName; }
            set { playlistName = value; OnPropertyChanged("PlaylistName"); }
        }

        public AddNewPlaylistWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = this;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
