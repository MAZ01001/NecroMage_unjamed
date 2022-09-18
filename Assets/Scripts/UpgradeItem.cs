using UnityEngine;
using UnityEngine.UI;

public class UpgradeItem{
    public bool isUpgrade{get; private set;}

    private Image icon;
    private string text;
    private GameObject newMinion = null;
    private Upgrade newUpgrade = null;

    public Image GetIcon => this.icon;
    public string GetText => this.text;
    public GameObject GetMinion => this.newMinion;
    public Upgrade GetUpgrade => this.newUpgrade;

    public UpgradeItem(GameObject minion){
        this.isUpgrade = false;
        this.newMinion = minion;
        Minion minionScript = minion.GetComponent<Minion>();
        if(minionScript.displayName == "") this.text = minion.gameObject.name;
        else this.text = minionScript.displayName;
        if(minionScript.displayDescription != "")this.text += $"\n\n{minionScript.displayDescription}";
    }
    public UpgradeItem(GameObject minion, Upgrade upgrade){
        this.isUpgrade = true;
        this.newMinion = minion;
        this.newUpgrade = upgrade;
        this.icon = upgrade.icon;
        this.text = upgrade.upgradeName;
        if(upgrade.upgradeDescription != "")this.text += $"\n\n{upgrade.upgradeDescription}";
    }
}
