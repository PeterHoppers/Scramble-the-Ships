using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    SerializedProperty onePlayerScreens;
    SerializedProperty twoPlayerScreens;
    SerializedProperty areLevelsSameToggle;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        onePlayerScreens = serializedObject.FindProperty("onePlayerLevelScreens");
        twoPlayerScreens = serializedObject.FindProperty("twoPlayerLevelScreens");
        areLevelsSameToggle = serializedObject.FindProperty("useOnePlayerForBoth");
    }

    // Update is called once per frame
    override public void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        // Show the custom GUI controls.
        areLevelsSameToggle.boolValue = EditorGUILayout.Toggle("Are Screens Same For Both Players?", areLevelsSameToggle.boolValue);
        EditorGUILayout.PropertyField(onePlayerScreens);       

        if (!areLevelsSameToggle.boolValue)
        {
            EditorGUILayout.PropertyField(twoPlayerScreens);
        }

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
}
