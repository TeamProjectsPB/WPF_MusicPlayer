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
using FolderPickerLib;
using Wpf_MusicPlayer.Model;


namespace Wpf_MusicPlayer.View
{
    /// <summary>
    /// Interaction logic for AddLibrary.xaml
    /// </summary>
    public partial class AddLibraryWindow : Window
    {
        private Window parentWindow;

        private MainWindow mainWindow;
        
        public AddLibraryWindow(Window parentWindow)
        {
            this.parentWindow = parentWindow;
            InitializeComponent();
            if (this.parentWindow is MainWindow)
            {
                mainWindow = parentWindow as MainWindow;
                LibrariesListView.ItemsSource = mainWindow.Libraries;
            }            
        }
        private void ReloadLibraryListViewItemsSource()
        {
            LibrariesListView.ItemsSource = null;
            LibrariesListView.ItemsSource = mainWindow.Libraries;
        }

        private void AddLibraryBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateNewLibraryWindow dialog = new CreateNewLibraryWindow();
            if (dialog.ShowDialog() == true)
            {
                var sourceUrl = dialog.SourceUrl;
                var name = dialog.LibName;
                mainWindow.CreateLibrary(name, sourceUrl);
                ReloadLibraryListViewItemsSource();
            }
        }

        private void DelLibraryBtn_Onclick(object sender, RoutedEventArgs e)
        {
            var selectedItem = LibrariesListView.SelectedItem as Library;
            mainWindow.RemoveLibrary(selectedItem);
            ReloadLibraryListViewItemsSource();
        }
    }
}
