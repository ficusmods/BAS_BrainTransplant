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
    class TransplantDataLoader
    {
        public static List<BrainTransplantEntry> load_entries()
        {
            List<BrainTransplantEntry> catalogEntries = Catalog.GetDataList(Catalog.Category.BehaviorTree).OfType<BrainTransplantEntry>().ToList();
            foreach(var entry in catalogEntries)
            {
                Logger.Basic("Found entry {0}", entry.id, entry.filePath);
            }

            return catalogEntries;
        }
    }
}
