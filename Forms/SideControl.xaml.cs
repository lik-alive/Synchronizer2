using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SideControl.xaml
    /// </summary>
    public partial class SideControl : UserControl
    {
        public SideControl()
        {
            InitializeComponent();

            dropZone.Dropped += DropZone_Dropped;
            buildZone.Completed += BuildZone_Completed;
            buildZone.Aborted += BuildZone_Aborted;

            tabs.SelectedIndex = 0;
        }

        private void DropZone_Dropped()
        {
            tabs.SelectedIndex = 1;
            buildZone.BuildTree(dropZone.Folder);
        }

        private void BuildZone_Completed()
        {
            tabs.SelectedIndex = 2;
        }
        private void BuildZone_Aborted()
        {
            tabs.SelectedIndex = 0;
        }
    }
}
