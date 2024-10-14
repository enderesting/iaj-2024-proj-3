using UnityEditor;
using UnityEngine;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class FormationManager
    {
        public Dictionary<Monster, int> SlotAssignment;

        private FormationPattern Pattern;

        // # A Static (i.e., position and orientation) representing the
        // # drift offset for the currently filled slots.

        public Vector3 AnchorPosition;

        public Vector3 Orientation;

        
        public FormationManager(List<Monster> Monsters, FormationPattern pattern, Vector3 position, Vector3 orientation )
        {
            this.SlotAssignment = new Dictionary<Monster, int>();
            this.Pattern = pattern;
            this.AnchorPosition = position;
            this.Orientation = orientation;

            int i = 0;
            foreach (Monster npc in Monsters)
            {
                npc.usingFormation = true;
                npc.FormationManager = this; 
                SlotAssignment[npc] = i;
                i++;
                if (SlotAssignment[npc] == 0) {
                    npc.formationLeader = true;
                }
                else {
                    npc.formationLeader = false;
                }
            }
            // the pattern may define specific rules for the orientation...
            //this.Orientation = pattern.GetOrientation( this, orientation );
        }

        public void UpdateSlotAssignements()
        {
            int i = 0; 
            foreach(var npc in SlotAssignment.Keys)
            {
                SlotAssignment[npc] = i;
                i++;
            }
        }

        public bool AddCharacter(Monster character)
        {
            var occupiedSlots = this.SlotAssignment.Count;
            if (this.Pattern.SupportSlot(occupiedSlots + 1))
            {
                SlotAssignment.Add(character, occupiedSlots);
                this.UpdateSlotAssignements();
                return true;
            }
            else return false;
        }

        public void RemoveCharacter(Monster character)
        {
            if (!SlotAssignment.ContainsKey(character)) return;
            var slot = SlotAssignment[character];
            SlotAssignment.Remove(character);
            character.usingFormation = false;
            character.formationLeader = false;
            character.FormationManager = null;
            UpdateSlotAssignements();
        }

        public void BreakFormation()
        {
            foreach (var npc in SlotAssignment.Keys) {
                npc.usingFormation = false;
                npc.formationLeader = false;
                npc.FormationManager = null;
            }

            SlotAssignment.Clear();
        }

        public void UpdateSlots()
        {
            var anchor = Pattern.FreeAnchor ?  AnchorPosition : SlotAssignment.FirstOrDefault(pair => pair.Value == 0).Key.transform.position;
            AnchorPosition = anchor;
            Orientation = Pattern.FreeAnchor ?  AnchorPosition : SlotAssignment.FirstOrDefault(pair => pair.Value == 0).Key.transform.forward;


            var orientationMatrix = Orientation;
            Debug.DrawRay(anchor, orientationMatrix);


            foreach (var npc in SlotAssignment.Keys)
            {
                if (SlotAssignment[npc] > 0 || Pattern.FreeAnchor)
                {
                    int slotNumber = SlotAssignment[npc];
                    var slot = Pattern.GetSlotLocation(this, slotNumber);

                    var locationPosition = anchor + orientationMatrix * slotNumber;
                    var locationOrientation = orientationMatrix + slot;

                    // and add drift componenet.

                    slot -= Vector3.one * 0.1f;
                    //locationOrientation -= 0.1f;

                    npc.StartPathfinding(slot);
                    npc.GetComponent<NavMeshAgent>().updateRotation = true;
                }
            }
        }
    }
}