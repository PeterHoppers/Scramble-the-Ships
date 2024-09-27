using UnityEditor;

[CustomEditor(typeof(Screen))]
public class ScreenEditor : Editor
{
    SerializedProperty startingItems;
    SerializedProperty overrideDefaultTransitionGrids;
    SerializedProperty transitionGrids;
    SerializedProperty overrideDefaultStartingPositions;
    SerializedProperty startingPlayerPositions;
    
    SerializedProperty useDifferentDialogueForBothPlayers;
    SerializedProperty screenDialogue;
    SerializedProperty twoPlayerDialogue;
    SerializedProperty effects;
    SerializedProperty playerAICommands;
    SerializedProperty screenMusicTransition;

    SerializedProperty enemySpawnInformation;
    SerializedProperty spawnsLoopAtTick;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        startingItems = serializedObject.FindProperty("startingItems");
        overrideDefaultTransitionGrids = serializedObject.FindProperty("overrideDefaultTransitionGrids");
        transitionGrids = serializedObject.FindProperty("transitionGrids");
        overrideDefaultStartingPositions = serializedObject.FindProperty("overrideDefaultStartingPositions");
        startingPlayerPositions = serializedObject.FindProperty("startingPlayerPositions");

        useDifferentDialogueForBothPlayers = serializedObject.FindProperty("useDifferentDialogueForBothPlayers");
        screenDialogue = serializedObject.FindProperty("screenDialogue");
        twoPlayerDialogue = serializedObject.FindProperty("twoPlayerDialogue");
        effects = serializedObject.FindProperty("effects");
        playerAICommands = serializedObject.FindProperty("playerAICommands");
        screenMusicTransition = serializedObject.FindProperty("screenMusicTransition");

        enemySpawnInformation = serializedObject.FindProperty("enemySpawnInformation");
        spawnsLoopAtTick = serializedObject.FindProperty("spawnsLoopAtTick");
    }

    // Update is called once per frame
    override public void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();        

        EditorGUILayout.PropertyField(startingItems);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(effects);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(enemySpawnInformation);
        EditorGUILayout.PropertyField(spawnsLoopAtTick);
        EditorGUILayout.PropertyField(screenDialogue);
        EditorGUILayout.PropertyField(playerAICommands);
        EditorGUILayout.PropertyField(screenMusicTransition);

        EditorGUILayout.Space();

        overrideDefaultTransitionGrids.boolValue = EditorGUILayout.Toggle("Override default transition grids?", overrideDefaultTransitionGrids.boolValue);

        if (overrideDefaultTransitionGrids.boolValue)
        {
            EditorGUILayout.PropertyField(transitionGrids);
        }

        overrideDefaultStartingPositions.boolValue = EditorGUILayout.Toggle("Override default starting positions?", overrideDefaultStartingPositions.boolValue);

        if (overrideDefaultStartingPositions.boolValue)
        {
            EditorGUILayout.PropertyField(startingPlayerPositions);
        }

        useDifferentDialogueForBothPlayers.boolValue = EditorGUILayout.Toggle("Have different dialogue for player amount?", useDifferentDialogueForBothPlayers.boolValue);

        if (useDifferentDialogueForBothPlayers.boolValue)
        {
            EditorGUILayout.PropertyField(twoPlayerDialogue);
        }

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
}
