using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputRenderer : MonoBehaviour
{
    public Material glitchedMaterial;
    private RendererAdapter _renderer;
    private Material _defaultMaterial;

    private float _glitchDuration = .25f;
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

        _defaultMaterial = _renderer.material;
    }

    public bool WillSpriteChange(Sprite newSprite)
    {
        if (_renderer == null || _renderer.sprite == null)
        {
            return false;
        }

        return (newSprite != _renderer.sprite);
    }

    public void SetSprite(Sprite sprite) 
    {
        if (_renderer == null) 
        {
            SetRenderer();
        }

        if (sprite == _renderer.sprite)
        {
            return;
        }

        var previousSprite = _renderer.sprite;
        if (gameObject.activeInHierarchy && previousSprite != null)
        {
            StartCoroutine(PerformGlitchEffect(_glitchDuration, sprite));
        }
        else
        { 
            _renderer.sprite = sprite;
        }
    }

    public void SelectInput()
    {
        _renderer.color = Color.white;
    }

    public void SetUnselectedInput()
    {
        // this method is here if we want to prefrom so sort of effect to those options not selected
    }

    public void ResetInput()
    {
        _renderer.color = new Color(.75f, .75f, .75f, 1f);
    }

    public void SetVisibility(bool isVisible)
    {
        if (_renderer == null)
        {
            SetRenderer();
        }

        _renderer.isEnable = isVisible;        
    }

    IEnumerator PerformGlitchEffect(float time, Sprite sprite)
    {
        _renderer.material = glitchedMaterial;
        yield return new WaitForSeconds(time / 2);
        _renderer.sprite = sprite;
        yield return new WaitForSeconds(time / 2);
        _renderer.material = _defaultMaterial;
    }
}
