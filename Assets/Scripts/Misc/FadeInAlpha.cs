using UnityEngine;

// Source: https://answers.unity.com/answers/1353307/view.html

public class FadeInAlpha : MonoBehaviour {

    [SerializeField] private AnimationCurve fadeCurve;
    Renderer _renderer;
    Color _color;
    float _timer = 0f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>(); // do this in awake, it has an impact on performances in Update
        _color = _renderer.material.color;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _color.a = fadeCurve.Evaluate(_timer);
        _renderer.material.color = _color;
    }
}