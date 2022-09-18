using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour{
    [SerializeField][Tooltip("the player object")]public GameObject player;
    [SerializeField][Tooltip("the audio mixer for changing volume")]public AudioMixer audioMixer;

    [Header("UI")]
    [SerializeField][Tooltip("the TMP dropbox to set screen resolution")]private TMP_Dropdown resolutionDropdownUI;
    [SerializeField]private TMP_Text timerText;
    [SerializeField][Tooltip("the pause menu ui game object")]private GameObject pauseMenuUI;
    [SerializeField][Tooltip("the game over menu ui game object")]private GameObject gameOverMenuUI;
    [SerializeField][Tooltip("the score TMP on the game over menu ui")]private TMP_Text highscoreTMPro;
    [SerializeField][Tooltip("the upgrade menu game object")]public GameObject upgradeMenuUI;

    [Header("Game Logic")]
    [SerializeField][Tooltip("starting time for countdown in seconds")]private int startTime = 0;
    [SerializeField]private int level = 1;
    [SerializeField]private int experience = 0;
    [SerializeField]private Slider experienceSlider;
    [SerializeField][Tooltip("empty that has only minions")]private Transform minionContainer;
    [SerializeField][Tooltip("List of all minion prefabs")]List<GameObject> allMinionPrefabs;
    [SerializeField][Tooltip("maximum number of clones for each minion")]int maxMinionClones;

    public int NextLevelXP => this.level * 1000;
    public int CurrentXP => this.experience;

    private Resolution[] screenResolutions;
    private List<GameObject> minionsSpawned;
    private List<int> minionClonesSpawned;
    private List<List<Upgrade>> minionUpgrades;
    private List<HashSet<Upgrade>> upgradesCollected;
    private List<UpgradeItem> upgradeItems;
    private int countdown = 0;
    public TMP_Text[] upgradeText;
    public Image[] upgradeImage;

    //~ reset time on start of game
    private void Awake(){ Time.timeScale=1f; }

    private void Start(){
        //~ initialize lists (in order !)
        this.minionsSpawned = new List<GameObject>();
        this.minionClonesSpawned = new List<int>();
        this.minionUpgrades = new List<List<Upgrade>>();
        this.upgradesCollected = new List<HashSet<Upgrade>>();
        for(int i = 0; i < this.allMinionPrefabs.Count; i++){
            Minion minion = this.allMinionPrefabs[i].GetComponent<Minion>();
            this.minionUpgrades.Add(new List<Upgrade>(minion.attackBehaviour.upgrades));
            minion.attackBehaviour.upgrades.Clear();
            this.upgradesCollected.Add(new HashSet<Upgrade>());
            //~ only here because you start with all 4 minions already
            this.minionsSpawned.Add(this.allMinionPrefabs[i]);
            //~ 0 instead of -1 here because you start with all 4 minions already
            this.minionClonesSpawned.Add(0);
        }
        //~ add screen resolution options if dropdownUI is available
        if(this.resolutionDropdownUI != null){
            this.screenResolutions = Screen.resolutions;
            HashSet<string> options = new HashSet<string>();
            int currentResolutionIndex = 0;
            for(int i = 0; i < this.screenResolutions.Length; i++){
                options.Add($"{this.screenResolutions[i].width}x{this.screenResolutions[i].height}");
                if(
                    this.screenResolutions[i].width == Screen.currentResolution.width
                    && this.screenResolutions[i].height == Screen.currentResolution.height
                ) currentResolutionIndex = i;
            }
            this.resolutionDropdownUI.ClearOptions();
            this.resolutionDropdownUI.AddOptions(options.ToList());
            this.resolutionDropdownUI.value = currentResolutionIndex;
            this.resolutionDropdownUI.RefreshShownValue();
        }else StartCoroutine(this.Timer());
    }

    /// <summary> for spawning initial minion every other minion and new clones of existing minions </summary>
    /// <param name="index"> the index of the minion/clone to spawn in <paramref name="allMinionPrefabs"/> </param>
    public void SpawnMinion(int index){
        if(index < 0 || index >= this.allMinionPrefabs.Count) return;
        if(this.minionClonesSpawned[index] >= this.maxMinionClones) return;
        GameObject minion = Object.Instantiate<GameObject>(this.allMinionPrefabs[index], this.minionContainer);
        this.minionsSpawned[index] = minion;
        this.minionClonesSpawned[index] += 1;
        Minion minionScript = minion.GetComponent<Minion>();
        foreach(Upgrade upgrade in this.upgradesCollected[index])minionScript.attackBehaviour.ApplyUpgrade(upgrade);
    }
    /// <summary>
    ///     get n random upgrades or a minion spawn if that's still available, as UpgradeItem List
    ///     <br/> new minion spawn is always [0] if available
    ///     <br/> execution is probably a "little" slow
    /// </summary>
    /// <param name="amount"> the amount of random upgrades to get, can be less if not that many upgrades are available anymore </param>
    /// <returns> a list of random (available) upgrade items </returns>
    private List<UpgradeItem> GetRandomUpgradeList(int amount = 3){
        List<UpgradeItem> upgradeList = new List<UpgradeItem>();
        //~ minions (if still available)
        if(this.minionsSpawned.Count < this.allMinionPrefabs.Count){
            List<GameObject> remainingMinions = new List<GameObject>();
            foreach(GameObject minion in this.allMinionPrefabs){
                if(this.minionsSpawned.Contains<GameObject>(minion))continue;
                remainingMinions.Add(minion);
            }
            upgradeList.Add(new UpgradeItem(remainingMinions[Random.Range(0, remainingMinions.Count)]));
            remainingMinions.Clear();
            amount--;
        }
        //~ upgrades (if still available)
        List<List<Upgrade>> mu = new List<List<Upgrade>>();
        List<int> mui = new List<int>();
        List<int> left = new List<int>(this.minionsSpawned.Count);
        HashSet<Upgrade> got = new HashSet<Upgrade>();
        for(int i = 0; i < this.minionsSpawned.Count; i++){
            if(this.minionUpgrades[i].Count > 0){
                mui.Add(i);
                mu.Add(this.minionUpgrades[i]);
                left[i] = this.minionUpgrades[i].Count;
            }
        }
        int count = mu.Count;
        int randomIndex;
        for(int i = 0; amount > 0 && count > 0; i++){
            if(i >= mu.Count) i = 0;
            if(left[i] <= 0){
                count--;
                continue;
            }
            do randomIndex = Random.Range(0, mu[i].Count);
            while(got.Contains(mu[i][randomIndex]));
            got.Add(mu[i][randomIndex]);
            left[i]--;
            amount--;
            upgradeList.Add(new UpgradeItem(this.minionsSpawned[mui[i]],mu[i][randomIndex]));
        }
        return upgradeList;
    }

    private IEnumerator Timer(){
        this.countdown = this.startTime;
        do{
            this.countdown--;
            this.timerText.text = string.Format("Time: {0:00}:{1:00}", (this.countdown / 60) % 60, this.countdown % 60);
            yield return new WaitForSeconds(1);
        }while(this.countdown > 0);
        this.OnGameOver();
    }

    //~ menu trigger
    public void OnPause(){
        Time.timeScale=0f;
        this.pauseMenuUI.SetActive(true);
        Cursor.visible=true;
        Cursor.lockState=CursorLockMode.None;
    }
    public void OnResume(){
        Cursor.lockState=CursorLockMode.Locked;
        Cursor.visible=false;
        this.pauseMenuUI.SetActive(false);
        Time.timeScale=1f;
    }
    public void OnGameOver(){
        Time.timeScale = 0f;
        float points = 0;

        points += this.level * 420f;
        points += (this.NextLevelXP / this.experience) * 10;
        points += this.player.GetComponent<PlayerManager>().GetHP * 100;
        points -= (this.countdown / this.startTime) * 10;

        this.highscoreTMPro.text = $"Your score\n{(int)points}";
        this.gameOverMenuUI.SetActive(true);
        Cursor.visible=true;
        Cursor.lockState=CursorLockMode.None;
    }

    public void OnLevelUp(){
        Time.timeScale = 0f;
        this.experience = 0;
        this.level++;
        this.UpdateExperienceSlider();
        foreach (TMP_Text t in upgradeText)
        {
            t.text = null;
        }
        foreach (Image t in upgradeImage)
        {
            t.sprite = null;
        }


        this.upgradeMenuUI.SetActive(true);
        this.upgradeItems = this.GetRandomUpgradeList(3);
        for (int i = 0; i < upgradeItems.Count; i++)
        {
            upgradeText[i].text = upgradeItems[i].GetText;
            upgradeImage[i].sprite = upgradeItems[i].GetIcon.sprite;
        }
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseLevelUpMenu(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        this.upgradeMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    private void UpdateExperienceSlider(){
        this.experienceSlider.maxValue = this.NextLevelXP;
        this.experienceSlider.value = this.experience;
    }

    public void AddExperience(int amount){
        this.experience += amount;
        if (experience >= NextLevelXP)
        {
            OnLevelUp();
        }
        this.UpdateExperienceSlider();
    }
    /// <summary>
    ///     applyes upgrade after calling <paramref name="GetRandomUpgradeList"/> to initiate button list
    ///     <br/> this handles the button press, needs the correct index of the list and applies that upgrade
    /// </summary>
    /// <param name="pressedButtonID"> the index in <paramref name="upgradeItems"/> of the selected upgrade </param>
    public void ApplyUpgrade(int pressedButtonID){
        UpgradeItem upgradeItem = this.upgradeItems[pressedButtonID];
        if(upgradeItem.isUpgrade){
            int minionIndex = this.allMinionPrefabs.IndexOf(upgradeItem.GetMinion);
            upgradeItem.GetMinion.GetComponent<Minion>().attackBehaviour.ApplyUpgrade(upgradeItem.GetUpgrade);
            this.upgradesCollected[minionIndex].Add(upgradeItem.GetUpgrade);
            this.minionUpgrades[minionIndex].Remove(upgradeItem.GetUpgrade);
        }else this.SpawnMinion(this.allMinionPrefabs.IndexOf(upgradeItem.GetMinion));
    }
    // TODO public void CollectUpgrade(GameObject minionPrefab, Upgrade upgrade){/* TODO add upgrade to list if minionPrefab is in minion list ~ add minion ?! */}

    //~ sceneloader
    /// <summary> Load the next scene of the build menu list </summary>
    public void PlayGame(){ SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); }
    /// <summary> load the "MainMenu" scene </summary>
    public void LoadMainMenu(){ SceneManager.LoadScene("MainMenu"); }
    /// <summary> Exits the game </summary>
    public void QuitGame(){ Application.Quit(); }

    //~ audio/video settings
    /// <summary> change the volume of the <paramref name="audioMixer"/> </summary>
    /// <param name="volume"> the new volume level </param>
    public void SetVolume(float volume){ this.audioMixer.SetFloat("volume", volume); }
    /// <summary> Set the game to fullscreen </summary>
    /// <param name="isFullscreen"> set the fullscreen state based on this value </param>
    public void SetFullscreen(bool isFullscreen){ Screen.fullScreen = isFullscreen; }
    /// <summary> Changes resolution </summary>
    /// <param name="resolutionIndex"> changes resolution based on this value </param>
    public void SetResolution(int resolutionIndex){
        Resolution resolution = this.screenResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
