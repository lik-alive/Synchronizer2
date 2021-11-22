using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Synchronizer2.Model
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
        public List<FSItem> Children { get; protected set; } = new List<FSItem>();

        /// <summary>
        /// Различающиеся дочерние объекты (папки + файлы)
        /// </summary>
        public List<FSItem> UnequalChildren
        {
            get
            {
                List<FSItem> res = new List<FSItem>();
                foreach (FSItem item in Children)
                {
                    if (!item.IsEqual) res.Add(item);
                }
                return res;
            }
        }

        #endregion


        #region Comparison

        /// <summary>
        /// Двойник
        /// </summary>
        public FSItem Twin { get; protected set; } = null;

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
        /// Флаг идентичности объектов
        /// </summary>
        public Boolean IsEqual { get; private set; } = false;

        /// <summary>
        /// Установка флага идентичности
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public static void SetEquals(FSItem item1, FSItem item2)
        {
            item1.IsEqual = true;
            item2.IsEqual = true;
        }

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
        /// Рекурсивная чистка всех флагов
        /// </summary>
        public void Clear()
        {
            foreach (FSItem item in Children)
            {
                item.Clear();
            }

            isExpanded = false;
            isChecked = false;
            Twin = null;
            IsEqual = false;
            NestedFlags = new Boolean[5] { false, false, false, false, false };
        }

        #endregion


        #region Checks

        /// <summary>
        /// Флаг выбора объекта для синхронизации
        /// </summary>
        private Boolean? isChecked = false;

        /// <summary>
        /// Статус выбора объекта для синхронизации
        /// </summary>
        public Boolean? IsChecked
        {
            get { return isChecked; }
            set
            {
                SetCheckedDown(value);
                if (Twin != null) Twin.SetCheckedDown(value);

                if (Parent != null) Parent.VerifyCheckState();
            }
        }

        /// <summary>
        /// Установка статуса выбора объекта
        /// </summary>
        /// <param name="value"></param>
        private void SetChecked(Boolean? value)
        {
            isChecked = value;
            OnPropertyChanged("IsChecked");
        }

        /// <summary>
        /// Установка статуса выбора объекта вниз по дереву
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        private void SetCheckedDown(bool? value)
        {
            if (value == isChecked) return;

            SetChecked(value);

            if (IsDirectory) UnequalChildren.ForEach(c => c.SetCheckedDown(value));
        }

        /// <summary>
        /// Проверка корректности статуса выбора
        /// </summary>
        private void VerifyCheckState()
        {
            bool? value = CalcCheckState();
            SetChecked(value);

            if (Twin != null)
            {
                bool? twinValue = Twin.CalcCheckState();
                Twin.SetChecked(twinValue);
            }

            if (Parent != null) Parent.VerifyCheckState();
        }

        /// <summary>
        /// Вычисление статуса выбора по дочерним объектам
        /// </summary>
        private bool? CalcCheckState()
        {
            bool? value = false;
            for (int i = 0; i < UnequalChildren.Count; i++)
            {
                bool? current = UnequalChildren[i].isChecked;
                if (i == 0) value = current;
                else if (value != current)
                {
                    value = null;
                    break;
                }
            }

            return value;
        }

        #endregion

        #region Appearance

        /// <summary>
        /// Иконка папки
        /// </summary>
        protected static Uri iconFolder = new Uri("/Synchronizer2;component/Resources/folder.ico", UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Иконка файла
        /// </summary>
        protected static Uri iconFile = new Uri("/Synchronizer2;component/Resources/file.ico", UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Иконка объекта
        /// </summary>
        public Uri Icon { get; protected set; }

        /// <summary>
        /// Флаг раскрытия списка вложенных элементов
        /// </summary>
        private Boolean isExpanded = false;

        /// <summary>
        /// Раскрытие списка вложенных элементов
        /// </summary>
        public Boolean IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                SetIsExpanded(value); 
                if (Twin != null) Twin.SetIsExpanded(value);
            }
        }

        /// <summary>
        /// Задание флага раскрытия списка вложенных элементов
        /// </summary>
        private void SetIsExpanded(Boolean value)
        {
            isExpanded = value;
            OnPropertyChanged("IsExpanded");
        }

        /// <summary>
        /// Задание флага раскрытия списка всех вложенных элементов
        /// </summary>
        /// <param name="value"></param>
        public void SetIsExpandedAll(Boolean value, Boolean isAnalyzed)
        {
            if (!value) SetIsCollapsedAll();
            else
            {
                Int32 quota = 1000;
                Int32 level = 0;
                while (quota > 0)
                {
                    Int32 newQuota = SetIsExpandedAllWave(level, quota, isAnalyzed);
                    if (newQuota == quota) break;

                    quota = newQuota;
                    level++;
                }
            }
        }

        /// <summary>
        /// Раскрытие элементов волнами (иначе вешает систему)
        /// </summary>
        /// <param name="value"></param>
        private Int32 SetIsExpandedAllWave(Int32 level, Int32 quota, Boolean isAnalyzed)
        {
            if (level == 0)
            {
                SetIsExpanded(true);
                if (isAnalyzed) quota -= UnequalChildren.Count;
                else quota -= Children.Count;
            }
            else
            {
                foreach (FSItem item in Children)
                {
                    if (item.IsDirectory && quota > 0) quota = item.SetIsExpandedAllWave(level - 1, quota, isAnalyzed);
                }
            }
            
            return quota;
        }

        /// <summary>
        /// Сокрытие всех элементов
        /// </summary>
        private void SetIsCollapsedAll()
        {
            SetIsExpanded(false);
            foreach (FSItem item in Children)
            {
                if (item.IsDirectory) item.SetIsCollapsedAll();
            }
        }

        /// <summary>
        /// Флаг выбора объекта
        /// </summary>
        private Boolean isSelected = false;

        /// <summary>
        /// Выбор объекта
        /// </summary>
        public Boolean IsShadowSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsShadowSelected");
            }
        }

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
