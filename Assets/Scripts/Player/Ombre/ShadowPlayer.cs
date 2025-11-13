using Mirror;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowPlayer : NetworkBehaviour
{
    public float shadowMoveSpeed = 7.0f;
    public PlayerStatus playerStatus = PlayerStatus.Alive;
    private List<ILightSource> _lightSources = new();
    private bool wasInLight = false;
    public bool hasKey = false;

    [SyncVar]
    public bool inLightSource = false;

    [SyncVar]
    public bool inEnemyLightSource = false;

    [SyncVar(hook = nameof(OnShadowFormChangedSync))]
    public bool inShadowForm = false;

    private ThirdPersonController _controller;
    private ParticleSystem _particleSystem;
    private Animator _animator;
    public float diveDuration = 0.2f;
    private bool inDiving = false;
    public float height = 2.0f;

    private CharacterController _characterController;
    private float originalMoveSpeed;
    private float originalSprintSpeed;
    private float originalJumpHeight;
    private Transform _visualRoot;
    private Transform _modelTransform;
    private float shadowCircleGroundOffset = 1.0f;

    private float _lastLightSourceUpdate = 0f;
    private const float LIGHT_SOURCE_UPDATE_INTERVAL = 1f; // Actualiser toutes les secondes

    private GameObject _shadowCircle;
    public float shadowCircleRadius = 0.5f;

    //private float _originalControllerHeight;
    //private Vector3 _originalControllerCenter;

    public float maxHealth = 20.0f;

    [SyncVar(hook = nameof(OnHealthChangedSync))]
    private float _health;
    public float health {
        get => _health;
        set {
            if (!isServer) return; // Seulement le serveur peut changer la santé
            float newHealth = Mathf.Clamp(value, 0f, maxHealth);
            _health = newHealth;
            if (_health <= 0 && playerStatus == PlayerStatus.Alive)
            {
                OnDeath();
            }
        }
    }
    public event Action<float> OnHealthChanged;

    private void OnHealthChangedSync(float oldHealth, float newHealth)
    {
        // Hook appelé sur tous les clients quand la santé change
        OnHealthChanged?.Invoke(newHealth);

        if (newHealth <= 0 && playerStatus == PlayerStatus.Alive)
        {
            OnDeath();
        }
    }


    private void OnShadowFormChangedSync(bool oldVal, bool newVal)
    {
        if (newVal && !inDiving)
            StartCoroutine(EnterShadowRoutine());
        else if (!newVal && !inDiving)
        {
            StopAllCoroutines();
            StartCoroutine(ExitShadowRoutine());
        }
    }

    public float healthRegenCooldown = 2.0f;
    public float healthRegenState = 0f;
    public float healthRegenRate = 2.0f;
    public float enemyDamageMult = 8.0f;

    [SerializeField] private LayerMask blockingLayers;

    public GameObject ghostPrefab; // Référence au prefab fantôme
    private GameObject spawnedGhost;
    private GameObject mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Initialiser la santé sur le serveur
        if (isServer)
        {
            health = maxHealth;
        }

        gameObject.SetActive(true);
        //_lightSources.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ILightSource>());

        // DEBUG : Afficher combien de sources de lumière ont été trouvées
        Debug.Log($"[ShadowPlayer] {_lightSources.Count} sources de lumière détectées :");
        foreach (var light in _lightSources)
        {
            Debug.Log($"  - {light.GetType().Name} (Gardien={light.IsGuardianLight()})");
        }

        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<ThirdPersonController>();
        _characterController = GetComponent<CharacterController>();
        _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem != null)
            _particleSystem.Pause();

        if (_controller != null)
        {
            originalMoveSpeed = _controller.MoveSpeed;
            originalSprintSpeed = _controller.SprintSpeed;
            originalJumpHeight = _controller.JumpHeight;
        }


        _visualRoot = _animator != null ? _animator.transform.parent ?? _animator.transform : transform;

        _modelTransform = transform.Find("Model");

        CreateShadowCircle();

        if (Camera.main != null)
            mainCamera = Camera.main.gameObject;

        if (isLocalPlayer && GameUIManager.Instance != null)
        {
            GameUIManager.Instance.RegisterLocalShadowPlayer(this);
            GameUIManager.Instance.SetPlayerRole(true); // true = Ombre
            Debug.Log("[ShadowPlayer] Enregistré auprès de GameUIManager");
        }
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
        UpdateLightSources();
        // Détection de lumière sur le serveur ET le joueur local
        if (isServer || isLocalPlayer)
        {
            InLightCheck();
        }

        // Gestion de la santé UNIQUEMENT sur le serveur
        if (isServer)
        {
            HandleHealth();
        }

        // Input UNIQUEMENT pour le joueur local
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Q) && !inShadowForm && !inLightSource && !inDiving)
        {
            CmdTryEnterShadow();
        }
        else if (inShadowForm && (Input.GetKeyUp(KeyCode.Q) || inLightSource) && !inDiving)
        {
            CmdExitShadow();
        }
    }

    [Command]
    void CmdTryEnterShadow()
    {
        if (!inShadowForm && !inLightSource && !inDiving)
        {
            inShadowForm = true;
            RpcEnterShadow();
        }
    }

    [Command]
    void CmdExitShadow()
    {
        if (inShadowForm && !inDiving)
        {
            inShadowForm = false;
            RpcExitShadow();
        }
    }

    [ClientRpc]
    void RpcEnterShadow()
    {
        if (!inDiving)
            StartCoroutine(EnterShadowRoutine());
    }

    [ClientRpc]
    void RpcExitShadow()
    {
        if (!inDiving)
        {
            StopAllCoroutines();
            StartCoroutine(ExitShadowRoutine());
        }
    }

    void TryEnterShadow()
    {
        inDiving = true;

        if (!inShadowForm)
            StartCoroutine(EnterShadowRoutine());
    }

    IEnumerator EnterShadowRoutine()
    {
        inDiving = true;

        if (_particleSystem != null)
            _particleSystem.Play();

        if (_modelTransform != null)
            _modelTransform.gameObject.SetActive(false);

        if (_shadowCircle != null)
            _shadowCircle.SetActive(true);

        if (_animator) _animator.SetTrigger("Dive");

        float elapsed = 0f;

        while (elapsed < diveDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_controller != null)
        {
            _controller.MoveSpeed = shadowMoveSpeed;
            _controller.SprintSpeed = shadowMoveSpeed;
            _controller.JumpHeight = 0;
        }

        Debug.Log("Player is now in shadow form (invisible).");
        inDiving = false;
    }

    //void ExitShadow()
    //{
    //    inDiving = true;

    //    StopAllCoroutines();
    //    StartCoroutine(ExitShadowRoutine());
    //}

    IEnumerator ExitShadowRoutine()
    {
        inDiving = true;

        if (_shadowCircle != null)
            _shadowCircle.SetActive(false);

        if (_particleSystem != null)
            _particleSystem.Stop();

        if (_animator) _animator.SetTrigger("Emerge");

        if (_modelTransform != null)
            _modelTransform.gameObject.SetActive(true);

        float elapsed = 0;

        while (elapsed < diveDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_controller != null)
        {
            _controller.MoveSpeed = originalMoveSpeed;
            _controller.SprintSpeed = originalSprintSpeed;
            _controller.JumpHeight = originalJumpHeight;
        }

        Debug.Log("Player emerged from shadow.");
        inDiving = false;
    }

    private void UpdateLightSources()
    {
        // Actualiser la liste toutes les secondes (pas à chaque frame pour la performance)
        if (Time.time - _lastLightSourceUpdate > LIGHT_SOURCE_UPDATE_INTERVAL)
        {
            _lastLightSourceUpdate = Time.time;

            _lightSources.Clear();
            _lightSources.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ILightSource>());

            if (_lightSources.Count > 0)
            {
                Debug.Log($"[ShadowPlayer] {_lightSources.Count} source(s) de lumière détectée(s) :");
                foreach (var light in _lightSources)
                {
                    Debug.Log($"  - {light.GetType().Name} (GameObject: {((MonoBehaviour)light).gameObject.name}, Gardien={light.IsGuardianLight()})");
                }
            }
        }
    }


    private void InLightCheck()
    {
        bool inLight = false;
        bool inEnemyLight = false;

        foreach (var lightSource in _lightSources)
        {
            if (lightSource == null) continue;

            // Vérifier si le joueur est dans la lumière
            bool playerInThisLight = lightSource.IsPlayerInLight(transform.position);

            if (playerInThisLight)
            {
                Vector3 directionToPlayer = (transform.position - lightSource.GetLightPosition()).normalized;
                float distance = Vector3.Distance(transform.position, lightSource.GetLightPosition());

                Debug.Log($"[ShadowPlayer] {lightSource.GetType().Name}: InLight={playerInThisLight}, Distance={distance:F2}m, IsGuardian={lightSource.IsGuardianLight()}");

                // Vérifier qu'il n'y a pas d'obstacle entre la lumière et le joueur
                if (!Physics.Raycast(lightSource.GetLightPosition(), directionToPlayer, distance, blockingLayers))
                {
                    inLight = true;

                    if (lightSource.IsGuardianLight())
                    {
                        inEnemyLight = true;
                        Debug.LogWarning($"[ShadowPlayer] DANS LA LUMIÈRE ENNEMIE : Health: {health:F1}/{maxHealth}");
                        break; // Pas besoin de vérifier les autres
                    }
                }
                else
                {
                    Debug.Log($"[ShadowPlayer] Lumière bloquée par un obstacle"); // Ici source de pb si layer sol mise dans le champ
                }
            }
        }

        // Mettre à jour les SyncVars sur le serveur
        if (isServer)
        {
            inEnemyLightSource = inEnemyLight;
            inLightSource = inLight;
        }
        // Ou envoyer une commande au serveur si on est le client
        else if (isLocalPlayer)
        {
            CmdUpdateLightStatus(inLight, inEnemyLight);
        }

        if (inLight)
            OnEnterLight();
        else
            OnExitLight();
    }  

    [Command]
    void CmdUpdateLightStatus(bool inLight, bool inEnemyLight)
    {
        inLightSource = inLight;
        inEnemyLightSource = inEnemyLight;
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

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerStatus();

        // Spawn fantôme uniquement pour le joueur local
        if (isLocalPlayer && ghostPrefab != null)
        {
            spawnedGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);

            if (mainCamera != null)
                mainCamera.transform.parent = spawnedGhost.transform;

            var ghostController = spawnedGhost.GetComponent<ThirdPersonController>();
            if (ghostController != null)
            {
                ghostController.enabled = true;
            }
        }

        // Désactiver le mesh pour tous les clients
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        // Désactiver les composants de contrôle
        if (_controller != null) _controller.enabled = false;
        if (_characterController != null) _characterController.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!isServer) return; // Les triggers sont gérés par le serveur

        Debug.Log("Player entered trigger.");

        if (collision.gameObject.CompareTag("Key") && !hasKey)
        {
            NetworkServer.Destroy(collision.gameObject);
            hasKey = true;
        }

        if (collision.gameObject.CompareTag("ExitDoor") && hasKey)
        {
            playerStatus = PlayerStatus.Escaped;

            if (GameManager.Instance != null)
                GameManager.Instance.UpdatePlayerStatus();

            gameObject.SetActive(false);
        }
    }


    private void HandleHealth()
    {
        if (!isServer) return; // CRITIQUE : Santé gérée juste par le serveur

        if (!inShadowForm)
            healthRegenState = 0;
        else
            healthRegenState += Time.deltaTime;

        if (inEnemyLightSource)
        {
            //health -= Time.deltaTime * enemyDamageMult;
            float damage = Time.deltaTime * enemyDamageMult;
            health -= damage;
            // Debug à chaque seconde
            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning($"[ShadowPlayer] OMBRE DEAD : Damage={damage:F2}, Health={health:F1}/{maxHealth}");
            }
        }
        else if (healthRegenState >= healthRegenCooldown && health < maxHealth)
        {
            health += Time.deltaTime * healthRegenRate;
        }
    }
}