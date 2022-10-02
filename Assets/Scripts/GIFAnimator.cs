using System.Collections.Generic;
using UnityEngine;

public class GIFAnimator : MonoBehaviour{
    [System.Serializable]
    public struct GIFAnimation{
        //~ inspector (public)
        [SerializeField][Tooltip("The material for the gif animation")]                 public Material gifMaterial;
        [SerializeField][Tooltip("The frames of the original gif file as 2d textures")] public List<Texture2D> frames;
        [SerializeField][Tooltip("The amount of frames drawn per second")]              public float fps;
    }
    //~ inspector (private)
    [SerializeField][Tooltip("GIF animations to animate in-game")] private List<GIFAnimation> gifAnimations;
    //~ private
    private List<float> frameTimer;

    private void Start(){
        this.frameTimer = new List<float>();
        for(int i = 0; i < this.gifAnimations.Count; i++) this.frameTimer.Add(0f);
    }

    private void Update(){
        for(int i = 0; i < this.gifAnimations.Count; i++){
            this.frameTimer[i] += (Time.deltaTime * this.gifAnimations[i].fps);
            if(this.frameTimer[i] >= this.gifAnimations[i].frames.Count) this.frameTimer[i] -= this.gifAnimations[i].frames.Count;
            this.gifAnimations[i].gifMaterial.mainTexture = this.gifAnimations[i].frames[(int)this.frameTimer[i]];
        }
    }
}
