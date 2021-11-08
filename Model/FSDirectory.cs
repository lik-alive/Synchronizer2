using System.Collections.Generic;
using System.IO;

namespace Synchronizer.Model
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
        

        #endregion // Properties

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