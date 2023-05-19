using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class SequenceNode : ControlNode
    {
        private int currentChildIdx_ = 0;

        public  SequenceNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        internal override NodeStatus Tick()
        {
            SetStatus(NodeStatus.RUNNING);

            while (currentChildIdx_ < childrenCount)
            {
                TreeNode currentNode = GetChild(currentChildIdx_);
                NodeStatus child_status = currentNode.ExecuteTick();

                switch (child_status)
                {
                    case NodeStatus.RUNNING:
                        {
                            return child_status;
                        }
                    case NodeStatus.FAILURE:
                        {
                            // Reset on failure
                            HaltChildren();
                            currentChildIdx_ = 0;
                            return child_status;
                        }
                    case NodeStatus.SUCCESS:
                        {
                            currentChildIdx_++;
                        }
                        break;

                    case NodeStatus.IDLE:
                        {
                            throw new LogicError("A child node must never return IDLE");
                        }
                }   // end switch
            }       // end while loop

            // The entire while loop completed. This means that all the children returned SUCCESS.
            if (currentChildIdx_ == childrenCount)
            {
                HaltChildren();
                currentChildIdx_ = 0;
            }
            return NodeStatus.SUCCESS;

        }

        public override void Halt()
        {
            currentChildIdx_ = 0;
        }
    }
}
