using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace BT
{
    using NodeBuilder = System.Func<string, NodeConfiguration, TreeNode>;
    using PortsList = Dictionary<string, PortInfo>;

    public class XmlParser : Parser
    {
        private List<XDocument> opened_documents_ = new List<XDocument>();

        private BehaviourTreeFactory factory_ = null;
        public XmlParser(BehaviourTreeFactory factory)
        {
            factory_ = factory;
        }

        public override void LoadFromFile(string filename, bool add_includes = true)
        {
            XDocument doc = XDocument.Load(filename);
            LoadDocImpl(doc, add_includes);

            opened_documents_.Add(doc);
        }

        public override void LoadFromText(string xml_text, bool add_includes = true)
        {
            XDocument doc = XDocument.Parse(xml_text);
            LoadDocImpl(doc, add_includes);

            opened_documents_.Add(doc);
        }

        private int suffix_count = 0;   // 计数
        private Dictionary<string, XElement> tree_roots_ = new Dictionary<string, XElement>();

        private void LoadDocImpl(XDocument doc, bool add_includes)
        {
            //    foreach (var el in doc.DescendantNodes().OfType<XElement>()
            //.Select(x => x))
            //    {
            //        Debug.Log(el.Attribute("path"));
            //    }

            // handle include elements
            IEnumerable<XElement> address =
                from el in doc.Root.Elements("include")
                select el;

            foreach (var v in address)
            {
                string path = v.Attribute("path").Value;
                Debug.Log(path);
                if (!IsAbsolute(path))
                {
                    path = path;
                }

                // include new doc
                XDocument newDoc = XDocument.Load(path);
                doc.Add(newDoc);

                LoadDocImpl(newDoc, add_includes);
            }

            // handle behavior trees
            IEnumerable<XElement> treeNodes =
                from el in doc.Root.Elements("BehaviorTree")
                select el;

            foreach (var node in treeNodes)
            {
                var attr = node.Attribute("ID");
                string tree_name = attr != null ? attr.Value : "BehaviorTree_" + (suffix_count);
                suffix_count++;
                tree_roots_.Add(tree_name, node);
            }


        }

        private bool IsAbsolute(string path)
        {
            return true;
        }


        public override List<string> RegisteredBehaviorTrees()
        {

            return new List<string>();
        }

        public override BehaviourTree InstantiateTree(Blackboard root_blackboard, string main_tree_to_execute="")
        {
            BehaviourTree output_tree = new BehaviourTree();
            string main_tree_ID = main_tree_to_execute;

            // use the main_tree_to_execute argument if it was provided by the user
            // or the one in the FIRST document opened
            if (string.IsNullOrEmpty(main_tree_ID))
            {
                XElement first_xml_root = opened_documents_.First().Root;

                var main_tree_attribute = first_xml_root.Attribute("main_tree_to_execute");
                if (main_tree_attribute != null)
                {
                    main_tree_ID = main_tree_attribute.Value;
                }
                else if (tree_roots_.Count == 1)
                {
                    // special case: there is only one registered BT.
                    main_tree_ID = tree_roots_.First().Key;
                }
                else
                {
                    throw new RuntimeError("[main_tree_to_execute] was not specified correctly");
                }
            }

            //--------------------------------------
            if (root_blackboard == null)
            {
                throw new RuntimeError("XMLParser::instantiateTree needs a non-empty root_blackboard");
            }
            // first blackboard
            output_tree.blackboard_stack.Add(root_blackboard);

            RecursivelyCreateTree(main_tree_ID,
                                      output_tree,
                                      root_blackboard,
                                      null);
            output_tree.Initialize();
            return output_tree;

        }

        void RecursivelyCreateTree(string tree_ID, BehaviourTree output_tree, Blackboard blackboard, TreeNode root_parent)
        {
            System.Action<TreeNode, XElement> recursiveStep = null;
            recursiveStep = (TreeNode parent, XElement element)=>
            {
                // create the node
                TreeNode node = CreateNodeFromXML(element, blackboard, parent);
                output_tree.nodes.Add(node);

                if(node.GetNodeType() == NodeType.SUBTREE )
                {
                    if(node.GetType() == typeof(SubtreeNode))
                    {
                        bool is_isolated = true;

                        foreach(var attr in element.Attributes())
                        {
                            if(attr.Name.ToString() == "__shared_blackboard"  &&
                                (attr.Value == "true" || attr.Value == "1"))
                            {
                                is_isolated = false;
                            }
                        }

                        if (!is_isolated)
                        {
                            RecursivelyCreateTree(node.name, output_tree, blackboard, node);
                        }
                        else
                        {
                            // Creating an isolated
                            var new_bb = new Blackboard(blackboard);

                            foreach(var attr in element.Attributes())
                            {
                                if (attr.Name.ToString() == "ID")
                                {
                                    continue;
                                }
                                new_bb.AddSubtreeRemapping(attr.Name.ToString(), attr.Value);
                            }
                            output_tree.blackboard_stack.Add(new_bb);
                            RecursivelyCreateTree(node.name, output_tree, new_bb, node);
                        }
                    }
                    else if (node.GetType() == typeof(SubtreePlusNode))
                    {
                        var new_bb = new Blackboard(blackboard);
                        output_tree.blackboard_stack.Add(new_bb);

                        HashSet<string> mapped_keys = new HashSet<string>();

                        bool do_autoremap = false;

                        foreach(var attr in element.Attributes())
                        {
                            string attr_name = attr.Name.ToString();
                            string attr_value = attr.Value;

                            if (attr_name == "ID")
                            {
                                continue;
                            }
                            if (attr_name == "__autoremap")
                            {
                                do_autoremap = ConvertBoolFromString(attr_value);
                                continue;
                            }

                            if (TreeNode.IsBlackboardPointer(attr_value))
                            {
                                // do remapping
                                string port_name = "";// TreeNode::stripBlackboardPointer(attr_value);
                                new_bb.AddSubtreeRemapping(attr_name, port_name);
                                mapped_keys.Add(attr_name);
                            }
                            else
                            {
                                // constant string: just set that constant value into the BB
                                //new_bb.Set(attr_name, new Blackboard.Entry(attr_value));
                                PortInfo port = new PortInfo();
                                port.ParseString(attr_value);
                                new_bb.Set(attr_name, new Blackboard.Entry(port));
                                mapped_keys.Add(attr_name);
                            }
                        }

                        if (do_autoremap)
                        {
                            List<string> remapped_ports = new List<string>();
                            var new_root_element = tree_roots_[node.name].FirstNode;//->FirstChildElement();

                            //getPortsRecursively(new_root_element, remapped_ports);
                            foreach(var port in remapped_ports)
                            {
                                if (!mapped_keys.Contains(port))
                                {
                                    new_bb.AddSubtreeRemapping(port, port);
                                }
                            }
                        }

                        RecursivelyCreateTree(node.name, output_tree, new_bb, node);
                    }
                }
                else
                {
                    foreach(var child_element in element.Elements())
                    {
                        recursiveStep(node, child_element);
                    }
                }
            };

            if (!tree_roots_.ContainsKey(tree_ID))
            {
                throw new RuntimeError(("Can't find a tree with name: ") + tree_ID);
            }

            var root_element = tree_roots_[tree_ID].FirstNode;

            // start recursion
            recursiveStep(root_parent, (XElement)root_element);
        }

        TreeNode CreateNodeFromXML(XElement element, Blackboard blackboard, TreeNode node_parent)
        {
            string element_name = element.Name.ToString();
            string ID;
            string instance_name;

            // Actions and Decorators have their own ID
            if (element_name == "Action" || element_name == "Decorator" ||
                element_name == "Condition" || element_name == "Control")
            {
                ID = element.Attribute("ID").Value;
            }
            else
            {
                ID = element_name;
            }

            var attr_alias = element.Attribute("name");
            if (attr_alias != null)
            {
                instance_name = attr_alias.Value;
            }
            else
            {
                instance_name = ID;
            }

            Dictionary<string,string> port_remap = new Dictionary<string, string>();

            if (element_name == "SubTree" ||
                element_name == "SubTreePlus")
            {
                instance_name = element.Attribute("ID").Value;
            }
            else
            {
                // do this only if it NOT a Subtree
                foreach(var att in element.Attributes())
                {
                    string attribute_name = att.Name.ToString();
                    if (attribute_name != "ID" && attribute_name != "name")
                    {
                        port_remap[attribute_name] = att.Value;
                    }
                }
            }

            NodeConfiguration config = new NodeConfiguration();
            config.blackboard = blackboard;

            //---------------------------------------------
            TreeNode child_node = null;

            // fixed me:
            if (factory_.builders.ContainsKey(ID))
            {
                var manifest = factory_.manifests[ID];

                //Check that name in remapping can be found in the manifest
                foreach(var remap_it in port_remap)
                {
                    if (!manifest.ports.ContainsKey(remap_it.Key))
                    {
                        throw new RuntimeError($"Possible typo? In the XML, you tried to remap port \"" +
                                           $"{ remap_it.Key } \" in node [ {ID} /  {instance_name}" +
                                           "], but the manifest of this node does not contain a port with this name.");
                    }
                }

                // Initialize the ports in the BB to set the type
                foreach (var port_it in manifest.ports)
                {
                    string port_name = port_it.Key;
                    var port_info = port_it.Value;

                    if (!port_remap.ContainsKey(port_name))
                    {
                        continue;
                    }
                    string param_value = port_remap[port_name];
                    var param_res = TreeNode.GetRemappedKey(port_name, param_value);
                    if (!string.IsNullOrEmpty(param_res))
                    {
                        var port_key = param_res;

                        var prev_info = blackboard.GetPortInfo(port_key);
                        if (prev_info == null)
                        {
                            // not found, insert for the first time.
                            blackboard.SetPortInfo(port_key, port_info);
                        }
                        else
                        {
                            // found. check consistency
                            if (prev_info.portType != null && port_info.portType != null && // null type means that everything is valid
                                prev_info.portType != port_info.portType)
                            {
                                //blackboard->debugMessage();

                                throw new RuntimeError($"The creation of the tree failed because the port [{port_key}" + 
                                                   $"] was initially created with type [{prev_info.portType.Name})," + 
                                                   $"] and, later type [{port_info.portType.Name})," +
                                                   $"] was used somewhere else.");
                            }
                        }
                    }
                }

                // use manifest to initialize NodeConfiguration
                foreach (var remap_it in port_remap)
                {
                    var port_name = remap_it.Key;
                    if (manifest.ports.ContainsKey(port_name))
                    {
                        var direction = manifest.ports[port_name].direction;
                        if (direction != PortDirection.OUTPUT)
                        {
                            Debug.Log(config + ":" + config.input_ports);
                            config.input_ports.Add(remap_it.Key,remap_it.Value);
                        }
                        if (direction != PortDirection.INPUT)
                        {
                            config.output_ports.Add(remap_it.Key, remap_it.Value);
                        }
                    }
                }

                // use default value if available for empty ports. Only inputs
                foreach(var port_it in manifest.ports)
                {
                    string port_name = port_it.Key;
                    PortInfo port_info = port_it.Value;

                    var direction = port_info.direction;
                    if (direction != PortDirection.OUTPUT &&
                        !config.input_ports.ContainsKey(port_name) &&
                        !string.IsNullOrEmpty(port_info.defaultValue))
                    {
                        config.input_ports.Add(port_name, port_info.defaultValue);
                    }
                }

                child_node = factory_.InstantiateTreeNode(instance_name, ID, config);
            }
            else if (tree_roots_.ContainsKey(ID))
            {
                child_node = new SubtreeNode(instance_name);
            }
            else
            {
                throw new RuntimeError(ID + " is not a registered node, nor a Subtree");
            }

            if (node_parent != null)
            {
                if (node_parent.GetType() == typeof(ControlNode))
                {
                    ((ControlNode)node_parent).AddChild(child_node);
                }
                if (node_parent.GetType() == typeof(DecoratorNode))
                {
                    ((DecoratorNode)node_parent).SetChild(child_node);
                }
            }
            return child_node;
        }

        private bool ConvertBoolFromString(string s)
        {
            return s.ToLower() == "true" || s == "1";
        }
    }
}

