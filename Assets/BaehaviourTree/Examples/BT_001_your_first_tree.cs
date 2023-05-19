using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BT;

public class BT_001_your_first_tree : MonoBehaviour
{
    string xml_text = "<root main_tree_to_execute = 'MainTree' > "
         + "< BehaviorTree ID='MainTree'>"
          + " <Sequence name = 'root_sequence' > "
          +    " < CheckBattery   name='battery_ok'/> "
          +    " <OpenGripper name = 'open_gripper' /> "
          +    " < ApproachObject name='approach_object'/> "
          +    " <CloseGripper name = 'close_gripper' /> "
          +" </ Sequence > "
       + " </ BehaviorTree > "
    +"</ root > " ;

    // Start is called before the first frame update
    void Start()
    {
        //var tree = BehaviourTree.CreateTreeFromText(xml_text);

        //// To "execute" a Tree you need to "tick" it.
        //// The tick is propagated to the children based on the logic of the tree.
        //// In this case, the entire sequence is executed, because all the children
        //// of the Sequence return SUCCESS.
        //tree.TickRoot();

        BehaviourTreeFactory factory = new BehaviourTreeFactory();
        var tree = factory.CreateTreeFromFile(Application.streamingAssetsPath + "/say.xml");
        tree.TickRoot();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
