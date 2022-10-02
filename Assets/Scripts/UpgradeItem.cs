using UnityEngine;
using UnityEngine.UI;

public class UpgradeItem{
    //~ private
    private bool isUpgrade;
    private Image icon;
    private string text;
    private GameObject newMinionPrefab = null;
    private Upgrade newUpgrade = null;
    //~ public (getter)
    public bool GetIsUpgrade => this.isUpgrade;
    public ref Image GetIcon => ref this.icon;
    public string GetText => this.text;
    public ref GameObject GetMinionPrefab => ref this.newMinionPrefab;
    public ref Upgrade GetUpgrade => ref this.newUpgrade;

    /// <summary> Creates an upgrade item for level up menu UI </summary>
    /// <param name="minionPrefab"> The minion (prefab) to unlock </param>
    public UpgradeItem(GameObject minionPrefab){
        this.isUpgrade = false;
        this.newMinionPrefab = minionPrefab;
        Minion minionScript = minionPrefab.GetComponent<Minion>();
        if(minionScript.DisplayName == "") this.text = minionPrefab.gameObject.name;
        else this.text = minionScript.DisplayName;
        if(minionScript.DisplayDescription != "")this.text += $"\n\n{minionScript.DisplayDescription}";
    }
    /// <summary> Creates an upgrade item for level up menu UI </summary>
    /// <param name="minionPrefab"> The upgrade to get/apply </param>
    public UpgradeItem(Upgrade upgrade){
        this.isUpgrade = true;
        this.newUpgrade = upgrade;
        this.icon = upgrade.icon;
        this.text = upgrade.upgradeName;
        if(upgrade.upgradeDescription != "")this.text += $"\n\n{upgrade.upgradeDescription}";
    }
}
