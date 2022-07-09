using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameField : MonoBehaviour
{
    public static GameField Instance;

    public float fadeSpeed = 0.5f;
    public float WinAnimationTime = 1f;

    public AudioSource MechanismSound;
    public AudioSource OpenSound;
    public AudioSource CloseSound;
    [HideInInspector] public List<Axle> Axles;
    [HideInInspector] public bool GameInProgress = false;
    [HideInInspector] public bool InAnimation;
    [HideInInspector] public bool GameWon;


    private void Awake()
    {
        foreach (var axl in GetComponentsInChildren<Axle>(true))
        {
            Axles.Add(axl);
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!GameInProgress) return;
        if (CheckGameWin())
        {
            OnWinGame();
        }
    }

    private void OnWinGame()
    {
        GameWon = true;
        GameInProgress = false;
        StartCoroutine(GoodRotationAnimation());
    }

    public void OpenGame()
    {
        gameObject.SetActive(true);
        foreach (var gear in GetComponentsInChildren<Gear>())
        {
            if (gear.isStatic)
            {
                continue;
            }

            gear.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            Destroy(gear.gameObject);
        }

        foreach (Axle axl in Axles)
        {
            axl.CurrentGear = Instantiate(axl.DefaultGearPrefab, axl.transform);
        }

        var sprites = GetComponentsInChildren<SpriteRenderer>().Where(x => x.enabled).ToArray();
        foreach (var sprite in sprites)
        {
            var color = sprite.color;
            color = new Color(color.r, color.g, color.b, 0);
            sprite.color = color;
        }

        OpenSound.Play();
        var task = StartCoroutine(OpenCloseAnimation(sprites, 1));
        GameInProgress = true;
    }

    private IEnumerator OpenCloseAnimation(SpriteRenderer[] sprites, float targetAlpha, bool close = false)
    {
        InAnimation = true;
        List<Color> defaultColor = new List<Color>();
        foreach (var sprite in sprites)
        {
            defaultColor.Add(sprite.color);
        }

        for (float i = 0; i < 1; i += Time.deltaTime / fadeSpeed)
        {
            for (int j = 0; j < sprites.Length; j++)
            {
                if (sprites[j] == null || sprites[j].gameObject == null)
                {
                    continue;
                }

                float alpha = Mathf.Lerp(defaultColor[j].a, targetAlpha, i * i);
                sprites[j].color = new Color(defaultColor[j].r, defaultColor[j].g, defaultColor[j].b, alpha);
            }

            yield return null;
        }

        for (int j = 0; j < sprites.Length; j++)
        {
            sprites[j].color = new Color(defaultColor[j].r, defaultColor[j].g, defaultColor[j].b, targetAlpha);
        }

        InAnimation = false;
        if (close)
        {
            gameObject.SetActive(false);
        }
    }

    private bool CheckGameWin()
    {
        return Axles.All(axl => !axl.CurrentGear.moving
                                && axl.CurrentGear.GearType == axl.TargetGear.GearType);
    }

    private IEnumerator GoodRotationAnimation()
    {
        yield return new WaitForFixedUpdate();
        var gears = GetComponentsInChildren<Gear>();
        Queue<Gear> leastGears = new Queue<Gear>();
        leastGears.Enqueue(gears.First());
        gears.First().RotationDirection = 1;
        ContactFilter2D cf = new ContactFilter2D();
        cf.SetLayerMask(LayerMask.GetMask("Gear", "StaticGear"));
        Collider2D[] contacts = new Collider2D[gears.Length];

        bool[] visited = new bool[gears.Length];
        while (leastGears.Count != 0)
        {
            Gear gear = leastGears.Dequeue();
            if (visited[Array.IndexOf(gears, gear)])
            {
                continue;
            }

            float radius = ((CircleCollider2D) gear.collider).radius;
            int count = gear.collider.OverlapCollider(cf, contacts);
            for (int i = 0; i < count; i++)
            {
                Gear g = contacts[i].GetComponentInParent<Gear>();
                if (g.RotationDirection == 0)
                {
                    g.RotationDirection = gear.RotationDirection * -1;
                    leastGears.Enqueue(g);
                }
            }

            visited[Array.IndexOf(gears, gear)] = true;
        }

        MechanismSound.Play();
        float direction = 1;
        float t;
        for (t = 0; t < 1f; t += Time.deltaTime / WinAnimationTime)
        {
            if (t < 0.1)
            {
                float multiplier = Mathf.Lerp(1, 0, t / 0.1f);
                direction = -1 * multiplier;
            }

            if (t >= 0.1)
            {
                direction = 1;
            }

            foreach (var gear in gears)
            {
                float angle = gear.RotationSpeed * Time.deltaTime * gear.RotationDirection * direction;
                gear.transform.Rotate(Vector3.forward, angle);
            }

            yield return null;
        }

        MechanismSound.Stop();
        CloseGame();
    }

    public void CloseGame()
    {
        GameInProgress = false;
        CloseSound.Play();
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        StartCoroutine(OpenCloseAnimation(sprites, 0, true));
    }
}