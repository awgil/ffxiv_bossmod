namespace BossMod.Global.MaskedCarnivale.Stage14.Act1;

public enum OID : uint
{
    Boss = 0x271D, //R=2.0
}

public enum AID : uint
{
    TheLastSong = 14756, // 271D->self, 6.0s cast, range 60 circle
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
            hints.Add("Take cover behind a barricade!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("These slimes start casting Final Song after death.\nWhile Final Song is not deadly, it does heavy damage and applies silence\nto you. Take cover! For act 2 the spell Loom is strongly recommended.\nThe slimes are strong against blunt melee damage such as J Kick.");
    }
}

class Stage14Act1States : StateMachineBuilder
{
    public Stage14Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<LastSong>()
            .ActivateOnEnter<LastSongHint>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !module.FindComponent<LastSongHint>()!.casting;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 624, NameID = 8108, SortOrder = 1)]
public class Stage14Act1 : BossModule
{
    public Stage14Act1(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
        ActivateComponent<Layout2Corners>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
