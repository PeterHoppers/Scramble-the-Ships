using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    SerializedProperty onePlayerScreens;
    SerializedProperty twoPlayerScreens;
    SerializedProperty areLevelsSameToggle;

    SerializedProperty startingEffects;

    SerializedProperty levelNumber;
    SerializedProperty levelName;
    SerializedProperty levelSong;
    SerializedProperty backgroundImage;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        onePlayerScreens = serializedObject.FindProperty("onePlayerLevelScreens");
        twoPlayerScreens = serializedObject.FindProperty("twoPlayerLevelScreens");
        areLevelsSameToggle = serializedObject.FindProperty("useOnePlayerForBoth");
        startingEffects = serializedObject.FindProperty("startingEffects");
        levelNumber = serializedObject.FindProperty("levelNumber");
        levelName = serializedObject.FindProperty("levelName");
        levelSong = serializedObject.FindProperty("levelSong");
        backgroundImage = serializedObject.FindProperty("levelBackground");
    }

    // Update is called once per frame
    override public void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        // Show the custom GUI controls.
        areLevelsSameToggle.boolValue = EditorGUILayout.Toggle("Same Screens For 1 & 2 Players?", areLevelsSameToggle.boolValue);
        EditorGUILayout.PropertyField(onePlayerScreens);       

        if (!areLevelsSameToggle.boolValue)
        {
            EditorGUILayout.PropertyField(twoPlayerScreens);
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(startingEffects);
        EditorGUILayout.PropertyField(levelNumber);
        EditorGUILayout.PropertyField(levelName);
        EditorGUILayout.PropertyField(levelSong);
        EditorGUILayout.PropertyField(backgroundImage);

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
}
