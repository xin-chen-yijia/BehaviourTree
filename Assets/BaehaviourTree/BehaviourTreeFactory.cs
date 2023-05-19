using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    using NodeBuilder = System.Func<string, NodeConfiguration, TreeNode>;
    using PortsList = Dictionary<string, PortInfo>;

    public class BehaviourTreeFactory
    {
        private XmlParser parser_ = null;
        HashSet<string> builtin_IDs_ = new HashSet<string>();


        public BehaviourTreeFactory()
        {
            XmlParser parser_ = new XmlParser(this);
            RegisterNodeType<FallbackNode>("Fallback");
            RegisterNodeType<SequenceNode>("Sequence");
            //RegisterNodeType<SequenceStarNode>("SequenceStar");
            RegisterNodeType<ParallelNode>("Parallel");
            //RegisterNodeType<ReactiveSequence>("ReactiveSequence");
            //RegisterNodeType<ReactiveFallback>("ReactiveFallback");
            //RegisterNodeType<IfThenElseNode>("IfThenElse");
            //RegisterNodeType<WhileDoElseNode>("WhileDoElse");

            //RegisterNodeType<InverterNode>("Inverter");
            //RegisterNodeType<RetryNodeTypo>("RetryUntilSuccesful"); //typo but back compatibility
            //RegisterNodeType<RetryNode>("RetryUntilSuccessful"); // correct one
            //RegisterNodeType<KeepRunningUntilFailureNode>("KeepRunningUntilFailure");
            RegisterNodeType<RepeatNode>("Repeat");
            //RegisterNodeType<TimeoutNode<>>("Timeout");
            //RegisterNodeType<DelayNode>("Delay");

            //RegisterNodeType<ForceSuccessNode>("ForceSuccess");
            //RegisterNodeType<ForceFailureNode>("ForceFailure");

            //RegisterNodeType<AlwaysSuccessNode>("AlwaysSuccess");
            //RegisterNodeType<AlwaysFailureNode>("AlwaysFailure");
            //RegisterNodeType<SetBlackboard>("SetBlackboard");

            RegisterNodeType<SubtreeNode>("SubTree");
            RegisterNodeType<SubtreePlusNode>("SubTreePlus");

            //RegisterNodeType<BlackboardPreconditionNode<int>>("BlackboardCheckInt");
            //RegisterNodeType<BlackboardPreconditionNode<double>>("BlackboardCheckDouble");
            //RegisterNodeType < BlackboardPreconditionNode < std::string>> ("BlackboardCheckString");
            //RegisterNodeType<BlackboardPreconditionNode<bool>>("BlackboardCheckBool");

            //RegisterNodeType < SwitchNode < 2 >> ("Switch2");
            //RegisterNodeType < SwitchNode < 3 >> ("Switch3");
            //RegisterNodeType < SwitchNode < 4 >> ("Switch4");
            //RegisterNodeType < SwitchNode < 5 >> ("Switch5");
            //RegisterNodeType < SwitchNode < 6 >> ("Switch6");

#if NCURSES_FOUND
            RegisterNodeType<ManualSelectorNode>("ManualSelector");
#endif
            foreach(var it in builders_)
            {
                builtin_IDs_.Add(it.Key);
            }

        }

        public BehaviourTree CreateTreeFromText(string text, Blackboard blackboard = null)
        {
            blackboard = blackboard ?? new Blackboard(null);

            XmlParser parser = new XmlParser(this);
            parser.LoadFromText(text);
            var tree = parser.InstantiateTree(blackboard);
            //tree.manifests = this->manifests();
            return tree;
        }

        public BehaviourTree CreateTreeFromFile(string file_path, Blackboard blackboard = null)
        {
            blackboard = blackboard ?? new Blackboard(null);

            XmlParser parser = new XmlParser(this);
            parser.LoadFromFile(file_path);
            var tree = parser.InstantiateTree(blackboard);
            //tree.manifests = this->manifests();
            return tree;
        }

        BehaviourTree CreateTree(string tree_name, Blackboard blackboard = null)
        {
            blackboard = blackboard ?? new Blackboard(null);

            var tree = parser_.InstantiateTree(blackboard, tree_name);
            //tree.manifests = this->manifests();
            return tree;
        }

        void RegisterNodeType<T>(string ID) where T : TreeNode
        {
            var t = typeof(T);
            Debug.Assert(typeof(ActionNodeBase).IsAssignableFrom(t) ||
                typeof(ControlNode).IsAssignableFrom(t) ||
                typeof(DecoratorNode).IsAssignableFrom(t) ||
                typeof(ConditionNode).IsAssignableFrom(t),
                          "[registerNode]: accepts only classed derived from either ActionNodeBase, DecoratorNode, ControlNode or ConditionNode");

            Debug.Assert(!t.IsAbstract, "Some methods are pure virtual. " +
                        "Did you override the methods tick() and halt()?");

            //constexpr bool default_constructable = std::is_constructible < T, const std::string&>::value;
            //constexpr bool param_constructable =
            //        std::is_constructible < T, const std::string&, const NodeConfiguration&>::value;
            //constexpr bool has_static_ports_list =
            //        has_static_method_providedPorts < T >::value;

            //static_assert(default_constructable || param_constructable,
            //              "[registerNode]: the registered class must have at least one of these two "
            //              "constructors: "
            //              "  (const std::string&, const NodeConfiguration&) or (const std::string&).\n"
            //              "Check also if the constructor is public!");

            //static_assert(!(param_constructable && !has_static_ports_list),
            //              "[registerNode]: you MUST implement the static method: "
            //              "  PortsList providedPorts();\n");

            //static_assert(!(has_static_ports_list && !param_constructable),
            //              "[registerNode]: since you have a static method providedPorts(), "
            //              "you MUST add a constructor sign signature (const std::string&, const "
            //              "NodeParameters&)\n");

            RegisterBuilder(CreateManifest<T>(ID), CreateBuilder<T>());
        }

        NodeBuilder CreateBuilder<T>()
        {
            return (name, config) =>
            {
                var t = typeof(T);
                return t.Assembly.CreateInstance(t.FullName) as TreeNode;
            };
        }

        TreeNodeManifest CreateManifest<T>(string ID, PortsList portlist = null) where T:TreeNode
        {
            var t = typeof(T);
            var constructor0 = t.GetConstructor(new System.Type[] { });
            var constructor1 = t.GetConstructor(new System.Type[] { typeof(string)});
            var constructor2 = t.GetConstructor(new System.Type[] { typeof(string), typeof(NodeConfiguration) });
            T obj = null;
            if(constructor0 != null)
            {
                obj = constructor0.Invoke(null) as T;
            }
            else if(constructor1 != null)
            {
                obj = constructor1.Invoke(new object[] { "node" }) as T;
            }
            else if(constructor2 != null)
            {
                obj = constructor2.Invoke(new object[] { "node" ,new NodeConfiguration()}) as T;
            }
            else
            {
                throw new BehaviorTreeException($"can't construct {t.FullName}");
            }

            if(portlist == null)
            {
                var getProvidedPorts = t.GetMethod("GetProvidedPorts", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (getProvidedPorts != null)
                {
                    portlist = getProvidedPorts.Invoke(null, new object[] { }) as PortsList;
                }
            }
            var nodeType = obj.GetNodeType();
            return new TreeNodeManifest(){ type = nodeType, registration_ID = ID, ports = portlist };
        }


        Dictionary<string,NodeBuilder> builders_ = new Dictionary<string, NodeBuilder>();
        public Dictionary<string, NodeBuilder> builders => builders_;

        Dictionary<string, TreeNodeManifest> manifests_ = new Dictionary<string, TreeNodeManifest>();
        public Dictionary<string, TreeNodeManifest> manifests => manifests_;

        void RegisterBuilder(TreeNodeManifest manifest, NodeBuilder builder)
        {
            if (builders_.ContainsKey(manifest.registration_ID))
            {
                throw new BehaviorTreeException($"ID [{manifest.registration_ID }] already registered");
            }

            builders_.Add(manifest.registration_ID, builder);
            manifests_.Add(manifest.registration_ID, manifest);
        }
        bool UnregisterBuilder(string ID)
        {
            if(builtin_IDs_.Contains(ID) )
            {
                throw new LogicError($"You can not remove the builtin registration ID [{ ID }]");
            }

            if (!builders_.ContainsKey(ID))
            {
                return false;
            }

            builders_.Remove(ID);
            manifests_.Remove(ID);
            return true;
        }

        public TreeNode InstantiateTreeNode( string name, string ID, NodeConfiguration config)
        {
            if (!builders_.ContainsKey(ID))
            {
                Debug.LogError(ID + " not included in this list:");
                foreach(var builder_it in builders_)
                {
                    Debug.LogError(builder_it.Key);
                }
                throw new RuntimeError($"BehaviorTreeFactory: ID [{ID}] not registered");
            }

            TreeNode node = builders_[ID](name, config);
            node.SetRegistrationID(ID);
            return node;
        }

    }
}

