using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{

    /**
     * @brief The ActionNodeBase is the base class to use to create any kind of action.
     * A particular derived class is free to override executeTick() as needed.
     *
     */

    public abstract class ActionNodeBase : LeafNode
    {
        public ActionNodeBase(string name, NodeConfiguration config) : base(name, config)
        {

        }

        public override sealed NodeType GetNodeType()
        {
            return NodeType.ACTION;
        }

    }

    /**
     * @brief The SyncActionNode is an ActionNode that
     * explicitly prevents the status RUNNING and doesn't require
     * an implementation of halt().
     */
    public abstract class SyncActionNode : ActionNodeBase
    {
        public SyncActionNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        public override NodeStatus ExecuteTick()
        {
            var stat = base.ExecuteTick();
            if (stat == NodeStatus.RUNNING)
            {
                throw new LogicError("SyncActionNode MUST never return RUNNING");
            }
            return stat;

        }

        public sealed override void Halt()
        {
            SetStatus(NodeStatus.IDLE);
        }
    }

    /**
     * @brief The SimpleActionNode provides an easy to use SyncActionNode.
     * The user should simply provide a callback with this signature
     *
     *    BT::NodeStatus functionName(TreeNode&)
     *
     * This avoids the hassle of inheriting from a ActionNode.
     *
     * SimpleActionNode is executed synchronously and does not support halting.
     * NodeParameters aren't supported.
     */

    public class SimpleActionNode : SyncActionNode
    {
        protected System.Func<TreeNode, NodeStatus> tickFunctor_;

        public SimpleActionNode(string name, System.Func<TreeNode, NodeStatus> tickFunctor, NodeConfiguration config):base(name, config)
        {
            this.tickFunctor_ = tickFunctor;
        }


        internal sealed override NodeStatus Tick()
        {
            NodeStatus prev_status = this.status;

            if (prev_status == NodeStatus.IDLE)
            {
                SetStatus(NodeStatus.RUNNING);
                prev_status = NodeStatus.RUNNING;
            }

            NodeStatus status = tickFunctor_(this);
            if (status != prev_status)
            {
                SetStatus(status);
            }
            return status;

        }

        /**
         * @brief The AsyncActionNode uses a different thread, where the action will be
         * executed.
         *
         * IMPORTANT: this action is quite hard to implement correctly. Please be sure that you know what you are doing.
         *
         * - In your overriden tick() method, you must check periodically
         *   the result of the method isHaltRequested() and stop your execution accordingly.
         *
         * - in the overriden halt() method, you can do some cleanup, but do not forget to
         *   invoke the base class method AsyncActionNode::halt();
         *
         * - remember, with few exceptions, a halted AsyncAction must return NodeStatus.IDLE.
         *
         * NOTE: when the thread is completed, i.e. the tick() returns its status,
         * a TreeNode::emitStateChanged() will be called.
         */
        public abstract class AsyncActionNode : ActionNodeBase
        {
            public AsyncActionNode(string name, NodeConfiguration config) : base(name, config)
            {

            }

            bool isHaltRequested()
            {
                return true;// halt_requested_.load();
            }

            // This method spawn a new thread. Do NOT remove the "final" keyword.
            public sealed override NodeStatus ExecuteTick()
            {
                //using lock_type = std::unique_lock<std::mutex>;
                //send signal to other thread.
                // The other thread is in charge for changing the status
                if (status == NodeStatus.IDLE)
                {
                    SetStatus(NodeStatus.RUNNING);
                    //halt_requested_ = false;
                    //        thread_handle_ = std::async(std::launch::async, [this]() {

                    //            try
                    //            {
                    //                setStatus(tick());
                    //            }
                    //            catch (std::exception&)
                    //{
                    //                std::cerr << "\nUncaught exception from the method tick(): ["
                    //                          << registrationName() << "/" << name() << "]\n" << std::endl;
                    //                // Set the exception pointer and the status atomically.
                    //                lock_type l(m_);
                    //                exptr_ = std::current_exception();
                    //                setStatus(BT::NodeStatus.IDLE);
                    //            }
                    //            emitStateChanged();
                    //            });
                    //        }

                    //        lock_type l(m_);
                    //        if (exptr_)
                    //        {
                    //            // The official interface of std::exception_ptr does not define any move
                    //            // semantics. Thus, we copy and reset exptr_ manually.
                    //            const auto exptr_copy = exptr_;
                    //            exptr_ = nullptr;
                    //            std::rethrow_exception(exptr_copy);
                    //        }
                    return status;

                }

                return status;

            }

            public override void Halt()
            {
                //halt_requested_.store(true);

                //if (thread_handle_.valid())
                //{
                //    thread_handle_.wait();
                //}
                //thread_handle_ = { };

            }

        }

    }

    /**
     * @brief The ActionNode is the prefered way to implement asynchronous Actions.
     * It is actually easier to use correctly, when compared with AsyncAction
     *
     * It is particularly useful when your code contains a request-reply pattern,
     * i.e. when the actions sends an asychronous request, then checks periodically
     * if the reply has been received and, eventually, analyze the reply to determine
     * if the result is SUCCESS or FAILURE.
     *
     * -) an action that was in IDLE state will call onStart()
     *
     * -) A RUNNING action will call onRunning()
     *
     * -) if halted, method onHalted() is invoked
     */

    public abstract class StatefulActionNode : ActionNodeBase
    {

        public StatefulActionNode(string name, NodeConfiguration config) : base(name, config)
        {

        }

        // do not override this method
        internal override sealed NodeStatus Tick()
        {
            NodeStatus initial_status = this.status;

            if (initial_status == NodeStatus.IDLE)
            {
                NodeStatus new_status = onStart();
                if (new_status == NodeStatus.IDLE)
                {
                    throw new LogicError("StatefulActionNode::onStart() must not return IDLE");
                }
                return new_status;
            }
            //------------------------------------------
            if (initial_status == NodeStatus.RUNNING)
            {
                NodeStatus new_status = onRunning();
                if (new_status == NodeStatus.IDLE)
                {
                    throw new LogicError("StatefulActionNode::onRunning() must not return IDLE");
                }
                return new_status;
            }
            //------------------------------------------
            return initial_status;

        }

        // do not override this method
        public override sealed void Halt()
        {
            if (status == NodeStatus.RUNNING)
            {
                onHalted();
            }
            SetStatus(NodeStatus.IDLE);
        }

        /// method to be called at the beginning.
        /// If it returns RUNNING, this becomes an asychronous node.
        public abstract NodeStatus onStart();

        /// method invoked by a RUNNING action.
        public abstract NodeStatus onRunning();

        /// when the method halt() is called and the action is RUNNING, this method is invoked.
        /// This is a convenient place todo a cleanup, if needed.
        public abstract void onHalted();

    }
}

