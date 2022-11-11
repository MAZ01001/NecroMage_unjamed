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
    [SerializeField][Tooltip("The player object")]                                    private GameObject player;
    [SerializeField][Tooltip("The audio mixer for changing volume")]                  private AudioMixer audioMixer;
    [Header("UI")]
    [SerializeField][Tooltip("The TMP dropbox to set screen resolution")]             private TMP_Dropdown resolutionDropdownUI;
    [SerializeField][Tooltip("The TMP text to display a timer")]                      private TMP_Text timerText;
    [SerializeField][Tooltip("The pause menu ui game object")]                        private GameObject pauseMenuUI;
    [SerializeField][Tooltip("The game over menu ui game object")]                    private GameObject gameOverMenuUI;
    [SerializeField][Tooltip("The score TMP on the game over menu ui")]               private TMP_Text highscoreTMPro;
    [SerializeField][Tooltip("The upgrade menu game object")]                         private GameObject upgradeMenuUI;
    [SerializeField][Tooltip("The XP slider ui element")]                             private Slider experienceSlider;
    [Header("Game Logic")]
    [SerializeField][Min(0)][Tooltip("The starting time for countdown in seconds")]   private int startTime = 10;
    [SerializeField][Tooltip("The minion container game object")]                     private Transform minionContainer;
    [SerializeField][Tooltip("A list of all minion prefabs")]                         private List<GameObject> allMinionPrefabs;
    [SerializeField][Min(0)][Tooltip("The maximum number of clones for each minion")] private int maxMinionClones;
    [SerializeField][Tooltip("A list of all available/collected upgrades")]           private List<Upgrade> allUpgrades;
    //~ private
    private uint level = 1;
    private ulong experience = 0;
    private Resolution[] screenResolutions;
    private List<List<GameObject>> minionsSpawned;
    private List<int> minionClonesSpawned;
    private int upgradesCollectedCount = 0;
    private int upgradesAppliedCount = 0;
    private List<UpgradeItem> lastUpgradeItems;
    private int countdown = 0;
    private double playTime = 0d;
    //~ public
    [HideInInspector] public ulong enemiesDefeated = 0;
    //~ public (getter)
    public ulong NextLevelXP => this.level * 1000;
    public ulong CurrentXP => this.experience;
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
        this.minionsSpawned = new List<List<GameObject>>();
        this.minionClonesSpawned = new List<int>(this.allMinionPrefabs.Count);
        for(int i = 0; i < this.allMinionPrefabs.Count; i++){
            this.minionsSpawned.Add(new List<GameObject>(this.maxMinionClones));
            //! line ↓↓ is only here because you start with all 4 minions already
            this.minionsSpawned[i].Add(this.allMinionPrefabs[i]);
            //! 0 instead of -1 here because you start with all 4 minions already
            this.minionClonesSpawned.Add(0);
        }
        this.upgradesCollectedCount = this.allUpgrades.Count;
        StartCoroutine(this.Timer());
        this.playTime = Time.realtimeSinceStartupAsDouble;
    }

    /// <summary> Handles in-game timer (in 1 sec intervals) and triggers game over if it reaches 0. </summary>
    private IEnumerator Timer(){
        this.countdown = this.startTime;
        do{
            this.countdown--;
            this.timerText.text = string.Format("Time: {0:00}:{1:00}", (this.countdown / 60) % 60, this.countdown % 60);
            yield return new WaitForSeconds(1);
        }while(this.countdown > 0);
        this.OnGameOver();
    }
    /// <summary> Updates the XP slider UI element. </summary>
    private void UpdateExperienceSlider(){
        this.experienceSlider.maxValue = this.NextLevelXP;
        this.experienceSlider.value = this.experience;
    }
    /// <summary>
    ///     Get <paramref name="amount"/> random upgrades or a minion spawn if that's still available, as <paramref name="UpgradeItem"/> List.
    ///     <br/>New minion spawn is always the first element, if it is available.
    ///     <br/>Execution is probably a "little" slow.
    /// </summary>
    /// <param name="amount"> The amount of random upgrades to get, can be less if not that many upgrades are available anymore. </param>
    /// <returns> A list of random (available) upgrade items. </returns>
    private List<UpgradeItem> GetRandomUpgradeList(int amount = 3){
        List<UpgradeItem> upgradeList = new List<UpgradeItem>(this.allUpgrades.Count);
        //~ minions (if still available)
        if(this.minionsSpawned.Count < this.allMinionPrefabs.Count){
            List<GameObject> remainingMinions = new List<GameObject>(this.allMinionPrefabs.Count);
            // foreach(GameObject minion in this.allMinionPrefabs){
            for(int i = 0; i < this.allMinionPrefabs.Count; i++){
                if(this.minionsSpawned[i].Count > 0) continue;
                remainingMinions.Add(this.allMinionPrefabs[i]);
            }
            upgradeList.Add(new UpgradeItem(remainingMinions[Random.Range(0, remainingMinions.Count)]));
            remainingMinions.Clear();
            amount--;
        }
        //~ upgrades (if still available)
        int count = this.allUpgrades.Count;
        int randomIndex;
        HashSet<int> got = new HashSet<int>(count);
        for(int i = 0; amount > 0 && count > 0; i++){
            do randomIndex = Random.Range(0, this.allUpgrades.Count);
            while(got.Contains(randomIndex));
            count--;
            amount--;
            upgradeList.Add(new UpgradeItem(this.allUpgrades[randomIndex]));
            got.Add(randomIndex);
        }
        return upgradeList;
    }

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
    /// <summary> Pauses the game, unlocks and shows cursor, and enables the level up menu. </summary>
    [ContextMenu("Trigger level up")]
    public void OnLevelUp(){
        Time.timeScale = 0f;
        this.experience = 0;
        this.level++;
        this.UpdateExperienceSlider();
        //~ reset update panels
        this.upgradeMenuUI.SetActive(true);
        //~ this may take some time
        this.lastUpgradeItems = this.GetRandomUpgradeList(3);
        //~ fill update panels
        /* TODO Level up UI elements
            remake this with a prefab UI element
            and auto group/align UI canvas script
                Unity UI Tutorial - Layout Groups
                https://youtu.be/RWcwEAILOCA?t=80
        */
        List<TMP_Text> upgradeText = new List<TMP_Text>(this.lastUpgradeItems.Count);
        List<Image> upgradeImage = new List<Image>(this.lastUpgradeItems.Count);
        for(int i = 0; i < this.lastUpgradeItems.Count; i++){
            upgradeText[i].text = this.lastUpgradeItems[i].GetText;
            upgradeImage[i].sprite = this.lastUpgradeItems[i].GetIcon.sprite;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    /// <summary> Resumes the game, locks and hides cursor, and disables the level up menu. </summary>
    public void OnResumeFromLevelUp(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.upgradeMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    /// <summary> Pauses the game, unlocks and shows cursor, and enables the game over menu. </summary>
    [ContextMenu("Trigger game over")]
    public void OnGameOver(){
        this.playTime = Time.realtimeSinceStartupAsDouble - this.playTime;
        Time.timeScale = 0f;
        decimal points = 0m;
        //~ calculate score
        points += this.enemiesDefeated * 2m;
        points += this.level * 420m;
        points += (this.NextLevelXP / this.experience) * 8m;
        points += this.player.GetComponent<PlayerManager>().GetHP * 69m;
        points += this.upgradesCollectedCount * 16m;
        points += this.upgradesAppliedCount * 32m;
        points -= (this.countdown / this.startTime) * 8m;
        if(points < 0m) points = 0m;
        //~ format and display score
        this.highscoreTMPro.text = $"Your score\n{points.ToString("N0")}";
        // TODO show unscaled time ~ for speedruns
        Debug.Log($"time survived: {this.playTime} seconds");
        this.gameOverMenuUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //~ other game events
    /// <summary> Gives XP to the player. </summary>
    /// <param name="amount"> The amount of XP given. </param>
    public void AddExperience(uint amount){
        this.experience += amount;
        if(experience >= NextLevelXP) this.OnLevelUp();
        this.UpdateExperienceSlider();
    }
    /// <summary> For spawning the initial minion, every other minion, and clones of existing minions. </summary>
    /// <param name="index"> The index of the minion/clone to spawn, in <paramref name="allMinionPrefabs"/>. </param>
    public void SpawnMinion(int index){
        if(
            index < 0
            || index >= this.allMinionPrefabs.Count
        ) return;
        if(this.minionClonesSpawned[index] >= this.maxMinionClones) return;
        GameObject minion = Object.Instantiate<GameObject>(
            this.allMinionPrefabs[index],
            this.player.transform.position
            + Vector3.forward,
            Quaternion.identity,
            this.minionContainer
        );
        this.minionsSpawned[index].Add(minion);
        this.minionClonesSpawned[index] += 1;
        minion.GetComponent<Minion>().AttackBehaviour.CopyValuesFrom(this.minionsSpawned[index][0].GetComponent<Minion>().AttackBehaviour);
    }
    /// <summary>
    ///     Applyes upgrade after calling <paramref name="GetRandomUpgradeList"/> to initiate button list.
    ///     <br/>This handles the button press, needs the correct index of the list and applies that upgrade.
    /// </summary>
    /// <param name="pressedButtonID"> The index in <paramref name="lastUpgradeItems"/> of the selected upgrade. </param>
    public void ApplyUpgrade(int pressedButtonID){
        UpgradeItem selectedUpgradeItem = this.lastUpgradeItems[pressedButtonID];
        if(selectedUpgradeItem.GetIsUpgrade){
            int minionIndex = this.allMinionPrefabs.IndexOf(selectedUpgradeItem.GetUpgrade.minionPrefab);
            foreach(GameObject minionSpawned in this.minionsSpawned[minionIndex])minionSpawned.GetComponent<Minion>().AttackBehaviour.ApplyUpgrade(selectedUpgradeItem.GetUpgrade);
            this.upgradesAppliedCount++;
            this.allUpgrades.Remove(selectedUpgradeItem.GetUpgrade);
        }else this.SpawnMinion(this.allMinionPrefabs.IndexOf(selectedUpgradeItem.GetMinionPrefab));
    }
    /// <summary>
    ///     Adds the given <paramref name="upgrade"/> to the collection.
    ///     <br/>This does not apply the update, just makes it available for future level ups.
    /// </summary>
    /// <param name="upgrade"> the upgrade collected. </param>
    public void CollectUpgrade(Upgrade upgrade){
        throw new System.NotImplementedException("[GameManager : CollectUpgrade] this function is not implemented (yet).");
        // TODO add upgrade to list if minionPrefab is in minion list ~ add minion ?!
        // upgrade.minionPrefab (upgrade is for this minion)
        //! if in this.allMinionPrefabs (get index)
        //! add to this.minionUpgrades (use index)
        // this.upgradesCollectedCount++;
    }

    //~ sceneloader
    /// <summary> Load the next scene of the build menu list. </summary>
    public void NextScene(){ SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); }
    /// <summary> Load the "MainMenu" scene. </summary>
    public void LoadMainMenu(){ SceneManager.LoadScene("MainMenu"); }
    /// <summary> Exits the game. </summary>
    public void QuitGame(){ Application.Quit(); }

    //~ audio/video settings
    /// <summary> Change the volume of the <paramref name="audioMixer"/>. </summary>
    /// <param name="volume"> The new volume level. </param>
    public void SetVolume(float volume){ this.audioMixer.SetFloat("volume", volume); }
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
