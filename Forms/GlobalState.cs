using System;
using System.Collections.Generic;
using System.Text;

namespace Synchronizer2.Forms
{
    public class GlobalState
    {
        private static GlobalState instance = null;

        /// <summary>
        /// Флаг раскрытия/скрытия всех элементов списка
        /// </summary>
        private Boolean isExpandAll = false;

        /// <summary>
        /// Событие раскрытия/скрытия всех элементов списка
        /// </summary>
        public event Action IsExpandAllChanged;

        /// <summary>
        /// Свойство раскрытия/скрытия всех элементов списка
        /// </summary>
        public Boolean IsExpandAll
        {
            get
            {
                return isExpandAll;
            }
            set
            {
                isExpandAll = value;
                IsExpandAllChanged?.Invoke();
            }
        }

        /// <summary>
        /// Перечень стадий
        /// </summary>
        public enum Stages
        {
            START,
            BUILDED,
            ANALYZING,
            ANALYZED,
            SYNCHRONIZING
        }

        /// <summary>
        /// Текущая стадия
        /// </summary>
        private Stages stage = Stages.START;

        /// <summary>
        /// Событие смены стадии
        /// </summary>
        public event Action StageChanged;

        /// <summary>
        /// Свойство смены стадии
        /// </summary>
        public Stages Stage
        {
            get
            {
                return stage;
            }
            set
            {
                stage = value;
                StageChanged?.Invoke();
            }
        }

        private GlobalState()
        {
        }

        public static GlobalState Instance
        {
            get
            {
                if (instance == null) instance = new GlobalState();
                return instance;
            }
        }
    }
}
