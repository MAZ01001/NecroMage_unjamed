using UnityEngine;

public class ChunkLoader : MonoBehaviour{
    [SerializeField][Tooltip("the distance at witch to load/unload terrain chuncks")]private float loadDistance = 200f;
    [SerializeField][Tooltip("the target from witch the load distance is measured")]private Transform target;

    private void LateUpdate(){
        for(int i = 0; i < this.transform.childCount; i++){
            Transform terrain = this.transform.GetChild(i);
            terrain.gameObject.SetActive(
                (this.target.position - terrain.position).sqrMagnitude
                <= (this.loadDistance * this.loadDistance)
            );
        }
    }

    /// <summary> [debug] if selected draws range </summary>
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.target.position, this.loadDistance);
    }
}
