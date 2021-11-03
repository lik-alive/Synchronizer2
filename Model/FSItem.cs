using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Synchronizer.Model
{
    public abstract class FSItem : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Сведения об объекте ФС
        /// </summary>
        protected FileSystemInfo Info;

        /// <summary>
        /// Полное имя объекта
        /// </summary>
        public String FullName
        {
            get
            {
                return Info.FullName;
            }
        }

        /// <summary>
        /// Имя объекта
        /// </summary>
        public String Name
        {
            get
            {
                return Info.Name;
            }
        }

        /// <summary>
        /// Имя объекта в нижнем регистре
        /// </summary>
        public String LowerName { get; protected set; }

        /// <summary>
        /// Является ли объект папкой
        /// </summary>
        public Boolean IsDirectory
        {
            get
            {
                return Info is DirectoryInfo;
            }
        }

        /// <summary>
        /// Родительский объект
        /// </summary>
        public FSDirectory Parent { get; protected set; }

        /// <summary>
        /// Дочерние объекты (папки + файлы)
        /// </summary>
        public List<FSItem> Children { get; protected set; } = null;

        /// <summary>
        /// Удаление файла из структуры
        /// </summary>
        public void Disembed()
        {
            if (Parent != null) Parent.Children.Remove(this);
        }

        #endregion


        #region Comparison

        /// <summary>
        /// Двойник
        /// </summary>
        public FSItem Twin { get; protected set; } = null;

        /// <summary>
        /// Флаг уникальности объекта
        /// </summary>
        public Boolean IsUnique
        {
            get
            {
                return Twin == null;
            }
        }

        /// <summary>
        /// Флаги сравнения [Уникальный, Новее, Старше, Больше, Меньше]
        /// Передаются вверх по дереву ФС
        /// </summary>
        public Boolean[] NestedFlags { get; protected set; } = new Boolean[5] { false, false, false, false, false };


        /// <summary>
        /// Установка всех флагов
        /// </summary>
        /// <param name="isUnique"></param>
        /// <param name="isNewer"></param>
        /// <param name="isOlder"></param>
        /// <param name="isBigger"></param>
        /// <param name="isSmaller"></param>
        public void SetAllFlags(Boolean isUnique, Boolean isNewer, Boolean isOlder, Boolean isBigger, Boolean isSmaller)
        {
            NestedFlags[0] = isUnique;
            NestedFlags[1] = isNewer;
            NestedFlags[2] = isOlder;
            NestedFlags[3] = isBigger;
            NestedFlags[4] = isSmaller;
        }

        /// <summary>
        /// Установка двойника
        /// </summary>
        /// <param name="twin"></param>
        public static void SetTwins(FSItem item1, FSItem item2)
        {
            item1.Twin = item2;
            item2.Twin = item1;
        }

        /// <summary>
        /// Проброс дочерних флагов вверх по дереву
        /// </summary>
        public void UpdateFlagsToRoot()
        {
            for (Int32 i = 0; i < 5; i++)
                if (NestedFlags[i]) SetFlagToRoot(Parent, i);
        }

        /// <summary>
        /// Обновление родительских флагов
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        private void SetFlagToRoot(FSItem item, Int32 i)
        {
            item.NestedFlags[i] = true;
            if (item.Parent != null) SetFlagToRoot(item.Parent, i);
        }

        #endregion


        #region Checks

        /// <summary>
        /// Флаг выбора объекта для синхронизации
        /// </summary>
        private Boolean? isChecked = true;

        /// <summary>
        /// Статус выбора объекта для синхронизации
        /// </summary>
        public Boolean? IsChecked
        {
            get { return isChecked; }
            set
            {
                SetIsChecked(value);
                if (Twin != null) Twin.SetIsChecked(value);
            }
        }

        /// <summary>
        /// Установка статуса выбора объекта
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        private void SetIsChecked(bool? value, bool updateChildren = true, bool updateParent = true)
        {
            if (value == isChecked) return;

            isChecked = value;

            if (updateChildren && IsDirectory)
                Children.ForEach(c => c.SetIsChecked(value, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        /// <summary>
        /// Проверка корректности статуса
        /// </summary>
        private void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Children.Count; i++)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0) state = current;
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }

        #endregion

        #region Appearance

        /// <summary>
        /// Иконка папки
        /// </summary>
        protected static Uri iconFolder = new Uri("/Synchronizer;component/Resources/folder.ico", UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Иконка файла
        /// </summary>
        protected static Uri iconFile = new Uri("/Synchronizer;component/Resources/file.ico", UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Иконка объекта
        /// </summary>
        public Uri Icon { get; protected set; }

        #endregion


        public FSItem(FileSystemInfo info, FSDirectory parent)
        {
            Info = info;
            Parent = parent;
            LowerName = info.Name.ToLower();
        }


        public override string ToString()
        {
            return Name;
        }

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
