using System;
using System.Collections;
using System.Collections.Generic;

namespace BT
{
    public class LogicError : Exception
    {
        private string message_;
        public LogicError(string message)
        {
            this.message_ = message;
        }
    }
}

