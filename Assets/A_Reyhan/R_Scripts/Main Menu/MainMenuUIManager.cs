using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

/// <summary>
/// MainMenuUIManager
/// Recreates the Ultimate Chicken Horse main menu feel:
///   1. Title drops in with elastic bounce (per word, staggered)
///   2. Player label + START button bounce in below
///   3. START button does a continuous breathe/pulse   loop
///   4. On START press → tablet panel slides in from left with a rotation snap
///   5. Clicking outside the panel (or Back) slides it out
///
/// SETUP IN INSPECTOR:
///   - Assign all RectTransform / TMP / Button references
///   - Set gameSceneName to your actual gameplay scene name
///   - All panels start at their OFF positions; this script animates them in on Start()
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR REFERENCES
    // ─────────────────────────────────────────────

    [Header("── Title Words (assign each word separately)")]
    [Tooltip("e.g. a TMP for 'CAT' and one for 'VS DOG' — stagger drops in")]
    public RectTransform[] titleWordRects;      // Each word of your title
    public float titleDropFromY     = 300f;     // How far above they start
    public float titleStaggerDelay  = 0.12f;    // Time between each word

    [Header("── Player Label (small text above START)")]
    public RectTransform playerLabelRect;       // "Player 1" small text
    public TMP_Text      playerLabelText;
    public CanvasGroup   playerLabelCanvasGroup; // For fade-in on Start press

    [Header("── START Button")]
    public RectTransform startButtonRect;       // The big START button
    public Button        startButton;
    public float startDropFromY = -120f;        // Starts below, bounces up

    [Header("── Tablet Panel")]
    public RectTransform tabletPanelRect;       // The dark rounded card
    public CanvasGroup   tabletCanvasGroup;     // For fade + raycast block
    public RectTransform tabletStartOffPos;     // Off-screen left anchor point
                                                // (create an empty GO positioned off-left)
    public float tabletOffscreenX   = -700f;   // How far left it starts
    public float tabletOnscreenX    =    0f;   // Final resting X position
    public float tabletRotationFrom =  -8f;    // Starts slightly rotated
    public Image dimOverlay;                    // Full-screen dim behind panel

    [Header("── Tablet Buttons")]
    public Button btnPlay;
    public Button btnSettings;
    public Button btnQuit;
    public Button btnBack;                      // Inside panel — closes it

    [Header("── Scene")]
    public string gameSceneName = "GameScene";

    [Header("── Timing")]
    public float titleIntroDelay    = 0.3f;    // Wait before title drops
    public float panelSlideDuration = 0.45f;
    public float panelSlideEase     = 0f;      // unused — eases hardcoded below

    [Header("── Title Slide-Out on Start Press")]
    [Tooltip("How far right the title words slide when the tablet opens")]
    public float titleSlideRightX     = 500f;
    public float titleSlideOutDuration = 0.45f;

    [Header("── Player Label Fade-In on Start Press")]
    public float playerLabelFadeDuration = 0.4f;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private bool   _panelOpen        = false;
    private Tween  _startBreathTween = null;
    private Vector2 _startButtonOrigin;
    private Vector2[] _titleWordOrigins;  // Cached final positions after intro

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Awake()
    {
        // Force all animated elements to their OFF state immediately
        // so there's no single-frame flash of the final position.
        ResetToOffState();
    }

    void Start()
    {
        WireButtons();
        StartCoroutine(PlayIntroSequence());
    }

    void OnDestroy()
    {
        // Kill all tweens owned by this manager to prevent errors on scene unload
        DOTween.Kill(this);
        _startBreathTween?.Kill();
    }

    // ─────────────────────────────────────────────
    // RESET — puts everything in OFF / hidden state
    // ─────────────────────────────────────────────

    void ResetToOffState()
    {
        // Title words: above screen, scale 0
        foreach (RectTransform word in titleWordRects)
        {
            if (word == null) continue;
            word.anchoredPosition += new Vector2(0f, titleDropFromY);
            word.localScale        = Vector3.zero;
        }

        // Player label: invisible, slightly above final position
        if (playerLabelRect != null)
        {
            playerLabelRect.localScale = Vector3.zero;
            playerLabelRect.gameObject.SetActive(true);
        }

        // Player label canvas group: fully transparent (will fade in on Start press)
        if (playerLabelCanvasGroup != null)
        {
            playerLabelCanvasGroup.alpha = 0f;
        }

        // START button: below screen, scale 0
        if (startButtonRect != null)
        {
            _startButtonOrigin      = startButtonRect.anchoredPosition;
            startButtonRect.anchoredPosition += new Vector2(0f, startDropFromY);
            startButtonRect.localScale        = Vector3.zero;
        }

        // Tablet panel: fully off-screen left, invisible
        if (tabletPanelRect != null)
        {
            tabletPanelRect.anchoredPosition = new Vector2(tabletOffscreenX,
                                                           tabletPanelRect.anchoredPosition.y);
            tabletPanelRect.localRotation    = Quaternion.Euler(0f, 0f, tabletRotationFrom);
        }

        if (tabletCanvasGroup != null)
        {
            tabletCanvasGroup.alpha          = 0f;
            tabletCanvasGroup.interactable   = false;
            tabletCanvasGroup.blocksRaycasts = false;
        }

        // Dim overlay: transparent, non-blocking
        if (dimOverlay != null)
        {
            Color c = dimOverlay.color;
            c.a              = 0f;
            dimOverlay.color = c;
            dimOverlay.raycastTarget = false;
        }
    }

    // ─────────────────────────────────────────────
    // BUTTON WIRING
    // ─────────────────────────────────────────────

    void WireButtons()
    {
        // START opens the tablet panel (UCH behaviour)
        if (startButton != null)
            startButton.onClick.AddListener(OpenTabletPanel);

        // Dim overlay click closes panel
        if (dimOverlay != null)
            dimOverlay.GetComponent<Button>()?.onClick.AddListener(CloseTabletPanel);

        // Tablet buttons
        if (btnPlay     != null) btnPlay    .onClick.AddListener(OnPlayPressed);
        if (btnSettings != null) btnSettings.onClick.AddListener(OnSettingsPressed);
        if (btnQuit     != null) btnQuit    .onClick.AddListener(OnQuitPressed);
        if (btnBack     != null) btnBack    .onClick.AddListener(CloseTabletPanel);
    }

    // ─────────────────────────────────────────────
    // INTRO SEQUENCE  (title → label → start button)
    // ─────────────────────────────────────────────

    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(titleIntroDelay);

        // ── 1. Title words drop in, staggered
        for (int i = 0; i < titleWordRects.Length; i++)
        {
            RectTransform word = titleWordRects[i];
            if (word == null) continue;

            // Cache the intended final position
            Vector2 finalPos = word.anchoredPosition - new Vector2(0f, titleDropFromY);

            // Position to final anchored pos (the Awake offset is baked in)
            word.DOAnchorPos(finalPos, 0.55f)
                .SetEase(Ease.OutBack)
                .SetId(this);

            word.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutBack)
                .SetId(this);

            // Stagger — don't wait on the last word
            if (i < titleWordRects.Length - 1)
                yield return new WaitForSeconds(titleStaggerDelay);
        }

        // Wait for the last title word to mostly land
        yield return new WaitForSeconds(0.35f);

        // Cache the final resting positions of each title word
        _titleWordOrigins = new Vector2[titleWordRects.Length];
        for (int i = 0; i < titleWordRects.Length; i++)
        {
            if (titleWordRects[i] != null)
                _titleWordOrigins[i] = titleWordRects[i].anchoredPosition;
        }

        // ── 2. Player label pops in (small, fast)
        if (playerLabelRect != null)
        {
            playerLabelRect.DOScale(1.15f, 0.22f)
                .SetEase(Ease.OutQuad)
                .SetId(this)
                .OnComplete(() =>
                {
                    playerLabelRect.DOScale(1f, 0.15f)
                        .SetEase(Ease.InQuad)
                        .SetId(this);
                });
        }

        yield return new WaitForSeconds(0.18f);

        // ── 3. START button bounces up from below
        if (startButtonRect != null)
        {
            startButtonRect.DOAnchorPos(_startButtonOrigin, 0.6f)
                .SetEase(Ease.OutElastic)
                .SetId(this);

            startButtonRect.DOScale(Vector3.one, 0.5f)
                .SetEase(Ease.OutElastic)
                .SetId(this)
                .OnComplete(StartBreathLoop);
        }
    }

    // ── Continuous gentle breathe on the START button (UCH "pulse")
    void StartBreathLoop()
    {
        if (startButtonRect == null) return;

        _startBreathTween = startButtonRect
            .DOScale(new Vector3(1.06f, 1.06f, 1f), 0.85f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId(this);
    }

    // ─────────────────────────────────────────────
    // TABLET PANEL — OPEN
    // ─────────────────────────────────────────────

    public void OpenTabletPanel()
    {
        if (_panelOpen) return;
        _panelOpen = true;

        // Kill the breathe loop while panel is open (cleaner feel)
        _startBreathTween?.Kill();
        startButtonRect?.DOScale(Vector3.one, 0.12f).SetId(this);

        // ── Title words slide to the right to make room for the tablet panel
        for (int i = 0; i < titleWordRects.Length; i++)
        {
            RectTransform word = titleWordRects[i];
            if (word == null) continue;

            Vector2 target = _titleWordOrigins[i] + new Vector2(titleSlideRightX, 0f);
            word.DOAnchorPos(target, titleSlideOutDuration)
                .SetEase(Ease.InOutCubic)
                .SetId(this);
        }

        // ── Player label ("Start" text) fades in
        if (playerLabelCanvasGroup != null)
        {
            playerLabelCanvasGroup.DOFade(1f, playerLabelFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetId(this);
        }

        // Enable raycasting on the panel
        if (tabletCanvasGroup != null)
        {
            tabletCanvasGroup.interactable   = true;
            tabletCanvasGroup.blocksRaycasts = true;
        }

        // Dim overlay fades in
        if (dimOverlay != null)
        {
            dimOverlay.raycastTarget = true;
            dimOverlay.DOFade(0.55f, 0.3f).SetId(this);
        }

        // Panel slides in from left with rotation snap
        // Step 1: slam to just past final with slight overshoot rotation
        tabletPanelRect.DOAnchorPosX(tabletOnscreenX + 18f, panelSlideDuration * 0.75f)
            .SetEase(Ease.OutCubic)
            .SetId(this);

        tabletPanelRect.DOLocalRotate(new Vector3(0f, 0f, 2f), panelSlideDuration * 0.75f)
            .SetEase(Ease.OutCubic)
            .SetId(this)
            .OnComplete(() =>
            {
                // Step 2: settle to final position with a little bounce-back
                tabletPanelRect.DOAnchorPosX(tabletOnscreenX, panelSlideDuration * 0.35f)
                    .SetEase(Ease.OutQuad)
                    .SetId(this);

                tabletPanelRect.DOLocalRotate(Vector3.zero, panelSlideDuration * 0.35f)
                    .SetEase(Ease.OutQuad)
                    .SetId(this);
            });

        // Fade panel in
        if (tabletCanvasGroup != null)
        {
            tabletCanvasGroup.DOFade(1f, 0.25f).SetId(this);
        }

        // Stagger-cascade the buttons inside the panel
        StartCoroutine(StaggerTabletButtons());
    }

    // ─────────────────────────────────────────────
    // TABLET PANEL — CLOSE
    // ─────────────────────────────────────────────

    public void CloseTabletPanel()
    {
        if (!_panelOpen) return;
        _panelOpen = false;

        // Dim overlay fades out
        if (dimOverlay != null)
        {
            dimOverlay.DOFade(0f, 0.25f)
                .SetId(this)
                .OnComplete(() => dimOverlay.raycastTarget = false);
        }

        // ── Title words slide back to their original positions
        for (int i = 0; i < titleWordRects.Length; i++)
        {
            RectTransform word = titleWordRects[i];
            if (word == null) continue;

            word.DOAnchorPos(_titleWordOrigins[i], titleSlideOutDuration)
                .SetEase(Ease.OutCubic)
                .SetId(this);
        }

        // ── Player label fades out
        if (playerLabelCanvasGroup != null)
        {
            playerLabelCanvasGroup.DOFade(0f, playerLabelFadeDuration * 0.6f)
                .SetEase(Ease.InQuad)
                .SetId(this);
        }

        // Panel slides back left with a slight tilt outward
        tabletPanelRect.DOLocalRotate(new Vector3(0f, 0f, tabletRotationFrom * 0.5f),
                                      panelSlideDuration * 0.4f)
            .SetEase(Ease.InQuad)
            .SetId(this);

        tabletPanelRect.DOAnchorPosX(tabletOffscreenX, panelSlideDuration)
            .SetEase(Ease.InBack)
            .SetId(this)
            .OnComplete(() =>
            {
                if (tabletCanvasGroup != null)
                {
                    tabletCanvasGroup.interactable   = false;
                    tabletCanvasGroup.blocksRaycasts = false;
                    tabletCanvasGroup.alpha           = 0f;
                }

                // Resume START button breathe
                StartBreathLoop();
            });

        if (tabletCanvasGroup != null)
            tabletCanvasGroup.DOFade(0f, 0.2f).SetId(this);
    }

    // ─────────────────────────────────────────────
    // BUTTON CASCADE — slide in tablet buttons one by one
    // ─────────────────────────────────────────────

    IEnumerator StaggerTabletButtons()
    {
        Button[] panelButtons = { btnPlay, btnSettings, btnQuit };
        float    stagger      = 0.07f;
        float    waitBefore   = panelSlideDuration * 0.5f;  // Wait for panel to arrive first

        yield return new WaitForSeconds(waitBefore);

        foreach (Button btn in panelButtons)
        {
            if (btn == null) continue;

            RectTransform rt = btn.GetComponent<RectTransform>();

            // Start each button shifted left, invisible
            Vector2 origin = rt.anchoredPosition;
            rt.anchoredPosition += new Vector2(-60f, 0f);

            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;

            // Slide right + fade in with easeOutBack
            rt.DOAnchorPos(origin, 0.35f)
                .SetEase(Ease.OutBack)
                .SetId(this);

            if (cg != null)
                cg.DOFade(1f, 0.25f).SetId(this);

            yield return new WaitForSeconds(stagger);
        }
    }

    // ─────────────────────────────────────────────
    // BUTTON HOVER JUICE
    //
    // Call these from EventTrigger components on each button's GameObject.
    // Add EventTrigger → PointerEnter → MainMenuUIManager.OnButtonHoverEnter(button)
    //                  → PointerExit  → MainMenuUIManager.OnButtonHoverExit(button)
    // ─────────────────────────────────────────────

    public void OnButtonHoverEnter(RectTransform rt)
    {
        if (rt == null) return;
        rt.DOKill();
        rt.DOScale(new Vector3(1.08f, 1.08f, 1f), 0.18f)
            .SetEase(Ease.OutBack)
            .SetId(this);
    }

    public void OnButtonHoverExit(RectTransform rt)
    {
        if (rt == null) return;
        rt.DOKill();
        rt.DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.OutQuad)
            .SetId(this);
    }

    /// <summary>
    /// Call this on button press for a satisfying punch-down feel.
    /// Pair with the actual action in OnComplete.
    /// </summary>
    public void PunchButton(RectTransform rt, System.Action onComplete = null)
    {
        if (rt == null) { onComplete?.Invoke(); return; }
        rt.DOKill();
        rt.DOPunchScale(new Vector3(-0.12f, -0.12f, 0f), 0.25f, 5, 0.5f)
            .SetId(this)
            .OnComplete(() => onComplete?.Invoke());
    }

    // ─────────────────────────────────────────────
    // TABLET BUTTON ACTIONS
    // ─────────────────────────────────────────────

    void OnPlayPressed()
    {
        PunchButton(btnPlay.GetComponent<RectTransform>(), () =>
        {
            // Transition to game scene
            SceneManager.LoadScene(gameSceneName);
        });
    }

    void OnSettingsPressed()
    {
        PunchButton(btnSettings.GetComponent<RectTransform>(), () =>
        {
            // TODO: Open settings panel
            Debug.Log("Settings pressed — hook up your settings panel here");
        });
    }

    void OnQuitPressed()
    {
        PunchButton(btnQuit.GetComponent<RectTransform>(), () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }
}
