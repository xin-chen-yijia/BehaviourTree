using System.Collections;
using System.Collections.Generic;

namespace BT
{
    /// <summary>
    /// 行为树节点，其他类似selector节点从这派生
    /// </summary>
    public abstract class TreeNode
    {
        private NodeStatus status_ = NodeStatus.IDLE;

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

        public abstract NodeType type();

        public abstract void Halt();

        public bool isHalt => status_ == NodeStatus.IDLE;

        public NodeStatus status => status_;

        private string name_;
        /// <summary>
        /// Name of the instance, not the type
        /// </summary>
        public string name => name_;
    }

}
