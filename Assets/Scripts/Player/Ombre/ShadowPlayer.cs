using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using StarterAssets;

public class ShadowPlayer : MonoBehaviour
{
    public float shadowMoveSpeed = 7.0f;
    public PlayerStatus playerStatus = PlayerStatus.Alive;
    private List<ILightSource> _lightSources = new();
    private bool wasInLight = false;
    public bool hasKey = false;
    public bool inLightSource = false;
    public bool inShadowForm = false;
    private ThirdPersonController _controller;
    private Animator _animator;
    public float diveDuration = 0.2f;
    private bool inDiving = false;
    public float height = 2.0f;

    private CharacterController _characterController;
    private float originalMoveSpeed;
    private float originalSprintSpeed;
    private float originalJumpHeight;
    private Transform _visualRoot;
    private float _originalControllerHeight;
    private Vector3 _originalControllerCenter;
    private float shadowCircleGroundOffset = 1.0f;

    private GameObject _shadowCircle;
    public float shadowCircleRadius = 0.5f;

    [SerializeField] private LayerMask blockingLayers;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
        _lightSources.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ILightSource>());
        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<ThirdPersonController>();
        _characterController = GetComponent<CharacterController>();

        originalMoveSpeed = _controller.MoveSpeed;
        originalSprintSpeed = _controller.SprintSpeed;

        _visualRoot = _animator != null ? _animator.transform.parent ?? _animator.transform : transform;
        
        CreateShadowCircle();
    }
    void CreateShadowCircle()
    {
        _shadowCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _shadowCircle.name = "ShadowCircle";
        _shadowCircle.transform.parent = transform;

        float groundY = -_characterController.height / 2f + shadowCircleGroundOffset;
        _shadowCircle.transform.localPosition = new Vector3(0, groundY, 0);
        _shadowCircle.transform.localScale = new Vector3(shadowCircleRadius * 2, 0.01f, shadowCircleRadius * 2);

        Destroy(_shadowCircle.GetComponent<Collider>());

        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
            shader = Shader.Find("HDRP/Unlit");

        Material shadowMat = new Material(shader);
        shadowMat.color = new Color(0, 0, 0, 0.5f);

        shadowMat.SetInt("_Surface", 1);
        shadowMat.SetOverrideTag("RenderType", "Transparent");
        shadowMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Renderer renderer = _shadowCircle.GetComponent<Renderer>();
        renderer.material = shadowMat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        _shadowCircle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        InLightCheck();
        
        if (Input.GetKeyDown(KeyCode.Q) && !inShadowForm && !inLightSource && !inDiving)
            TryEnterShadow();
        else if (inShadowForm && (Input.GetKeyUp(KeyCode.Q) || inLightSource) && !inDiving)
            ExitShadow();
    }
    
    void TryEnterShadow()
    {
        inDiving = true;

        if (!inShadowForm)
            StartCoroutine(EnterShadowRoutine());
    }

    IEnumerator EnterShadowRoutine()
    {
        inShadowForm = true;
        inDiving = true;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        // Enable the shadow circle
        _shadowCircle.SetActive(true);

        if (_animator) _animator.SetTrigger("Dive");

        float elapsed = 0f;

        while (elapsed < diveDuration)
        {
            float t = elapsed / diveDuration;

            elapsed += Time.deltaTime;

            yield return null;
        }

        _controller.MoveSpeed = shadowMoveSpeed;
        _controller.SprintSpeed = shadowMoveSpeed;
        _controller.JumpHeight = 0;

        Debug.Log("Player is now in shadow form (invisible).");
        inDiving = false;
    }

    void ExitShadow()
    {
        inDiving = true;
        
        StopAllCoroutines();
        StartCoroutine(ExitShadowRoutine());
    }
    
    IEnumerator ExitShadowRoutine()
    {
        inShadowForm = false;
        _shadowCircle.SetActive(false);
        
        if (_animator) _animator.SetTrigger("Emerge");

        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = true;

        float elapsed = 0;
        Vector3 currentScale = _visualRoot.localScale;

        while (elapsed < diveDuration)
        {
            float t = elapsed / diveDuration;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _controller.MoveSpeed = originalMoveSpeed;
        _controller.SprintSpeed = originalSprintSpeed;
        _controller.JumpHeight = originalJumpHeight;

        Debug.Log("Player emerged from shadow.");
        inDiving = false;
    }
    
    private void InLightCheck()
    {
        bool inLight = false;
        foreach (var lightSource in _lightSources)
        {
            if (lightSource.IsPlayerInLight(transform.position))
            {
                // Perform a raycast to check for obstructions
                Vector3 directionToPlayer = (transform.position - lightSource.GetLightPosition()).normalized;
                float distance = Vector3.Distance(transform.position, lightSource.GetLightPosition());

                if (!Physics.Raycast(lightSource.GetLightPosition(), directionToPlayer, distance, blockingLayers))
                {
                    inLight = true;
                    break;
                }
            }
        }
        
        inLightSource = inLight;
        
        if (inLight)
            OnEnterLight();
        else
            OnExitLight();
    }
    
    private void OnEnterLight()
    {
        if (!wasInLight)
        {
            wasInLight = true;
            Debug.Log("Player entered light!");
        }
    }

    private void OnExitLight()
    {
        if (wasInLight)
        {
            wasInLight = false;
            Debug.Log("Player left light.");
        }
    }

    private void OnDeath()
    {
        playerStatus = PlayerStatus.Dead;
        GameManager.Instance.UpdatePlayerStatus();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Player entered trigger.");
        if (collision.gameObject.CompareTag("Key") && !hasKey)
        {
            Destroy(collision.gameObject);
            hasKey = true;
        }
        
        if (collision.gameObject.CompareTag("ExitDoor") && hasKey)
        {
            playerStatus = PlayerStatus.Escaped;
            GameManager.Instance.UpdatePlayerStatus();
            gameObject.SetActive(false);
        }
    }
}