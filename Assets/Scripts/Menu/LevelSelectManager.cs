using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField]
    private List<LevelSelectNode> _levelSelectNodes = new List<LevelSelectNode>();
    private int _selectedNodeIndex;
    int SelectedNodeIndex
    {
        get { return _selectedNodeIndex; }
        set
        {
            _selectedNodeIndex = value;

            _backArrow.SetActive(_selectedNodeIndex != 0);
            _forwardArrow.SetActive(_selectedNodeIndex < (_levelSelectNodes.Count- 1));
        }
    }

    private LevelSelectNode _selectedNode;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI _titleText;
    [SerializeField]
    private TextMeshProUGUI _descriptionText;
    [SerializeField]
    private Image _levelImage;
    [SerializeField]
    private CountdownUI _countdownUI;
    [SerializeField]
    private GameObject _forwardArrow;
    [SerializeField]
    private GameObject _backArrow;

    [Space]
    [SerializeField]
    private float _levelTransitionDuration;

    private InputSystemUIInputModule _inputSystem;
    private GlitchAdapter _glitchAdapter;

    private void OnEnable()
    {
        _inputSystem = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
        if (_inputSystem != null)
        {
            _inputSystem.submit.action.performed += OnSubmit;
            _inputSystem.move.action.performed += OnMove;
        }
    }

    private void OnDisable()
    {
        if (_inputSystem != null ) 
        {
            _inputSystem.submit.action.performed -= OnSubmit;
        }
    }

    private void Start()
    {
        _glitchAdapter = Camera.main.GetComponent<GlitchAdapter>();

        SelectedNodeIndex = 0;
        SetSelectedNode(SelectedNodeIndex, true);
        _countdownUI.StartCountdown(OnNodeSelect);
    }   

    void OnSubmit(InputAction.CallbackContext context)
    {
        var fired = context.ReadValueAsButton();

        if (fired == true && context.performed)
        {
            OnNodeSelect();
        }        
    }

    void OnNodeSelect()
    {
        if (_selectedNode == null)
        {
            _selectedNode = _levelSelectNodes[0];
        }

        GlobalAudioManager.Instance.PlayClickSFX();
        GlobalGameStateManager.Instance.LoadLevel(_selectedNode.level);
    }

    void OnMove(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();

        if (context.performed && direction.x != 0)
        {
            ChangeSelectedNode(direction.x > 0);
        }
    }

    void ChangeSelectedNode(bool isForward)
    {
        var newIndex = (isForward) ? _selectedNodeIndex + 1 : _selectedNodeIndex - 1;

        if (newIndex < 0 || newIndex >= _levelSelectNodes.Count)
        {
            return;
        }

        GlobalAudioManager.Instance.PlayClickSFX();
        SetSelectedNode(newIndex);
    }

    void SetSelectedNode(int nodeIndex, bool isImmediatelyChanging = false)
    {
        SelectedNodeIndex = nodeIndex;
        _selectedNode = _levelSelectNodes[nodeIndex];

        StartCoroutine(UpdateDisplay(isImmediatelyChanging));
    }

    IEnumerator UpdateDisplay(bool isImmediatelyChanging)
    {
        _glitchAdapter.PerformDefaultGlitchTransitionEffect();
        var halfTransitionDuration = _levelTransitionDuration / 2;

        if (!isImmediatelyChanging)
        {
            yield return new WaitForSeconds(halfTransitionDuration);
        }

        _titleText.text = _selectedNode.title;
        _descriptionText.text = _selectedNode.description;
        _levelImage.sprite = _selectedNode.levelImage;

        yield return new WaitForSeconds(halfTransitionDuration);
        _glitchAdapter.ClearGlitchEffects();
    }
}
