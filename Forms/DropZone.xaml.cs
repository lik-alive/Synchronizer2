using Synchronizer.Log;
using Synchronizer.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
    /// Ранее посещённый путь
    /// </summary>
    public class HistoryItem
    {
        public String Path { get; }

        public String ShortPath
        {
            get
            {
                return new DirectoryInfo(Path).Name;
            }
        }

        public HistoryItem(String path)
        {
            Path = path;
        }
    }

    public class HistoryItemComparer : IEqualityComparer<HistoryItem>
    {
        public bool Equals(HistoryItem x, HistoryItem y)
        {
            return x.Path == y.Path;
        }

        public int GetHashCode(HistoryItem obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// Interaction logic for DropZone.xaml
    /// </summary>
    public partial class DropZone : UserControl, INotifyPropertyChanged
    {
        public Brush ZoneBrush { get; set;} = Brushes.Gray;
        
        public Brush ZoneBackground { get; set; } = Brushes.LightGray;

        /// <summary>
        /// Корневая папка
        /// </summary>
        public String Folder { get; set; } = null;

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

        private void checkFolder(String folder)
        {
            //Проверка существования директории
            if (!Directory.Exists(folder))
            {
                new InfoOK("Incorrect path").ShowDialog();
                return;
            }

            Folder = folder;

            //Обновление истории
            history.Insert(0, Folder);
            OnPropertyChanged("LastFolders");

            Dropped?.Invoke();
        }

        private void zone_Drop(object sender, DragEventArgs e)
        {
            zone_Leave(sender, e);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);

                checkFolder(folders[0]);
            }
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            fbd.SelectedPath = history[0];
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                checkFolder(fbd.SelectedPath);
            }
        }

        #region History

        /// <summary>
        /// Список ранее синхронизировавшихся папок
        /// </summary>
        public ObservableCollection<HistoryItem> LastFolders {
            get
            {
                ObservableCollection<HistoryItem> res = new ObservableCollection<HistoryItem>();
                foreach (String folder in history)
                {
                    if (res.Count == 3) break;
                    if (!Directory.Exists(folder)) continue;

                    HistoryItem item = new HistoryItem(folder);
                    if (!res.Contains(item, new HistoryItemComparer())) {
                        res.Add(item);
                    }
                }
                return res;
            }
        }

        private List<String> history = new List<String>();

        /// <summary>
        /// Задание списка ранее синхронизировавшихся папок
        /// </summary>
        /// <param name="history"></param>
        public void SetHistory(List<String> history)
        {
            this.history = history;
            OnPropertyChanged("LastFolders");
        }

        /// <summary>
        /// Быстрый выбор папки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LastFolder_Click(object sender, RoutedEventArgs e)
        {
            checkFolder(((sender as Button).DataContext as HistoryItem).Path);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion


        
    }
}
