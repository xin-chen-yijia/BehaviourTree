using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BT
{
    public class ParallelNode : ControlNode
    {
        public uint successThreshold { get; set; } = 0;
        public uint failureThreshold { get; set; } = 0;

        private HashSet<int> skipList;

        public ParallelNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        internal override NodeStatus Tick()
        {
            //if (read_parameter_from_ports_)
            //{
            //    if (!getInput(THRESHOLD_SUCCESS, success_threshold_))
            //    {
            //        throw RuntimeError("Missing parameter [", THRESHOLD_SUCCESS, "] in ParallelNode");
            //    }

            //    if (!getInput(THRESHOLD_FAILURE, failure_threshold_))
            //    {
            //        throw RuntimeError("Missing parameter [", THRESHOLD_FAILURE, "] in ParallelNode");
            //    }
            //}

            int success_childred_num = 0;
            int failure_childred_num = 0;

            int children_count = childrenCount;

            if (children_count < successThreshold)
            {
                throw new LogicError("Number of children is less than threshold. Can never succeed.");
            }

            if (children_count < failureThreshold)
            {
                throw new LogicError("Number of children is less than threshold. Can never fail.");
            }

            // Routing the tree according to the sequence node's logic:
            for (int i = 0; i < children_count; i++)
            {
                TreeNode child_node = childrenNodes_[i];

                bool in_skip_list = (skipList.Contains(i));

                NodeStatus child_status;
                if (in_skip_list)
                {
                    child_status = child_node.status;
                }
                else
                {
                    child_status = child_node.ExecuteTick();
                }

                switch (child_status)
                {
                    case NodeStatus.SUCCESS:
                        {
                            if (!in_skip_list)
                            {
                                skipList.Add(i);
                            }
                            success_childred_num++;

                            if (success_childred_num == successThreshold)
                            {
                                skipList.Clear();
                                HaltChildren();
                                return NodeStatus.SUCCESS;
                            }
                        }
                        break;

                    case NodeStatus.FAILURE:
                        {
                            if (!in_skip_list)
                            {
                                skipList.Add(i);
                            }
                            failure_childred_num++;

                            // It fails if it is not possible to succeed anymore or if 
                            // number of failures are equal to failure_threshold_
                            if ((failure_childred_num > children_count - successThreshold)
                                || (failure_childred_num == failureThreshold))
                            {
                                skipList.Clear();
                                HaltChildren();
                                return NodeStatus.FAILURE;
                            }
                        }
                        break;

                    case NodeStatus.RUNNING:
                        {
                            // do nothing
                        }
                        break;

                    default:
                        {
                            throw new LogicError("A child node must never return IDLE");
                        }
                }
            }

            return NodeStatus.RUNNING;

        }

        public sealed override void Halt()
        {
            skipList.Clear();
        }
    }
}

