using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public abstract class LeafNode : TreeNode
    {
        public LeafNode(string name, NodeConfiguration config) : base(name, config)
        {

        }
    }
}

