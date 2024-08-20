using System;
using System.Collections.Generic;
using System.Text;

namespace Synchronizer2.Model
{
    public class FSTree
    {
        /// <summary>
        /// Корневая папка
        /// </summary>
        public FSDirectory Root { get; private set; } = null;

        /// <summary>
        /// Полный путь к корневой папке
        /// </summary>
        public String FullName { get; set; } = null;

        /// <summary>
        /// Список различающихся файлов
        /// </summary>
        public List<FSItem> UnequalChildren
        {
            get
            {
                return Root?.UnequalChildren;
            }
        }

        /// <summary>
        /// Статус анализа
        /// </summary>
        public Boolean IsAnalyzed { get; set; } = false;

        public FSTree(FSDirectory root)
        {
            Root = root;
            FullName = root?.FullName;
        }
    }
}
