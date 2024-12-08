using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Fireable
{   
    public AudioClip spawnSound;

    public Color PreviewColor { private get; set; }

    private const int BULLET_PREVIEW_ORDER = 20;

    public override void OnPreviewableCreation()
    {
        base.OnPreviewableCreation();
        var audioSource = GetComponent<AudioSource>();

        audioSource.clip = spawnSound;
        audioSource.Play();
    }

    public override Color GetPreviewOutline()
    {
        if (PreviewColor != Color.clear)
        {
            return PreviewColor;
        }

        return base.GetPreviewOutline();
    }

    public override Vector2 GetPreviewScale()
    {
        return transform.localScale * 1.15f; //slightly larger so that the it can be easily seen
    }

    public override void SetPreviewObject(PreviewableBase newPreviewable)
    {
        base.SetPreviewObject(newPreviewable);

        var renderer = newPreviewable.GetRenderer();
        renderer.sortingOrder = BULLET_PREVIEW_ORDER;
    }
}
