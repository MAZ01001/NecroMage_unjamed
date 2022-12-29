using UnityEngine;

public class CameraConfig : MonoBehaviour{
    //~ inspector (private)
    // [Header("Wall cut out")]
    // [SerializeField] private Transform targetObject;
    // [SerializeField] private LayerMask wallMask;
    // [SerializeField] private float cutoutSize = 0.1f;
    // [SerializeField] private float transformSmoothTime = 0.5f;
    [Header("Aspect ratio")]
    [SerializeField][Tooltip("the horizontal width relative to the vertical height")] private float aspectHorizontal = 16f;
    [SerializeField][Tooltip("the vertical height relative to the horizontal width")] private float aspectVertical = 9f;
    //~ private
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;
    //~ unity methods (private)
    private void LateUpdate(){
        //~ only if screen size is different from last update
        if(
            this.lastScreenWidth != Screen.width
            || this.lastScreenHeight != Screen.height
        ){
            this.lastScreenWidth = Screen.width;
            this.lastScreenHeight = Screen.height;
            //~ calculate the relation of width and height of the given aspect ratio and the screen size
            //~ (ideally 1 | <1 higher than wide | >1 wider than height)
            float aspectWidthToHeight = (
                (this.aspectVertical * (float)this.lastScreenWidth)
                / (this.aspectHorizontal * (float)this.lastScreenHeight)
            );
            //~ get current render view
            Rect rect = Camera.main.rect;
            rect.size = Vector2.one;
            rect.position = Vector2.zero;
            //~ letterbox or pillarbox
            if(aspectWidthToHeight < 1f){
                rect.height = aspectWidthToHeight;
                rect.y = (1f - aspectWidthToHeight) * 0.5f;
            }else if(aspectWidthToHeight > 1f){
                float aspectHeightToWidth = 1f / aspectWidthToHeight;
                rect.width = aspectHeightToWidth;
                rect.x = (1f - aspectHeightToWidth) * 0.5f;
            }
            //~ override with modified render view
            Camera.main.rect = rect;
        }
    }

    /* TODO wall cut out camera with stencil buffer
        ! only render player, minions, and enemies above all ~ a dither effect to see where there at, but NOT like below ~
        ! because it is better visually
            Stealth Vision in Unity Shader Graph and Universal Render Pipeline
            https://youtu.be/eLIe95csKWE
        ///////////////////////////////////////////////////////////////////////////////////////////
        ? depth, anything that is more in direction Z+ is behind the player and in direction Z- is in front of the player and should be see through, if the player is not visible ~
        > if pos.Z < player.Z then have circle cutout
        > else not have circle cutout

        how it shuld be:
            See through walls shader | Under60sec Unity tutorial
            https://youtu.be/0rEF8A3wF9U

        more in deepth about stencils:
            Impossible Geometry with Stencil Shaders in Unity URP
            https://youtu.be/EzM8LGzMjmc?t=139

        ///////////////////////////////////////////////////////////////////////////////////////////
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
    */
}
