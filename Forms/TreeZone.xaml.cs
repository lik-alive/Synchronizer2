using Synchronizer2.Model;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Synchronizer2.Forms
{
    public partial class TreeZone : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Событие закрытия папки
        /// </summary>
        public event Action Close;

        /// <summary>
        /// Событие обновления папки
        /// </summary>
        public event Action Refresh;

        /// <summary>
        /// Дерево папок
        /// </summary>
        public FSTree Tree { get; private set; } = new FSTree(null);

        /// <summary>
        /// Показать панель действий
        /// </summary>
        public Boolean ShowActions { get; private set; } = true;

        /// <summary>
        /// Флаг раскрытия всего списка
        /// </summary>
        public Boolean IsExpandAll { 
            get
            {
                return GlobalState.Instance.IsExpandAll;
            }
            private set
            {
                GlobalState.Instance.IsExpandAll = value;
            }
        }

        public TreeZone()
        {
            InitializeComponent();
            DataContext = this;

            GlobalState.Instance.IsExpandAllChanged += () =>
            {
                OnPropertyChanged("IsExpandAll");
                Tree.Root?.SetIsExpandedAll(IsExpandAll, Tree.IsAnalyzed);
            };
        }

        /// <summary>
        /// Отрисовка дерева
        /// </summary>
        /// <param name="tree"></param>
        public void Redraw(FSTree tree)
        {
            tree.Root?.SetIsExpandedAll(IsExpandAll, Tree.IsAnalyzed);
            OnPropertyChanged("Tree", tree);
        }

        /// <summary>
        /// Отображение/скрытие панели действий
        /// </summary>
        /// <param name="status"></param>
        public void SetMenuVisibility(Boolean visible)
        {
            OnPropertyChanged("ShowActions", visible);
        }

        /// <summary>
        /// Раскрытие/сворачивание всего дерева
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void expandAll_Click(object sender, RoutedEventArgs e)
        {
            IsExpandAll = !IsExpandAll;
        }

        /// <summary>
        /// Перестроение дерева
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh?.Invoke();
        }

        /// <summary>
        /// Удаление дерева
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void close_Click(object sender, RoutedEventArgs e)
        {
            Redraw(new FSTree(null));
            Close?.Invoke();
        }

        /// <summary>
        /// Обработка нажатия клавиш
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Tree.IsAnalyzed && e.Key == System.Windows.Input.Key.Space)
            {
                FSItem item = ((sender as TreeViewItem).DataContext as FSItem);
                item.IsChecked = !item.IsChecked;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка события получения фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TreeViewItem).DataContext is FSItem item && item.Twin != null)
            {
                item.Twin.IsShadowSelected = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка события потери фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeItem_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TreeViewItem).DataContext is FSItem item && item.Twin != null)
            {
                item.Twin.IsShadowSelected = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка события нажатия правой кнопкой мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeItem_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as TreeViewItem).Focus();
            e.Handled = true;
        }

        /// <summary>
        /// Обработка события открытия объекта в Проводнике
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void explorer_Click(object sender, RoutedEventArgs e)
        {
            FSItem item = (sender as MenuItem).DataContext as FSItem;

            // Open parent directory for files
            if (!item.IsDirectory) item = item.Parent;

            Process.Start("explorer.exe", item.FullName);
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
