using UnityEngine;

public class AnimGif : MonoBehaviour{
    [SerializeField] private Texture2D[] frames;
    [SerializeField] private float fps = 10.0f;

    private Material mat;

    private void Start(){ this.mat = this.GetComponent<Renderer>().material; }

    void FixedUpdate(){
        int index = (int)(Time.time * this.fps);
        index = index % this.frames.Length;
        this.mat.mainTexture = this.frames[index]; // usar en planeObjects
        //GetComponent<RawImage> ().texture = frames [index];
    }
}
