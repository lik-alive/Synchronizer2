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
    public partial class TreeZone : UserControl, INotifyPropertyChanged
    {

        public Boolean IsReadyToSync {get; set;} = false;

        public TreeZone()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region FolderProperty

        public static readonly DependencyProperty folderProperty =
            DependencyProperty.Register(
                "Tree",
                typeof(List<FSItem>),
                typeof(TreeZone),
                new PropertyMetadata(null, new PropertyChangedCallback(ChangeTree)));

        public List<FSItem> Tree
        {
            get => (List<FSItem>)GetValue(folderProperty);
            set => SetValue(folderProperty, value);
        }

        private static void ChangeTree(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
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
