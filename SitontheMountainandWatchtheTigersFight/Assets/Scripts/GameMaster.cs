using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class GameMaster : MonoBehaviour
{
    // Players
    public PlayerManager Player1 { get; set; }
    public PlayerManager Player2 { get; set; }

    private EventSystem eventSystem;

    // Shared Game Attributes
    private float initialTime;
    public float TimeLimit { get; set; }
    public bool TimerOn { get; set; }

    public int stock;
    public bool StockOn { get; set; }

    private bool isPaused;

    private bool gameOn;

    // Camera
    public Camera mainCamera;
    public Camera p1Camera;
    public Camera p2Camera;

    // UI Variables
    public GameObject titleUI;
    public GameObject gameUI;

    public GameObject timedUI;
    public GameObject stockUI;

    public GameObject pauseUI;
    public GameObject controlsUI;
    public GameObject optionsUI;
    public GameObject winnerUI;

    public Text winnerNameLabel;
    public Text winnerNameShadowLabel;
    public Text winnerKillsLabel;
    public Text winnerKillsShadowLabel;
    public Text WinnerDeathsLabel;
    public Text WinnerDeathsShadowLabel;

    public Text timerLabel;
    public Text timerShadowLabel;

    public Text p1NameLabel;
    public Text p2NameLabel;

    public Text p1StatLabel;
    public Text p1StatShadowLabel;

    public Text p2StatLabel;
    public Text p2StatShadowLabel;

    // Audio Variables
    [SerializeField]
    [Range(0f, 1f)]
    public float intensity;
    public float Intensity { get { return intensity; } set { intensity = value; } }

    private static EventInstance musicSource;
    private static EventInstance soundEffectsSource;
    private static EventInstance ambienceSource;
    private static EventInstance dialogueSource;

    [FMODUnity.EventRef]
    public string menuMusicPath;
    [FMODUnity.EventRef]
    public string fightMusicPath;
    [FMODUnity.EventRef]
    public string victoryMusicPath;

    [FMODUnity.EventRef]
    public string gameStartPath;
    [FMODUnity.EventRef]
    public string gameEndPath;
    [FMODUnity.EventRef]
    public string winnerIsPath;

    [FMODUnity.EventRef]
    public string gongPath;
    [FMODUnity.EventRef]
    public string menuSelectPath;

    [FMODUnity.EventRef]
    public string mountinAmbiencePath;
    [FMODUnity.EventRef]
    public string cheerAmbiencePath;

    // Use this for initialization
    void Start()
    {
        // Setting up Cameras
        mainCamera.enabled = true;
        p1Camera.enabled = false;
        p2Camera.enabled = false;

        //Audio
        PlayMenuMusic();
        PlayMountainAmbience();

        Intensity = 0;

        // Initializing players
        Player1 = GameObject.Find("Player1").GetComponent<PlayerManager>();
        Player2 = GameObject.Find("Player2").GetComponent<PlayerManager>();

        Player1.Name = "Gero";
        Player2.Name = "Kero";

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        // Sets up Title Screen
        Cursor.lockState = CursorLockMode.Locked;
        PlayMenuMusic();

        titleUI.SetActive(true);
        optionsUI.SetActive(false);
        gameUI.SetActive(false);
        pauseUI.SetActive(false);
        controlsUI.SetActive(false);
        winnerUI.SetActive(false);

        Time.timeScale = 0.0f;
        TimerOn = false;

        isPaused = false;
        gameOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Dynamically updates intensity parameter in FMOD
        musicSource.setParameterByName("Intensity", Intensity);

        // Transition from title to game
        if (titleUI.activeSelf && Input.anyKey)
        {
            titleUI.SetActive(false);


            NewGame(stock, TimeLimit);
        }

        if (gameOn)
        {
            // Updates timer if timer is on
            if (TimerOn)
            {
                UpdateTimer();

                // Checks for timer endgame
                if (TimeLimit <= 0)
                {
                    TimeLimit = 0;
                    TimerOn = false;
                    Time.timeScale = 0.2f;

                    Invoke("EndGame", 0.5f);
                }
            }

            // Checks for stock endgame
            if (StockOn && (Player1.Stock == 0 || Player2.Stock == 0))
            {
                Time.timeScale = 0.2f;
                PlayGameSound();
                gameOn = false;
                Invoke("EndGame", 0.5f);
            }

            // Transistion from game to pause menu
            if (!isPaused && gameUI.activeSelf && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)))
            {
                PlayGongNoise();
                Pause();
            }

            if (isPaused)
            {
                // If right or left bumper are pushed, camera zooms in on players
                if ((Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton4))
                    || (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad2)))
                {
                    if (mainCamera.enabled)
                    {
                        mainCamera.enabled = !mainCamera.enabled;
                        p1Camera.enabled = !p1Camera.enabled;
                    }
                    else
                    {
                        p1Camera.enabled = !p1Camera.enabled;
                        p2Camera.enabled = !p2Camera.enabled;
                    }
                }
            }
        }

        if (winnerUI.activeSelf && (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0)))
        {
            winnerUI.SetActive(false);
            gameOn = false;

            titleUI.SetActive(true);
            PlayMountainAmbience();
            PlayMenuMusic();
        }
    }

    // Sets up a new game
    public void NewGame(int stockNum, float setTime)
    {
        Intensity = 0;
        stock = stockNum;

        Player1.Kills = 0;
        Player2.Kills = 0;

        Player1.Stock = stock;
        Player2.Stock = stock;

        mainCamera.enabled = true;
        p1Camera.enabled = false;
        p2Camera.enabled = false;

        if (stock > 0)
        {
            UpdateStats();
            TimeLimit = 0;

            StockOn = true;

            stockUI.SetActive(true);
            timedUI.SetActive(false);
        }
        else
        {
            UpdateStats();
            stock = 0;

            TimeLimit = (setTime) * 60;
            TimerOn = true;

            stockUI.SetActive(false);
            timedUI.SetActive(true);
        }

        Player1.Spawn();
        Player2.Spawn();

        // Transitioning to gamestate
        gameUI.SetActive(true);

        Time.timeScale = 0.3f;

        StartCoroutine(CountDown());

        gameOn = true;
    }

    // Checks for the winner when game ends
    public void EndGame()
    {
        Intensity = 0;
        Time.timeScale = 0f;

        gameUI.SetActive(false);
        winnerUI.SetActive(true);

        // Stock battle
        if (StockOn)
        {
            // Player 1 win
            if (Player1.Stock > Player2.Stock)
            {
                mainCamera.enabled = false;
                p1Camera.enabled = true;
                VictoryCircle();

                winnerNameLabel.text = Player1.Name;
                winnerNameShadowLabel.text = Player1.Name;

                winnerKillsLabel.text = stock.ToString();
                winnerKillsShadowLabel.text = stock.ToString();

                WinnerDeathsLabel.text = (stock - Player1.Stock).ToString();
                WinnerDeathsShadowLabel.text = (stock - Player1.Stock).ToString();
            }
            // Player 2 win
            else
            {
                mainCamera.enabled = false;
                p2Camera.enabled = true;
                VictoryCircle();

                winnerNameLabel.text = Player2.Name;
                winnerNameShadowLabel.text = Player2.Name;

                winnerKillsLabel.text = stock.ToString();
                winnerKillsShadowLabel.text = stock.ToString();

                WinnerDeathsLabel.text = (stock - Player2.Stock).ToString();
                WinnerDeathsShadowLabel.text = (stock - Player2.Stock).ToString();
            }
        }
        // Timed battle
        else
        {
            // Tie
            if (Player1.Kills == Player2.Kills)
            {
                //tieWinScreen.SetActive(True);
            }
            // Player 1 win
            else if (Player1.Kills > Player2.Kills)
            {
                mainCamera.enabled = false;
                p1Camera.enabled = true;
                VictoryCircle();

                winnerNameLabel.text = Player1.Name;
                winnerNameShadowLabel.text = Player1.Name;

                winnerKillsLabel.text = Player1.Kills.ToString();
                winnerKillsShadowLabel.text = Player1.Kills.ToString();

                WinnerDeathsLabel.text = Player2.Kills.ToString();
                WinnerDeathsShadowLabel.text = Player2.Kills.ToString();
            }
            // Player 2 win
            else
            {
                mainCamera.enabled = false;
                p2Camera.enabled = true;

                VictoryCircle();

                winnerNameLabel.text = Player2.Name;
                winnerNameShadowLabel.text = Player2.Name;

                winnerKillsLabel.text = Player2.Kills.ToString();
                winnerKillsShadowLabel.text = Player2.Kills.ToString();

                WinnerDeathsLabel.text = Player1.Kills.ToString();
                WinnerDeathsShadowLabel.text = Player1.Kills.ToString();
            }
        }
    }

    // Pause settings
    public void Pause()
    {
        isPaused = true;

        Time.timeScale = 0.0f;
        musicSource.setVolume(0.3f);
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(HighlightBtn());

        gameUI.SetActive(false);
        pauseUI.SetActive(true);
    }

    // Resumes game
    public void Resume()
    {
        PlayGongNoise();
        isPaused = false;

        p1Camera.enabled = false;
        p2Camera.enabled = false;
        mainCamera.enabled = true;

        pauseUI.SetActive(false);
        gameUI.SetActive(true);

        Time.timeScale = 1.0f;
        musicSource.setVolume(1f);
    }

    // Toggles controls screen - Off by default
    public void ToggleControls()
    {
        controlsUI.SetActive(!controlsUI.activeSelf);
        pauseUI.SetActive(!pauseUI.activeSelf);

        if (controlsUI.activeSelf)
        {
            GameObject.Find("Return").GetComponent<Button>().Select();
        }
        else
        {
            StartCoroutine(HighlightBtn());
        }
    }

    // Updates timer
    public void UpdateTimer()
    {
        if (TimerOn)
            TimeLimit -= Time.deltaTime;
        Intensity = (initialTime - TimeLimit) / initialTime;


        var minutes = TimeLimit / 60; //Divide the guiTime by sixty to get the minutes.
        var seconds = TimeLimit % 60;//Use the euclidean division for the seconds.
        var fraction = (TimeLimit * 100) % 100;

        if (minutes < 0 && seconds < 0 && fraction <= 0)
        {
            fraction = 0;
        }

        // Formats and updates timer label
        if (minutes >= 10)
        {
            timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerShadowLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else if (minutes < 10 && minutes >= 1)
        {
            timerLabel.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            timerShadowLabel.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
        else if (minutes < 0 && seconds >= 10)
        {
            timerLabel.text = string.Format("{0:00}.{1:00}", seconds, fraction);
            timerShadowLabel.text = string.Format("{0:00}.{1:00}", seconds, fraction);
        }
        else
        {
            timerLabel.text = string.Format("{0:0}.{1:00}", seconds, fraction);
            timerShadowLabel.text = string.Format("{0:0}.{1:00}", seconds, fraction);
        }
    }

    IEnumerator HighlightBtn()
    {
        eventSystem.SetSelectedGameObject(null);
        PlayMenuSelectNoise();
        yield return null;
        eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
    }

    public void UpdateStats()
    {
        if (TimerOn)
        {
            p1StatLabel.text = Player1.Kills.ToString();
            p1StatShadowLabel.text = Player1.Kills.ToString();

            p2StatLabel.text = Player2.Kills.ToString();
            p2StatShadowLabel.text = Player2.Kills.ToString();
        }
        else if (Player1.Stock > 0 && Player2.Stock > 0)
        {
            p1StatLabel.text = Player1.Stock.ToString();
            p1StatShadowLabel.text = Player1.Stock.ToString();

            p2StatLabel.text = Player2.Stock.ToString();
            p2StatShadowLabel.text = Player2.Stock.ToString();
        }
    }
    public void Quit()
    {
        Application.Quit();
    }

    // AUDIO
    public void VictoryCircle()
    {
        PlayWinnerIsSound();
        PlayCheerAmbience();
        PlayVictoryMusic();
    }
    //Music
    public void PlayGameMusic()
    {
        musicSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicSource.release();
        musicSource = FMODUnity.RuntimeManager.CreateInstance(fightMusicPath);
        musicSource.start();
    }
    public void PlayMenuMusic()
    {
        musicSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicSource.release();
        musicSource = FMODUnity.RuntimeManager.CreateInstance(menuMusicPath);
        musicSource.start();
    }
    public void PlayVictoryMusic()
    {
        musicSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicSource.release();
        musicSource = FMODUnity.RuntimeManager.CreateInstance(victoryMusicPath);
        musicSource.start();
    }
    // Dialogue
    public void PlayStartSound()
    {
        dialogueSource = FMODUnity.RuntimeManager.CreateInstance(gameStartPath);
        dialogueSource.start();
        dialogueSource.release();
    }
    public void PlayGameSound()
    {
        dialogueSource = FMODUnity.RuntimeManager.CreateInstance(gameEndPath);
        dialogueSource.start();
        dialogueSource.release();
    }
    public void PlayWinnerIsSound()
    {
        dialogueSource = FMODUnity.RuntimeManager.CreateInstance(winnerIsPath);
        dialogueSource.start();
        dialogueSource.release();
    }

    // Sound Effects
    public void PlayGongNoise()
    {
        soundEffectsSource = FMODUnity.RuntimeManager.CreateInstance(gongPath);
        soundEffectsSource.start();
        soundEffectsSource.release();
    }
    public void PlayMenuSelectNoise()
    {
        soundEffectsSource = FMODUnity.RuntimeManager.CreateInstance(menuSelectPath);
        soundEffectsSource.start();
        soundEffectsSource.release();
    }

    // Ambience
    public void PlayMountainAmbience()
    {
        ambienceSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ambienceSource.release();
        ambienceSource = FMODUnity.RuntimeManager.CreateInstance(mountinAmbiencePath);
        ambienceSource.start();
    }
    public void PlayCheerAmbience()
    {
        ambienceSource.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ambienceSource.release();
        ambienceSource = FMODUnity.RuntimeManager.CreateInstance(cheerAmbiencePath);
        ambienceSource.start();
    }

    // Game start
    IEnumerator CountDown()
    {
        PlayStartSound();

        yield return new WaitForSeconds(0.475f);

        PlayGameMusic();
        PlayMountainAmbience();
        Time.timeScale = 1.0f;
    }
}