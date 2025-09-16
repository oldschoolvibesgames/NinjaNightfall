using UnityEngine;

public class ChangeSprite : MonoBehaviour
{
    public SpriteRenderer renderer;
    public Sprite[] sprites;

    public void SetSprite(int index)
    {
        if(index < 0 || index >= sprites.Length) return;

        renderer.sprite = sprites[index];
    }
}
