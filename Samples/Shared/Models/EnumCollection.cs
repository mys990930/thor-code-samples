using System;

[Serializable]
public enum MoneyType
{
    gold,
    sp,
    stone,
    awakeningBook,
    crystal,
    skillBook,
    goldDungeonTicket,
    xpDungeonTicket,
    stoneDungeonTicket,
    skillDungeonTicket,
    xp,

    Max
}
public enum StatType
{
    BaseStat,
    SpecialStat,
    AwakeningStat,
    ArtifactStat
}
public enum BaseStatType
{
    DMG,
    MAXHP,
    HEAL,
    DEF,
    CRATE,
    CDMG,

    Max
}
public enum SpecialStatType
{
    GOLD_GAIN,
    XP_GAIN,
    MAGNET,
    BOX_RATE,

    Max
}
public enum ArtifactStatType
{
    moru,
    goul,
    baqui,
    sigie,

    Max
}
public enum RewardState
{
    disable,
    enable,
    complete
}
public enum PopUpType
{
    message,
    warning,
    instant,
    reward,
    help
}
public enum ItemType
{
    weapon,
    glove,
    belt,

    Max
}
public enum DrawType
{
    weapon,
    glove,
    belt,

    Max
}
public enum QuestType
{
    dailyQuest,
    achievement,
    repeatedQuest,

    Max
}
public enum QuestConditionType
{
    stageClear,
    lookScenario,
    goldDungeonClear,
    xpDungeonClear,
    etcDungeon0,
    etcDungeon1,
    monsterKill,    // 배정완료
    itemLevelUp,    // 배정 완료
    itemCompose,    // 배정 완료
    charaRankUp,    // 배정 완료
    baseStatUp, // 배정 완료
    specialStatUp,  // 배정 완료
    awakeningUp,    // 배정 완료
    artifactUp, // 배정 완료
    itemDraw,   // 배정 완료
    skillLevelUp,   // 배정 완료
    attendance,     // 배정 완료

    Max
}
public enum TextSoFolderType
{
    resource,
    scriptable
}
public enum PlayController
{
    keyboard,
    joystick
}
public enum AdvertisingBuffType
{
    goldBuff,
    xpBuff
}
public enum BuffState
{
    idle,
    activated,
    impossible
}
public enum ItemGrade
{
    normal,
    rare,
    epic,
    unique,
    legendary,
    divine,

    Max
}
public enum MusicSoundType
{
    yoongterrr,
    bgm1
}
public enum ClickSoundType
{
    defaultClick,
    closePanel,
    statUp,
    itemUpgrade,
    awakening,
    equipping,
    notice,
    drawing,
    purchase
}
public enum BattleSoundType
{
    gameOver,
    beHit,
    hit,
    itemPickUp,
    expPickUp,
    clear,
    levelUp,

    Max
}
public enum SkillSoundType
{
    skill0001,
    skill0002,
    skill0003,
    skill0004,
    skill0005,
    skill0006,
    skill0007,
    skill0008,
    skill0009,
    skill0010,
    skill0011,
    skill0012,
    skill0013_1,
    skill0013_2,
    skill0014_1,
    skill0014_2,
    skill0015,
    //skill3001,
    skill3002,
    skill3003,
    skill3004,
    skill3005,
    skill3011,
    skill3012,
    skill3013,
    skill3013_1,
    skill3014_1,
    skill3014_2,
    skill3014_3,
    skill3015,

    Max
}
public enum SizeUnits
{
    Byte, KB, MB, GB
}
public enum NotifyType
{
    setting,
    dailyQuest,
    achievementQuest,
    mail,
    notice,
    baseStat,
    specialStat,
    awakeningStat,
    artifactStat,
    itemWeaponLv,
    itemGloveLv,
    itemBeltLv,
    itemWeaponCompose,
    itemGloveCompose,
    itemBeltCompose,
    skill,
    dungeon,
    shop,
    drawWeapon,
    drawGlove,
    drawBelt,

    Max
}
public enum SubMenuType
{
    profile,
    quest,
    mail,
    addMenu,
    option,
    notice,
    eventMenu,
    ranking,
    powerSaving,
    adBuff
}
public enum MainMenuType
{
    stat,
    item,
    skill,
    contents,
    shop,
    draw
}
