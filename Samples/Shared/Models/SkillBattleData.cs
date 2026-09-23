public class SkillBattleData
{
    public SkillType type;
    public string code;
    public string name;
    public string description;
    public int grade;
    //public int dmg;

    public SkillBattleData Copy()
    {
        SkillBattleData copy = new SkillBattleData();
        copy.type = type;
        copy.code = code;
        copy.name = name;
        copy.description = description;
        copy.grade = grade;
        //copy.dmg = dmg;
        return copy;
    }
}