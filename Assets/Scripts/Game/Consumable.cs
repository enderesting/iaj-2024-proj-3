using UnityEngine;

namespace Assets.Scripts.Game
{
    public class Consumable : MonoBehaviour
    {
        private Vector3 initialPosition;

        void Awake()
        {
            initialPosition = this.transform.position;
        }

        public void Reset()
        {
            this.transform.position = initialPosition;
        }
    }
}