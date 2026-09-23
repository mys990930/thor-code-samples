using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Chara : MonoBehaviour, CharaInfoChangeListener, BattlePauseListener
{
    public enum CharaState { eIdle, eOnBattle, eDead }
    public CharaState charaState = CharaState.eIdle;

    public Autopilot autopilot;
    public bool isAutopilot = false;

    private big hp = 100;
    private int exp = 0;

    public big HP { get { return hp; } set { hp = (value <= stat.MAXHP ? value : stat.MAXHP); } }
    public int EXP {  get { return exp; } set { exp = value; } }

    public int LV = 1;

    private Transform tr;
    public Transform charaSprite;
    public CapsuleCollider2D col;
    public IAnimator animator;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColorPool = new Color[50];
    private InputManager inputManager;

    public SerializableDictionary<string, NormalSkill> normalSkills = new SerializableDictionary<string, NormalSkill>();
    public Dictionary<string, CombinationSkill> combiSkills = new Dictionary<string, CombinationSkill>();
    public Dictionary<string, PassiveSkill> passiveSkills = new Dictionary<string, PassiveSkill>();
    public Transform skillParentObject;

    [HideInInspector] public CharaStat stat;
    [HideInInspector] public CharaBattleStat battleStat =  new CharaBattleStat();
    [HideInInspector] public bool levelUpControl = false;
    public EffectController effectController;


    private bool isPaused = false;
    private float tickInterval = 0.33f;
    public byte isNotInvincible = 1;
    public bool IsInvincible => isNotInvincible == 0;
    private Dictionary<GameObject, float> enemyTimers = new Dictionary<GameObject, float>();


    private void Awake()
    {
        RegisterListener();
    }

    private void OnEnable()
    {
        autopilot = GetComponent<Autopilot>();

        tr = GetComponent<Transform>();
        charaSprite = tr.GetChild(0);
        col = GetComponent<CapsuleCollider2D>();
        animator = new AnimationAdapter(charaSprite.GetComponent<Character>());
        inputManager = InputManager.instance;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColorPool[i] = spriteRenderers[i].color;
        }

        stat = StatsDataContainer.instance.charaStat;
        //CharaBuilder로 옮길지? (너무 길어짐 방지)

        hp = stat.MAXHP;
        skillParentObject = Instantiate(ClientMainAssetContainer.instance.SkillParentPrefab).transform;

        effectController = transform.GetChild(1).GetComponent<EffectController>();
        StartCoroutine(Recover());
    }

    public void RegisterListener()
    {
        EventManager.instance.onCharaLookChange += OnCharaLookChange;
        EventManager.instance.onCharaStatChange += OnCharaStatChange;
        BattleEventManager.instance.onBattlePause += OnBattlePause;
        BattleEventManager.instance.onBattleResume += OnBattleResume;
    }

    public void DeregisterListener()
    {
        EventManager.instance.onCharaLookChange -= OnCharaLookChange;
        EventManager.instance.onCharaStatChange -= OnCharaStatChange;
        BattleEventManager.instance.onBattlePause -= OnBattlePause;
        BattleEventManager.instance.onBattleResume -= OnBattleResume;
    }

    // Update is called once per frame
    private void Update()
    {
        skillParentObject.GetChild(0).transform.position = tr.position;
        if (!isPaused)
        {
            switch (charaState)
            {
                case CharaState.eOnBattle:
                    if (inputManager.hasKeyboardInput || inputManager.hasTouchInput)
                    {
                        inputManager.hasKeyboardInput = false;
                        animator.SetMovement(CharaMovement.WALK);
                        tr.Translate(
                            inputManager.direction.x * (float)stat.SPD * battleStat.SpeedFactor * Time.deltaTime,
                            inputManager.direction.y * (float)stat.SPD * battleStat.SpeedFactor * Time.deltaTime,
                            0);
                    }
                    else
                    {
                        animator.SetMovement(CharaMovement.IDLE);
                    }
                    if (inputManager.lookingRight) charaSprite.rotation = Quaternion.Euler(0, 0, 0);
                    else charaSprite.rotation = Quaternion.Euler(0, 180, 0);
                    /*
                    if (!isAutopilot)
                    {
                        if (inputManager.hasKeyboardInput || inputManager.hasTouchInput)
                        {
                            inputManager.hasKeyboardInput = false;
                            animator.SetMovement(CharaMovement.WALK);
                            tr.Translate(
                                inputManager.direction.x * stat.SPD * battleStat.SpeedFactor * Time.deltaTime,
                                inputManager.direction.y * stat.SPD * battleStat.SpeedFactor * Time.deltaTime,
                                0);
                        }
                        else
                        {
                            animator.SetMovement(CharaMovement.IDLE);
                        }
                        if (inputManager.lookingRight) charaSprite.rotation = Quaternion.Euler(0, 0, 0);
                        else charaSprite.rotation = Quaternion.Euler(0, 180, 0);
                    }
                    */
                    break;
                case CharaState.eDead:
                    Debug.Log("Dead");
                    break;
            }
        }
    }

    public void ToggleAutopilot(bool isAutopilot)
    {
        this.isAutopilot = isAutopilot;
        if (charaState == CharaState.eOnBattle)
        {
            if (isAutopilot)
            {
                autopilot.StartAutopilot();
            }
            else autopilot.StopAutopilot();
        }
    }

    public void ChangeState(CharaState state)
    {
        switch (state)
        {
            case CharaState.eIdle:
                charaState = CharaState.eIdle;
                break;
            case CharaState.eOnBattle:
                charaState = CharaState.eOnBattle;
                StartBattle();
                break;
            case CharaState.eDead:
                charaState = CharaState.eDead;
                autopilot.StopAutopilot();
                //EventManager.instance.onBattleEnd();
                break;
            default:
                break;
        }
    }

    private void StartBattle()
    {
        foreach (NormalSkill skill in normalSkills.Values)
        {
            //skill.Init();
        }
        if (isAutopilot)
        {
            autopilot.StartAutopilot();
        }
    }

    public Damage CalculateDamage(big dmg)
    {
        bool isCrit;
        big value;
        float rand = UnityEngine.Random.Range(0f, 1f);
        if (stat.CRATE <= 0 || (stat.CRATE < 1 && rand >= stat.CRATE))
        {
            value = dmg;
            isCrit = false;
        }
        else
        {
            value = dmg * (1 + stat.CDMG);// *0.01f);
            isCrit = true;
        }
        
        return new Damage(value, isCrit);

    }

    public void AddExperience(int exp)
    {
        if (levelUpControl) return;
        EXP += exp;
        BattleEventManager.instance.onEXPGet(exp);
        if (EXP >= ClientDataContainer.instance.levelTableSO.table[LV].exp)
        {
            LV += 1;
            EXP -= ClientDataContainer.instance.levelTableSO.table[LV - 1].exp;
            BattleEventManager.instance.PauseBattle();
            BattleEventManager.instance.onLevelUp();
        }
    }

    public void EnableInvincible(bool isEnabled)
    {
        if (isEnabled)
        {
            foreach (SpriteRenderer ren in spriteRenderers)
            {
                ren.color = Color.yellow;
            }
            isNotInvincible = 0;
        }
        else
        {
            int cnt = 0;
            foreach (SpriteRenderer ren in spriteRenderers)
            {
                ren.color = originalColorPool[cnt++];
            }
            isNotInvincible = 1;
        }
    }

    private bool CanTakeContactDamage()
    {
        return BattleManager.instance.currentSurvivalState == SurvivalBattleState.eOnSurvivalBattle
            && charaState == CharaState.eOnBattle
            && !isPaused
            && !IsInvincible;
    }

    private void OnHit(IMonster enemy)
    {
        if (CanTakeContactDamage())
        {
            Damage dmg = new Damage(enemy.ATK * 0.33f * GameManager.frameConst * isNotInvincible, false);
            dmg.value -= stat.DEF;
            dmg.value = dmg.value <= 1 ? 1 : dmg.value;
            HP -= dmg.value;
            if (HP <= 0 && charaState != CharaState.eDead) Kill();
            if (ObjectPoolManager.instance.dmgObjPool.TryGet(out GameObject obj))
            {
                BattleTextMesh dmgText = obj.GetComponent<BattleTextMesh>();
                dmgText.ShowHitDamage(dmg, new Vector2(tr.position.x, tr.position.y + 0.5f));
            }
        }
    }

    private IEnumerator Recover()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            yield return BattleManager.instance.waitPause;
            HP += stat.MAXHP * battleStat.RecoveryFactor;
        }
    }

    public void OnBattlePause()
    {
        isPaused = true;
        animator.Pause();
    }

    public void OnBattleResume()
    {
        isPaused = false;
        animator.Resume();
    }

    public void Kill()
    {
        ChangeState(CharaState.eDead);
        animator.ShowDeathAnim();
        BattleManager.instance.FinishBattle(StageResultState.FAIL);
    }

    public void OnCharaStatChange()
    {
    }

    public void OnCharaLookChange()
    {
    }

    public void UpdateSkill(SkillBattleData skillData)
    {
        List<string> worldPositionSkills = new() { "1140013", "1143013", "1140014", "1143014", "1140011", "1143011", "1142009" };
        switch (skillData.type)
        {
            case SkillType.ACTIVE:
                if (skillData.grade == 1)
                {
                    GameObject skillObj = Instantiate(ClientMainAssetContainer.instance.skillPrefabs[skillData.code]);
                    if(!worldPositionSkills.Contains(skillData.code))
                        skillObj.transform.parent = skillParentObject.GetChild(0);
                    else
                        skillObj.transform.parent = skillParentObject.GetChild(1);
                    NormalSkill skill = skillObj.GetComponent<NormalSkill>();
                    normalSkills.Add(skillData.code, skill);
                    skill.Init();
                }
                else if (skillData.grade <= 4)
                {
                    NormalSkill skill = normalSkills[skillData.code];
                    skill.LevelUp();
                }
                else
                {
                    GameManager.Error("Invalid Skill Grade Value");
                }
                break;
            case SkillType.PASSIVE:
                if (skillData.grade == 1)
                {
                    GameObject skillObj = Instantiate(ClientMainAssetContainer.instance.skillPrefabs[skillData.code]); 
                    if (!worldPositionSkills.Contains(skillData.code))
                        skillObj.transform.parent = skillParentObject.GetChild(0);
                    else
                        skillObj.transform.parent = skillParentObject.GetChild(1);
                    PassiveSkill skill = skillObj.GetComponent<PassiveSkill>();
                    passiveSkills.Add(skillData.code, skill);
                    skill.Init();
                }
                else if (skillData.grade <= 4)  
                {
                    PassiveSkill skill = passiveSkills[skillData.code];
                    skill.LevelUp();
                }
                else
                {
                    GameManager.Error("Invalid Skill Grade Value");
                }
                break;
            case SkillType.COMBINATION:
                if (true)
                {
                    GameObject skillObj = Instantiate(ClientMainAssetContainer.instance.skillPrefabs[skillData.code]);
                    if (!worldPositionSkills.Contains(skillData.code))
                        skillObj.transform.parent = skillParentObject.GetChild(0);
                    else
                        skillObj.transform.parent = skillParentObject.GetChild(1);
                    CombinationSkill skill = skillObj.GetComponent<CombinationSkill>();
                    combiSkills.Add(skillData.code, skill);

                    string combiSkillCode = ClientSkillsDataContainer.instance.skillAssetDatas[skillData.code].combinationTargetCode;
                    GameObject removeTarget = normalSkills[combiSkillCode].gameObject;
                    normalSkills.Remove(combiSkillCode);
                    removeTarget.SetActive(false);

                    skill.Init();
                }
                break;
            default:
                throw new System.NotImplementedException();
        }
        
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (!CanTakeContactDamage() || !col.CompareTag("Enemy")) return;

        GameObject enemyObj = col.gameObject;

        // 딕셔너리에 없으면 등록
        if (!enemyTimers.ContainsKey(enemyObj))
            enemyTimers[enemyObj] = 0f;

        // 타이머 갱신
        enemyTimers[enemyObj] -= Time.deltaTime;
        if (enemyTimers[enemyObj] <= 0f)
        {
            IMonster monster = col.GetComponent<IMonster>();
            if (monster != null)
                OnHit(monster);

            enemyTimers[enemyObj] = tickInterval; // 타이머 리셋
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
            enemyTimers.Remove(col.gameObject); // 빠져나가면 타이머 제거
    }


    private void OnDestroy()
    {
        Destroy(skillParentObject.gameObject);
        DeregisterListener();
    }
}
