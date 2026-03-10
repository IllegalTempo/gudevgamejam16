using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameCore : MonoBehaviour
{
    public static GameCore Instance { get; private set; }
    public PlayerInput playerInput;
    public Material BlackWhiteMat;
    public GameObject playerPrefab;
    public LayerMask SelectableItems;

    public PlayerMovement player;
    public PlayerMovement clone;
    public Material cloneMat;
    public PlayerMovement currentPlayer;


    [Header("UI")]
    public GameObject interaction_pickup;
    public TMP_Text pickupText;
    public GameObject interaction_freeze;
    public TMP_Text freezeText;

    public TMP_Text freezeAmmoText;
    public TMP_Text continueAmmoText;

    public TMP_Text statusDisplay;
    private Coroutine statusCoroutine;

    public TMP_Text timedisplayText;
    private Vector2 scroll;
    private float baseFixedDeltaTime;
    public float timeScaleStep = 0.1f;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.player.Enable();

        // store the default fixed delta time so we can scale physics when changing timeScale
        baseFixedDeltaTime = Time.fixedDeltaTime;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        scroll = playerInput.player.timespeed.ReadValue<Vector2>();
        // adjust game speed using scroll Y (mouse wheel). Clamp between 0.1x and 3x.
        if (Mathf.Abs(scroll.y) > 0.001f)
        {
            float newScale = Mathf.Clamp(Time.timeScale + scroll.y * timeScaleStep, 0.1f, 3f);
            if (!Mathf.Approximately(newScale, Time.timeScale))
            {
                Time.timeScale = newScale;
                // scale fixed delta so physics behave consistently
                Time.fixedDeltaTime = baseFixedDeltaTime * Time.timeScale;
                timedisplayText.text = $"x{Time.timeScale:0.##}";
                // show feedback to player
            }
        }
    }
    private void Start()
    {
        interaction_freeze.SetActive(false);
        interaction_pickup.SetActive(false);
    }
    private void OnEnable()
    {
        playerInput.player.swap.performed += ctx => StartCoroutine(OnSwitchTimeLine());

    }
    private void OnDisable()
    {
        playerInput.player.swap.performed -= ctx => StartCoroutine(OnSwitchTimeLine());
        playerInput.player.Disable();
    }
    public void displayStatusText(string text)
    {
        //show the status text for 2 seconds, then fade it out
        if (statusDisplay == null) return;

        // If a previous status coroutine is running, stop it so messages don't overlap
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
            statusCoroutine = null;
        }

        statusCoroutine = StartCoroutine(DisplayStatusTextCoroutine(text));

    }

    private IEnumerator DisplayStatusTextCoroutine(string text)
    {
        const float visibleDuration = 2f;
        const float fadeDuration = 0.5f;

        statusDisplay.text = text;
        var color = statusDisplay.color;
        color.a = 1f;
        statusDisplay.color = color;
        statusDisplay.gameObject.SetActive(true);

        // Wait while fully visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(1f, 0f, t);
            statusDisplay.color = color;
            yield return null;
        }

        // Ensure fully transparent and hide
        color.a = 0f;
        statusDisplay.color = color;
        statusDisplay.gameObject.SetActive(false);

        statusCoroutine = null;
    }

    private PlayerMovement otherPlayer()
    {
        if (currentPlayer == player) return clone;
        else return player;
    }
    public IEnumerator OnSwitchTimeLine()
    {
        yield return currentPlayer.DisablePlayer();
        otherPlayer().EnablePlayer();
    }


}
