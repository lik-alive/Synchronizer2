using System;
using System.IO;

namespace Synchronizer2.Model
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
    }
}