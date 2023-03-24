using System.Collections;
using System.Collections.Generic;

namespace BT
{
    /// <summary>
    /// 行为树
    /// </summary>
    public class BehaviourTree
    {
        private List<TreeNode> nodes { get; } = new List<TreeNode>();

        public TreeNode rootNode => nodes.Count == 0 ? null : nodes[0];

        public void TickRoot()
        {
            rootNode.ExecuteTick();   
        }

        public static BehaviourTree CreateTreeFromText(string text)
        {
            return new BehaviourTree();
        }

        public static BehaviourTree createTreeFromFile(string filePath)
        {
            return new BehaviourTree();
        }
    }
}

