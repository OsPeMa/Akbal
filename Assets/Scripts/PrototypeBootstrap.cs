using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PrototypeBootstrap : MonoBehaviour
{
    [Tooltip("Optional. If null, a default in-memory pattern is created.")]
    public RhythmPattern defaultPattern;

    [Header("Enemies")]
    public int enemyCount = 3;
    public float enemySpawnRadius = 7f;

    void Awake()
    {
        BuildGround();
        var player = BuildPlayer();
        BuildCamera(player.transform);
        EnsureEventSystem();
        var canvas = BuildCanvas();
        BuildConductor();
        BuildConductorDebugHUD(canvas);
        var hud = BuildHUD(canvas);
        var rhythmInput = BuildRhythmInputReader();
        var mini = BuildMinigame(rhythmInput, hud);
        BuildFeedbackHUD(canvas, mini);

        var pc = player.GetComponent<PlayerController>();
        pc.rhythm = mini;

        BuildHealthBar(canvas, player.GetComponent<Health>());

        SpawnEnemies(player.transform);

        hud.gameObject.SetActive(false);
    }

    void BuildGround()
    {
        if (GameObject.Find("Ground") != null) return;
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        var mr = ground.GetComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(mr.sharedMaterial) { color = new Color(0.18f, 0.2f, 0.24f) };
    }

    GameObject BuildPlayer()
    {
        var playerGo = GameObject.Find("Player");
        if (playerGo == null)
        {
            playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player";
            playerGo.transform.position = new Vector3(0f, 1f, 0f);
            var mr = playerGo.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(mr.sharedMaterial) { color = new Color(0.95f, 0.85f, 0.4f) };
        }
        playerGo.tag = "Player";
        if (playerGo.GetComponent<Rigidbody>() == null) playerGo.AddComponent<Rigidbody>();
        if (playerGo.GetComponent<Health>() == null) playerGo.AddComponent<Health>();
        if (playerGo.GetComponent<PlayerInputReader>() == null) playerGo.AddComponent<PlayerInputReader>();
        if (playerGo.GetComponent<PlayerMover>() == null) playerGo.AddComponent<PlayerMover>();
        if (playerGo.GetComponent<PlayerDash>() == null) playerGo.AddComponent<PlayerDash>();
        if (playerGo.GetComponent<PlayerParry>() == null) playerGo.AddComponent<PlayerParry>();
        if (playerGo.GetComponent<PlayerController>() == null) playerGo.AddComponent<PlayerController>();
        return playerGo;
    }

    void BuildCamera(Transform target)
    {
        var camGo = Camera.main != null ? Camera.main.gameObject : null;
        if (camGo == null)
        {
            camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);

        var follow = camGo.GetComponent<OrbitCameraFollow>() ?? camGo.AddComponent<OrbitCameraFollow>();
        follow.target = target;
        follow.pitch = 30f;
        follow.yaw = 45f;
        follow.distance = 25f;

        Quaternion isoRot = Quaternion.Euler(follow.pitch, follow.yaw, 0f);
        camGo.transform.SetPositionAndRotation(
            target.position - isoRot * Vector3.forward * follow.distance, isoRot);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    GameObject BuildCanvas()
    {
        var canvasGo = GameObject.Find("HudCanvas");
        if (canvasGo != null) return canvasGo;
        canvasGo = new GameObject("HudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = canvasGo.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        var sc = canvasGo.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920f, 1080f);
        return canvasGo;
    }

    RhythmHUD BuildHUD(GameObject canvas)
    {
        var hudGo = GameObject.Find("RhythmHUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("RhythmHUD", typeof(RectTransform));
            hudGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)hudGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        return hudGo.GetComponent<RhythmHUD>() ?? hudGo.AddComponent<RhythmHUD>();
    }

    Conductor BuildConductor()
    {
        var existing = FindFirstObjectByType<Conductor>();
        if (existing != null) return existing;
        var go = new GameObject("Conductor", typeof(AudioSource), typeof(Conductor));
        return go.GetComponent<Conductor>();
    }

    void BuildConductorDebugHUD(GameObject canvas)
    {
        if (GameObject.Find("ConductorDebugHUD") != null) return;
        var go = new GameObject("ConductorDebugHUD", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        go.AddComponent<ConductorDebugHUD>();
    }

    RhythmInputReader BuildRhythmInputReader()
    {
        var existing = FindFirstObjectByType<RhythmInputReader>();
        if (existing != null) return existing;
        return new GameObject("RhythmInput").AddComponent<RhythmInputReader>();
    }

    RhythmMinigame BuildMinigame(RhythmInputReader input, RhythmHUD hud)
    {
        var mini = FindFirstObjectByType<RhythmMinigame>();
        if (mini == null) mini = new GameObject("RhythmMinigame").AddComponent<RhythmMinigame>();
        if (defaultPattern == null) defaultPattern = ScriptableObject.CreateInstance<RhythmPattern>();
        mini.pattern = defaultPattern;
        mini.input = input;
        mini.hud = hud;
        hud.minigame = mini;
        return mini;
    }

    void BuildFeedbackHUD(GameObject canvas, RhythmMinigame mini)
    {
        if (GameObject.Find("RhythmFeedbackHUD") != null) return;
        var go = new GameObject("RhythmFeedbackHUD", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var fb = go.AddComponent<RhythmFeedbackHUD>();
        fb.minigame = mini;
    }

    void BuildHealthBar(GameObject canvas, Health health)
    {
        var existing = GameObject.Find("HealthBar");
        if (existing != null) return;
        var go = new GameObject("HealthBar", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var hb = go.AddComponent<HealthBarHUD>();
        hb.source = health;
    }

    void SpawnEnemies(Transform playerTf)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            float a = (i / (float)enemyCount) * Mathf.PI * 2f;
            var pos = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * enemySpawnRadius;
            pos.y = 1f;
            SpawnEnemy(pos, playerTf);
        }
    }

    GameObject SpawnEnemy(Vector3 pos, Transform target)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Enemy";
        go.transform.position = pos;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(mr.sharedMaterial) { color = new Color(0.7f, 0.25f, 0.25f) };
        go.AddComponent<Rigidbody>();
        var health = go.AddComponent<Health>();
        var e = go.AddComponent<Enemy>();
        e.target = target;
        var bar = go.AddComponent<EnemyCorruptionBar>();
        bar.source = health;
        return go;
    }
}
