using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player playerShip;
    public AIPlayer playerAIShip;
    [Space]
    public List<PlayerShipInfo> shipInfos = new List<PlayerShipInfo>();
    public DialogueSystem dialogueSystem;

    //GameManager Events
    public delegate void LevelStart(int levelId);
    public LevelStart OnLevelStart;

    public delegate void LevelEnd(int levelNumber, int energyLeft, int energyPercentageLeft, int continuesUsed);
    public LevelEnd OnLevelEnd;

    public delegate void TickStart(float timeToTickEnd, int currentTickNumber);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart, int nextTickNumber);
    public TickEnd OnTickEnd;

    public delegate void ScreenChange(int nextScreenIndex, int maxScreens);
    public ScreenChange OnScreenChange;

    public delegate void ScreenResetStart();
    public ScreenResetStart OnScreenResetStart;

    public delegate void ScreenResetEnd();
    public ScreenResetEnd OnScreenResetEnd;

    public delegate void GameStateChanged(GameState newState);
    public GameStateChanged OnGameStateChanged;

    public delegate void PlayerJoined(Player player);
    public PlayerJoined OnPlayerJoinedGame;

    public delegate void PlayerLeave(Player player);
    public PlayerLeave OnPlayerLeaveGame;

    public delegate void PlayerDeath(Player player, string killingObject);
    public PlayerDeath OnPlayerDeath;

    public delegate void ScoreSubmit(int newScore);
    public ScoreSubmit OnScoreSubmit;

    public delegate void PlayerConditionStart(Player player, Condition condition);
    public PlayerConditionStart OnPlayerConditionStart;

    public delegate void PlayerConditionEnd(Player player, Condition condition);
    public PlayerConditionEnd OnPlayerConditionEnd;

    public delegate void PlayerPickupStart(Player player, PickupType pickupType);
    public PlayerPickupStart OnPlayerPickup;

    //Private Variables
    GameState _currentGameState = GameState.Intro;
    GameState _previousGameState = GameState.Intro;   
    private List<Player> _players = new List<Player>();
    private int _playerCount = 0;
    List<GridCoordinate> _startingPlayerPositions;

    GridSystem _gridSystem;
    SpawnSystem _spawnSystem;
    CutsceneSystem _cutsceneSystem;
    ScreenSystem _screenSystem;
    EnergySystem _energySystem;
    EffectsSystem _effectsSystem;
    ActionSystem _previewSystem;

    public EffectsSystem EffectsSystem { get => _effectsSystem; }
    public EnergySystem EnergySystem { get => _energySystem; }

    float _tickDuration = .5f;
    float _tickEndDuration = .5f / 5;
    float TickDuration
    {
        get => _tickDuration;
        set
        {
            _tickDuration = value;
        }
    }

    InputMoveStyle _inputMoveStyle;
    float _tickElapsed = 0f;
    float _lastTickEndedAt = 0f;
    int _playerFinishedWithScreen = 0;
    bool _tickIsOccuring = false;
    int _ticksSinceScreenStart = 0;    
    int _continuesUsed = 0;

    GameInputProgression _currentScrambleType;

    void Awake()
    {
        _gridSystem = GetComponent<GridSystem>();
        _spawnSystem = GetComponent<SpawnSystem>();
        _cutsceneSystem = GetComponent<CutsceneSystem>();
        _effectsSystem = GetComponent<EffectsSystem>();
        _screenSystem = GetComponent<ScreenSystem>();
        _energySystem = GetComponent<EnergySystem>();
        _previewSystem = GetComponent<ActionSystem>();

        _effectsSystem.OnTickDurationChanged += (float newDuration) => TickDuration = newDuration;
        _effectsSystem.OnTickEndDurationChanged += (float newEndDuration) => _tickEndDuration = newEndDuration;
        _effectsSystem.OnInputMoveStyleChanged += (InputMoveStyle moveStyle) => _inputMoveStyle = moveStyle;
        _effectsSystem.OnGameInputProgressionChanged += (GameInputProgression scrambleType) => _currentScrambleType = scrambleType;
    }

    IEnumerator Start()
    {
        var _isAI = GlobalGameStateManager.Instance.IsAIPlaying;
        var _currentLevel = GlobalGameStateManager.Instance.CurrentLevel;

        if (!_isAI)
        {
            StartCoroutine(PlayIntroCutscene(_currentLevel.GetDisplayName()));
        }

        foreach (IManager managerObjects in FindAllManagers())
        {
            managerObjects.InitManager(this);
        }

        UpdateGameState(GameState.Intro);
        yield return new WaitForSeconds(.125f); //TODO: Fix race condition
        OptionsManager.Instance.AfterInitManager();

        _playerCount = GlobalGameStateManager.Instance.PlayerCount;        

        if (_playerCount == 0)
        {
            _playerCount = 1;
        }

        _screenSystem.SetScreens(_currentLevel, _playerCount);
        _startingPlayerPositions = _screenSystem.GetStartingPlayerPositions(_playerCount);

        CreatePlayerShip(_isAI);

        if (_playerCount == 2)
        {
            CreatePlayerShip(_isAI);
        }

        _energySystem.SetEnergy(_playerCount);
        _screenSystem.TriggerStartingEffects(_effectsSystem);
        UpdateGameState(GameState.Transition);
        OnLevelStart?.Invoke(GlobalGameStateManager.Instance.ActiveLevelIndex);

        if (_isAI)
        {
            StartCoroutine(SetupNextScreen(TickDuration, false));
        }
    }

    private List<IManager> FindAllManagers()
    {
        IEnumerable<IManager> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IManager>();

        return new List<IManager>(dataPersistenceObjects);
    }

    private void CreatePlayerShip(bool isAI)
    {
        var playerObject = (isAI) ? Instantiate(playerAIShip) : Instantiate(playerShip);
        var newPlayer = playerObject.GetComponent<Player>();

        int playerId = _players.Count;
        newPlayer.InitPlayer(this, shipInfos[playerId], playerId, _inputMoveStyle);
        newPlayer.transform.SetParent(transform);
        newPlayer.name = "Player " + (playerId + 1);

        _players.Add(newPlayer);       

        var startingTile = GetStartingTileForPlayer(playerId);
        UpdatePlayerStartRotation(newPlayer);
        MovePreviewableOffScreenToTile(playerObject, startingTile, 0);
        newPlayer.OnSpawn();
        OnPlayerJoinedGame?.Invoke(newPlayer);
    }

    public void CreateDummyShip(ObstaclePlayer dummyShip)
    {
        if (_players.Count >= 2)
        {
            Debug.LogWarning("We're making a dummy ship, while we already have 2 ships. Did we want that?");
        }
        dummyShip.InitPlayer(this, dummyShip.playerInfo, 1, _inputMoveStyle);
        dummyShip.transform.SetParent(transform);
        dummyShip.name = "Dummy Player";

        OnPlayerJoinedGame?.Invoke(dummyShip);
    }

    Tile GetStartingTileForPlayer(int playerId)
    {
        var startingPosition = _startingPlayerPositions[playerId];

        _gridSystem.TryGetTileByCoordinates(startingPosition, out var startingTile);
        return startingTile;
    }

    void UpdatePlayerStartRotation(Player player)
    {
        var startingDirection = _screenSystem.GetStartingPlayerRotation(_playerCount);
        var startingRotation = _spawnSystem.GetRotationForSpawnDirections(startingDirection, false);
        player.TransitionToRotation(startingRotation, .0f);
    }

    public void MovePreviewableOffScreenToTile(Previewable preview, Tile tile, float duration)
    {        
        _spawnSystem.MovePreviewableOffScreenToPosition(preview, preview.GetTransfromAsReference().up, tile.GetTilePosition(), duration);
    }

    void MovePlayerOnScreenToTile(Player player, Tile tile, float duration)
    {
        _spawnSystem.MovePreviewableOffScreenToPosition(player, player.GetTransfromAsReference().up, tile.GetTilePosition(), 0, true);
        player.TransitionToTile(tile, duration);
        player.OnMoveOnScreen();
    }

    void MovePlayerOntoStartingTitle(Player player, float duration)
    {
        UpdatePlayerStartRotation(player);
        var startingTile = GetStartingTileForPlayer(player.PlayerId);
        MovePlayerOnScreenToTile(player, startingTile, duration);
    }

    public void PlayerLeaveGame(Player player)
    { 
        OnPlayerLeaveGame?.Invoke(player);
    }

    public void PlayerGainedCondition(Player player, Condition condition)
    { 
        OnPlayerConditionStart?.Invoke(player, condition);
    }

    public void PlayerLostCondition(Player player, Condition condition)
    {
        OnPlayerConditionEnd?.Invoke(player, condition);
    }

    public void PlayerPickup(Player player, PickupType pickupType)
    { 
        OnPlayerPickup?.Invoke(player, pickupType);
    }

    public void PlayerFired(Player player)
    {
        _energySystem.OnPlayerFired();
    }

    public bool CanPlayerPerformAction(Previewable previewable, InputValue inputValue)
    {
        var targetTile = _previewSystem.GetTileFromInput(previewable, inputValue);

        return (targetTile != null && targetTile.IsVisible);
    }

    public void PlayerPreviewMove(Player performingAction, Player playerPerformedOn, InputValue inputToPreview)
    {
        _previewSystem.CreatePlayerMovementPreview(performingAction, playerPerformedOn, inputToPreview);
    }

    public void PlayerPreviewFire(Player performingAction, Player playerPerformedOn, ShipInfo firingInfo)
    {
        _previewSystem.CreatePlayerActionPreview(performingAction, playerPerformedOn, firingInfo);
    }

    public void PreviewMove(Previewable previewObject, InputValue inputToPreview)
    { 
        _previewSystem.CreateMovementPreview(previewObject, inputToPreview);
    }

    public void PreviewFire(Previewable previewObject, ShipInfo firingInfo)
    {
        _previewSystem.CreateActionPreview(previewObject, firingInfo);
    }

    public void ClearPreviousPlayerAction(Player player)
    { 
        _previewSystem.ClearPreviousPlayerAction(player);
    }

    public void ActivateCutscene(CutsceneType type, float cutsceneDuration)
    {
        if (_currentGameState == GameState.Cutscene)
        {
            return;
        }

        ToggleIsPlaying(false, GameState.Cutscene);
        _cutsceneSystem.ActivateCutscene(type, cutsceneDuration);
    }

    public IEnumerator PlayIntroCutscene(string levelName)
    {
        var introDuration = .25f;
        
        StartCoroutine(_cutsceneSystem.PlayLevelIntro(levelName, introDuration));
        yield return new WaitForSeconds(introDuration);
        StartCoroutine(SetupNextScreen(TickDuration, false));
    }

    /// <summary>
    /// Screen Change Trigger occurs when a player hits the screen change trigger
    /// </summary>
    /// <param name="player"></param>
    public void ScreenChangeTriggered(Player player, SpawnDirections transitionDirection)
    {
        if (_currentGameState == GameState.Playing)
        {
            StartCoroutine(MovePlayerOffScreen(player, transitionDirection));
        }
    }

    IEnumerator MovePlayerOffScreen(Player player, SpawnDirections transitionDirection)
    {
        _playerFinishedWithScreen++;

        if (_playerFinishedWithScreen >= _players.Count)
        {
            ToggleIsPlaying(false, GameState.Transition);
            _playerFinishedWithScreen = 0;
            _previewSystem.ClearAllPreviews();
            EndScreen(TickDuration);
        }
        else
        {
            player.SetInputStatus(false);
            player.SetActiveStatus(false);
        }

        var directionQuaterion = _spawnSystem.GetRotationForSpawnDirections(transitionDirection, true);
        if (player.GetTransfromAsReference().rotation.eulerAngles != directionQuaterion.eulerAngles)
        {
            player.TransitionToRotation(directionQuaterion, _tickEndDuration);
            yield return new WaitForSeconds(_tickEndDuration);
        }

        var currentPos = player.CurrentTile.GetTilePosition();
        _spawnSystem.MovePreviewableOffScreenToPosition(player, player.GetTransfromAsReference().up, currentPos, TickDuration);
        player.OnMoveOffScreen();
    }

    void EndScreen(float endingDuration)
    {
        int screensRemainingInLevel = _screenSystem.ScreenAmount - _screenSystem.ScreensLoaded;
        if (screensRemainingInLevel <= 0)
        {
            UpdateGameState(GameState.Win);
            int energyPercentageRemaining = (int)(_energySystem.GetEnergyPercentRemaining() * 100);
            OnLevelEnd?.Invoke(_screenSystem.GetCurrentLevelNumber(), _energySystem.CurrentEnergy, energyPercentageRemaining, _continuesUsed);                                
        }
        else
        {
            StartCoroutine(SetupNextScreen(endingDuration));
        }
    }

    IEnumerator SetupNextScreen(float tickDuration, bool playTransitionCutscene = true)
    {
        if (playTransitionCutscene)
        {
            ActivateCutscene(CutsceneType.ScreenTransition, tickDuration);
            yield return new WaitForSeconds(tickDuration);
        }

        foreach (Player player in _players)
        {
            player.SetActiveStatus(true);
        }

        _screenSystem.SetupNewScreen(_spawnSystem, _gridSystem, _effectsSystem, dialogueSystem);
        _startingPlayerPositions = _screenSystem.GetStartingPlayerPositions(_playerCount);
        _ticksSinceScreenStart = 0;
        OnScreenChange?.Invoke(_screenSystem.ScreensLoaded, _screenSystem.ScreenAmount);
        yield return new WaitForSeconds(tickDuration);

        //move ships on screen
        foreach (Player player in _players)
        {
            MovePlayerOntoStartingTitle(player, tickDuration);
        }

        yield return new WaitForSeconds(tickDuration);

        UpdateGameState(GameState.Dialogue);
        if (dialogueSystem.HasDialogue())
        {
            dialogueSystem.StartDialogue();
            if (GlobalGameStateManager.Instance.IsAIPlaying)
            {
                ToggleIsPlaying(true);
            }
            else
            {
                dialogueSystem.OnDialogueEnd += WaitUntilDialogueEnds;
                void WaitUntilDialogueEnds()
                {
                    dialogueSystem.OnDialogueEnd -= WaitUntilDialogueEnds;
                    StartCoroutine(DelayUnpausing(_tickEndDuration * 2));
                }
            }            
        }
        else
        {
            ToggleIsPlaying(true);
        }
    }

    IEnumerator DelayUnpausing(float delayAmount)
    { 
        yield return new WaitForSeconds(delayAmount);
        ToggleIsPlaying(true);
    }

    public void ToggleIsPlaying(bool isPlaying, GameState disabledState = GameState.Paused)
    {
        foreach (Player player in _players)
        {
            player.SetInputStatus(isPlaying);
        }

        var newGameState = (isPlaying) ? GameState.Playing : disabledState;
        UpdateGameState(newGameState);
    }

    public void PauseGame()
    {
        if (_currentGameState == GameState.Paused)
        {
            if (_previousGameState == GameState.Playing)
            {
                ToggleIsPlaying(true);
            }
            else
            {
                UpdateGameState(_previousGameState);
            }
        }
        else if (_currentGameState == GameState.Playing)
        {
            ToggleIsPlaying(false);
        }
        else
        {
            UpdateGameState(GameState.Paused);
        }
    }

    public void ContinuePerformed()
    {
        if (_currentGameState == GameState.Win || _currentGameState == GameState.GameOver)
        {
            StopAllCoroutines();
            GlobalGameStateManager.Instance.ConsumeCredits(_playerCount);
            GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.Game;
            _energySystem.RefillEnergy();
            _continuesUsed++;
            StartCoroutine(ResetScreen(true));
        }
    }

    public GameState GetGameState()
    {
        return _currentGameState;
    }

    void UpdateGameState(GameState gameState) 
    {
        _previousGameState = _currentGameState;
        _currentGameState = gameState;

        switch (gameState) 
        {
            case GameState.Playing:
                if (_previousGameState != GameState.Paused) // don't end the current tick if we're just leaving pausing
                {
                    StartNextTick();
                }
                break;
            case GameState.Dialogue:
                dialogueSystem.SetDialogueIsEnable(true);
                break;
            case GameState.Paused:
                dialogueSystem.SetDialogueIsEnable(false);
                break;
            case GameState.Transition:
            case GameState.Resetting:
            case GameState.GameOver: //this might run into a race condition with on tick end
                _tickIsOccuring = false;
                break;
        }

        if (_currentGameState == GameState.Paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        OnGameStateChanged?.Invoke(_currentGameState);
    }

    public void HandlePlayerCollision(Player playerAttack, Player playerHit)
    {
        if (_currentGameState != GameState.Playing)
        {
            return;
        }

        if (playerAttack.IsPlayerDeadOnHit())
        {
            HandlePlayerDeath(playerAttack, playerHit.name);
        }

        if (playerHit.IsPlayerDeadOnHit())
        {
            HandlePlayerDeath(playerHit, playerAttack.name);
        }

        UpdateStateOnPlayerDeath();
    }

    public void HandleGridObjectCollision(GridObject attacking, GridObject hit) //Should this be here in this state? Feels like something the grid object itself should be in charge of
    {
        if (!attacking.CannotBeDestoryed())
        {
            attacking.DestroyObject();
        }

        if (hit.TryGetComponent<Player>(out var player))
        {
            if (player.IsPlayerDeadOnHit())
            {
                HandlePlayerDeath(player, attacking.name);
                UpdateStateOnPlayerDeath();
            }
        }
        else
        {
            if (hit.CanBeShot() && !hit.CannotBeDestoryed())
            {
                hit.DestroyObject();
            }
        }
    }

    void HandlePlayerDeath(Player player, string killingObject)
    {
        player.OnDeath();
        OnPlayerDeath?.Invoke(player, killingObject);
    }

    void UpdateStateOnPlayerDeath()
    {
        if (_currentGameState != GameState.Playing)
        {
            return;
        }

        if (_energySystem.CanPlayerDieAndGameContinue())
        {
            StartCoroutine(ResetScreen(false));
        }
        else
        {
            OnGameOver();
        }
    }

    void OnGameOver()
    {
        if (_currentGameState != GameState.Playing)
        {
            return;
        }

        ToggleIsPlaying(false, GameState.GameOver);
        _previewSystem.ClearAllPreviews();
        _energySystem.CurrentEnergy = 0; 
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.GameOver;

        foreach (Player player in _players)
        {
            player.OnGameOver();
        }
    }

    public void OnSubmitScore(int newScore)
    { 
        OnScoreSubmit?.Invoke(newScore);
        GlobalGameStateManager.Instance.CurrentScore = newScore;
    }

    IEnumerator ResetScreen(bool isOnContinue)
    {
        ToggleIsPlaying(false, GameState.Resetting);
        _previewSystem.ClearAllPreviews();

        if (!isOnContinue)
        {
            yield return new WaitForSeconds(_tickDuration * 1.5f);
        }
        else
        {
            _screenSystem.PlayLevelSong();
        }

        OnScreenResetStart?.Invoke();
        _cutsceneSystem.PerformRewindEffect();

        float rewindDuration = 1.25f;
        yield return new WaitForSeconds(rewindDuration);
        _effectsSystem.PerformEffect(EffectType.DigitalGlitchIntensity, 0);
        _effectsSystem.PerformEffect(EffectType.ScanLineJitter, 0);
        _effectsSystem.PerformEffect(EffectType.HorizontalShake, 0);

        _playerFinishedWithScreen = 0;
        _ticksSinceScreenStart = 0;
        _screenSystem.ConfigureCurrentScreen(_spawnSystem, _gridSystem, _effectsSystem);

        foreach (Player player in _players)
        {
            player.OnSpawn();
            MovePlayerOntoStartingTitle(player, _tickDuration);
        }

        if (!isOnContinue)
        {
            OnScreenResetEnd?.Invoke();
        }

        yield return new WaitForSeconds(_tickDuration);
        ToggleIsPlaying(true);
    }

    public List<Player> GetAllPlayers()
    {
        return _players;
    }

    public int GetPlayersRemaining()
    {
        return _playerCount - _playerFinishedWithScreen;
    }

    public void EndCurrentTick(Player player)
    { 
        if ((_previewSystem.GetPlayerActionCount() + _playerFinishedWithScreen) >= _players.Count) //wait until all the players have inputted before advancing
        {
            _tickElapsed = TickDuration;
        }
    }

    void Update()
    {
        if (!_tickIsOccuring || _currentGameState != GameState.Playing)
        {
            return;
        }

        _tickElapsed += Time.deltaTime;
        if (_tickElapsed >= TickDuration) 
        {
            StartCoroutine(SetupNewTick());
        }
    }

    IEnumerator SetupNewTick()
    {
        _tickIsOccuring = false;
        var tickEndDuration = _tickEndDuration;
        if (_currentScrambleType == GameInputProgression.SimpleMovement) //if we're not scrambled, speed up the animation between ticks because people know their next input will be
        {
            tickEndDuration /= 1.5f;
        }

        EndCurrentTick(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

        if (_energySystem.CurrentEnergy <= 0)
        {
            OnGameOver();
        }

        //something might have occured during the tick end where the game state changed from playing, so only start another tick if we're still playing
        if (_currentGameState == GameState.Playing)
        {
            StartNextTick();
        }
    }

    void EndCurrentTick(float tickEndDuration)
    {
        _previewSystem.ResolveAllPreviews(tickEndDuration);
        _lastTickEndedAt = GetCurrentTime();
        OnTickEnd?.Invoke(tickEndDuration, _ticksSinceScreenStart);
    }

    void StartNextTick()
    {
        _previewSystem.ClearAllPreviews();
        OnTickStart?.Invoke(TickDuration, _ticksSinceScreenStart);
        _tickIsOccuring = true;
        _ticksSinceScreenStart++;
        _tickElapsed = 0;
    }

    public float GetTimeRemainingInTick()
    {
        if (_currentGameState != GameState.Playing)
        {
            return 0f;
        }

        float timeRemaining = TickDuration - _tickElapsed;
        if (timeRemaining < 0)
        { 
            timeRemaining = 0;
        }
        return timeRemaining;
    }

    public float GetMsUntilNextTick()
    {
        var currentTime = GetCurrentTime();
        var timeSinceTickEnded = currentTime - _lastTickEndedAt;
        var timeUntilNextTick = _tickEndDuration - timeSinceTickEnded;
        return timeUntilNextTick;
    }

    float GetCurrentTime()
    { 
        return Time.timeSinceLevelLoad;
    }
}

public enum GameState
{ 
    Intro,
    Playing,
    Paused,
    Resetting,
    Transition,
    Cutscene,
    Dialogue,
    GameOver,
    Win
}
