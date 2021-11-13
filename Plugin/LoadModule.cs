using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;

namespace BrainTransplant
{
    public class LoadModule : LevelModule
    {
        public string mod_version = "0.0";
        public string mod_name = "UnnamedMod";
        public string logger_level = "Basic";

        public List<BrainTransplantEntry> entries;

        public override IEnumerator OnLoadCoroutine()
        {
            Logger.init(mod_name, mod_version, logger_level);
            Logger.Basic("Loading {0}", mod_name);

            entries = TransplantDataLoader.load_entries();

            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;

            return base.OnLoadCoroutine();
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (creature.isPlayer) return;
            Logger.Basic("Applying entries to {0} ({1}, {2})", creature.name, creature.creatureId, creature.GetInstanceID());
            ApplyTransplants(creature);
        }

        private void ApplyTransplants(Creature creature)
        {
            Transplanter.transplant(creature, entries);
        }
    }
}
