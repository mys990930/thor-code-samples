using System;
using System.Collections;
using System.Xml;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Pool;

public class BattleTextMesh : MonoBehaviour, IObjectPoolable
{
    public TMP_Text text;
    public IPool pool;
    private RectTransform tr;
    private MeshRenderer ren;

    private Color32 upperColor = new Color32(244, 255, 25, 255);
    private Color32 lowerColor = new Color32(255, 13, 159, 255);
    private Color32 redColor = new Color32(200, 0, 0, 255);

    private void OnEnable()
    {
        text = GetComponent<TMP_Text>();
        tr = GetComponent<RectTransform>();
        ren = GetComponent<MeshRenderer>();
        ren.sortingLayerName = "UI";
        ren.sortingOrder = -1;
    }

    public void InitializePool(IPool pool)
    {
        this.pool = pool;
    }

    public void Show(Damage dmg, Vector2 targetPos)
    {
        BeginEffect(dmg.value, targetPos, dmg.isCrit ? ShowCritEffect() : ShowEffect());
    }

    public void ShowGoldValue(big value, Vector2 targetPos)
    {
        BeginEffect(value, targetPos, ShowGoldEffect());
    }

    public void ShowHitDamage(Damage dmg, Vector2 targetPos)
    {
        BeginEffect(dmg.value, targetPos, ShowHitEffect());
    }

    private void BeginEffect(big value, Vector2 targetPos, IEnumerator effect)
    {
        bool started = false;
        try
        {
            text.text = value.ToShortString();
            tr.position = targetPos;
            started = StartCoroutine(ReturnAfterEffect(effect)) != null;
        }
        finally
        {
            if (!started) pool.Release(gameObject);
        }
    }

    private IEnumerator ReturnAfterEffect(IEnumerator effect)
    {
        try
        {
            while (effect.MoveNext()) yield return effect.Current;
        }
        finally
        {
            (effect as IDisposable)?.Dispose();
            pool.Release(gameObject);
        }
    }

    private IEnumerator ShowEffect()
    {
        text.fontSize = 6;
        text.alpha = 0f;
        text.color = Color.white;
        text.colorGradient = new VertexGradient(Color.white, Color.white, Color.white, Color.white);
        while (text.fontSize < 8)
        {
            text.fontSize += 0.3f * GameManager.frameConst;
            text.alpha += 0.15f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
        }
        yield return BattleManager.instance.waitPause;
        int frame = 0;
        while (frame < 20)
        {
            text.alpha -= 0.07f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
            frame++;
        }
    }
    private IEnumerator ShowGoldEffect()
    {
        text.fontSize = 6;
        text.alpha = 0f;
        text.color = Color.gold;
        text.colorGradient = new VertexGradient(Color.gold, Color.gold, Color.gold, Color.gold);
        while (text.fontSize < 8)
        {
            text.fontSize += 0.3f * GameManager.frameConst;
            text.alpha += 0.15f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
        }
        yield return BattleManager.instance.waitPause;
        int frame = 0;
        while (frame < 20)
        {
            text.alpha -= 0.07f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
            frame++;
        }
    }

    private IEnumerator ShowCritEffect()
    {
        text.fontSize = 7;
        text.alpha = 0f;
        text.colorGradient = new VertexGradient(upperColor, upperColor, lowerColor, lowerColor);

        while (text.fontSize < 9)
        {
            text.fontSize += 0.3f * GameManager.frameConst;
            text.alpha += 0.15f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
        }
        yield return BattleManager.instance.waitPause;
        int frame = 0;
        while (frame < 20)
        {
            text.alpha -= 0.07f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
            frame++;
        }
    }

    private IEnumerator ShowHitEffect()
    {
        text.fontSize = 6;
        text.alpha = 0f;
        text.colorGradient = new VertexGradient(redColor, redColor, redColor, redColor);

        while (text.fontSize < 9)
        {
            text.fontSize += 0.3f * GameManager.frameConst;
            text.alpha += 0.15f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
        }
        yield return BattleManager.instance.waitPause;
        int frame = 0;
        while (frame < 20)
        {
            text.alpha -= 0.07f * GameManager.frameConst;
            yield return BattleManager.instance.waitPause;
            frame++;
        }
    }

    public void Disable()
    {
        StopAllCoroutines();
    }
}