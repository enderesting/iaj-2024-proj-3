using System.Collections;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public abstract class FormationPattern
    {
        public bool FreeAnchor;
        public abstract Vector3 GetOrientation(FormationManager formation);

        public abstract Vector3 GetSlotLocation(FormationManager formation, int slotNumber);

        public abstract bool SupportSlot(int slotCount);
    }
}