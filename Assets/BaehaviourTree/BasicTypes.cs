using System.Collections;
using System.Collections.Generic;

namespace BT
{

    public enum NodeStatus
    {
        IDLE = 0,
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public enum NodeType
    {
        UNDEFINED = 0,
        ACTION,
        CONDITION,
        CONTROL,
        DECORATOR,
        SUBTREE
    };

    public enum PortDirection
    {
        INPUT=0,
        OUTPUT,
        INOUT
    };


    public class PortInfo
    {
        private System.Type info_ = null;
        private System.Func<string, object> converter_ = null;
        public PortInfo(PortDirection direction = PortDirection.INOUT)
        {
            direction_ = direction;
            portType_ = null;

        }

        public PortInfo(PortDirection direction, System.Type type_info, System.Func<string,object> conv)
        {
            direction_ = direction;
            portType_ = type_info;
            converter_ = conv;
        }


        private System.Type portType_ = null;
        public System.Type portType {
            get
            {
                return portType_;
            }
        }

        private PortDirection direction_ = PortDirection.INOUT;
        public PortDirection direction
        {
            get
            {
                return direction_;
            }
        }

        private string default_value_ = "";
        public string defaultValue => default_value_;

        public void ParseString(string ss)
        {
            // fixed me
        }

        private string description_ = "";
        public string description
        {
            get => description_;
            set => description_ = value;
        }
    }

    public class PortUtility
    {
        public static KeyValuePair<string, PortInfo> CreatePort<T>(PortDirection direction, string name, string description = "")
        {

            PortInfo port = null;
            if (typeof(T) == typeof(void))
            {
                port = new PortInfo(direction);
            }
            else
            {
                port = new PortInfo(direction, typeof(T), null /*GetAnyFromStringFunctor<T>()*/);
            }

            if (!string.IsNullOrEmpty(description))
            {
                port.description = (description);
            }

            KeyValuePair<string, PortInfo> pair = new KeyValuePair<string, PortInfo>(name, port);
            return pair;
        }

        public static KeyValuePair<string, PortInfo> InputPort<T>(string name, string description = "")
        {
            return CreatePort<T>(PortDirection.INPUT, name, description);

        }
    }

}


