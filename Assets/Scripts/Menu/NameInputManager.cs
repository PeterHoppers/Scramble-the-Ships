using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NameInputManager : MonoBehaviour
{
    [Header("Character Related Inputs")]
    public NameCharacterInput characterInputPrefab;
    public Button submitNameButton;
    public CountdownUI countdownUI;

    [Space]
    public TextMeshProUGUI initialsMessage;
    public TextMeshProUGUI nameDisplay;
    public GameObject charactersHolder;

    public delegate void NameInputChange(string currentName, bool isMaxLength);
    public NameInputChange OnNameInputChange;

    private const int MAX_CHARACTERS = 3;
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string _nameInputted = "";
    private string _defaultInitialMessage = "";

    private List<string> _submittedNames = new List<string>();
    private int _playerCount = 0;
    private bool _isMaxCharacters = false;

    private const float _buttonSelectDelay = .25f;  //Prevent race condition with other code to set the selected button

    public string NameInputted
    {
        get
        { 
            return _nameInputted;
        }
        set
        {
            _nameInputted = value;
            nameDisplay.text = _nameInputted;

            _isMaxCharacters = (_nameInputted.Length >= MAX_CHARACTERS);
            if (_isMaxCharacters)
            {
                EventSystem.current.SetSelectedGameObject(submitNameButton.gameObject);
            }

            submitNameButton.interactable = (_nameInputted.Length != 0);
            OnNameInputChange?.Invoke(_nameInputted, _isMaxCharacters);
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        NameInputted = "";
        _playerCount = GlobalGameStateManager.Instance.PlayerCount;

        foreach (var charater in CHARS)
        {
            var charInput = Instantiate(characterInputPrefab, charactersHolder.transform).GetComponent<NameCharacterInput>();
            charInput.name = $"Character Button: {charater}";
            charInput.SetupCharacterInput(this, charater);
        }

        _defaultInitialMessage = initialsMessage.text;

        if (_playerCount > 1)
        {
            UpdateInitialsMessage(_submittedNames.Count);
        }

        countdownUI.StartCountdown(() => OnSubmitName());
        yield return new WaitForSeconds(_buttonSelectDelay);
        ResetInputter();
    }

    public void AddCharacter(NameCharacterInput characterInput)
    { 
        NameInputted += characterInput.GetCharacter();
        StartCoroutine(ReselectButton(characterInput.gameObject));
    }

    IEnumerator ReselectButton(GameObject selectedGO)
    {
        yield return new WaitForSeconds(_buttonSelectDelay);
        if (EventSystem.current.currentSelectedGameObject == null && !_isMaxCharacters)
        {
            EventSystem.current.SetSelectedGameObject(selectedGO);
        }
    }

    public void DeleteLastCharacter()
    {
        if (_nameInputted.Length == 0) 
        {
            return;
        }

        NameInputted = _nameInputted[..^1];
    }

    public void OnSubmitName()
    {
        if (_nameInputted == "")
        {
            _nameInputted = "AAA";
        }

        _submittedNames.Add(_nameInputted);

        if (_submittedNames.Count >= _playerCount)
        {
            GlobalGameStateManager.Instance.SubmitNameForScore(_submittedNames);
        }
        else
        {
            ResetInputter();
            UpdateInitialsMessage(_submittedNames.Count);
        }
    }

    void ResetInputter()
    {
        NameInputted = "";
        countdownUI.ResetCountdown();
        EventSystem.current.SetSelectedGameObject(charactersHolder.GetComponentInChildren<Button>().gameObject);
    }

    void UpdateInitialsMessage(int playerNumber)
    {
        initialsMessage.text = $"Player {playerNumber + 1}: {_defaultInitialMessage}";
    }
}
