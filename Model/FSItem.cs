using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

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
        /// Заглушка для корректного отображения дерева объектов
        /// </summary>
        public virtual List<FSItem> UnequalChildren
        {
            get;
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
        public virtual void Clear()
        {
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
                // Обновление статуса выбора текущего элемента и вниз по дереву
                UpdateCheckedThisAndDown(value);
                Twin?.UpdateCheckedThisAndDown(value);

                // Проверка корректности статуса выбора вверх по дереву
                Parent?.VerifyCheckUp();
                Twin?.Parent?.VerifyCheckUp();
            }
        }

        /// <summary>
        /// Установка статуса выбора объекта и вызов обновления интерфейса
        /// </summary>
        /// <param name="value"></param>
        protected void SetChecked(Boolean? value)
        {
            isChecked = value;
            OnPropertyChanged("IsChecked");
        }

        /// <summary>
        /// Обновление статуса выбора объекта вниз по дереву
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        public virtual void UpdateCheckedThisAndDown(bool? value)
        {
            SetChecked(value);
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
                Twin?.SetIsExpanded(value);
            }
        }

        /// <summary>
        /// Задание флага раскрытия списка вложенных элементов
        /// </summary>
        protected void SetIsExpanded(Boolean value)
        {
            isExpanded = value;
            OnPropertyChanged("IsExpanded");
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
