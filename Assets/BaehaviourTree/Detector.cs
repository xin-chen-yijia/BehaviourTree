using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public abstract class DecoratorNode : ControlNode
    {

        public DecoratorNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        protected TreeNode childNode_;
        public void SetChild(TreeNode child)
        {
            if (childNode_ == null)
            {
                throw new BehaviorTreeException($"Decorator [${ name }] has already a child assigned");
            }

            childNode_ = child;

        }

        public TreeNode child => childNode_;

        /// The method used to interrupt the execution of this node
        public override void Halt()
        {
            HaltChild();
            SetStatus(NodeStatus.IDLE);

        }

        /// Halt() the child node
        public void HaltChild()
        {
            if (childNode_ == null)
            {
                return;
            }
            if (childNode_.status == NodeStatus.RUNNING)
            {
                childNode_.Halt();
            }
            childNode_.SetStatus(NodeStatus.IDLE);

        }

        public override NodeType GetNodeType() => NodeType.DECORATOR;

        public override NodeStatus ExecuteTick()
        {
            NodeStatus status = base.ExecuteTick();
            NodeStatus child_status = child.status;
            if (child_status == NodeStatus.SUCCESS || child_status == NodeStatus.FAILURE)
            {
                child.SetStatus(NodeStatus.IDLE);
            }
            return status;

        }

    }

    /**
     * @brief The SimpleDecoratorNode provides an easy to use DecoratorNode.
     * The user should simply provide a callback with this signature
     *
     *    BT::NodeStatus functionName(BT::NodeStatus child_status)
     *
     * This avoids the hassle of inheriting from a DecoratorNode.
     *
     * SimpleDecoratorNode does not support halting, NodeParameters, nor Blackboards.
     */
    public class SimpleDecoratorNode : DecoratorNode
    {
        protected System.Func<NodeStatus, TreeNode, NodeStatus> tickFunctor_;

        public  SimpleDecoratorNode(string name, System.Func<NodeStatus,TreeNode,NodeStatus> tickFunctor, NodeConfiguration config) : base(name, config)
        {
            this.tickFunctor_ = tickFunctor;
        }

        internal override NodeStatus Tick()
        {
            return tickFunctor_(child.ExecuteTick(), this);
        }
    }
}
