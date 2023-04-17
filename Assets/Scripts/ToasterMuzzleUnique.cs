using UnityEngine;

public class ToasterMuzzleUnique : MonoBehaviour
{
    public Sprite[] visuals;

    SpriteRenderer _childSprites;

    private void Start()
    {
        _childSprites = GetComponentInChildren<SpriteRenderer>();
        _childSprites.sprite = visuals[Random.Range(0, visuals.Length)];

        this.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _childSprites.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}