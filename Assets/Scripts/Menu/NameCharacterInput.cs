using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NameCharacterInput : MonoBehaviour
{
    private Button _charcterSelect;
    private TextMeshProUGUI _characterDisplay;

    private char _characterName;

    public void SetupCharacterInput(NameInputManager manager, char character)
    {
        _charcterSelect = GetComponent<Button>();
        _characterDisplay = GetComponentInChildren<TextMeshProUGUI>();

        _characterName = character;
        _characterDisplay.text = character.ToString();
        _charcterSelect.onClick.AddListener(() =>
        {
            manager.AddCharacter(_characterName);
        });

        manager.OnNameInputChange += (string _, bool isMaxLength) => 
        { 
            _charcterSelect.interactable = !isMaxLength;
        };
    }
}
