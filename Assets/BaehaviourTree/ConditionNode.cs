using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public abstract class ConditionNode : LeafNode
    {
        public ConditionNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        public sealed override void Halt()
        {
            SetStatus(NodeStatus.IDLE);
        }

        public sealed override NodeType GetNodeType()
        {
            return NodeType.CONDITION;
        }

    }

    /**
     * @brief The SimpleConditionNode provides an easy to use ConditionNode.
     * The user should simply provide a callback with this signature
     *
     *    BT::NodeStatus functionName(void)
     *
     * This avoids the hassle of inheriting from a ActionNode.
     *
     * SimpleConditionNode does not support halting, NodeParameters, nor Blackboards.
     */

    public class SimpleConditionNode : ConditionNode
    {
        protected System.Func<TreeNode, NodeStatus> tickFunctor_;

        public SimpleConditionNode(string name, System.Func<TreeNode, NodeStatus> tickFunctor, NodeConfiguration config) : base(name, config)
        {
            this.tickFunctor_ = tickFunctor;
        }

        internal override NodeStatus Tick()
        {
            return tickFunctor_(this);
        }
    }
}

