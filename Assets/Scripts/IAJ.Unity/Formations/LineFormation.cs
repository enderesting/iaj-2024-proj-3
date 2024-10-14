using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class LineFormation : FormationPattern 
    {
        // This is a very simple line formation, with the anchor being the position of the character at index 0.
        private static readonly float offset = -3.0f;

        public LineFormation()
        {
            this.FreeAnchor = false;
        }

        public override Vector3 GetOrientation(FormationManager formation )
        {
            //In this formation, the orientation is defined by the first character's transform rotation...

            //antigo
            //Quaternion rotation = formation.SlotAssignment[0].transform.rotation;

            //lat year
            //Quaternion rotation = formation.SlotAssignment.Keys.First().transform.rotation;

            //novo
            return formation.SlotAssignment.Keys.First().transform.forward;

            //Vector2 orientation = new Vector2(rotation.x, rotation.z);
            //return orientation;

            //return new Vector3(rotation.x,rotation.y,rotation.z);
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber) => slotNumber switch
        {
            0 => formation.AnchorPosition,
            _ => formation.AnchorPosition + offset * slotNumber * this.GetOrientation(formation)
        };

        public override  bool SupportSlot(int slotCount)
        {
            return (slotCount <= 3); 
        }

        
    }
}