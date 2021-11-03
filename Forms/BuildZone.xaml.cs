using Synchronizer.Log;
using Synchronizer.Model;
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
    public partial class BuildZone : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Поток выполнения
        /// </summary>
        public AsyncBase task;

        /// <summary>
        /// Дерево объектов
        /// </summary>
        private FSDirectory tree;

        /// <summary>
        /// Получение дочерних объектов
        /// </summary>
        public List<FSItem> Children
        {
            get
            {
                return tree != null ? tree.Children : null;
            }
        }

        /// <summary>
        /// Событие завершения построения дерева
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// Событие прерывания построения дерева
        /// </summary>
        public event Action Aborted;

        public BuildZone()
        {
            InitializeComponent();
        }

        public void BuildTree(String folder)
        {
            DirectoryInfo info = new DirectoryInfo(folder);
            Thread thread = new Thread(() =>
            {
                // Build folder trees
                DateTime start = DateTime.Now;
                Logger.RaiseMessage("Build started");

                FSBuilder fsb = new FSBuilder(info);
                StartAndJoin(fsb);
                if (fsb.IsAborted)
                {
                    Dispatcher.Invoke(() => Aborted());
                    return;
                }

                tree = fsb.Root;

                Logger.RaiseMessage("Build completed (" + (DateTime.Now - start).TotalSeconds + " s)");
                OnPropertyChanged("Children");
                Dispatcher.Invoke(() => Completed());
            });
            thread.Start();
        }

        /// <summary>
        /// Запуск подпроцесса в синхронном режиме
        /// </summary>
        /// <param name="task"></param>
        /// <param name="updateProgress"></param>
        private void StartAndJoin(AsyncBase asyncBase)
        {
            task = asyncBase;
            task.Start();

            while (task.IsRunning)
            {
                progress_Update(task.Progress);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Обновление полосы прогресса
        /// </summary>
        /// <param name="obj"></param>
        private void progress_Update(Double obj)
        {
            Dispatcher.Invoke(() =>
            {
                if (obj == -1) progressBar.IsIndeterminate = true;
                else progressBar.IsIndeterminate = false;

                progressBar.Value = obj;
            }, System.Windows.Threading.DispatcherPriority.Input);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            task.Abort();
        }
    }
}
