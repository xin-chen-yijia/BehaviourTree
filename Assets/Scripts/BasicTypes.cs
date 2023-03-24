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

}


