using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InputRenderer : MonoBehaviour
{
    private SpriteRenderer _renderer;
    // Start is called before the first frame update
    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite sprite) 
    {
        if (_renderer == null) 
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        _renderer.sprite = sprite;
    }

    public void SelectInput()
    {
        _renderer.color = Color.grey;
    }

    public void DeselectInput()
    { 
        _renderer.color = Color.white;
    }
}
