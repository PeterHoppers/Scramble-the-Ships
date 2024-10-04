using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System.Linq;

public class AIPlayer : Player
{
    [SerializeField]
    private float _inputDelay = .25f;

    private int _tickIndex = 0;
    private SerializedDictionary<int, InputValue> _shipCommands;

    private InputValue _currentInputValue = InputValue.None;

    public override void InitPlayer(GameManager manager, PlayerShipInfo shipInfo, int id, InputMoveStyle style)
    {
        base.InitPlayer(manager, shipInfo, id, InputMoveStyle.OnInputStart);
        manager.OnTickStart += CreateNextPreview;
        manager.OnScreenChange += GetCommandsForScreen;
    }

    private void GetCommandsForScreen(int nextScreenIndex, int maxScreens)
    {
        var screen = _manager.GetComponent<ScreenSystem>().GetCurrentScreen();
        _shipCommands = screen.playerAICommands.commands;
    }

    protected void CreateNextPreview(float _)
    {
        StartCoroutine(PerformCommand(_inputDelay));
    }

    IEnumerator PerformCommand(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_shipCommands != null && _shipCommands.TryGetValue(_tickIndex, out var inputValue))
        {
            _currentInputValue = inputValue;
        }

        var actionIEnumerator = scrambledActions.Where(x => x.Value.inputValue == _currentInputValue);
        if (actionIEnumerator.Any())
        {
            var keyPair = actionIEnumerator.First();
            SendInput(keyPair.Key, keyPair.Value);
        }
    }

    protected override void OnTickEnd(float tickEndDuration, int nextTickNumber)
    {
        base.OnTickEnd(tickEndDuration, nextTickNumber);
        _tickIndex = nextTickNumber;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview;
        StopAllCoroutines();
    }
}
