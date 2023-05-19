using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /**
     * @brief The RepeatNode is used to execute a child several times, as long
     * as it succeed.
     *
     * To succeed, the child must return SUCCESS N times (port "num_cycles").
     *
     * If the child returns FAILURE, the loop is stopped and this node
     * returns FAILURE.
     *
     * Example:
     *
     * <Repeat num_cycles="3">
     *   <ClapYourHandsOnce/>
     * </Repeat>
     */
    public class RepeatNode : DecoratorNode
    {
        public RepeatNode(string name, int NTries):base(name,new NodeConfiguration())
        {
            this.num_cycles_ = NTries;
        }

        public RepeatNode(string name, NodeConfiguration config) : base(name, config)
        {

        }


        public static Dictionary<string, PortInfo> GetProvidedPorts()
        {
            var d = PortUtility.InputPort<int>(NUM_CYCLES, "Repeat a succesful child up to N times. Use -1 to create an infinite loop.");
            Dictionary<string, PortInfo> res = new Dictionary<string, PortInfo>();
            res.Add(d.Key,d.Value);

            return res;
        }

        private int num_cycles_;
        private int try_index_;

        private bool read_parameter_from_ports_;
        private const string NUM_CYCLES = "num_cycles";

        internal override NodeStatus Tick()
        {
            //if (read_parameter_from_ports_)
            //{
            //    if (!getInput(NUM_CYCLES, num_cycles_))
            //    {
            //        throw new RuntimeError($"Missing parameter [{NUM_CYCLES}] in RepeatNode");
            //    }
            //}

            SetStatus(NodeStatus.RUNNING);

            while (try_index_ < num_cycles_ || num_cycles_ == -1)
            {
                NodeStatus child_state = childNode_.ExecuteTick();

                switch (child_state)
                {
                    case NodeStatus.SUCCESS:
                        {
                            try_index_++;
                            HaltChild();
                        }
                        break;

                    case NodeStatus.FAILURE:
                        {
                            try_index_ = 0;
                            HaltChild();
                            return (NodeStatus.FAILURE);
                        }

                    case NodeStatus.RUNNING:
                        {
                            return NodeStatus.RUNNING;
                        }

                    default:
                        {
                            throw new LogicError("A child node must never return IDLE");
                        }
                }
            }

            try_index_ = 0;
            return NodeStatus.SUCCESS;

        }

        public override void Halt()
        {
            try_index_ = 0;
        }

    };

}


