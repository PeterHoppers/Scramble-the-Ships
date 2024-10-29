using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    public float startDelay = .5f;
    public float endDelay = .75f;
    [Range(0, .1f)]
    public float dialogueSpeed;
    [Range(0, 45)]
    public float autoTextAdvanceInSeconds;
    [Range(0, 15)]
    public float aiAutoTextAdvanceInSeconds;
    [SerializeField]
    private GameObject dialogueHolder;
    [SerializeField] 
    private GameObject continuePrompt;
    [SerializeField]
    private TypewriterByCharacter dialogueTypewriter;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip _openSFX;
    [SerializeField]
    private AudioClip _closeSFX;

    public delegate void DialogueStart();
    public DialogueStart OnDialogueStart;

    public delegate void DialogueNodeEnd();
    public DialogueNodeEnd OnDialogueNodeEnd;

    public delegate void DialogueAdvance(int nextNodeId);
    public DialogueAdvance OnDialogueAdvance;

    public delegate void DialogueEnd();
    public DialogueEnd OnDialogueEnd;

    Animator _dialogueBoxAnimator;
    AudioSource _dialogueAudioSource;
    List<DialogueNode> _currentDialogueNodes = null;
    int _currentIndex = 0;
    InputSystemUIInputModule _inputSystem;
    bool _isAI = false;

    bool _currentLineShown;
    bool CurrentLineShown
    {
        get => _currentLineShown;
        set
        {
            _currentLineShown = value;
            if (!GlobalGameStateManager.Instance.IsAIPlaying)
            {
                continuePrompt.SetActive(value);
            }
        }
    }

    bool _canInteractWithDialogue = true;

    void Awake()
    {
        _dialogueBoxAnimator = GetComponent<Animator>();
        _dialogueAudioSource = GetComponent<AudioSource>();
        CurrentLineShown = false;
        continuePrompt.SetActive(false);
        _isAI = GlobalGameStateManager.Instance.IsAIPlaying;

        dialogueTypewriter.onTextShowed.AddListener(() => 
        {
            CurrentLineShown = true;
            if (autoTextAdvanceInSeconds > 0)
            {
                var advanceTime = _isAI ? aiAutoTextAdvanceInSeconds : autoTextAdvanceInSeconds;
                StartCoroutine(AutoAdvanceDialogue(advanceTime));
            }
        });
        dialogueTypewriter.waitForNormalChars = dialogueSpeed;
        dialogueTypewriter.waitMiddle = dialogueSpeed * 10;
        dialogueTypewriter.waitLong = dialogueSpeed * 20;
        dialogueHolder.SetActive(false);

        if (!_isAI)
        {
            _inputSystem = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
            _inputSystem.submit.action.performed += OnSubmit;
        }       
    }

    void OnSubmit(InputAction.CallbackContext context)
    {
        if (!_canInteractWithDialogue)
        {
            return;
        }

        var fired = context.ReadValueAsButton();

        if (fired == true && context.performed && HasDialogue())
        {
            AdvanceDialoguePressed();
        }
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
        dialogueTypewriter.ShowText("");
        _currentIndex = 0;
        dialogueHolder.SetActive(true);
        _dialogueBoxAnimator.Play("dialogueEnter");
        PlaySFX(_openSFX);

        StartCoroutine(OpenDialogue());
    }

    IEnumerator OpenDialogue()
    {
        yield return new WaitForSeconds(startDelay);
        UpdateText(_currentIndex);
        OnDialogueStart?.Invoke();
    }

    public void SetDialogueIsEnable(bool isEnable)
    { 
        _canInteractWithDialogue = isEnable;
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
        if (!_isAI)
        {
            GlobalAudioManager.Instance.PlayClickSFX();
        }

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
        CurrentLineShown = false;
        StartCoroutine(CloseDialogue());    
    }

    IEnumerator CloseDialogue()
    {
        PlaySFX(_closeSFX);
        _dialogueBoxAnimator.Play("dialogueExit");
        yield return new WaitForSeconds(endDelay);
        OnDialogueEnd?.Invoke();
        _currentDialogueNodes = null;
        dialogueHolder.SetActive(false);
    }

    void PlaySFX(AudioClip audioClip)
    {
        _dialogueAudioSource.clip = audioClip;
        _dialogueAudioSource.Play();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (_inputSystem != null) 
        {
            _inputSystem.submit.action.performed -= OnSubmit;
        }
    }
}
