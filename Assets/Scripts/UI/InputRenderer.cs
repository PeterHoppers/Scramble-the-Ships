using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TransformTransition))]
public class InputRenderer : MonoBehaviour
{
    public Material glitchedMaterial;
    public float scaleAmount = .35f;
    private RendererAdapter _renderer;
    private Material _defaultMaterial;
    private TransformTransition _transformTransition;

    private float _glitchDuration = .25f; //TODO: Set this up to be the same length as the tick end
    private float _scaleDuration = .1f;
    private Vector3 _defaultScale;
    private Vector3 _selectedScale;
    private Sprite _incomingSprite;

    // Start is called before the first frame update
    void Awake()
    {
        SetRenderer();
        _transformTransition = GetComponent<TransformTransition>();
        _defaultScale = transform.localScale;
        _selectedScale = new Vector3(_defaultScale.x + scaleAmount, _defaultScale.y + scaleAmount, _defaultScale.z + scaleAmount);
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

        _incomingSprite = sprite;
        var previousSprite = _renderer.sprite;
        if (gameObject.activeInHierarchy && previousSprite != null)
        {
            StartCoroutine(PerformGlitchEffect(_glitchDuration, _incomingSprite));
        }
        else
        { 
            _renderer.sprite = _incomingSprite;
        }
    }

    public void SelectInput()
    {
        if (!isActiveAndEnabled || !_renderer.isEnable)
        {
            return;
        }

        _transformTransition.ScaleTo(_selectedScale, _scaleDuration); 
    }

    public void DeselectInput()
    {
        if (!isActiveAndEnabled || !_renderer.isEnable)
        {
            return;
        }

        _transformTransition.StopAllCoroutines();
        _transformTransition.ScaleTo(_defaultScale, _scaleDuration);
    }

    public void OnNoInputSelected()
    {
        _renderer.color = new Color(.4f, .4f, .4f, 1f);
    }

    public void OnTickStart()
    {
        _renderer.color = new Color(1f, 1f, 1f, 1f);        
    }

    public IEnumerator OnTickEnd(float tickEndDuration)
    {
        _glitchDuration = tickEndDuration;
        if (!isActiveAndEnabled || transform.localScale == _defaultScale)
        {
            yield return null;
        }

        if (transform.localScale != _selectedScale) 
        {
            yield return new WaitForSeconds(tickEndDuration / 3);
        }
        
        _transformTransition.StopAllCoroutines();
        _transformTransition.ScaleTo(_defaultScale, tickEndDuration / 3);
    }

    public void SetVisibility(bool isVisible)
    {
        if (_renderer == null)
        {
            SetRenderer();
        }

        _renderer.isEnable = isVisible;

        if (!isVisible)
        {
            OnRendererDisable();
        }
    }

    IEnumerator PerformGlitchEffect(float time, Sprite sprite)
    {
        var durationOfEachPiece = time / 2;
        if (transform.localScale != _selectedScale && transform.localScale != _defaultScale)
        {
            durationOfEachPiece = time / 3;
            yield return new WaitForSeconds(durationOfEachPiece);
        }

        _renderer.material = glitchedMaterial;
        yield return new WaitForSeconds(durationOfEachPiece);
        _renderer.sprite = sprite;
        yield return new WaitForSeconds(durationOfEachPiece);
        _renderer.material = _defaultMaterial;
    }

    private void OnDisable()
    {
        OnRendererDisable();
    }

    private void OnRendererDisable()
    {
        StopAllCoroutines();
        _transformTransition.StopAllCoroutines();

        if (_renderer != null && _defaultMaterial != null)
        {
            _renderer.material = _defaultMaterial;
            _renderer.sprite = _incomingSprite;
        }

        transform.localScale = _defaultScale;
    }
}
