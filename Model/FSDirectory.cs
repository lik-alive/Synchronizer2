using System;
using System.Collections.Generic;
using System.IO;

namespace Synchronizer2.Model
{   
    public class FSDirectory : FSItem
    {
        #region Properties

        /// <summary>
        /// Информация о директории
        /// </summary>
        public new DirectoryInfo Info
        {
            get
            {
                return base.Info as DirectoryInfo;
            }
        }

        /// <summary>
        /// Дочерние объекты (папки + файлы)
        /// </summary>
        public List<FSItem> Children { get; }

        /// <summary>
        /// Различающиеся дочерние объекты (папки + файлы)
        /// </summary>
        public override List<FSItem> UnequalChildren
        {
            get
            {
                List<FSItem> res = new List<FSItem>();
                foreach (FSItem item in Children)
                {
                    if (!item.IsEqual)
                    {
                        res.Add(item);
                    }
                }
                return res;
            }
        }

        #endregion // Properties

        #region Comparison

        /// <summary>
        /// Двойник
        /// </summary>
        public new FSDirectory Twin { 
            get
            {
                return base.Twin as FSDirectory;
            }
        }

        /// <summary>
        /// Рекурсивная чистка всех флагов
        /// </summary>
        public override void Clear()
        {
            // Очистка флагов всех дочерних объектов
            Children.ForEach(c => c.Clear());

            base.Clear();
        }

        #endregion //Comparison

        #region Checks

        /// <summary>
        /// Обновление статуса выбора текущего элемента и вниз по дереву
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateChildren"></param>
        /// <param name="updateParent"></param>
        public override void UpdateCheckedThisAndDown(bool? value)
        {
            base.UpdateCheckedThisAndDown(value);

            UnequalChildren.ForEach(c =>
            {
                if (c.IsChecked != value)
                {
                    c.UpdateCheckedThisAndDown(value);
                }
            });
        }

        /// <summary>
        /// Проверка корректности статуса выбора вверх по дереву
        /// </summary>
        public void VerifyCheckUp()
        {
            bool? value = CalcCheckState();

            if (value != IsChecked)
            {
                SetChecked(value);

                Parent?.VerifyCheckUp();
            }
        }

        /// <summary>
        /// Вычисление статуса выбора по дочерним объектам
        /// </summary>
        private bool? CalcCheckState()
        {
            if (UnequalChildren.Count == 0) return false;

            bool? value = UnequalChildren[0].IsChecked;

            // Неопределённое значение, если есть и выбранные, и невыбранные объекты
            if (UnequalChildren.Exists(c => c.IsChecked != value))
            {
                return null;
            }

            return value;
        }

        #endregion

        #region Appearance

        /// <summary>
        /// Задание флага раскрытия списка всех вложенных элементов
        /// </summary>
        /// <param name="value"></param>
        public void SetIsExpandedAll(Boolean value, Boolean isAnalyzed)
        {
            if (!value) CollapseAll();
            else
            {
                Int32 quota = 1000;
                Int32 level = 0;
                while (quota > 0)
                {
                    Int32 newQuota = ExpandAllWave(level, quota, isAnalyzed);
                    if (newQuota == quota) break;

                    quota = newQuota;
                    level++;
                }
            }
        }

        /// <summary>
        /// Сокрытие всех элементов
        /// </summary>
        private void CollapseAll()
        {
            SetIsExpanded(false);

            // Сокрытие дочерних папок
            Children.FindAll(c => c.IsDirectory).ForEach(c => (c as FSDirectory).CollapseAll());
        }

        /// <summary>
        /// Раскрытие элементов волнами (иначе вешает систему)
        /// </summary>
        /// <param name="value"></param>
        private Int32 ExpandAllWave(Int32 level, Int32 quota, Boolean isAnalyzed)
        {
            if (level == 0)
            {
                SetIsExpanded(true);

                if (isAnalyzed) quota -= UnequalChildren.Count;
                else quota -= Children.Count;
            }
            else
            {
                Children.ForEach(c =>
                {
                    if (c.IsDirectory && quota > 0) quota = (c as FSDirectory).ExpandAllWave(level - 1, quota, isAnalyzed);
                });
            }

            return quota;
        }

        #endregion

        public FSDirectory(DirectoryInfo info, FSDirectory parent) : base(info, parent)
        {
            Children = new List<FSItem>();
            Icon = iconFolder;
        }

        /// <summary>
        /// Встраивание папки в структуру
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public FSDirectory EmbedDirectory(DirectoryInfo info)
        {
            FSDirectory dir = new FSDirectory(info, this);
            Children.Add(dir);
            return dir;
        }

        /// <summary>
        /// Встраивание файла в структуру
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public FSFile EmbedFile(FileInfo info)
        {
            FSFile file = new FSFile(info, this);
            Children.Add(file);
            return file;
        }
    }
}