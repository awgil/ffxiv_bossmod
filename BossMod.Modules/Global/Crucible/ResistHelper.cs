namespace BossMod.Global.Crucible;

public class ResistHelper(BossModule module) : BossComponent(module)
{
    public enum Resistance
    {
        Poison,
        Paralysis,
        Blind,
        Petrification,
        Sleep,
        Doom,
        Stun,
        Physical,
        Magic,

        Count
    }

    /*
    public override void AddGlobalHints(GlobalHints hints)
    {
        for (var r = Resistance.Poison; r < Resistance.Count; r++)
        {
            var exp = StatusExpire[(int)r];
            if (exp == DateTime.MaxValue)
                hints.Add($"{r} immunity (permanent)");
            else if (exp > WorldState.CurrentTime)
                hints.Add($"{r} immunity for {(exp - WorldState.CurrentTime).TotalSeconds:f1}s");
        }
    }
    */

    readonly List<(uint Type, Resistance Resistance, DateTime Expiration)> _buffs = [];

    public readonly DateTime[] StatusExpire = new DateTime[(int)Resistance.Count];

    public DateTime this[Resistance index] => StatusExpire[(int)index];

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (actor.Type != ActorType.Player)
            return;

        switch ((BST.SID)status.ID)
        {
            case BST.SID.PoisonResistanceUp:
                Add(status.ID, Resistance.Poison, status.ExpireAt);
                break;
            case BST.SID.SpiritOfTheBreathtaker:
                Add(status.ID, Resistance.Poison, DateTime.MaxValue);
                break;
            case BST.SID.Blink:
                Add(status.ID, Resistance.Physical, status.ExpireAt);
                break;
            case BST.SID.Excellence:
                for (var r = Resistance.Poison; r < Resistance.Count; r++)
                    Add(status.ID, r, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (actor.Type != ActorType.Player)
            return;

        var id = status.ID;
        if (_buffs.RemoveAll(b => b.Type == id) > 0)
            Modify();
    }

    void Add(uint sid, Resistance r, DateTime expire)
    {
        _buffs.Add((sid, r, expire));
        Modify();
    }

    void Modify()
    {
        for (var i = Resistance.Poison; i < Resistance.Count; i++)
            StatusExpire[(int)i] = _buffs.Where(b => b.Resistance == i).DefaultIfEmpty(default).Max(b => b.Expiration);
    }
}
