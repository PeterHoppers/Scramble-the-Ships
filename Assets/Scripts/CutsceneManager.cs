using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public List<PlayerShipInfo> shipInfos = new List<PlayerShipInfo>();
    public List<Dialogue> dialogues = new List<Dialogue>();

    [SerializeField]
    private GameObject shipHolder;
    [SerializeField]
    private GameObject shipBase;

    [SerializeField]
    private float _dialogueDelay;
    [SerializeField]
    private DialogueSystem _dialogueSystem;

    private void Awake()
    {
        var numberOfPlayers = GlobalGameStateManager.Instance.PlayerCount;
        CreateCutsceneShip(shipInfos[0]);

        if (numberOfPlayers == 2)
        {
            CreateCutsceneShip(shipInfos[1]);
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(_dialogueDelay);
        var dialogueId = GlobalGameStateManager.Instance.ActiveLevelIndex;
        var dialogue = dialogues[dialogueId];

        _dialogueSystem.SetDialogue(dialogue);

        if (_dialogueSystem.HasDialogue())
        {
            _dialogueSystem.StartDialogue();
            _dialogueSystem.OnDialogueEnd += WaitUntilDialogueEnds;
            void WaitUntilDialogueEnds()
            {
                GlobalGameStateManager.Instance.AdvanceFromCutsceneToGame();
                _dialogueSystem.OnDialogueEnd -= WaitUntilDialogueEnds;
            }
        }
    }

    void CreateCutsceneShip(PlayerShipInfo shipInfo)
    { 
        var shipSprite = shipInfo.cutsceneSprite;
        var shipImage = Instantiate(shipBase, shipHolder.transform);
        shipImage.name = shipInfo.name;
        shipImage.GetComponent<Image>().sprite = shipSprite;
    }
}
