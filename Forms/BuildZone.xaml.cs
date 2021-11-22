using Synchronizer2.Model;
using Synchronizer2.Processing;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Synchronizer2.Forms
{
    public partial class BuildZone : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Поток выполнения
        /// </summary>
        public AsyncBase task;

        /// <summary>
        /// Корневая папка
        /// </summary>
        public String Folder { get; private set; }

        /// <summary>
        /// Прогресс выполнения
        /// </summary>
        public Double Progress { get; private set; }

        /// <summary>
        /// Событие завершения построения дерева
        /// </summary>
        public event Action<FSTree> Completed;

        /// <summary>
        /// Событие прерывания построения дерева
        /// </summary>
        public event Action Aborted;

        public BuildZone()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void BuildTree(String folder)
        {
            OnPropertyChanged("Folder", folder);
            OnPropertyChanged("Progress", 0);

            DirectoryInfo info = new DirectoryInfo(folder);
            Thread thread = new Thread(() =>
            {
                FSBuilder fsb = new FSBuilder(info);
                if (StartAndJoin(fsb))
                {
                    Dispatcher.Invoke(() => Completed?.Invoke(fsb.Tree));
                }
                else
                {
                    Dispatcher.Invoke(() => Aborted?.Invoke());
                }
            });
            thread.Start();
        }

        /// <summary>
        /// Запуск подпроцесса в синхронном режиме
        /// </summary>
        /// <param name="task"></param>
        /// <param name="updateProgress"></param>
        private Boolean StartAndJoin(AsyncBase asyncBase)
        {
            task = asyncBase;
            task.Start();

            // Monitor task progress
            while (task.IsRunning)
            {
                OnPropertyChanged("Progress", task.Progress);
                Thread.Sleep(100);
            }

            // Check for task failure
            if (task.Status == AsyncBase.AsyncStatus.ABORTED) return false;

            // Finalize progress increment
            OnPropertyChanged("Progress", 100);

            return true;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            task.Abort();
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(String propertyName, Object value)
        {
            PropertyInfo prop = GetType().GetProperty(propertyName);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(this, value, null);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }
}
