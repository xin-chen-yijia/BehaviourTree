using System;
using System.Collections;
using System.Collections.Generic;

namespace BT
{
    public class BehaviorTreeException : Exception
    {
        protected string message_;
        public BehaviorTreeException(string message)
        {
            this.message_ = message;
        }

        //public BehaviorTreeException<T>(const SV&... args): message_(StrCat (args...))
        //{ }


        string What()
        {
            return message_;
        }

    }


public class LogicError : BehaviorTreeException
    {
        public LogicError(string message) : base(message)
        {
        }
    }

    // This errors are usually related to problems that are relted to data or conditions
    // that happen only at run-time
    public class RuntimeError : BehaviorTreeException
    {
        public RuntimeError(string message) : base(message)
        { }
    };
}



