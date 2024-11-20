using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class ActionSystem : MonoBehaviour
{
    [SerializeField]
    private PreviewableBase previewableBase;

    Dictionary<Player, PreviewAction> _attemptedPlayerActions = new Dictionary<Player, PreviewAction>();
    List<PreviewAction> _previewActions = new List<PreviewAction>();

    SpawnSystem _spawnSystem; 
    GridSystem _gridSystem;
    GameManager _gameManager;

    private void Awake()
    {
        _spawnSystem = GetComponent<SpawnSystem>();
        _gridSystem = GetComponent<GridSystem>();
        _gameManager = GetComponent<GameManager>();
    }

    public PreviewAction CreatePlayerMovementPreview(Player playerPerforming, Player playerPerformedOn, InputValue inputValue)
    {
        var playerPreviousAction = playerPerformedOn.previewObject;
        var inputs = new List<InputValue>() { inputValue };

        if (playerPreviousAction != null)
        {
            inputs.AddRange(playerPreviousAction.inputsPreviewing);
            playerPerformedOn.ClearPreviewObject();
        }

        var previewAction = CreateMovementPreview(playerPerformedOn, inputs);
        
        AddPlayerPreviewAction(playerPerforming, previewAction);
        return previewAction;
    }

    public PreviewAction CreateMovementPreview(Previewable previewableObject, InputValue inputToPreview)
    {
        return CreateMovementPreview(previewableObject, new List<InputValue>
        {
            inputToPreview
        });
    }

    PreviewAction CreateMovementPreview(Previewable previewableObject, List<InputValue> inputsToPreview)
    {
        //get the direction to figure out what tile this preview exists at
        var rotation = previewableObject.ConvertInputValueToRotation(inputsToPreview);
        var previewTile = GetTileFromInput(previewableObject, inputsToPreview);

        var newPreview = CreatePreviewForAction(previewableObject, previewTile, rotation);
        newPreview.inputsPreviewing = inputsToPreview;
        previewableObject.SetPreviewObject(newPreview);

        var previewAction = new PreviewAction()
        {
            sourcePreviewable = previewableObject,
            previewObject = newPreview,
            previewTile = previewTile,
        };

        AddPreviewAction(previewAction);

        return previewAction;
    }

    public PreviewAction CreatePlayerActionPreview(Player playerPerforming, Player playerPerformedOn, ShipInfo shipInfo)
    {
        var previewAction = CreateActionPreview(playerPerformedOn, shipInfo);
        AddPlayerPreviewAction(playerPerforming, previewAction);
        return previewAction;
    }

    public PreviewAction CreateActionPreview(Previewable previewable, ShipInfo shipInfo)
    {
        var fireable = shipInfo.fireable;
        var firePreviewTile = GetTileFromInput(previewable, InputValue.Fire);

        var moveable = CreateMovableForPreview(previewable, fireable, firePreviewTile);
        moveable.gameObject.transform.localRotation = previewable.GetTransfromAsReference().localRotation;
        
        var bullet = moveable.GetComponent<Bullet>();
        bullet.owner = previewable; 
        bullet.PreviewColor = shipInfo.baseColor;

        moveable.GetComponentInChildren<SpriteRenderer>().sprite = shipInfo.bulletSprite;
        moveable.name = $"Bullet of {previewable.name}";        
        
        var newPreview = CreatePreviewForAction(moveable, firePreviewTile, moveable.GetTransfromAsReference().rotation);
        newPreview.inputsPreviewing = new List<InputValue>() { InputValue.Forward };
        moveable.SetPreviewObject(newPreview);

        var previewAction = new PreviewAction()
        { 
            sourcePreviewable = moveable,
            previewObject = newPreview,
            creatorOfPreview = previewable,
            previewTile = firePreviewTile
        };        

        AddPreviewAction(previewAction);

        return previewAction;
    }

    void AddPreviewAction(PreviewAction preview)
    {
        _previewActions.Add(preview);
    }

    public int GetPlayerActionCount()
    {
        return _attemptedPlayerActions.Count;
    }

    public void ResolveAllPreviews(float tickEndDuration)
    {
        foreach (var preview in _previewActions)
        {
            var movingObject = preview.sourcePreviewable;
            if (movingObject == null)
            {
                continue;
            }

            if (preview.creatorOfPreview)
            {
                ResolvePreviewFire(preview, movingObject);
            }

            foreach (var input in preview.previewObject.inputsPreviewing)
            {
                if (input == InputValue.Clockwise || input == InputValue.Counterclockwise)
                {
                    movingObject.UpdateRotationToPreview(tickEndDuration);
                }
                else if (input != InputValue.Fire)
                {
                    movingObject.TransitionToTile(preview.previewTile, tickEndDuration);
                }

                movingObject.ResolvePreviewable();
            }            
        }
    }

    void ResolvePreviewFire(PreviewAction preview, Previewable movingObject)
    {
        movingObject.OnPreviewableCreation();
        preview.creatorOfPreview.CreatedNewPreviewable(movingObject);

        if (preview.creatorOfPreview.TryGetComponent<Player>(out var player))
        {
            _gameManager.PlayerFired(player);
        }
    }

    void AddPlayerPreviewAction(Player playerPerformingAction, PreviewAction newPreview)
    {
        _attemptedPlayerActions.Add(playerPerformingAction, newPreview);
    }

    public void ClearPreviousPlayerAction(Player playerSent)
    {
        //change this so that we look at what the player sent, then delete said preview
        if (_attemptedPlayerActions.TryGetValue(playerSent, out var previousPreview))
        {
            //need to delete the item they created if it isn't made
            _previewActions.Remove(previousPreview);
            _attemptedPlayerActions.Remove(playerSent);

            previousPreview.sourcePreviewable.ClearPreviewObject();

            if (previousPreview.creatorOfPreview)
            {
                Destroy(previousPreview.sourcePreviewable.gameObject);
            }
        }
    }

    public void ClearAllPreviews()
    {
        //check if we should remove the preview, rather than always removing it
        for (int index = _previewActions.Count - 1; index >= 0; index--)
        {
            var preview = _previewActions[index];
            preview.sourcePreviewable.ClearPreviewObject();
            _previewActions.Remove(preview);
        }

        _attemptedPlayerActions.Clear();
    }

    PreviewableBase CreatePreviewForAction(Previewable previewObject, Tile previewTitle, Quaternion spawnRotation)
    {
        var preview = _spawnSystem.CreateSpawnObject(previewableBase.gameObject, previewTitle, spawnRotation);
        preview.name = $"Preview of {previewObject.name}";
        preview.transform.localScale = previewObject.GetPreviewScale();
        var previewImage = previewObject.GetPreviewSprite();

        var renderer = preview.GetComponent<SpriteRenderer>();
        renderer.sprite = previewImage;
        renderer.color = previewObject.GetPreviewColor();
        var previewBase = preview.GetComponent<PreviewableBase>();
        previewBase.SetPreviewOutlineColor(previewObject.GetPreviewOutline(), previewImage);        

        return previewBase;
    }

    GridMovable CreateMovableForPreview(Previewable previewableCreatingMovable, GridMovable movableToBeCreated, Tile previewTile, InputValue movingInput = InputValue.Forward)
    {
        var spawnedMovable = _spawnSystem.CreateSpawnObject(movableToBeCreated.gameObject, previewableCreatingMovable.CurrentTile, previewableCreatingMovable.transform.rotation);
        var moveable = spawnedMovable.GetComponent<GridMovable>();
        moveable.SetupMoveable(_gameManager, _spawnSystem, previewTile);
        moveable.movingInput = movingInput;
        moveable.gameObject.SetActive(false);

        return moveable;
    }

    public Tile GetTileFromInput(Previewable inputSource, InputValue input)
    {
        return GetTileFromInput(inputSource, new List<InputValue>
        {
            input
        });
    }

    Tile GetTileFromInput(Previewable inputSource, List<InputValue> inputs)
    {
        var targetCoordinates = inputSource.GetGridCoordinates();
        var targetTransform = inputSource.GetTransfromAsReference();

        foreach(var input in inputs) 
        {
            switch (input)
            {
                case InputValue.Forward:
                case InputValue.Fire:
                    targetCoordinates += (Vector2)targetTransform.up;
                    continue;
                case InputValue.Backward:
                    targetCoordinates += (Vector2)targetTransform.up * -1;
                    continue;
                case InputValue.Port:
                    targetCoordinates += (Vector2)targetTransform.right * -1;
                    continue;
                case InputValue.Starboard:
                    targetCoordinates += (Vector2)targetTransform.right;
                    continue;
                default:
                    continue;
            }
        }

        _gridSystem.TryGetTileByCoordinates(targetCoordinates, out var tile);

        return tile;
    }
}

public struct PreviewAction
{
    public Previewable sourcePreviewable;
    public PreviewableBase previewObject;
#nullable enable
    public Previewable? creatorOfPreview;
#nullable disable
    public Tile previewTile;
    public int previewFinishedTick;

    public override string ToString()
    {
        return $"Source: {sourcePreviewable.name} at {previewTile.name} at tick {previewFinishedTick}";
    }
}
