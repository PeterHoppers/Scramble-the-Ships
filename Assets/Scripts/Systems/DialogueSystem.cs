using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    [Range(0, .1f)]
    public float dialogueSpeed;
    [Range(0, 15)]
    public float autoTextAdvanceInSeconds;
    [SerializeField]
    private GameObject dialogueHolder;
    [SerializeField] 
    private GameObject continuePrompt;
    [SerializeField]
    private TypewriterByCharacter dialogueTypewriter;

    public delegate void DialogueStart();
    public DialogueStart OnDialogueStart;

    public delegate void DialogueNodeEnd();
    public DialogueNodeEnd OnDialogueNodeEnd;

    public delegate void DialogueAdvance(int nextNodeId);
    public DialogueAdvance OnDialogueAdvance;

    public delegate void DialogueEnd();
    public DialogueEnd OnDialogueEnd;

    List<DialogueNode> _currentDialogueNodes = null;
    int _currentIndex = 0;

    bool _currentLineShown;
    bool CurrentLineShown
    {
        get => _currentLineShown;
        set
        {
            _currentLineShown = value;
            continuePrompt.SetActive(value);
        }
    }

    void Awake()
    {
        CurrentLineShown = false;
        dialogueTypewriter.onTextShowed.AddListener(() => 
        {
            CurrentLineShown = true;
            if (autoTextAdvanceInSeconds > 0)
            {
                StartCoroutine(AutoAdvanceDialogue(autoTextAdvanceInSeconds));
            }
        });

        var inputSystem = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
        inputSystem.submit.action.performed += (InputAction.CallbackContext context) => 
        {
            var fired = context.ReadValueAsButton();

            if (fired == true && context.performed && HasDialogue())
            {
                AdvanceDialoguePressed();
            }
        };

        dialogueTypewriter.waitForNormalChars = dialogueSpeed;
        dialogueTypewriter.waitMiddle = dialogueSpeed * 10;
        dialogueTypewriter.waitLong = dialogueSpeed * 20;
        dialogueHolder.SetActive(false);
    }

    public void SetDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            return;
        }

        _currentDialogueNodes = dialogue.dialogueNodes;
    }

    public bool HasDialogue()
    { 
        return (_currentDialogueNodes != null);
    }

    public void StartDialogue()
    {
        _currentIndex = 0;
        dialogueHolder.SetActive(true);
        UpdateText(_currentIndex);
        OnDialogueStart?.Invoke();
    }

    IEnumerator AutoAdvanceDialogue(float waitDuration)
    { 
        yield return new WaitForSeconds(waitDuration);
        AdvanceDialogue();
    }

    public void AdvanceDialoguePressed()
    {
        if (CurrentLineShown)
        {
            AdvanceDialogue();
        }
        else
        {
            dialogueTypewriter.SkipTypewriter();
        }
    }

    void AdvanceDialogue()
    {
        StopAllCoroutines();
        CurrentLineShown = false;
        _currentIndex++;

        if (_currentIndex >= _currentDialogueNodes.Count) 
        { 
            EndDialogue();
            return;
        }

        UpdateText(_currentIndex);
        OnDialogueAdvance?.Invoke(_currentIndex);
    }

    void UpdateText(int index)
    {
        var nextText = _currentDialogueNodes[index].textToDisplay;
        dialogueTypewriter.ShowText(nextText);
    }

    void EndDialogue()
    {
        dialogueTypewriter.ShowText("");
        dialogueHolder.SetActive(false);
        OnDialogueEnd?.Invoke();
        _currentDialogueNodes = null;
        CurrentLineShown = false;
    }
}
