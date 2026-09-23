public struct Damage
{
    public big value;
    public bool isCrit;
    
    public Damage(big dmg, bool isCrit)
    {
        this.value = dmg;
        this.isCrit = isCrit;
    }
}