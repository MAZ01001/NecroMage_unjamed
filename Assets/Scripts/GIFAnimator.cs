using System.Collections.Generic;
using UnityEngine;

public class GIFAnimator : MonoBehaviour{
    [System.Serializable]
    public struct GIFAnimation{
        /// <summary> The material for the gif animation </summary>
        public Material gifMaterial;
        /// <summary> The frames of the original gif file as 2d textures </summary>
        public List<Texture2D> frames;
        /// <summary> The amount of frames drawn per second </summary>
        public float fps;

        /// <summary> Creates a GIF animation structure </summary>
        /// <param name="gifMaterial"> The material for the gif animation </param>
        /// <param name="frames"> The frames of the original gif file as 2d textures </param>
        /// <param name="fps"> The amount of frames drawn per second </param>
        public GIFAnimation(ref Material gifMaterial, ref List<Texture2D> frames, float fps){
            this.gifMaterial = gifMaterial;
            this.frames = frames;
            this.fps = fps;
        }
    }

    [SerializeField][Tooltip("GIF animations to animate in-game")]List<GIFAnimation> gifAnimations;

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
