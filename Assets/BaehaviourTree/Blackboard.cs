using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class Blackboard
    {
        public class Entry
        {

            public object value;
            public PortInfo port_info;

            public Entry(PortInfo info)
            {
                value = null;
                port_info = (info);
            }

            public Entry(object other_any, PortInfo info)
            {
                value = other_any;
                port_info = info;
            }
        };

        private Dictionary<string, Entry> storage_;

        private Blackboard parent_ = null;

        public Blackboard(Blackboard parent = null)
        {
            parent_ = parent;
        }

        public T Get<T>(string key) where T : Entry
        {
            if(storage_.ContainsKey(key))
            {
                return (T)storage_[key];
            }

            throw new System.Exception($"Blackboard::get() error. Missing key [${key}]");
        }  
        
        public void Set<T>(string key,T value) where T : Entry
        {
            // fixed me
            storage_[key] = (Entry)value;
        }

        private Dictionary<string, string> internal_to_external_ = new Dictionary<string, string>();
        public void AddSubtreeRemapping(string internal_tree, string external_tree)
        {
            internal_to_external_.Add(internal_tree, external_tree);
        }

        public void SetPortInfo(string key, PortInfo info)
        {
            //std::unique_lock<std::mutex> lock(mutex_);

            //if(auto parent = parent_bb_.lock())
            {
                if(internal_to_external_.ContainsKey(key))
                {
                    parent_.SetPortInfo(internal_to_external_[key], info);
                }
            }

            if (!storage_.ContainsKey(key))
            {
                storage_.Add(key, new Entry(info));
            }
            else
            {
                var old_type = storage_[key].port_info.portType;
                if (old_type != null && old_type != info.portType)
                {
                    throw new LogicError("Blackboard::set() failed: once declared, the type of a port shall not change. " +

                                     $"Declared type [{old_type.Name}," +
                                     $"] != current type [{info.portType.Name}]");
                }
            }
        }


        public PortInfo GetPortInfo(string key)
        {
            //std::unique_lock<std::mutex> lock(mutex_);
            if(storage_.ContainsKey(key))
            {
                return storage_[key].port_info;
            }

            return null;
        }

    }
}

