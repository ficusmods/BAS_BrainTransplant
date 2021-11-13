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
    public class BehaviorTreeVisitor
    {
        public virtual void visit(Node node)
        {}
    }

    public class BehaviorTreeTraverser
    {
        protected BehaviorTreeData tree;
        protected Node root;
        protected BehaviorTreeTraverser(BehaviorTreeData _tree)
        {
            tree = _tree;
            root = tree.rootNode;
        }

        public virtual void traverse(BehaviorTreeVisitor visitor)
        {}
        public static bool is_control(Node n)
        {
            return Catalog.IsSameOrSubclass(typeof(ControlNode), n.GetType());
        }
        public static bool is_decorator(Node n)
        {
            return Catalog.IsSameOrSubclass(typeof(DecoratorNode), n.GetType());
        }
        public static bool is_childtree(Node n)
        {
            return Catalog.IsSameOrSubclass(typeof(ChildTreeNode), n.GetType());
        }
    }

    public class BehaviorTreeTraverserDepth : BehaviorTreeTraverser
    {
        public BehaviorTreeTraverserDepth(BehaviorTreeData tree) : base(tree)
        {}

        public override void traverse(BehaviorTreeVisitor visitor)
        {
            traverse_impl(visitor, root);
        }

        private void traverse_impl(BehaviorTreeVisitor visitor, Node n)
        {
            visitor.visit(n);
            if (is_control(n))
            {
                List<Node> childs = (n as ControlNode).childs;
                foreach (var child in childs)
                {
                    traverse_impl(visitor, child);
                }
            }
            else if (is_decorator(n))
            {
                Node child = (n as DecoratorNode).child;
                if (child != null)
                {
                    traverse_impl(visitor, child);
                }
            }
            else if (is_childtree(n))
            {
                ChildTreeNode ctn = n as ChildTreeNode;
                string id = ctn.childTreeID;
                if(id == null || id == ""){
                    id = ctn.childTreeName;
                }
                BehaviorTreeData subtree = Catalog.GetData<BehaviorTreeData>(id, true);
                if (subtree != null)
                {
                    new BehaviorTreeTraverserDepth(subtree).traverse(visitor);
                }
            }
        }
    }
}
