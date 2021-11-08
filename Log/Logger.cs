using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Synchronizer.Log
{
    public class LogItem
    {
        public String Message { get; private set; }

        public Boolean IsError {get; private set;}

        public LogItem(String message, Boolean isError)
        {
            Message = message;
            IsError = isError;
        }

        public override string ToString()
        {
            return Message;
        }
    }


    public class Logger
    {
        public static event Action<LogItem> OnMessage;

        public static void RaiseLog(String msg)
        {
            OnMessage?.Invoke(new LogItem("[" + DateTime.Now + "] " + msg, false));
        }

        public static void RaiseError(String msg)
        {
            OnMessage?.Invoke(new LogItem("[" + DateTime.Now + "] " + msg, true));
        }
    }
}
