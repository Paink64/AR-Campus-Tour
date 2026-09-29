using UnityEngine;
using UnityEngine.UI;

public class POISlideshow : MonoBehaviour
{
    [Header("UI")]
    public Image galleryImage;

    [Header("Settings")]
    public bool autoplay = true;
    public float secondsPerSlide = 2.5f;
    public bool loop = true;

    private Sprite[] _slides;
    private int _index;
    private float _timer;

    void Awake()
    {
        if (galleryImage == null)
            galleryImage = GetComponent<Image>();
    }

    void Update()
    {
        if (!autoplay) return;
        if (_slides == null || _slides.Length <= 1) return;
        if (galleryImage == null) return;

        _timer += Time.deltaTime;
        if (_timer >= secondsPerSlide)
        {
            _timer = 0f;
            Next();
        }
    }

    /// <summary>Call this when the POI changes.</summary>
    public void SetSlides(Sprite[] slides, int startIndex = 0)
    {
        _slides = slides;
        _timer = 0f;
        _index = 0;

        if (_slides == null || _slides.Length == 0)
        {
            SetVisible(false);
            return;
        }

        _index = Mathf.Clamp(startIndex, 0, _slides.Length - 1);
        Show(_index);
    }

    public void Next()
    {
        if (_slides == null || _slides.Length == 0) return;

        int next = _index + 1;
        if (next >= _slides.Length)
        {
            if (!loop) return;
            next = 0;
        }

        Show(next);
    }

    public void Prev()
    {
        if (_slides == null || _slides.Length == 0) return;

        int prev = _index - 1;
        if (prev < 0)
        {
            if (!loop) return;
            prev = _slides.Length - 1;
        }

        Show(prev);
    }

    private void Show(int i)
    {
        _index = i;
        _timer = 0f;

        // Skip null sprites safely
        int tries = 0;
        while (tries < _slides.Length && _slides[_index] == null)
        {
            _index = (_index + 1) % _slides.Length;
            tries++;
        }

        if (_slides[_index] == null)
        {
            SetVisible(false);
            return;
        }

        galleryImage.sprite = _slides[_index];
        galleryImage.preserveAspect = true;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (galleryImage != null)
            galleryImage.enabled = visible;
    }
}
