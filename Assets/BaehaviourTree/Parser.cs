using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /**
     * @brief The BehaviorTreeParser is a class used to read the model
     * of a BehaviorTree from file or text and instantiate the
     * corresponding tree using the BehaviorTreeFactory.
     */
    public abstract class Parser
    {
        public abstract void LoadFromFile(string filename, bool add_includes = true);

        public abstract void LoadFromText(string xml_text, bool add_includes = true);

        public abstract List<string> RegisteredBehaviorTrees();

        public abstract BehaviourTree InstantiateTree(Blackboard root_blackboard, string tree_name);
    };

}
