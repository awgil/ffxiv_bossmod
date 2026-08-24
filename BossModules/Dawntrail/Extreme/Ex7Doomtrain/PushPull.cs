namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class DeadMansExpress(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DeadMansExpress, 30, kind: Kind.DirForward, stopAtWall: true);
class DeadMansWindpipe(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DeadMansWindpipeBoss, 30, kind: Kind.DirForward, stopAtWall: true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Select(src => src with { Direction = src.Direction + 180.Degrees() });
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => pos.InRect(Arena.Center - new WDir(0, 15), new WDir(0, 10), 10);
}

class Plasma(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }
    string? next;

    public void Reset() { NumCasts = 0; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (next != null)
            hints.Add($"Next: {next}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeadMansOverdraughtSpread:
                next = "Spread";
                break;
            case AID.DeadMansOverdraughtStack:
                next = "Stack";
                break;
            case AID.DeadMansExpress:
            case AID.DeadMansWindpipeBoss:
                EnableHints = false;
                Activate();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Plasma:
                Spreads.Clear();
                NumCasts++;
                next = null;
                break;
            case AID.HyperexplosivePlasma:
                Stacks.Clear();
                NumCasts++;
                next = null;
                break;
            case AID.DeadMansExpress:
            case AID.DeadMansWindpipe:
                EnableHints = true;
                break;
        }
    }

    void Activate()
    {
        switch (next)
        {
            case "Spread":
                foreach (var player in Raid.WithoutSlot())
                    Spreads.Add(new(player, 5));
                break;
            case "Stack":
                foreach (var player in Raid.WithoutSlot().OrderByDescending(r => r.Class.IsSupport()).Take(4))
                    Stacks.Add(new(player, 5, maxSize: 2));
                break;
        }
    }
}

class DeadMansBlastpipe(BossModule module) : Components.GenericAOEs(module, AID.DeadMansBlastpipe)
{
    private Actor? Caster;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster != null)
            yield return new(new AOEShapeRect(10, 10), Arena.Center - new WDir(0, 15), default);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeadMansWindpipe)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Caster = null;
        }
    }
}
