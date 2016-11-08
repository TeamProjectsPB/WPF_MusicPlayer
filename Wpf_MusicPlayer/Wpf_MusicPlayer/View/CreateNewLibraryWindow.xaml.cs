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
using Wpf_MusicPlayer.Converter;

namespace Wpf_MusicPlayer.View
{
    /// <summary>
    /// Interaction logic for CreateNewLibraryWindow.xaml
    /// </summary>
    public partial class CreateNewLibraryWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private string _libName;

        public string LibName
        {
            get { return _libName; }
            set
            {
                _libName = value;
                OnPropertyChanged("LibName");
            }
        }

        public string SourceUrl
        {
            get { return FolderPickerControl.SelectedPath; }
        }

        public CreateNewLibraryWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = this;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this["SourceUrl"]) && Validator.IsValid(this))
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show(this["LibName"] + " " + this["SourceUrl"]);
            }           
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region IDataErrorInfo
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "LibName":
                        if (string.IsNullOrWhiteSpace(LibName))
                        {
                            return "Wprowadz nazwę.";
                        }
                        break;
                    case "SourceUrl":
                        if (string.IsNullOrWhiteSpace(SourceUrl))
                        {
                            return "Wybierz ścieżkę.";
                        }
                        else if (!System.IO.Directory.Exists(SourceUrl))
                        {
                            return "Wybrany folder nie istnieje.";
                        }
                        break;
                }
                return string.Empty;
            }
        }

        public string Error { get; } 
        #endregion
    }
}
