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

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.player.Enable();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
