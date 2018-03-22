using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fading : MonoBehaviour {

    private Image _fadingImage;
    private RectTransform _fadingRect;

    private float _fadingTimeInSec;
    private int _fadeDir;
    private float _alpha;
    private bool _withFreeze;


    void Awake()
    {
        Transform fadingObj = GameComponents.GetHUD().transform.Find("Fading");
        _fadingRect = fadingObj.GetComponent<RectTransform>();
        _fadingImage = fadingObj.GetComponent<Image>();
    }

    IEnumerator Fade()
    {
        while (_fadeDir == -1 && _alpha > 0 || _fadeDir == 1 && _alpha < 1)
        {
            _alpha += (Time.unscaledDeltaTime / _fadingTimeInSec) * _fadeDir;
            _alpha = Mathf.Clamp01(_alpha);

            if (_withFreeze || _fadeDir == -1 && Time.timeScale < 1)
            {
                float tmpTimeScale = Time.timeScale;
                tmpTimeScale += (Time.deltaTime / _fadingTimeInSec) * _fadeDir * -1;
                tmpTimeScale = Mathf.Clamp01(tmpTimeScale);
                Time.timeScale = tmpTimeScale;
            }

            _fadingRect.sizeDelta = new Vector2(Screen.width, Screen.height);
            _fadingImage.color = new Color(0, 0, 0, _alpha);

            yield return new WaitForEndOfFrame();
        };
    }

    public void FadeIn (float f)
    {
        _fadingTimeInSec = f;
        _fadeDir = -1;
        _alpha = 1;
        StartCoroutine(Fade());
    }

    public void FadeOut (float f, bool withFreeze=false)
    {
        _fadingTimeInSec = f;
        _fadeDir = 1;
        _alpha = 0;
        _withFreeze = withFreeze;
        StartCoroutine(Fade());
    }
}
