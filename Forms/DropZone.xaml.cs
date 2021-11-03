using Synchronizer.Log;
using Synchronizer.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Synchronizer.Forms
{
    /// <summary>
    /// Interaction logic for DropZone.xaml
    /// </summary>
    public partial class DropZone : UserControl, INotifyPropertyChanged
    {
        public Brush ZoneBrush { get; set;} = Brushes.Gray;
        
        public Brush ZoneBackground { get; set; } = Brushes.LightGray;

        public String Folder { get; set; }

        public event Action Dropped;

        public DropZone()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void zone_Enter(object sender, DragEventArgs e)
        {
            ZoneBrush = Brushes.Blue;
            ZoneBackground = Brushes.LightSlateGray;
            OnPropertyChanged("ZoneBrush");
            OnPropertyChanged("ZoneBackground");
        }

        private void zone_Leave(object sender, DragEventArgs e)
        {
            ZoneBrush = Brushes.Gray;
            ZoneBackground = Brushes.LightGray;
            OnPropertyChanged("ZoneBrush");
            OnPropertyChanged("ZoneBackground");
        }

        private void zone_Drop(object sender, DragEventArgs e)
        {
            zone_Leave(sender, e);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Проверка существования директории
                if (!Directory.Exists(folders[0]))
                {
                    Dispatcher.Invoke(() => new InfoOK("Incorrect path").ShowDialog());
                    return;
                }

                Folder = folders[0];
                OnPropertyChanged("Folder");
                Dropped();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }
}
