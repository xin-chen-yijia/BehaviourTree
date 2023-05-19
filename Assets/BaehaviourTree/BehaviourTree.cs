using System.Collections;
using System.Collections.Generic;

namespace BT
{
    /// <summary>
    /// 行为树
    /// </summary>
    public class BehaviourTree
    {
        public List<TreeNode> nodes { get; } = new List<TreeNode>();
        public List<Blackboard> blackboard_stack = new List<Blackboard>();

        public TreeNode rootNode => nodes.Count == 0 ? null : nodes[0];

        public void TickRoot()
        {
            rootNode.ExecuteTick();   
        }

        public void Initialize()
        {
            //wake_up_ = std::make_shared<WakeUpSignal>();
            //for (auto & node: nodes)
            //{
            //    node->setWakeUpInstance(wake_up_);
            //}
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

