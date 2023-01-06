using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("The player object")]                                           private GameObject player;
    [SerializeField][Tooltip("The audio mixer for changing volume")]                         private AudioMixer audioMixer;
    [Header("UI")]
    [SerializeField][Tooltip("The TMP dropbox to set screen resolution")]                    private TMP_Dropdown resolutionDropdownUI;
    [SerializeField][Tooltip("The TMP text to display a timer")]                             private TMP_Text timerText;
    [SerializeField][Tooltip("The XP slider ui element")]                                    private Slider experienceSlider;
    [SerializeField][Tooltip("The pause menu ui game object")]                               private GameObject pauseMenuUI;
    [Header("UI Upgrade")]
    [SerializeField][Tooltip("The upgrade menu ui game object")]                             private GameObject upgradeMenuUI;
    [SerializeField][Tooltip("The upgrade icon image")]                                      private Image upgradeIcon;
    [SerializeField][Tooltip("The upgrade title text")]                                      private TMP_Text upgradeTitle;
    [SerializeField][Tooltip("The upgrade description text")]                                private TMP_Text upgradeDescription;
    [SerializeField][Tooltip("The upgrade level requirement text")]                          private TMP_Text upgradeLevelRequirement;
    [SerializeField][Tooltip("The upgrade apply button")]                                    private Button upgradeApplyButton;
    [Header("UI Game Over")]
    [SerializeField][Tooltip("The game over menu ui game object")]                           private GameObject gameOverMenuUI;
    [SerializeField][Tooltip("The score TMP on the game over menu ui")]                      private TMP_Text highscoreTMPro;
    [SerializeField][Tooltip("The time TMP on the game over menu ui")]                       private TMP_Text timeTMPro;
    [SerializeField][Tooltip("The time unscaled TMP on the game over menu ui")]              private TMP_Text timeUnscaledTMPro;
    [Header("Game Logic")]
    [SerializeField][Range(10, 3600)][Tooltip("The starting time for countdown in seconds")] private int startTime = 600;
    [SerializeField][Tooltip("The minion container game object")]                            private Transform minionContainer;
    [SerializeField][Tooltip("A list of all minion prefabs")]                                private List<GameObject> allMinionPrefabs;
    [SerializeField][Range(1, 8)][Tooltip("The maximum number of clones for each minion")]   private int maxMinionClones = 3;
    //~ private
    private Resolution[] screenResolutions;
    private int countdown = 0;
    private double playTime = 0d;
    private double playTimeUnscaled = 0d;
    private uint level = 1;
    private ulong experience = 0;
    private List<List<GameObject>> minionsSpawned;
    private List<Upgrade> appliedUpgrades;
    private Collectible lastCollectible = null;
    //~ public
    [HideInInspector] public ulong enemiesDefeated = 0;
    //~ public (getter)
    public uint CurrentLevel => this.level;
    public ulong CurrentXP => this.experience;
    public ulong NextLevelXP => this.level * 1000;
    public ref GameObject Player => ref this.player;
    public ref AudioMixer AudioMixer => ref this.audioMixer;

    //~ reset time on start of game
    //~ why: timeScale is keeped across scenes so when exiting wile paused, time is paused when navigating back into the game !
    private void Awake(){ Time.timeScale = 1f; }

    private void Start(){
        //~ add screen resolution options if dropdownUI is available
        if(this.resolutionDropdownUI != null){
            this.screenResolutions = Screen.resolutions;
            HashSet<string> options = new HashSet<string>(this.screenResolutions.Length);
            int currentResolutionIndex = 0;
            for(int i = 0; i < this.screenResolutions.Length; i++){
                options.Add($"{this.screenResolutions[i].width} x {this.screenResolutions[i].height}");
                if(
                    this.screenResolutions[i].width == Screen.currentResolution.width
                    && this.screenResolutions[i].height == Screen.currentResolution.height
                ) currentResolutionIndex = i;
            }
            this.resolutionDropdownUI.ClearOptions();
            this.resolutionDropdownUI.AddOptions(options.ToList());
            this.resolutionDropdownUI.value = currentResolutionIndex;
            this.resolutionDropdownUI.RefreshShownValue();
        }
        //~ do not do anything else in MainMenu scene
        Scene scene = SceneManager.GetActiveScene();
        if(
            scene !=null
            && scene.name == "MainMenu"
        ) return;
        //~ initialize lists (in order !)
        this.minionsSpawned = new List<List<GameObject>>(this.allMinionPrefabs.Count);
        for(int i = 0; i < this.allMinionPrefabs.Count; i++){
            this.minionsSpawned.Add(new List<GameObject>(8));
            // TODO set only one minion as the start minion
            //! loop ↓↓ is only here because you start with all 4 minions already
            for(int j = 0; j < this.minionContainer.transform.childCount; j++){
                //~ compare names of the minions in scene and add them to the correct sublist of the minionsSpawned list
                if(
                    this.minionContainer.transform.GetChild(j) is Transform startingMinionTransform
                    && startingMinionTransform.name == this.allMinionPrefabs[i].name
                ) this.minionsSpawned[i].Add(startingMinionTransform.gameObject);
            }
        }
        StartCoroutine(this.Timer());
        this.playTime = Time.timeAsDouble;
        this.playTimeUnscaled = Time.unscaledTimeAsDouble;
    }

    /// <summary> Handles in-game timer (in 1 sec intervals) and triggers game over if it reaches 0. </summary>
    private IEnumerator Timer(){
        this.countdown = this.startTime;
        do{
            this.countdown--;
            this.timerText.text = $"Time: {System.TimeSpan.FromSeconds(this.countdown).ToString(@"hh\:mm\:ss")}";
            yield return new WaitForSeconds(1);
        }while(this.countdown > 0);
        this.OnGameOver();
    }
    /// <summary> Updates the XP slider UI element. </summary>
    private void UpdateExperienceSlider() => this.experienceSlider.value = -1f * (this.experience / this.NextLevelXP);

    //~ menu trigger
    /// <summary> Pauses the game, unlocks and shows cursor, and enables the pause menu. </summary>
    public void OnPause(){
        Time.timeScale = 0f;
        this.pauseMenuUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    /// <summary> Resumes the game, locks and hides cursor, and disables the pause menu. </summary>
    public void OnResumefromPause(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    /// <summary> Shows the upgrade screen </summary>
    /// <param name="collectible"> the collectible collected. </param>
    public void ShowCollectibleScreen(Collectible collectible){
        Time.timeScale = 0f;
        this.lastCollectible = collectible;
        //~ set UI fields
        this.upgradeIcon.sprite = collectible.Upgrade.Icon.sprite;
        this.upgradeTitle.text = collectible.Upgrade.UpgradeName;
        this.upgradeDescription.text = collectible.Upgrade.UpgradeDescription;
        this.upgradeLevelRequirement.text = $"Level\n{this.level} / {collectible.Upgrade.LevelRequirement}";
        this.upgradeApplyButton.interactable = this.level >= collectible.Upgrade.LevelRequirement;
        this.upgradeMenuUI.SetActive(true);
    }
    /// <summary> Closes the upgrade screen and if <paramref name="apply"/> is true applies upgrade and removes collectible. </summary>
    /// <param name="apply"> [Optional] If this upgrade should be applied. </param>
    public void CloseCollectibleScreen(bool apply = false){
        if(apply){
            this.ApplyUpgrade(this.lastCollectible.Upgrade);
            GameObject.Destroy(this.lastCollectible.gameObject);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.upgradeMenuUI.SetActive(false);
        this.lastCollectible = null;
        Time.timeScale = 1f;
    }
    /// <summary> Pauses the game, unlocks and shows cursor, and enables the game over menu. </summary>
    [ContextMenu("Trigger game over")]
    public void OnGameOver(){
        this.playTime = Time.timeAsDouble - this.playTime;
        this.playTimeUnscaled = Time.unscaledTimeAsDouble - this.playTimeUnscaled;
        Time.timeScale = 0f;
        decimal points = 0m;
        //~ calculate score
        points += this.enemiesDefeated * 2m;
        points += this.level * 420m;
        points += (this.NextLevelXP / this.experience) * 8m;
        points += this.player.GetComponent<PlayerManager>().GetHP * 69m;
        points += this.appliedUpgrades.Count * 32m;
        points -= (this.countdown / this.startTime) * 8m;
        if(points < 0m) points = 0m;
        //~ format and display score
        this.highscoreTMPro.text = $"Your score\n{points.ToString("N0")}";
        //~ format ans display time
        this.timeTMPro.text = $"Time Survived\n{System.TimeSpan.FromSeconds(this.playTime).ToString(@"hh\:mm\:ss\.ffff")}";
        this.timeUnscaledTMPro.text = $"Real Time\n{System.TimeSpan.FromSeconds(this.playTimeUnscaled).ToString(@"hh\:mm\:ss\.ffff")}";
        //~ show UI
        this.gameOverMenuUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //~ other game events
    /// <summary> Gives XP to the player. </summary>
    /// <param name="amount"> The amount of XP given. </param>
    public void AddExperience(uint amount){
        if((this.experience += amount) >= this.NextLevelXP){
            this.experience -= this.NextLevelXP;
            this.level++;
        }
        this.UpdateExperienceSlider();
    }
    /// <summary> For spawning the initial minion, every other minion, and clones of existing minions. </summary>
    /// <param name="index"> The index of the minion/clone to spawn, in <paramref name="allMinionPrefabs"/>. </param>
    public void SpawnMinion(int index){
        if(
            index < 0
            || index >= this.allMinionPrefabs.Count
            || this.minionsSpawned[index].Count >= this.maxMinionClones
        ) return;
        GameObject minion = GameObject.Instantiate<GameObject>(
            this.allMinionPrefabs[index],
            (
                this.minionsSpawned[index].Count == 0
                ? this.player.transform.position
                : this.minionsSpawned[index][0].transform.position
            ) + Vector3.forward,
            Quaternion.identity,
            this.minionContainer
        );
        //~ apply previous upgrades
        Minion minionScript = minion.GetComponent<Minion>();
        if(this.minionsSpawned[index].Count > 0) minionScript.CopyUpgradesFrom(this.minionsSpawned[index][0].GetComponent<Minion>());
        else foreach(Upgrade upgrade in this.appliedUpgrades){
            if(
                !upgrade.IsMinionUnlock
                && upgrade.MinionPrefabs.Contains(this.allMinionPrefabs[index])
            ) minionScript.ApplyUpgrade(upgrade);
        }
        this.minionsSpawned[index].Add(minion);
    }
    /// <summary> Applies the given upgrade. </summary>
    /// <param name="upgrade"> The upgrade to apply. </param>
    public void ApplyUpgrade(Upgrade upgrade){
        foreach(GameObject minionPrefab in upgrade.MinionPrefabs){
            int index = this.allMinionPrefabs.FindIndex(m => m == minionPrefab);
            if(upgrade.IsMinionUnlock) this.SpawnMinion(index);
            else foreach (GameObject minionSpawned in this.minionsSpawned[index]) minionSpawned.GetComponent<Minion>().ApplyUpgrade(upgrade);
        }
        this.appliedUpgrades.Add(upgrade);
    }

    //~ sceneloader
    /// <summary> Load the next scene of the build menu list. </summary>
    public void NextScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    /// <summary> Load the "MainMenu" scene. </summary>
    public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
    /// <summary> Exits the game. </summary>
    public void QuitGame() => Application.Quit();

    //~ audio/video settings
    /// <summary> Change the volume of the <paramref name="audioMixer"/>. </summary>
    /// <param name="volume"> The new volume level. </param>
    public void SetVolume(float volume) => this.audioMixer.SetFloat("volume", volume);
    /// <summary> Set the game to full screen or windowed mode. </summary>
    /// <param name="isFullScreen"> If set to true, sets the <paramref name="fullScreenMode"/> to <paramref name="FullScreenWindow"/> else to <paramref name="Windowed"/>. </param>
    public void SetFullScreen(bool isFullScreen = true){
        Screen.fullScreenMode = isFullScreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }
    /// <summary> Changes display resolution. </summary>
    /// <param name="resolutionIndex"> The index of the desired resolution in <paramref name="screenResolutions"/>. </param>
    public void SetResolution(int resolutionIndex){
        Resolution resolution = this.screenResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }
}
