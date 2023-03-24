using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BT
{
    public abstract class ControlNode : TreeNode
    {
        protected List<TreeNode> childrenNodes_;

        public void AddChild(TreeNode node)
        {
            childrenNodes_.Add(node);
        }

        public int childrenCount => childrenNodes_.Count;

        public TreeNode GetChild(int index)
        {
            Debug.Assert(index < 0 || index > childrenNodes_.Count) ;
            return childrenNodes_[index];
        }

        public void HaltChild(int index)
        {
            var child = childrenNodes_[index];
            if (child.status == NodeStatus.RUNNING)
            {
                child.Halt();
            }
            child.SetStatus(NodeStatus.IDLE);
        }

        public void HaltChildren()
        {
            for (int i = 0; i < childrenCount; i++)
            {
                HaltChild(i);
            }
        }


        public override NodeType type()
        {
            return NodeType.CONTROL;
        }

    }
}
