using System;
using System.IO;

namespace Synchronizer.Model
{
    public class FSFile : FSItem
    {
        #region Properties

        /// <summary>
        /// Информация о файле
        /// </summary>
        public new FileInfo Info
        {
            get
            {
                return base.Info as FileInfo;
            }
        }

        /// <summary>
        /// Размер файла
        /// </summary>
        public long Length
        {
            get
            {
                return Info.Length;
            }
        }

        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                return Info.LastWriteTime;
            }
        }

        #endregion // Properties

        public FSFile(FileInfo info, FSDirectory parent) : base(info, parent)
        {
            Icon = iconFile;
        }

        /// <summary>
        /// Установка флагов для двойников
        /// </summary>
        /// <param name="twin"></param>
        public static void SetTwinFlags(FSFile file1, FSFile file2)
        {
            file1.SetAllFlags(false, file1.LastWriteTime > file2.LastWriteTime,
                                file1.LastWriteTime < file2.LastWriteTime,
                                file1.Length > file2.Length,
                                file1.Length < file2.Length);

            file2.SetAllFlags(false, file1.LastWriteTime < file2.LastWriteTime,
                                file1.LastWriteTime > file2.LastWriteTime,
                                file1.Length < file2.Length,
                                file1.Length > file2.Length);
        }
    }
}