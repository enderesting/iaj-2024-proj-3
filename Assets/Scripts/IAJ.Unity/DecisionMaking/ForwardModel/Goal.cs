using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class Goal
    {
        public string Name { get; private set; }
        public float InsistenceValue { get; set; }
        public float NormalizedInsistenceValue { get; set; }
        public float ChangeRate { get; set; }
        public float Weight { get; private set; }
        public float Max;
        public float Min;

        public Goal(string name, float weight, float min, float max)
        {
            this.Name = name;
            this.Weight = weight;
            this.Min = min;
            this.Max = max;
        }

        public override bool Equals(object obj)
        {
            var goal = obj as Goal;
            if (goal == null) return false;
            else return this.Name.Equals(goal.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public float NormalizeGoalValue(float value, float min, float max)
        {
            if (value < 0) value = 0.0f;
            if (value > max) value = max;
            // Normalizing to 0-1
            var x = (value - min) / (max - min);

            // Multiplying it by 10
            x *= 10;
            NormalizedInsistenceValue = x;
            return x;
        }

        public float GetDiscontentment()
        {
            var insistence = this.NormalizedInsistenceValue;
            return this.Weight * insistence * insistence;
        }

        public float GetDiscontentment(float normalizedValue)
        {
            return this.Weight*normalizedValue* normalizedValue;
        }
    }
}
