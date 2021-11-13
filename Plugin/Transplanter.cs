using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using ThunderRoad.AI;
using UnityEngine;

namespace BrainTransplant
{

    public class SubtreeMapBuilder : BehaviorTreeVisitor
    {
        public struct NodeDescriptor
        {
            public Node parentNode;
            public int index;
        }

        public Dictionary<string, List<NodeDescriptor>> descriptors = new Dictionary<string, List<NodeDescriptor>>(); 

        public override void visit(Node node)
        {
            if (BehaviorTreeTraverser.is_decorator(node))
            {
                DecoratorNode dn = node as DecoratorNode;
                if (dn.child != null)
                {
                    if (BehaviorTreeTraverser.is_childtree(dn.child))
                    {
                        addToDescriptors(node, 0, dn.child as ChildTreeNode);
                    }
                }
            }
            else if (BehaviorTreeTraverser.is_control(node))
            {
                ControlNode cn = node as ControlNode;
                for (int index = 0; index < cn.childs.Count; index++)
                {
                    if (BehaviorTreeTraverser.is_childtree(cn.childs[index]))
                    {
                        addToDescriptors(node, index, cn.childs[index] as ChildTreeNode);
                    }
                }
            }
        }

        private void addToDescriptors(Node parent, int index, ChildTreeNode childTreeNode)
        {
            string treeid = childTreeNode.childTreeID;
            string treename = childTreeNode.childTreeName;
            string id = treeid;
            if (treeid == null)
            {
                if (treename == null) return;
                id = treename;
            }
            NodeDescriptor descriptor = new NodeDescriptor();
            descriptor.parentNode = parent;
            descriptor.index = index;
            if (descriptors.ContainsKey(id))
            {
                descriptors[id].Add(descriptor);
            }
            else
            {
                List<NodeDescriptor> desclist = new List<NodeDescriptor>();
                desclist.Add(descriptor);
                descriptors.Add(id, desclist);
            }
        }
    }

    class Transplanter
    {

        public static Blackboard get_blackboard(ChildTreeNode node)
        {
            return node.GetType().GetField("blackboard", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(node) as Blackboard;
        }

        public static void set_childtree(BehaviorTreeData tree, ChildTreeNode node)
        {
            node.GetType().GetField("childTree", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(node, tree);
        }
        public static BehaviorTreeData get_childtree(ChildTreeNode node)
        {
            return node.GetType().GetField("childTree", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(node) as BehaviorTreeData;
        }

        public static void transplant(Creature creature, List<BrainTransplantEntry> entries)
        {
            if (!creature.brain || creature.brain.instance == null) return;

            creature.brain.instance.Stop();

            BehaviorTreeData creature_btree = creature.brain.instance.tree;
            Blackboard blackboard = creature_btree.blackboard;
            BehaviorTreeTraverserDepth traverser = new BehaviorTreeTraverserDepth(creature_btree);

            Logger.Detailed("Building brain tree for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID());
            SubtreeMapBuilder mapbuilder = new SubtreeMapBuilder();
            traverser.traverse(mapbuilder);
            Logger.Detailed("Finished building brain tree for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID());

            Dictionary<string, List<SubtreeMapBuilder.NodeDescriptor>> child_tree_parent_map = mapbuilder.descriptors;

            foreach (BrainTransplantEntry entry in entries)
            {
                Logger.Detailed("Implanting {0} to {1}", entry.id, entry.bt_startid);
                if (child_tree_parent_map.TryGetValue(entry.bt_startid, out List<SubtreeMapBuilder.NodeDescriptor> descriptors))
                {
                    foreach (var descriptor in descriptors)
                    {
                        if (BehaviorTreeTraverser.is_decorator(descriptor.parentNode))
                        {
                            DecoratorNode dn = descriptor.parentNode as DecoratorNode;
                            dn.child = new ChildTreeNode(blackboard, entry.id);
                            ChildTreeNode ctn = dn.child as ChildTreeNode;
                            ctn.reference = ChildTreeNode.ChildNodeRef.DynamicID;
                            ctn.Init(creature, blackboard);
                        }
                        else if (BehaviorTreeTraverser.is_control(descriptor.parentNode))
                        {
                            ControlNode cn = descriptor.parentNode as ControlNode;
                            cn.childs[descriptor.index] = new ChildTreeNode(blackboard, entry.id);
                            ChildTreeNode ctn = cn.childs[descriptor.index] as ChildTreeNode;
                            ctn.reference = ChildTreeNode.ChildNodeRef.DynamicID;
                            ctn.Init(creature, blackboard);
                        }
                    }
                }
                else
                {
                    Logger.Detailed("{0} not found in brain tree", entry.bt_startid);
                }

                blackboard.UpdateVariable<string>(entry.id, entry.id);
            }

            Logger.Detailed("Reinitializing brain for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID());
            creature.brain.instance.Start();
            creature.brain.instance.Update(true);

            traverser = new BehaviorTreeTraverserDepth(creature_btree);
            mapbuilder = new SubtreeMapBuilder();
            traverser.traverse(mapbuilder);
            Logger.Detailed("New Brain tree for {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID());

            foreach (var pair in mapbuilder.descriptors)
            {
                Logger.Detailed("Subtree: {0}", pair.Key);
                foreach (var desc in pair.Value)
                {
                    Logger.Detailed("   {0} - {1}", desc.parentNode, desc.index);
                }
            }
        }
    }
}
