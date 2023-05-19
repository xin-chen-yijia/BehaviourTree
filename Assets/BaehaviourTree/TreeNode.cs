using System.Collections;
using System.Collections.Generic;

namespace BT
{
    using PortsList = Dictionary<string, PortInfo>;
    using PortsRemapping = Dictionary<string, string>;


    public struct NodeConfiguration
    {
        public Blackboard blackboard;
        public PortsRemapping input_ports;
        public PortsRemapping output_ports;
    };

    /// This information is used mostly by the XMLParser.
    public struct TreeNodeManifest
    {
        public NodeType type;
        public string registration_ID;
        public PortsList ports;
    };



    /// <summary>
    /// 行为树节点，其他类似selector节点从这派生
    /// </summary>
    public abstract class TreeNode
    {
        private NodeStatus status_ = NodeStatus.IDLE;

        public TreeNode(string name, NodeConfiguration config)
        {
            this.name_ = name;
            this.config_ = config;
        }

        /// <summary>
        /// The method that should be used to invoke tick() and setStatus();
        /// </summary>
        /// <returns></returns>
        public virtual NodeStatus ExecuteTick()
        {
            NodeStatus status = Tick();
            SetStatus(status);
            return status;
        }

        public void SetStatus(NodeStatus status)
        {
            status_ = status;
        }

        /// <summary>
        /// Method to be implemented by the user
        /// </summary>
        /// <returns></returns>
        internal abstract NodeStatus Tick();

        public abstract NodeType GetNodeType();

        public abstract void Halt();

        public bool isHalt => status_ == NodeStatus.IDLE;

        public NodeStatus status => status_;

        private string name_;
        /// <summary>
        /// Name of the instance, not the type
        /// </summary>
        public string name => name_;

        NodeConfiguration config_;

        public static bool IsBlackboardPointer(string str)
        {
            int size = str.Length;
            if (size >= 3 && str[str.Length-1] == '}')
            {
                if (str[0] == '{')
                {
                    return true;
                }
                if (size >= 4 && str[0] == '$' && str[1] == '{')
                {
                    return true;
                }
            }
            return false;
        }

        public static string StripBlackboardPointer(string str)
        {
            var size = str.Length;
            if (size >= 3 && str[size-1] == '}')
            {
                if (str[0] == '{')
                {
                    return str.Substring(1, size - 2);
                }
                if (str[0] == '$' && str[1] == '{')
                {
                    return str.Substring(2, size - 3);
                }
            }
            return "";
        }


        public static string GetRemappedKey(string port_name, string remapping_value)
        {
            if (remapping_value == "=")
            {
                return port_name;
            }
            if (IsBlackboardPointer(remapping_value))
            {
                return StripBlackboardPointer(remapping_value);
            }
            return "";
        }

        private string registration_ID_ = "";
        public void SetRegistrationID(string ID)
        {
            registration_ID_ = ID;
        }

    }
}
