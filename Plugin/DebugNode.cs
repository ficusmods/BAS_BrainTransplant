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
    public class DebugNode : ActionNode
    {
        public string message = "DebugNodeString";

        public override State Evaluate()
        {
            Logger.Detailed(message);
            return base.Evaluate();
        }
    }
}
