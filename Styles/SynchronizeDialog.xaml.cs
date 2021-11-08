using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Synchronizer.Forms
{
    /// <summary>
    /// Информационное сообщение
    /// </summary>
    public partial class SynchronizeDialog : Window
    {
        private Boolean? status = false;

        private Thread thread = null;
        private Boolean forceStop = false;

        public String LeftPath { get; private set; }

        public String RightPath { get; private set; }

        public Boolean IsLeftToRight { get; private set; }

        public Boolean HasFileForDeletion { get; private set; }

        public Boolean Duplicate { get; set; } = false;

        public SynchronizeDialog(String left, String right, Boolean leftToRight, Boolean hasFileForDeletion)
        {
            LeftPath = left;
            RightPath = right;
            IsLeftToRight = leftToRight;
            HasFileForDeletion = hasFileForDeletion;
            InitializeComponent();

            DataContext = this;
        }

        public Boolean? ShowExtendedDialog()
        {
            ShowDialog();

            // Window was closed by pressing x-icon
            if (thread != null && thread.IsAlive)
            {
                forceStop = true;
                thread.Join();
            }
            return status;
        }

        /// <summary>
        /// Подтверждение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YES_Click(object sender, RoutedEventArgs e)
        {
            thread = new Thread(() =>
            {
                Dispatcher.Invoke(() => {
                    iconYes.Visibility = Visibility.Collapsed;
                    buttonYes.IsEnabled = false;
                });

                if (PrintAndPause("3...")) return;
                if (PrintAndPause("2...")) return;
                if (PrintAndPause("1...")) return;


                if (!forceStop)
                {
                    if (Duplicate) status = true;
                    else status = null;
                    Dispatcher.Invoke(() => Close());
                }
            });
            thread.Start();
        }

        /// <summary>
        /// Обратный отсчёт на кнопке
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Boolean PrintAndPause(String text)
        {
            Dispatcher.Invoke(() => textYes.Text = text);
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(100);
                if (forceStop) return true;
            }
            return false;
        }

        /// <summary>
        /// Отказ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NO_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Подтверждение с помощью горячих клавиш
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                YES_Click(null, null);
            }
        }
    }
}
