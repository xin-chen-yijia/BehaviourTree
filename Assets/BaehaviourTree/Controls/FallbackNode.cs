using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class FallbackNode : ControlNode
    {
        private int currentChildIdx_;

        public FallbackNode(string name) : base(name, new NodeConfiguration())
        {

        }
        public override void Halt()
        {
            currentChildIdx_ = 0;
        }

        internal override NodeStatus Tick()
        {
            int children_count = childrenCount;

            SetStatus(NodeStatus.RUNNING);

            while (currentChildIdx_ < children_count)
            {
                TreeNode current_child_node = childrenNodes_[currentChildIdx_];
                NodeStatus child_status = current_child_node.ExecuteTick();

                switch (child_status)
                {
                    case NodeStatus.RUNNING:
                        {
                            return child_status;
                        }
                    case NodeStatus.SUCCESS:
                        {
                            HaltChildren();
                            currentChildIdx_ = 0;
                            return child_status;
                        }
                    case NodeStatus.FAILURE:
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

            // The entire while loop completed. This means that all the children returned FAILURE.
            if (currentChildIdx_ == children_count)
            {
                HaltChildren();
                currentChildIdx_ = 0;
            }

            return NodeStatus.FAILURE;

        }
    }
}

