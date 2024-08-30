using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class RendererAdapter
{
    public abstract Sprite sprite { get; set; }
    public abstract Color color { get; set; }
    public abstract GameObject gameObject { get; }
    public abstract bool isEnable { get; set; }
    public abstract Material material { get; set; }
}

public class ImageAdapter : RendererAdapter
{
    Image adaptee;
    public ImageAdapter(Image adaptee) { this.adaptee = adaptee; }

    public override Sprite sprite { get => adaptee.sprite; set => adaptee.sprite = value; }
    public override Color color { get => adaptee.color; set => adaptee.color = value; }
    public override GameObject gameObject { get => adaptee.gameObject; }
    public override bool isEnable { get => adaptee.enabled; set => adaptee.enabled = true; } //this is set like this so that items in the UI can't be turned off individually
    public override Material material { get => adaptee.material; set => adaptee.material = value; }
}

public class SpriteRendererAdapter : RendererAdapter
{
    SpriteRenderer adaptee;
    public SpriteRendererAdapter(SpriteRenderer adaptee) { this.adaptee = adaptee; }

    public override Sprite sprite { get => adaptee.sprite; set => adaptee.sprite = value; }
    public override Color color { get => adaptee.color; set => adaptee.color = value; }
    public override GameObject gameObject { get => adaptee.gameObject; }
    public override bool isEnable { get => adaptee.enabled; set => adaptee.enabled = value; }
    public override Material material { get => adaptee.material; set => adaptee.material = value; }
}