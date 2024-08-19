using UnityEngine;
using UnityEngine.UI;

public class InputRenderer : MonoBehaviour
{
    private RendererAdapter _renderer;
    // Start is called before the first frame update
    void Awake()
    {
        SetRenderer();
    }

    void SetRenderer()
    {
        if (GetComponent<Image>())
            _renderer = new ImageAdapter(GetComponent<Image>());
        else if (GetComponent<SpriteRenderer>())
            _renderer = new SpriteRendererAdapter(GetComponent<SpriteRenderer>());
        else
            Debug.LogError("Input renderer fails to have a valid renderer on it.");
    }

    public void SetSprite(Sprite sprite) 
    {
        if (_renderer == null) 
        {
            SetRenderer();
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

    public void SetVisibility(bool isVisible)
    {
        if (_renderer == null)
        {
            SetRenderer();
        }

        _renderer.isEnable = isVisible;
    }
}
