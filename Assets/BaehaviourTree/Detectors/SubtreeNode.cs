using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class SubtreeNode : DecoratorNode
    {
        public SubtreeNode(string name) : base(name, new NodeConfiguration())
        {

        }

        internal override NodeStatus Tick()
        {
            throw new System.NotImplementedException();
        }

        public override NodeType GetNodeType()
        {
            return NodeType.SUBTREE;
        }
    }

    public class SubtreePlusNode : DecoratorNode
    {
        public SubtreePlusNode(string name) : base(name, new NodeConfiguration())
        {

        }

        internal override NodeStatus Tick()
        {
            throw new System.NotImplementedException();
        }

        public override NodeType GetNodeType()
        {
            return NodeType.SUBTREE;
        }
    }
}

