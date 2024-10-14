using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelCreator))]
public class LevelDesignInspector : Editor
{
    // This is were we can create our own custom inspector which allows us to instanteate and destroy the map objects
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelCreator manager = (LevelCreator)target;

        if(GUILayout.Button("Generate Map"))
        {
            manager.LoadGrid();
            manager.GridMapVisual();
        }

        if (GUILayout.Button("Clean Map"))
        {
            manager.CleanMap();
        }
    }
   
}
