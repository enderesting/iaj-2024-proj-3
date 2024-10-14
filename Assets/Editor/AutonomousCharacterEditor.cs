using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutonomousCharacter))]
public class AutonomousCharacterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the serialized object - required for making undo/redo work
        serializedObject.Update();

        // Get a reference to the target object of this custom editor
        AutonomousCharacter character = (AutonomousCharacter)target;

        // Get the CharacterControlType property
        SerializedProperty characterControlProp = serializedObject.FindProperty("characterControl");

        // Display Hero Actions fields without condition
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Optional Hero Actions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ReactToEnemy"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LevelUp"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("GetHealthPotion"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SwordAttack"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("GetManaPotion"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShieldOfFaith"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DivineSmite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Teleport"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("LayOnHands"));
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("Rest"));


        // Display CharacterControlType enum field
        EditorGUILayout.PropertyField(characterControlProp, new GUIContent("Character Control"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("worldModelSettings"), new GUIContent("World Model"));

        // Check the value of CharacterControlType and display fields conditionally
        if (character.characterControl == AutonomousCharacter.CharacterControlType.GOB ||
            character.characterControl == AutonomousCharacter.CharacterControlType.GOAP)
        {
            EditorGUILayout.Space(); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SurviveGoalWeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GainLevelGoalWeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BeQuickGoalWeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GetRichWeight"));
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GainLevelInitialInsistence"), new GUIContent("Wished Level"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GetRichInitialInsistence"), new GUIContent("Get Rich"));

        }
        else if (character.characterControl == AutonomousCharacter.CharacterControlType.MCTS ||
            character.characterControl == AutonomousCharacter.CharacterControlType.MCTS_BiasedPlayout)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MCTS_MaxIterations"), new GUIContent("Max Iterations"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MCTS_MaxIterationsPerFrame"), new GUIContent("Max Iterations per Frame"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MCTS_MaxPlayoutDepth"), new GUIContent("Max Playout Depth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MCTS_NumberPlayouts"), new GUIContent("Number of Playouts"));
        }
        else if (character.characterControl == AutonomousCharacter.CharacterControlType.TabularQLearning ||
            character.characterControl == AutonomousCharacter.CharacterControlType.NeuralNetwork) 
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RLLOptions"), new GUIContent("Train or Test?"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxEpisodes"), new GUIContent("Training Episodes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LearningRate"), new GUIContent("Learning Rate"));
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
