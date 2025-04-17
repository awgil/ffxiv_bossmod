namespace BossMod.Global.MaskedCarnivale.Stage14.Act2;

public enum OID : uint
{
    Boss = 0x271E, //R=2.0
}

public enum AID : uint
{
    Syrup = 14757, // 271E->player, no cast, range 4 circle, applies heavy to player
    TheLastSong = 14756, // 271E->self, 6.0s cast, range 60 circle, heavy dmg, applies silence to player
}

class LastSong(BossModule module) : Components.GenericLineOfSightAOE(module, AID.TheLastSong, 60, true); //TODO: find a way to use the obstacles on the map and draw proper AOEs, this does nothing right now

class LastSongHint(BossModule module) : BossComponent(module)
{
    public bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheLastSong)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheLastSong)
            casting = false;
    }
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Use the cube to take cover!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as first act, but the slimes will apply heavy to you.\nUse Loom to get out of line of sight as soon as Final Song gets casted.");
    }
}

class Stage14Act2States : StateMachineBuilder
{
    public Stage14Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<LastSong>()
            .ActivateOnEnter<LastSongHint>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !module.FindComponent<LastSongHint>()!.casting;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 624, NameID = 8108, SortOrder = 2)]
public class Stage14Act2 : BossModule
{
    public Stage14Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<LayoutBigQuad>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
