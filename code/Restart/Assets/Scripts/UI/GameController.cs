using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public enum UiState
    {
        DISABLED, MAIN, PAUSE, CONTROLS, SETTINGS, OTHER, SELECTION, SIGN, FINISH, CREDITS, VIDEO
    }
    [RequireComponent(typeof(PlayerInput))]
    public class GameController : MonoBehaviour
    {
        public float timeScale = 1f;
    
        private UiState uiState = UiState.DISABLED;
    
        private string MainMenu = "MainMenu";
        public static bool GameIsPaused = false;
        private PlayerInput playerInput;

        public GameObject pauseMenuUI;
        public GameObject settingsUI;
        public GameObject controlsUI;
        public GameObject mainMenuUI;
        public GameObject videoUI;
        public GameObject creditsUI;
        public CreditsScroll creditsScroll; 
        public GameObject levelSelectionMenuUI;
        public GameObject playerUI; // only disable on MainMenu. should still be seen in pause menu etc. 
        public GameObject finishUI;
        public GameObject signUI;
        public TextMeshProUGUI signText;

        public SignInteraction currentSign = null;
        public bool inGoal = false;

        public AudioSource finishReward;
        public AudioSource click;

        [SerializeField] protected LevelDataManager levelDataManager;
        [SerializeField] private LevelController levelController;
        [SerializeField] private VideoController videoController;

        private void Start()
        {
            Time.timeScale = timeScale;
            playerInput = GetComponent<PlayerInput>();
            if (SceneManager.GetActiveScene().name.Equals(MainMenu))
            {
                uiState = UiState.MAIN;
                playerInput.SwitchCurrentActionMap("UI");
                Time.timeScale = 0;
                if (playerUI != null) playerUI.SetActive(false);
            } 
            else
            {
                uiState = UiState.DISABLED;
                UpdateUi();
                if (playerUI != null) playerUI.SetActive(true);
            }
        }

        private void LoadMainMenu()
        {
            uiState = UiState.MAIN;
            UpdateUi();
            playerInput.SwitchCurrentActionMap("UI");
            
            levelDataManager.ApplySavedData();
        }

        public void Quit()
        {
            //Debug.Log("quitting game...");
            Application.Quit();
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMenu()
        {
            SceneManager.LoadScene(MainMenu);
            LoadMainMenu();
            Time.timeScale = timeScale;
        }

        public void LoadScene(string scene)
        {
            //Debug.Log("loading scene...");
            SceneManager.LoadScene(scene);
            playerInput.SwitchCurrentActionMap("Player");
        }

        public void OpenSettings()
        {
            uiState = UiState.SETTINGS;
            UpdateUi();
        }
    
        public void OpenControls()
        {
            uiState = UiState.CONTROLS;
            UpdateUi();
        }

        public void OpenSelection()
        {
            if (uiState == UiState.MAIN)
            {
                int introShown = PlayerPrefs.GetInt("introShown", 0);
                if (introShown == 0)
                {
                    PlayVideo();
                    PlayerPrefs.SetInt("introShown", 1);
                    return;    
                }
            }
            if (uiState == UiState.VIDEO) videoController.InterruptVideo();
            uiState = UiState.SELECTION;
            UpdateUi();
        }
    
        public void PauseGame(InputAction.CallbackContext context)
        {
            if (context.started && !SceneManager.GetActiveScene().name.Equals(MainMenu))
            {
                if (GameIsPaused)
                {
                    uiState = UiState.DISABLED;
                    UpdateUi();
                    Resume();
                }
                else
                {
                    uiState = UiState.PAUSE;
                    UpdateUi();
                    Pause();
                }
            }
        }

        public void Resume()
        {
            uiState = UiState.DISABLED;
            UpdateUi();
            //Debug.Log("resume");
            Time.timeScale = timeScale;
            GameIsPaused = false;
            playerInput.SwitchCurrentActionMap("Player");
            if (playerUI != null) playerUI.SetActive(true);
        }

        void Pause()
        {
            //Debug.Log("pause");
            Time.timeScale = 0f;
            GameIsPaused = true;
            playerInput.SwitchCurrentActionMap("UI");
            if (playerUI != null) playerUI.SetActive(false);
        }

        private void UpdateUi()
        {
            // set all false
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            if (settingsUI != null) settingsUI.SetActive(false);
            if (controlsUI != null) controlsUI.SetActive(false);
            if (mainMenuUI != null) mainMenuUI.SetActive(false);
            if (levelSelectionMenuUI != null) levelSelectionMenuUI.SetActive(false);
            if (signUI != null) signUI.SetActive(false);
            if (finishUI != null) finishUI.SetActive(false);
            if (creditsUI != null) creditsUI.SetActive(false);
            if (videoUI != null) videoUI.SetActive(false);
                //if (playerUI != null) playerUI.SetActive(false);
        
            switch (uiState)
            {
                case UiState.DISABLED:
                {
                    // deactivate all panels (already done)
                    break;
                }
                case UiState.MAIN:
                {
                    // activate main panel
                    if (mainMenuUI != null) mainMenuUI.SetActive(true);
                    break;
                }
                case UiState.PAUSE:
                {
                    if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
                    break;
                }
                case UiState.CONTROLS:
                {
                    if (controlsUI != null) controlsUI.SetActive(true);
                    break;
                }
                case UiState.SETTINGS:
                {
                    if (settingsUI != null) settingsUI.SetActive(true);
                    break;
                }
                case UiState.OTHER:
                {
                    break;
                }
                case UiState.SELECTION:
                {
                    if (levelSelectionMenuUI != null) levelSelectionMenuUI.SetActive(true);
                    levelDataManager.ApplySavedData();
                    break;
                }
                case UiState.SIGN:
                {
                    if (signUI != null) signUI.SetActive(true);
                    break;
                }
                case UiState.FINISH:
                {
                    if (finishUI != null) finishUI.SetActive(true);
                    break;
                }
                case UiState.CREDITS:
                {
                    if (creditsUI != null) creditsUI.SetActive(true);
                    break;
                }
                case UiState.VIDEO:
                {
                    if (videoUI != null) videoUI.SetActive(true);
                    break;
                }
            }
        }

        public void GoBackInHierarchy()
        {
            if (uiState == UiState.CREDITS)
            {
                creditsScroll.StopCredits();
            }
            if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
            {
                uiState = UiState.MAIN;
            }
            else
            {
                if (uiState == UiState.FINISH) return; 
                if (uiState != UiState.PAUSE && uiState != UiState.DISABLED && uiState != UiState.SIGN)
                {
                    uiState = UiState.PAUSE;
                }
                else
                {
                    uiState = UiState.DISABLED;
                    Resume();
                }
            }
            UpdateUi();
        }
        public void GoBackInHierarchy(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            GoBackInHierarchy();
        }

        public void ShowSign(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (currentSign == null) return;
        
            if (playerUI != null) playerUI.SetActive(false);
        
            // actually show sign
            playerInput.SwitchCurrentActionMap("UI");
            // assign text
            signText.text = currentSign.text;
        
            uiState = UiState.SIGN;
            UpdateUi();
        
            click.Play();
        }

        public void ShowFinishUI(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (inGoal == false) return;
        
            if (playerUI != null) playerUI.SetActive(false);
            playerInput.SwitchCurrentActionMap("UI");
            Time.timeScale = 0f;
            uiState = UiState.FINISH;
            UpdateUi();
            
            // apply collected objectives to levelData
            levelDataManager.SaveLevelData(levelController.currentLevel, levelController.objectives);
        
            finishReward.Play();
        
            click.Play();
        }

        public void OpenCredits()
        {
            uiState = UiState.CREDITS;
            UpdateUi();
            creditsScroll.ActivateCredits();
        }

        public void PlayVideo()
        {
            Debug.Log("play video");
            uiState = UiState.VIDEO;
            UpdateUi();
            videoController.PlayVideo();
        }
    }
}