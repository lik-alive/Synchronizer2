using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Synchronizer.Model;
using Synchronizer.Forms;
using Synchronizer.Log;
using Synchronizer.Processing;

namespace Synchronizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Потоки выполнения задания
        /// </summary>
        private Thread leftThread;
        private Thread rightThread;

        /// <summary>
        /// Деревья папок и файлов
        /// </summary>
        private FSDirectory leftList = null;
        private FSDirectory rightList = null;

        #endregion

        #region Properties

        /// <summary>
        /// Направление синхронизации
        /// </summary>
        public Boolean Forward { get; set; } = true;

        /// <summary>
        /// Получение/задание статуса проведения сравнения данных
        /// </summary>
        public Boolean HasCompared { get; set; } = false;

        /// <summary>
        /// Получение/задание списка объектов из левой части
        /// </summary>
        public List<FSItem> LeftList
        {
            get
            {
                return leftList != null ? leftList.Children : null;
            }
        }

        /// <summary>
        /// Получение/задание списка объектов из правой части
        /// </summary>
        public List<FSItem> RightList
        {
            get
            {
                return rightList != null ? rightList.Children : null;
            }
        }

        #endregion

        public MainWindow()
        {
            Logger.OnMessage += Logger_OnMessage;

            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Logger.OnMessage -= Logger_OnMessage;

            InterruptAll();

            base.OnClosing(e);
        }

        /// <summary>
        /// Прерывание выполнения текущего задания
        /// </summary>
        public void InterruptAll()
        {
            if (leftThread != null && leftThread.IsAlive) leftThread.Interrupt();
            if (rightThread != null && rightThread.IsAlive) rightThread.Interrupt();
        }

        #region Compare

        /// <summary>
        /// Сравнение папок
        /// </summary>
        /// <param name="fullRefresh"></param>
        /*public void Compare()
        {
            if (taskThread != null && (taskThread.ThreadState == ThreadState.Running || taskThread.ThreadState == ThreadState.WaitSleepJoin))
            {
                Logger.RaiseMessage("Stop current task or wait for results");
                return;
            }
            //Проверка существования директорий
            if (!Directory.Exists(LeftPath) || !Directory.Exists(RightPath))
            {
                Logger.RaiseMessage("Select correct paths");
                return;
            }
            //Compare trees
            DirectoryInfo leftInfo = new DirectoryInfo(LeftPath);
            DirectoryInfo rightInfo = new DirectoryInfo(RightPath);
            taskThread = new Thread(() =>
            {
                try
                { 
                    // Build folder trees
                    DateTime start = DateTime.Now;
                    Logger.RaiseMessage("Build has started");
                                        
                    progress_Update(0);

                    FSCoupleBuilder cb = new FSCoupleBuilder(leftInfo, rightInfo);
                    StartAndJoin(cb);

                    Logger.RaiseMessage("Build has completed (" + (DateTime.Now - start).TotalSeconds + " s)");

                    // Compare folder trees
                    start = DateTime.Now;
                    Logger.RaiseMessage("Comparison has started");

                    FSComparator al = new FSComparator(cb.Root1, cb.Root2);
                    StartAndJoin(al, false);

                    ItemFaceBuilder ifb = new ItemFaceBuilder(al.Root1, al.Root2);
                    StartAndJoin(ifb, false);

                    leftList = ifb.Root1;
                    rightList = ifb.Root2;

                    HasCompared = true;

                    progress_Update(100);
                    Logger.RaiseMessage("Comparison has completed (" + (DateTime.Now - start).TotalSeconds + " s)");

                    OnPropertyChanged("LeftList");
                    OnPropertyChanged("RightList");

                    if (leftList.Children.Count == 0 && rightList.Children.Count == 0)
                        Dispatcher.Invoke(() => new InfoOK("No files to synchronize!").ShowDialog());
                }
                catch (Exception)
                {
                    Logger.RaiseMessage("Comparison has aborted");
                    Dispatcher.Invoke(() => new InfoOK("Comparison has aborted!").ShowDialog());
                }
            });
            taskThread.Start();
        }*/

        #endregion //Compare

        #region Synchronize

        /*public void Synchronize()
        {
            if (leftThread != null && (leftThread.ThreadState == ThreadState.Running || leftThread.ThreadState == ThreadState.WaitSleepJoin))
            {
                Logger.RaiseMessage("Stop current task or wait for results");
                return;
            }
            if (!HasCompared)
            {
                new InfoOK("Compare first!").ShowDialog();
                return;
            }
            if (leftList.Children.Count == 0 && rightList.Children.Count == 0)
            {
                new InfoOK("No files to synchronize!").ShowDialog();
                return;
            }
            //Запрос на разрешение зеркального удаления файлов
            bool? res = new DeleteFilesDialog().ShowExtendedDialog();
            if (!res.HasValue) return;

            HasCompared = false;
            leftThread = new Thread(() =>
            {
                try
                {
                    DateTime start = DateTime.Now;
                    Logger.RaiseMessage("Synchronization has started");

                    progress_Update(0);

                    FSSyncManual syn;
                    if (Forward)
                    {
                        syn = new FSSyncManual(leftList.CmpItem as CmpDirectory, rightList.CmpItem as CmpDirectory, res.Value);
                    }
                    else
                    {
                        syn = new FSSyncManual(rightList.CmpItem as CmpDirectory, leftList.CmpItem as CmpDirectory, res.Value);
                    }
                    StartAndJoin(syn);
                    
                    progress_Update(100);

                    Logger.RaiseMessage("Synchronization has completed (" + (DateTime.Now - start).TotalSeconds + " s)");

                    leftList = null;
                    rightList = null;

                    OnPropertyChanged("LeftList");
                    OnPropertyChanged("RightList");
                }
                catch (Exception)
                {
                    if (curTask != null && curTask.IsRunning)
                    {
                        curTask.Abort();
                        curTask.Join();
                    }
                    Logger.RaiseMessage("Synchronization has aborted");
                    Dispatcher.Invoke(() => new InfoOK("Synchronization has aborted!").ShowDialog());
                }
            });
            leftThread.Start();
        }*/

        #endregion //Synchronize

        #region Inner Task

        /// <summary>
        /// Запуск подпроцесса в синхронном режиме
        /// </summary>
        /// <param name="task"></param>
        /// <param name="updateProgress"></param>
        private void StartAndJoin(AsyncBase task, String progress = null)
        {
            task.Start();

            while (task.IsRunning)
            {
                if (progress == "global") progress_Update(task.Progress);
                Thread.Sleep(100);
            }
        }

        #endregion //Inner Task

        #region Components' Handlers

        private void tree_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
                ((sender as TreeView).Parent as ScrollViewer).LineUp();
            }
            else
            {
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
                ((sender as TreeView).Parent as ScrollViewer).LineDown();
            }

        }

        private void drop_Over(object sender, DragEventArgs e)
        {

        }

        private void tree_Drop(object sender, DragEventArgs e)
        {
            Boolean isLeft = (e.Source as TreeView).Name == "leftTree";

            //Нельзя менять директорию во время построения или синхронизации
            Thread thread = isLeft ? leftThread : rightThread;
            if (thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.WaitSleepJoin))
            {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Проверка существования директории
                if (!Directory.Exists(folders[0]))
                {
                    Dispatcher.Invoke(() => new InfoOK("Incorrect path").ShowDialog());
                    return;
                }

                //Build tree
                DirectoryInfo dirInfo = new DirectoryInfo(folders[0]);
                thread = new Thread(() =>
                {
                    try
                    {
                        // Build folder trees
                        DateTime start = DateTime.Now;
                        Logger.RaiseMessage("Build started");

                        FSBuilder fsb = new FSBuilder(dirInfo);
                        progress_Update(0);
                        StartAndJoin(fsb, isLeft ? "left" : "right");
                        progress_Update(100);

                        if (isLeft) leftList = fsb.Root;
                        else rightList = fsb.Root;

                        Logger.RaiseMessage("Build completed (" + (DateTime.Now - start).TotalSeconds + " s)");

                        // Compare folder trees
                        if (leftList != null && rightList != null)
                        {
                            start = DateTime.Now;
                            Logger.RaiseMessage("Comparison started");

                            FSComparator al = new FSComparator(leftList, rightList);
                            StartAndJoin(al);

                            HasCompared = true;

                            Logger.RaiseMessage("Comparison completed (" + (DateTime.Now - start).TotalSeconds + " s)");

                            if (leftList.Children.Count == 0 && rightList.Children.Count == 0)
                                Dispatcher.Invoke(() => new InfoOK("Nothing to synchronize!").ShowDialog());
                        }
                    }
                    catch (Exception)
                    {
                        if (isLeft) leftList = null;
                        else rightList = null;

                        Logger.RaiseMessage("Comparison aborted");
                        Dispatcher.Invoke(() => new InfoOK("Comparison aborted!").ShowDialog());
                    }

                    OnPropertyChanged("LeftList");
                    OnPropertyChanged("RightList");
                });
                thread.Start();

                if (isLeft) leftThread = thread;
                else rightThread = thread;
            }
        }

        #endregion //Components' Handlers

        #region Project Menu

        /// <summary>
        /// Обработка события синхронизации
        /// </summary>
        private void Menu_Synchronize(object sender, RoutedEventArgs e)
        {
            //Synchronize();
        }

        /// <summary>
        /// Обработка события остановки
        /// </summary>
        private void Menu_Stop(object sender, RoutedEventArgs e)
        {
            InterruptAll();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Обновление полосы прогресса
        /// </summary>
        /// <param name="obj"></param>
        private void progress_Update(Double obj)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (obj == -1) progressBar.IsIndeterminate = true;
                else progressBar.IsIndeterminate = false;

                progressBar.Value = obj;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Добавление сообщения в журнал
        /// </summary>
        /// <param name="obj"></param>
        private void Logger_OnMessage(String msg)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                statusComboBox.Items.Insert(0, msg);
                statusComboBox.SelectedIndex = 0;
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        #endregion

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
