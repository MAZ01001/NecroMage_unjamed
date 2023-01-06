using UnityEngine;
using UnityEngine.UI;

public class RainbowColor : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("Graphic to set rainbow effect as the color")]      private Graphic graphic;
    [SerializeField][Range(0f, 1f)][Tooltip("Speed of the rainbow effect")]      private float speed = 0.3f;
    [SerializeField][Range(0f, 1f)][Tooltip("Saturation of the rainbow effect")] private float saturation = 1f;
    [SerializeField][Range(0f, 1f)][Tooltip("Brightness of the rainbow effect")] private float brightness = 0.2f;
    //~ private
    private float timer = 0f;
    //~ unity methods (private)
    private void Update(){
        this.timer = (this.timer + (Time.deltaTime * this.speed)) % 1f;
        this.graphic.color = Color.HSVToRGB(this.timer, this.saturation, this.brightness);
    }
}
