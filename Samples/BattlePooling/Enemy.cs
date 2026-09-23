using Assets.FantasyMonsters.Scripts;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy: MonoBehaviour, IObjectPoolable, BattlePauseListener, IHittable, IMonster
{
    public int idx;

    public IPool pool;
    public string monsterName;
    public big MAXHP = 100;
    public big HP = 100;
    public int EXP = 10;
    public int SPD = 10;
    public big ATK {  get; set; }

    private Vector2 initPos = new Vector2(500, 500);
    private bool isPaused = false;
    public bool isDead { get; set; }
    private bool isDying = false;
    private WaitForSeconds wait = new WaitForSeconds(0.5f);

    public Dictionary<string, GameObject> viewPrefabs = new Dictionary<string, GameObject>();
    public Transform tr;
    private Monster sprite;
    private List<SpriteRenderer> spriteRenderers;
    private CapsuleCollider2D col;
    private Rigidbody2D rig;
    private Camera cam;
    private IAssetLoadManager assetLoadManager;

    private Chara chara;

    private string expMatCode = "expIcon_mat";

    private void Awake()
    {
        tr = GetComponent<Transform>();
        col = GetComponent<CapsuleCollider2D>();
        rig = GetComponent<Rigidbody2D>();
        sprite = GetComponent<Monster>();
        sprite.Animator.speed = 0.4f;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();

        assetLoadManager = new AssetLoadManager();
        tr.position = initPos;
    }

    private void OnEnable()
    {
        col.enabled = true;
        isDead = false;
        isDying = false;
        knockbackCool = true;
        isPaused = false;
        sprite.Animator.speed = 0.4f;
    }

    public void RegisterListener()
    {
        BattleEventManager.instance.onBattlePause += OnBattlePause;
        BattleEventManager.instance.onBattleResume += OnBattleResume;
    }

    public void DeregisterListener()
    {
        BattleEventManager.instance.onBattlePause -= OnBattlePause;
        BattleEventManager.instance.onBattleResume -= OnBattleResume;
    }

    public void InitializePool(IPool pool)
    {
        this.pool = pool;
    }

    public void Init(string enemyCode, Vector2 summonPos)
    {
        RegisterListener();
        isDead = false;
        isDying = false;

        tr.position = summonPos;
        EnemyInfo enemyInfo = ClientDataContainer.instance.enemyInfoSO.enemyInfos[enemyCode];

        monsterName = enemyInfo.monsterName;

        big coeff = BattleManager.instance.battleDataContainer.currentStageData.atkCoeff;

        ATK = enemyInfo.ATK * coeff;
        MAXHP = enemyInfo.HP * coeff;
        HP = MAXHP;
        EXP = enemyInfo.EXP;
        SPD = enemyInfo.SPD;

        InitSprite();
        if (BattleManager.instance.currentSurvivalState == SurvivalBattleState.ePauseState ||
            BattleManager.instance.currentBattleState == BattleState.ePauseState)
            OnBattlePause();
        else
            OnBattleResume();
        col.enabled = true;
        cam = Camera.main;
        chara = BattleManager.instance.chara;

        idx = BattleManager.instance.summonedObjNum++;
        BattleManager.instance.liveEnemyDict.Add(idx, this);
        BattleManager.instance.liveEnemyNum++;

        if (BattleManager.instance.currentSurvivalState == SurvivalBattleState.eOnSurvivalBattle ||
            BattleManager.instance.currentBattleState == BattleState.eBattleState)
        {
            StartCoroutine(CheckDistance());
        }
    }

    private void InitSprite()
    {
        sprite.SetState(MonsterState.Walk);
        foreach (var ren in spriteRenderers)
        {
            ren.color = Color.white;
        }
    }

    private void LateUpdate()
    {
        if (BattleManager.instance.currentBattleState == BattleState.eBattleState ||
            BattleManager.instance.currentSurvivalState == SurvivalBattleState.eOnSurvivalBattle)
        {
            if (!isPaused && !isDead)
            {
                tr.position = Vector2.MoveTowards(tr.position, chara.transform.position, Time.deltaTime * SPD * 0.1f);
                if (chara.transform.position.x - tr.position.x > 0)
                {
                    tr.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                }
                else
                {
                    tr.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }
            }
        }
    }
    public void OnBattlePause()
    {
        isPaused = true;
        sprite.Animator.speed = 0f;
    }

    public void OnBattleResume()
    {
        isPaused = false;
        sprite.Animator.speed = 0.4f;
    }

    public bool IsInViewport()
    {
        Vector2 viewportPos = cam.WorldToViewportPoint(tr.position);
        if (0 <= viewportPos.x && viewportPos.x <= 1 && 0 <= viewportPos.y && viewportPos.y <= 1)
        {
            return true;
        }
        return false;
    }

    public void Hit(big initDmg, bool shouldKnockback)
    {
        if (isDead || isDying) return;
        if (IsInViewport())
        {
            AudioManager.instance.PlaySoundEffectOneShot(BattleSoundType.hit);
            if (shouldKnockback) StartCoroutine(Knockback());
            Damage dmg = chara.CalculateDamage(initDmg);
            HP -= dmg.value;
            if (HP <= 0)
            {
                OnDeath();
            }
            ShowDamage(dmg);
        }
    }

    public void Hit(big initDmg, bool shouldKnockback, Action<bool> callback)
    {
        if (isDead || isDying) return;
        if (IsInViewport())
        {
            AudioManager.instance.PlaySoundEffectOneShot(BattleSoundType.hit);
            if (shouldKnockback) StartCoroutine(Knockback());
            Damage dmg = chara.CalculateDamage(initDmg);
            HP -= dmg.value;
            if (HP <= 0)
            {
                OnDeath();
                callback(true);
            }
            ShowDamage(dmg);
        }
    }

    public void Kill()
    {
        if (isDead || isDying) return;
        big dmg = MAXHP;
        StartCoroutine(Knockback());
        OnDeath();
        ShowDamage(new Damage(dmg, true));
    }

    private void ShowDamage(Damage dmg)
    {
        if (!ObjectPoolManager.instance.dmgObjPool.TryGet(out GameObject obj)) return;
        BattleTextMesh dmgText = obj.GetComponent<BattleTextMesh>();
        dmgText.Show(dmg, new Vector2(tr.position.x, tr.position.y + 0.1f));
    }

    private IEnumerator Knockback()
    {
        if (knockbackCool)
        {
            int frame = 0;
            knockbackCool = false;
            Vector2 charaPos = BattleManager.instance.charaObj.transform.position;
            Vector2 pos = tr.position;
            Vector2 targetPos = new Vector2(charaPos.x - (charaPos.x - pos.x) * 1.1f, charaPos.y - (charaPos.y - pos.y) * 1.1f);
            while (frame < GameManager.fps * 0.2f)
            {
                tr.position = Vector2.MoveTowards(tr.position, targetPos, 0.02f * GameManager.frameConst);
                yield return BattleManager.instance.waitPause;
                frame++;
            }
            StartCoroutine(KnockbackCooldown());
        }
    }
    private bool knockbackCool = true;
    private IEnumerator KnockbackCooldown()
    {
        for(int cooltime = 0; cooltime < 5; cooltime++)
        {
            yield return new WaitForSeconds(0.1f);
        }
        knockbackCool = true;
    }

    public void OnDeath()
    {
        if (isDead || isDying) return;

        isDying = true;
        Disable();
        StartCoroutine(Die());
        //kill event 발생?
        int r = UnityEngine.Random.Range(0, 2);
        BattleManager.instance.RecordEnemyKill();
        switch (BattleManager.instance.currentStageType)
        {
            case StageType.MAIN:
                if (BattleManager.instance.battleMode == BattleMode.eSurvivalMode)
                {
                    CreateEXP();
                }
                else if (BattleManager.instance.battleMode == BattleMode.eIdleMode)
                {
                    CreateRandomItem();
                }
                break;
            case StageType.GOLD_DUNGEON:
                if (r == 0) CreateEXP();
                else CreateItem(MoneyType.gold);
                break;
            case StageType.STONE_DUNGEON:
                if (r == 0) CreateEXP();
                else CreateItem(MoneyType.stone);
                break;
            default:
                break;
        }
    }

    private void CreateEXP()
    {
        float bt = BattleManager.instance.battleTime; // 초

        // 정규화 구간
        float t3_6 = Mathf.Clamp01((bt - 180f) / 180f);  // 3→6분
        float t6_12 = Mathf.Clamp01((bt - 360f) / 360f); // 6→12분

        // 50EXP: 3분 이전 0%, 3~6분 0→50%, 이후 50% 유지
        float p50 = Mathf.SmoothStep(0f, 1f, t3_6) * 0.50f; // [0,0.5]
        p50 = Mathf.Min(p50, 0.50f);

        // 100EXP: 6분 이전 0%, 6~12분 0→20%, 이후 20% 유지
        // (이전 예시: 6분에 5%에서 12분 20%였던 걸, 시작점을 0%로 내림)
        float p100 = Mathf.SmoothStep(0f, 1f, t6_12) * 0.20f; // [0,0.2]
        p100 = Mathf.Min(p100, 0.20f);

        // 3분 이전엔 강제로 p50=0, 6분 이전엔 p100=0 (명시적 안전장치)
        if (bt < 180f) p50 = 0f;
        if (bt < 360f) p100 = 0f;

        // 누적 확률이 1을 넘지 않도록 클램프
        float cap = Mathf.Min(1f, p50 + p100);
        if (cap > 1f)
        {
            float scale = 1f / cap;
            p50 *= scale;
            p100 *= scale;
        }

        // 샘플링
        float r = UnityEngine.Random.value;
        int finalExp = (r < p100) ? 10 : (r < p100 + p50) ? 5 : 1;
        finalExp *= EXP;

        // 스폰
        if (ObjectPoolManager.instance.expObjPool.TryGet(out GameObject obj))
        {
            var exp = obj.GetComponent<EXP>();
            exp.Init(tr.position, finalExp);
        }
        else
        {
            chara.AddExperience((int)(finalExp * chara.battleStat.EXPEarnFactor));
        }
    }

    private void CreateRandomItem()
    {
        BattleDataContainer container = BattleManager.instance.battleDataContainer;
        int rand = UnityEngine.Random.Range(0, 7);
        if (rand == 0)
        {
            Reward reward = container.GetRandomItem();
            if (reward.moneyType == MoneyType.xp) GetXp(reward.value);
            else
            {
                NetworkManager.instance.AddToBuffer(reward.moneyType, reward.value);
                DataContainer.instance.money.wallet[reward.moneyType].value += reward.value;
                string code = ClientDataContainer.instance.moneyDataTable[(int)reward.moneyType].materialCode;
                Material icon = ClientMainAssetContainer.instance.materialDatas[code];
                //Vector2 position = Camera.main.WorldToScreenPoint(tr.position);
                Vector2 position = tr.position;
                RewardEffectSpawner.instance.SpawnRewardEffect(MoneyType.crystal, position, icon);
            }
        }
    }

    private void CreateItem(MoneyType type)
    {
        BattleDataContainer container = BattleManager.instance.battleDataContainer;
        big value = container.GetDropItemValue(type);

        BattleManager.instance.battleDataContainer.earnedMoney[type] += value;
        BattleManager.instance.currentBattle.surviveUIView.ShowItemGetEffect(tr.position, type);

        string code = ClientDataContainer.instance.moneyDataTable[(int)type].materialCode;
        Material icon = ClientMainAssetContainer.instance.materialDatas[code];
        //RewardEffectSpawner.instance.SpawnRewardEffect(type, tr.position, icon);
    }

    private IEnumerator Die()
    {
        int frame = 0;
        sprite.SetState(MonsterState.Idle);

        while (frame < GameManager.fps * 0.25f) //0.25초간
        {
            foreach (var ren in spriteRenderers)
            {
                ren.color = new Color(1, 1, 1, ren.color.a - 0.04f * GameManager.frameConst);
            }
            yield return BattleManager.instance.waitPause;
            frame++;
        }
        DeregisterListener();
        transform.position = initPos;
        yield return BattleManager.instance.waitPause;
        pool.Release(this.gameObject);
    }

    private IEnumerator CheckDistance()
    {
        while (true)
        {
            if (Vector2.Distance(tr.position, chara.transform.position) > 7.5f)
            {
                Disable();
                DeregisterListener();
                pool.Release(this.gameObject);
            }
            yield return wait;
        }
    }

    public void Disable()
    {
        if (isDead) return;

        isDead = true;
        col.enabled = false;
        if (BattleManager.instance.liveEnemyDict.Remove(idx))
        {
            BattleManager.instance.liveEnemyNum = Mathf.Max(0, BattleManager.instance.liveEnemyNum - 1);
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Skill"))
        {
            //skill에 달려있음
        }
    }

    private void OnDestroy()
    {
        DeregisterListener();
    }
    private void GetXp(big value)
    {
        big currentExp = DataContainer.instance.userData.exp;

        Material icon = ClientMainAssetContainer.instance.materialDatas[expMatCode];

        RewardEffectSpawner.instance.SpawnExpEffect(tr.position, icon, currentExp, (result) =>
        {
            DataContainer.instance.userData.exp += value;
            NetworkManager.instance.xpBufferValue += value;
        });
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
