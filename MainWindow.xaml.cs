using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Synchronizer2.Model;
using Synchronizer2.Forms;
using Synchronizer2.Processing;
using System.Reflection;
using System.Windows.Media;

namespace Synchronizer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Текущий процесс
        /// </summary>
        private AsyncBase task;

        /// <summary>
        /// Прогресс выполнения
        /// </summary>
        public Double Progress { get; private set; } = 100;

        /// <summary>
        /// Отображение кнопки анализа
        /// </summary>
        public Boolean? ShowAnalysis { get; private set; } = false;

        /// <summary>
        /// Отображение кнопки синхронизации
        /// </summary>
        public Boolean? ShowSync { get; private set; } = false;

        /// <summary>
        /// Отображение кнопки остановки
        /// </summary>
        public Boolean ShowAbort { get; private set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            left.Twin = right;
            right.Twin = left;

            left.Builded += Builded;
            right.Builded += Builded;

            GlobalState.Instance.StageChanged += Instance_StageChanged;
        }

        /// <summary>
        /// Закрытие программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Closed(object sender, EventArgs e)
        {
            // Прерывание текущего процесса
            task?.Abort();
        }

        /// <summary>
        /// Обработка смены текущей стадии
        /// </summary>
        private void Instance_StageChanged()
        {
            switch (GlobalState.Instance.Stage)
            {
                case GlobalState.Stages.START:
                    OnPropertyChanged("ShowAnalysis", false);
                    OnPropertyChanged("ShowSync", false);
                    OnPropertyChanged("ShowAbort", false);
                    break;
                case GlobalState.Stages.BUILDED:
                    OnPropertyChanged("ShowAnalysis", true);
                    OnPropertyChanged("ShowSync", false);
                    OnPropertyChanged("ShowAbort", false);
                    break;
                case GlobalState.Stages.ANALYZING:
                    OnPropertyChanged("ShowAnalysis", false);
                    OnPropertyChanged("ShowSync", false);
                    OnPropertyChanged("ShowAbort", true);
                    break;
                case GlobalState.Stages.ANALYZED:
                    OnPropertyChanged("ShowAnalysis", false);
                    OnPropertyChanged("ShowSync", true);
                    OnPropertyChanged("ShowAbort", false);
                    break;
                case GlobalState.Stages.SYNCHRONIZING:
                    OnPropertyChanged("ShowAnalysis", false);
                    OnPropertyChanged("ShowSync", false);
                    OnPropertyChanged("ShowAbort", true);
                    break;
            }
        }

        /// <summary>
        /// Запуск анализа папок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analyze_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                FSAnalyzer fsa = new FSAnalyzer(left.Tree, right.Tree);
                if (StartAndJoin(fsa, true))
                {
                    Dispatcher.Invoke(() =>
                    {
                        left.Redraw();
                        right.Redraw();

                        //Проверка отсутствия файлов на синхронизацию
                        if (left.Tree.Root.IsChecked == false && right.Tree.Root.IsChecked == false)
                        {
                            new InfoOK("Folders are equal").ShowDialog();
                            return;
                        }
                    });
                } else
                {
                    Dispatcher.Invoke(() =>
                    {
                        left.Redraw();
                        right.Redraw();

                        GlobalState.Instance.Stage = GlobalState.Stages.BUILDED;
                    });
                }
            });
            thread.Start();

            GlobalState.Instance.Stage = GlobalState.Stages.ANALYZING;
        }

        /// <summary>
        /// Запуск синхронизации папок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sync_Click(object sender, RoutedEventArgs e)
        {
            FSTree tree1, tree2;
            if ((sender as Button).Name == "toRight")
            {
                tree1 = left.Tree;
                tree2 = right.Tree;
            }
            else
            {
                tree2 = left.Tree;
                tree1 = right.Tree;
            }

            //Проверка отсутствия файлов на синхронизацию
            if (left.Tree.Root.IsChecked == false && right.Tree.Root.IsChecked == false)
            {
                new InfoOK("Folders are equal").ShowDialog();
                return;
            }

            //Запрос на запуск
            bool? res = new SynchronizeDialog(left.Tree.FullName, right.Tree.FullName, (sender as Button).Name == "toRight", HasFileForDeletion(tree2.Root)).ShowExtendedDialog();
            if (res == false) return;

            Thread thread = new Thread(() =>
            {
                // Synchronize
                FSSyncher fss = new FSSyncher(tree1, tree2, res == true);
                StartAndJoin(fss, true);

                Dispatcher.Invoke(() =>
                {
                    // Rebuild and reanalyze
                    left.Rebuild();
                    right.Rebuild();

                    if (fss.IsAborted)
                    {
                        new InfoOK("Completed with errors (check status panel)").ShowDialog();
                    }
                });

                GlobalState.Instance.Stage = GlobalState.Stages.ANALYZED;
            });
            thread.Start();

            GlobalState.Instance.Stage = GlobalState.Stages.SYNCHRONIZING;
        }

        /// <summary>
        /// Прерывание текущего процесса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void abort_Click(object sender, RoutedEventArgs e)
        {
            task.Abort();

            if (task is FSAnalyzer) GlobalState.Instance.Stage = GlobalState.Stages.BUILDED;
            else GlobalState.Instance.Stage = GlobalState.Stages.ANALYZED;
        }

        /// <summary>
        /// Проверка на наличие файлов для удаления
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private Boolean HasFileForDeletion(FSDirectory root)
        {
            foreach (FSItem item in root.Children)
            {
                if (item.IsChecked == true && item.IsUnique) return true;

                if (item.IsDirectory && HasFileForDeletion(item as FSDirectory)) return true;
            }

            return false;
        }

        /// <summary>
        /// Структура папки построена
        /// </summary>
        private void Builded()
        {
            if (left.Tree.Root != null && right.Tree.Root != null)
            {
                analyze_Click(null, null);
            }
        }

        /// <summary>
        /// Структура папки очищена
        /// </summary>
        private void Cleared()
        {
            GlobalState.Instance.Stage = GlobalState.Stages.START;
        }

        #region Inner Task

        /// <summary>
        /// Запуск подпроцесса в синхронном режиме
        /// </summary>
        /// <param name="task"></param>
        /// <param name="updateProgress"></param>
        private Boolean StartAndJoin(AsyncBase asyncBase, Boolean progress = false)
        {
            task = asyncBase;
            task.Start();

            // Block menu
            left.SetMenuVisibility(false);
            right.SetMenuVisibility(false);

            // Monitor task progress
            Int32 skipped = 0;
            while (task.IsRunning)
            {
                // Do not show progress for fast tasks
                if (progress && skipped > 5) statusPanel.SetProgress(task.Progress);
                else skipped++;
                Thread.Sleep(100);
            }

            // Allow menu
            left.SetMenuVisibility(true);
            right.SetMenuVisibility(true);

            // Check for task failure
            if (task.Status == AsyncBase.AsyncStatus.ABORTED) return false;

            // Finalize progress increment
            if (progress) statusPanel.SetProgress(100);

            // Show next step
            if (task is FSAnalyzer) OnPropertyChanged("ShowSync", true);
            OnPropertyChanged("ShowAbort", false);

            return true;
        }

        #endregion //Inner Task

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
