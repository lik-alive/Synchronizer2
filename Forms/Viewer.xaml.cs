using Synchronizer2.Model;
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

namespace Synchronizer2.Forms
{
    public partial class Viewer : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Окно-двойник
        /// </summary>
        public Viewer Twin { get; set; } = null;

        /// <summary>
        /// Дерево объектов
        /// </summary>
        public FSTree Tree { get; private set; } = new FSTree(null);

        /// <summary>
        /// Событие построения дерева
        /// </summary>
        public event Action Builded;

        public Viewer()
        {
            InitializeComponent();
            
            dropZone.Dropped += DropZone_Dropped;
            buildZone.Completed += BuildZone_Completed;
            buildZone.Aborted += BuildZone_Aborted;
            treeZone.Refresh += TreeZone_Refresh;
            treeZone.Close += TreeZone_Close;

            tabs.SelectedIndex = 0;

            Loaded += (sender, e) =>
            {
                LoadFromHistory();
            };
        }

        /// <summary>
        /// Перестроение дерева
        /// </summary>
        public void Rebuild()
        {
            String fullName = Tree.FullName;
            Tree = new FSTree(null);
            tabs.SelectedIndex = 1;
            buildZone.BuildTree(fullName);
        }

        /// <summary>
        /// Перерисовка дерева
        /// </summary>
        public void Redraw()
        {
            treeZone.Redraw(Tree);
        }

        /// <summary>
        /// Отображение/скрытие панели действий
        /// </summary>
        /// <param name="status"></param>
        public void SetMenuVisibility(Boolean visible)
        {
            treeZone.SetMenuVisibility(visible);
        }

        /// <summary>
        /// Папка выбрана
        /// </summary>
        private void DropZone_Dropped()
        {
            tabs.SelectedIndex = 1;
            buildZone.BuildTree(dropZone.Folder);
            SaveToHistory();
        }

        /// <summary>
        /// Дерево построено
        /// </summary>
        private void BuildZone_Completed(FSTree tree)
        {
            Tree = tree;
            Redraw();
            tabs.SelectedIndex = 2;
            Builded?.Invoke();
        }

        /// <summary>
        /// Построение прервано
        /// </summary>
        private void BuildZone_Aborted()
        {
            // Aborted during first build
            if (Tree.Root == null)
            {
                tabs.SelectedIndex = 0;
            } 
            // Aborted during refresh
            else
            {
                tabs.SelectedIndex = 2;
            }
        }

        /// <summary>
        /// Дерево перестроено
        /// </summary>
        private void TreeZone_Refresh()
        {
            tabs.SelectedIndex = 1;
            buildZone.BuildTree(dropZone.Folder);
            
            // Rebuild twin viewer
            if (Twin.Tree.Root != null)
            {
                Twin.Tree = new FSTree(null);
                Twin.buildZone.BuildTree(Twin.dropZone.Folder);
            }

            GlobalState.Instance.Stage = GlobalState.Stages.START;
        }

        /// <summary>
        /// Очистка отображения
        /// </summary>
        private void TreeZone_Close()
        {
            tabs.SelectedIndex = 0;
            Tree = new FSTree(null);

            if (Twin.Tree.Root != null && Twin.Tree.IsAnalyzed)
            {
                Twin.Tree.IsAnalyzed = false;
                Twin.Tree.Root.Clear();
                Twin.Redraw();
            }

            GlobalState.Instance.Stage = GlobalState.Stages.START;
        }

        #region History

        /// <summary>
        /// Имя файла настроек
        /// </summary>
        private String historyFile = System.IO.Path.GetTempPath() + "sync_history.txt";

        /// <summary>
        /// Загрузка истории из темпового файла
        /// </summary>
        private void LoadFromHistory()
        {
            if (File.Exists(historyFile))
            {
                List<String> history = new List<String>();
                StreamReader sr = new StreamReader(historyFile);
                while (!sr.EndOfStream)
                {
                    String str = sr.ReadLine();
                    if (str.StartsWith(Name + "_"))
                    {
                        history.Insert(0, str.Substring(Name.Length + 1));
                    }
                }
                sr.Close();

                dropZone.SetHistory(history);
            }
        }

        /// <summary>
        /// Сохранение пути в темповый файл
        /// </summary>
        private void SaveToHistory()
        {
            StreamWriter sw = new StreamWriter(new FileStream(historyFile, FileMode.Append));
            sw.WriteLine(Name + "_" + dropZone.Folder);
            sw.Close();
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
