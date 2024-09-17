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
    public TextMeshProUGUI nameDisplay;
    public GameObject charactersHolder;

    public delegate void NameInputChange(string currentName, bool isMaxLength);
    public NameInputChange OnNameInputChange;

    private const int MAX_CHARACTERS = 3;
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string _nameInputted = "";

    private List<string> _submittedNames = new List<string>();
    private int _playerCount = 0;

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

            var isMaxCharacters = (_nameInputted.Length >= MAX_CHARACTERS);
            if (isMaxCharacters)
            {
                EventSystem.current.SetSelectedGameObject(submitNameButton.gameObject);
            }

            submitNameButton.interactable = (_nameInputted.Length != 0);
            OnNameInputChange?.Invoke(_nameInputted, isMaxCharacters);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerCount = GlobalGameStateManager.Instance.PlayerCount;

        foreach (var charater in CHARS)
        {
            var charInput = Instantiate(characterInputPrefab, charactersHolder.transform).GetComponent<NameCharacterInput>();
            charInput.SetupCharacterInput(this, charater);
        }

        countdownUI.StartCountdown(() => OnSubmitName());
        ResetInputter();
    }

    public void AddCharacter(char character)
    { 
        NameInputted += character;        
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
        }
    }

    void ResetInputter()
    {
        NameInputted = "";
        countdownUI.ResetCountdown();
        EventSystem.current.SetSelectedGameObject(charactersHolder.GetComponentInChildren<Button>().gameObject);
    }
}
