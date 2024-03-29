﻿using Synchronizer2.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Synchronizer2.Styles
{
    /// <summary>
    /// Interaction logic for StatusWithProgress.xaml
    /// </summary>
    public partial class StatusWithProgress : UserControl, INotifyPropertyChanged
    {

        /// <summary>
        /// Журнал событий
        /// </summary>
        public List<LogItem> Logs { get; private set; } = new List<LogItem>();

        /// <summary>
        /// Отображение журнала событий
        /// </summary>
        public Visibility LogsVisibility
        {
            get
            {
                return Logs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Прогресс выполнения
        /// </summary>
        public Double Progress { get; private set; }

        public StatusWithProgress()
        {
            Logger.OnMessage += Logger_OnMessage;

            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Задание текущего прогресса
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(Double value)
        {
            OnPropertyChanged("Progress", value);
        }

        /// <summary>
        /// Добавление сообщения в журнал
        /// </summary>
        /// <param name="obj"></param>
        private void Logger_OnMessage(LogItem log)
        {
            Logs.Insert(0, log);
            OnPropertyChanged("Logs");
            OnPropertyChanged("LogsVisibility");
        }

        /// <summary>
        /// Always select the last item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void status_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.Items.Count > 0)
            {
                cb.SelectedItem = cb.Items[0];
                cb.SelectedIndex = 0;
            }
            e.Handled = true;
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
