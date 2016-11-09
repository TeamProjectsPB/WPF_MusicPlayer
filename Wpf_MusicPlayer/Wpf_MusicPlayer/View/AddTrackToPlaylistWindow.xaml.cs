using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for AddTrackToPlaylistWindow.xaml
    /// </summary>
    public partial class AddTrackToPlaylistWindow : Window
    {
        List<string> playlistsToString;

        public string SelectedPlaylistName { get; set; }

        public AddTrackToPlaylistWindow(List<string> playlistsToString)
        {
            InitializeComponent();
            this.playlistsToString = playlistsToString;
            PlaylistListView.ItemsSource = this.playlistsToString;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CurrentPlaylistItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedPlaylistName = PlaylistListView.SelectedItem.ToString();
            DialogResult = true;
        }
    }
}
