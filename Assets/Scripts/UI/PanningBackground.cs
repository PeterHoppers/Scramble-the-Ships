using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PanningBackground : MonoBehaviour
{
    public float panningSpeed;
    public Vector2 panningDirection;
    public Transform backgroundHolder;
    public SpriteRenderer backgroundRenderer;

    float _backgroundSectionHeight;
    SpriteRenderer _topRenderer;
    SpriteRenderer _bottomRenderer;

    bool _isTopBeingViewed = true;

    // Start is called before the first frame update
    void Start()
    {
        _backgroundSectionHeight = backgroundRenderer.bounds.size.y;

        var topBackgroundPiece = Instantiate(backgroundRenderer.gameObject, backgroundHolder);
        topBackgroundPiece.transform.position = new Vector3(0, _backgroundSectionHeight / 2, 0);
        _topRenderer = topBackgroundPiece.GetComponent<SpriteRenderer>();

        var bottomBackgroundPiece = Instantiate(backgroundRenderer.gameObject, backgroundHolder);
        bottomBackgroundPiece.transform.position = new Vector3(0, - _backgroundSectionHeight / 2, 0);
        _bottomRenderer = bottomBackgroundPiece.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var backgroundTransform = backgroundHolder.transform;

        if (panningSpeed > 0) 
        {
            backgroundTransform.Translate(panningSpeed * Time.deltaTime * panningDirection);
        }

        var isTopViewed = _topRenderer.isVisible;
        var isBottomViewed = _bottomRenderer.isVisible;

        if (isTopViewed)
        {
            if (!isBottomViewed && !_isTopBeingViewed)
            {
                _isTopBeingViewed = true;
                MoveGameObjectAroundOther(_topRenderer.gameObject, _bottomRenderer.gameObject);
            }
        }
        else
        {
            if (isBottomViewed && _isTopBeingViewed)
            {
                _isTopBeingViewed = false;
                MoveGameObjectAroundOther(_bottomRenderer.gameObject, _topRenderer.gameObject);
            }
        }
    }

    void MoveGameObjectAroundOther(GameObject station, GameObject moving)
    {
        var stationPosition = station.transform.localPosition;
        var yOffset = stationPosition.y + (_backgroundSectionHeight * -panningDirection.y);
        moving.transform.localPosition = new Vector3(stationPosition.x, yOffset, stationPosition.z);
    }
}
