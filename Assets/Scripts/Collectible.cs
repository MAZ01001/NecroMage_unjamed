using UnityEngine;

public class Collectible : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("the game manager object")] private GameManager gameManager;
    [SerializeField][Tooltip("The upgrade to collect")]  private Upgrade upgrade;
    //~ public (getter)
    public Upgrade Upgrade => this.upgrade;

    private void OnTriggerEnter(Collider other){
        if(other.gameObject == this.gameManager.Player){
            this.gameManager.ShowCollectibleScreen(this);
        }
    }
}
