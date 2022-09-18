using UnityEngine;

public class CutoutObject : MonoBehaviour{
    [SerializeField]private Transform targetObject;
    [SerializeField]private LayerMask wallMask;
    [SerializeField]private float cutoutSize = 0.1f;
    [SerializeField]private float fallofSize = 0.05f;

    private Camera mainCamera;

    private void Awake(){ this.mainCamera = this.GetComponent<Camera>(); }

    private void LateUpdate(){
        Vector2 cutoutPos = this.mainCamera.WorldToViewportPoint(this.targetObject.position);
        cutoutPos.y = (cutoutPos.y * Screen.height) / Screen.width;

        Vector3 offset = this.targetObject.position - this.transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(
            this.transform.position,
            offset,
            offset.sqrMagnitude,
            this.wallMask
        );

        for(int i = 0; i < hitObjects.Length; i++){
            Renderer renderer = hitObjects[i].transform.GetComponent<Renderer>();
            if(renderer == null)continue;
            Material[] materials = renderer.materials;

            for(int m = 0; m < materials.Length; m++){
                materials[m].SetVector("_CutoutPos", cutoutPos);
                materials[m].SetFloat("_CutoutSize", this.cutoutSize);
                materials[m].SetFloat("_FalloffSize", this.fallofSize);
            }
        }
    }
}
