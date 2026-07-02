using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// MenuButtonJuice
/// Drop this on any UI button in your main menu.
/// Handles hover scale, press punch, and an optional idle float bob.
///
/// SETUP: Just attach this component to any Button GameObject.
///        No Inspector references needed — it grabs its own RectTransform.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MenuButtonJuice : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("── Hover")]
    public float hoverScale    = 1.08f;
    public float hoverDuration = 0.18f;

    [Header("── Press")]
    public float pressScaleDown  = 0.92f;
    public float pressDuration   = 0.08f;

    [Header("── Idle Float Bob (optional)")]
    [Tooltip("Enable to make this button float up and down like UCH menu items")]
    public bool  enableIdleBob   = false;
    public float bobAmount       = 6f;      // Pixels to move up/down
    public float bobDuration     = 0.9f;    // Seconds for one half-cycle

    [Header("── Wiggle on hover (Cuphead style)")]
    public bool  enableHoverWiggle = false;
    public float wiggleDegrees     = 4f;

    // ─────────────────────────────────────────────
    private RectTransform _rt;
    private Vector2       _originPos;
    private Tween         _bobTween;
    private Tween         _wiggleTween;
    private bool          _isPressed = false;

    void Awake()
    {
        _rt        = GetComponent<RectTransform>();
        _originPos = _rt.anchoredPosition;
    }

    void Start()
    {
        if (enableIdleBob)
            StartBob();
    }

    void OnDestroy()
    {
        _bobTween?.Kill();
        _wiggleTween?.Kill();
        DOTween.Kill(_rt);
    }

    // ─────────────────────────────────────────────
    // IDLE BOB
    // ─────────────────────────────────────────────

    void StartBob()
    {
        _bobTween = _rt.DOAnchorPosY(_originPos.y + bobAmount, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void PauseBob()
    {
        _bobTween?.Pause();
        // Snap back to origin Y smoothly
        _rt.DOAnchorPosY(_originPos.y, 0.15f).SetEase(Ease.OutQuad);
    }

    void ResumeBob()
    {
        if (!enableIdleBob) return;
        // Restart from current position to avoid a snap
        _bobTween?.Kill();
        StartBob();
    }

    // ─────────────────────────────────────────────
    // POINTER EVENTS
    // ─────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isPressed) return;

        // Pause bob so hover doesn't fight with it
        if (enableIdleBob) PauseBob();

        // Scale up with easeOutBack overshoot
        _rt.DOKill();
        _rt.DOScale(new Vector3(hoverScale, hoverScale, 1f), hoverDuration)
            .SetEase(Ease.OutBack);

        // Optional wiggle
        if (enableHoverWiggle)
        {
            _wiggleTween?.Kill();
            _wiggleTween = _rt
                .DORotate(new Vector3(0f, 0f, wiggleDegrees), 0.08f)
                .SetEase(Ease.InOutSine)
                .SetLoops(6, LoopType.Yoyo)
                .OnComplete(() => _rt.DORotate(Vector3.zero, 0.06f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPressed) return;

        _rt.DOKill();
        _rt.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad);
        _rt.DORotate(Vector3.zero, 0.1f);

        // Resume bob
        if (enableIdleBob) ResumeBob();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        _rt.DOKill();
        // Press down: squish a little
        _rt.DOScale(new Vector3(pressScaleDown, pressScaleDown, 1f), pressDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        // Bounce back up past normal size, then settle
        _rt.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.12f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _rt.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack);
            });
    }
}
