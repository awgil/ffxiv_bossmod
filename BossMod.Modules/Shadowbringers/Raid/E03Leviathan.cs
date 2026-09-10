namespace BossMod.Shadowbringers.Raid.E03Leviathan;

public enum OID : uint
{
    Boss = 0x296B,
    Helper = 0x233C,
    MaelstromVoidZone = 0x1EAC2B,
}
public enum AID : uint
{
    TidalRoar = 16324, // Boss->self, 4.0s cast, single-target
    RipCurrentCast = 16326, // Boss->self, 5.0s cast, single-target
    RipCurrentHit = 16327, // Helper->self, no cast, range 50 width 6 rect
    TemporaryCurrentRight = 16334, // Boss->self, 6.0s cast, range 58+R width 30 rect
    DrenchingPulse = 16328, // Boss->self, 3.0s cast, single-target
    MonsterWave = 16330, // Helper->location, 4.0s cast, range 6 circle
    FreakWave = 16331, // Helper->players, 6.0s cast, range 5 circle
    TemporaryCurrentLeft = 16333, // Boss->self, 6.0s cast, range 58+R width 30 rect
    TidalWave = 16339, // Helper->self, 10.0s cast, range 60+R width 60 rect
    UnderseaQuakeSides = 16337, // Helper->self, 6.0s cast, range 40+R width 10 rect
    CrashingPulse = 16329, // Boss->self, 3.0s cast, single-target
    KillerWave = 16332, // Helper->players, 6.0s cast, range 6 circle
    Tsunami = 17435, // Helper->self, 11.0s cast, range 80 circle
    SurgingTsunami = 16401, // Helper->self, no cast, range 80 circle
    SmotheringTsunami = 16343, // Helper->players, no cast, range 6 circle
    SplashingTsunami = 16342, // Helper->self, no cast, range 6 circle
    SwirlingTsunami = 16341, // Helper->players, no cast, range ?-10 donut
    Maelstrom = 16344, // Boss->self, 3.0s cast, single-target
    SpinningDive = 16347, // Boss->self, 5.0s cast, range 46+R width 20 rect
    UnderseaQuakeMid = 16335, // Boss->self, 6.0s cast, range 40+R width 20 rect
}

public enum SID : uint
{
    SplashingWaters = 1851, // Spread mechanic when debuff expires, dorito-esque icon.
    SmotheringWaters = 1853, // Stack mechanic when debuff expires.
    SurgingWaters = 1850, // Knockback mechanic when debuff expires
    SwirlingWaters = 1852, // Donut? mechanic when debuff expires

}

public enum IconID : uint
{
    RipCurrentTarget = 23, // player->self
    DrenchingPulseSpread = 169, // player->self
    CrashingPulseStack = 62, // player->self
    TsunamiKnockbackFromPlayer = 173, // player->self
}

class RipCurrent(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(50f, 3f), (uint)IconID.RipCurrentTarget, AID.RipCurrentCast, activationDelay: 5.1f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    // if not targetted, limit how far they will run to try to dodge bait.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(new AOEShapeDonut(8, 100), new WPos(100, 95), default, b.Activation);
            }
        }
    }
}

class TemporaryCurrentRight(BossModule module) : Components.StandardAOEs(module, AID.TemporaryCurrentRight, new AOEShapeRect(76f, 15f));
class TemporaryCurrentLeft(BossModule module) : Components.StandardAOEs(module, AID.TemporaryCurrentLeft, new AOEShapeRect(76f, 15f));
class TidalRoar(BossModule module) : Components.RaidwideCast(module, AID.TidalRoar);
class Tsunami(BossModule module) : Components.RaidwideCast(module, AID.Tsunami);

class MonsterWave(BossModule module) : Components.StandardAOEs(module, AID.MonsterWave, 6f);

class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave, 20, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (s.Origin.X > 100)
            {
                hints.AddForbiddenZone(new AOEShapeRect(20, 15, 20), Arena.Center - new WDir(10, 0), default, s.Activation);
            }
            else
            {
                hints.AddForbiddenZone(new AOEShapeRect(20, 15, 20), Arena.Center + new WDir(10, 0), default, s.Activation);
            }
        }
    }
}
class UnderseaQuakeSides(BossModule module) : Components.StandardAOEs(module, AID.UnderseaQuakeSides, new AOEShapeRect(40f, 5f));
class UnderseaQuakeMid(BossModule module) : Components.StandardAOEs(module, AID.UnderseaQuakeMid, new AOEShapeRect(40f, 10f))
{
    public DateTime Activation;
    public DateTime VoidZoneTime;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.UnderseaQuakeMid)
        {
            Activation = Module.CastFinishAt(spell);
            VoidZoneTime = Activation.AddSeconds(3f);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        //split to sides by light party to make the next spread more feasible for AI.
        if (WorldState.CurrentTime < Activation)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.VeryHigh);
            switch (assignment)
            {
                case PartyRolesConfig.Assignment.OT:
                case PartyRolesConfig.Assignment.M2:
                case PartyRolesConfig.Assignment.H2:
                case PartyRolesConfig.Assignment.R2:
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(90, 80), new WPos(90, 120), 10f), Activation);//don't go left
                    break;
                default:
                    hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(110, 80), new WPos(110, 120), 10f), Activation);//don't go right
                    break;
            }
        }
        //There is a frame after the aoe before the floor drops out and arena changes. AI healer will try to run in to heal, and plummet to their death after successfully dodging the aoe.
        //This should treat it as a void zone for an additional 3 seconds.
        else if (WorldState.CurrentTime < VoidZoneTime)
        {
            hints.AddForbiddenZone(ShapeDistance.Rect(new WPos(100, 80), new WPos(100, 120), 10f));
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ArenaChange(BossModule module) : BossComponent(module)
{
    PolygonClipper.Operand MakePlatform(float offX, float offZ) => new(CurveApprox.Rect(new WDir(offX, offZ), new WDir(5, 0), new WDir(0, 20)));

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        base.OnEventDirectorUpdate(updateID, param1, param2, param3, param4);
        if (updateID == 0x8000000D)
        {
            if (param1 == 15) //Ryne restored arena.
            {
                Arena.Bounds = new ArenaBoundsRect(20, 20);
            }
            if (param1 == 63) //Leviathan heads removed sides of platform.
            {
                Arena.Bounds = new ArenaBoundsRect(10, 20); //center of platform remains.
            }
            if (param1 == 1984) //Leviathan has removed the middle of the arena.
            {
                Arena.Bounds = new ArenaBoundsCustom(20, new PolygonClipper().UnionAll(MakePlatform(15, 0), MakePlatform(-15, 0)));
            }
        }
    }
}

//Difficulty: These voidzones only have an animation state as far as I can tell. The underlying eventObj persists long after the void zones are safe. We need to make them safe once the corresponding animation has finished.
class MaelstromVoidZones(BossModule module) : Components.Voidzone(module, 8f, OID.MaelstromVoidZone)
{
    public List<Actor> voidZoneActors = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => voidZoneActors.Select(s => new AOEInstance(Shape, s.Position));
    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if (actor.OID == (uint)OID.MaelstromVoidZone)
        {
            voidZoneActors.Add(actor);
        }
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        base.OnActorEAnim(actor, state);
        if (actor.OID == (uint)OID.MaelstromVoidZone)
        {
            if (state == 0x00040008)
            {
                voidZoneActors.Remove(actor);
            }
        }
    }
    //make sure AI hints also clear void zones when they have animated.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var v in voidZoneActors)
        {
            hints.AddForbiddenZone(Shape.Distance(v.Position, v.Rotation));
        }
    }
}
class SpinningDive(BossModule module) : Components.StandardAOEs(module, AID.SpinningDive, new AOEShapeRect(64f, 10f)); //I think this is the Tsunami portal charges?

class DrenchingPulseSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DrenchingPulseSpread, AID.FreakWave, 5f, 7f);
class CrashingPulseStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.CrashingPulseStack, AID.KillerWave, 5f, 7f, 4) //same icon and radius as SmotheringWaters. icon appears ~4 seconds before effect.
{
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        base.OnStatusLose(actor, status);
        if (status.ID == (uint)SID.SmotheringWaters)
        {
            Stacks.Clear();
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var s in Stacks)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(19, 100), new WPos(100, 80), default, s.Activation); // bring stack marker to boss.
        }
    }
}

class TsunamiDonut(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(5, 10);

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SwirlingWaters:
                CurrentBaits.Add(new(actor, actor, Donut, status.ExpireAt));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SwirlingTsunami:
                CurrentBaits.RemoveAll(b => b.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            foreach (var b in CurrentBaits)
            {
                if (actor == b.Source)
                {
                    hints.AddForbiddenZone(new AOEShapeDonut(1, 100), new WPos(100, 84), default, b.Activation); // stack right in front of boss.
                }
            }
        }
    }
}
class SurgingWaters(BossModule module) : Components.Knockback(module)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources.Select(s => new Source(s.Source.Position, 12, s.Activation)); //TODO: verify knockback distance

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.SurgingWaters)
            _sources.Add((actor, status.ExpireAt));
    }
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.SurgingWaters)
            _sources.RemoveAll(s => s.Source == actor);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var s in _sources)
        {
            if (actor == s.Source)
            {
                hints.AddForbiddenZone(new AOEShapeDonut(1, 100), new WPos(100, 81), default, s.Activation); //knock group straight back to max melee
            }
            else
            {
                hints.AddForbiddenZone(new AOEShapeDonut(1, 100), new WPos(100, 84), default, s.Activation); //group stack in center
            }
        }
    }
}

class TsunamiStackSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public float StackRadius = 5;
    public float SpreadRadius = 5;
    public int MinStackSize = 4;
    public int MaxStackSize = 8;

    public IEnumerable<Actor> ActiveStackTargets => ActiveStacks.Select(s => s.Target);
    public IEnumerable<Actor> ActiveSpreadTargets => ActiveSpreads.Select(s => s.Target);

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SplashingWaters:
                AddSpread(actor, status.ExpireAt);
                break;
            case SID.SmotheringWaters:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SplashingWaters:
                Spreads.Clear();
                break;
            case SID.SmotheringWaters:
                Stacks.Clear();
                break;

        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var s in Stacks)
        {
            if (s.Target == actor)
            {
                hints.AddForbiddenZone(new AOEShapeDonut(1, 100), new WPos(100, 84), default, s.Activation); // stack in the donut
            }
        }
    }
}

class E03LeviathanStates : StateMachineBuilder
{
    public E03LeviathanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<RipCurrent>()
            .ActivateOnEnter<TemporaryCurrentRight>()
            .ActivateOnEnter<TemporaryCurrentLeft>()
            .ActivateOnEnter<MonsterWave>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<UnderseaQuakeSides>()
            .ActivateOnEnter<UnderseaQuakeMid>()
            .ActivateOnEnter<Tsunami>()
            .ActivateOnEnter<SurgingWaters>()
            .ActivateOnEnter<TsunamiStackSpread>()
            .ActivateOnEnter<TsunamiDonut>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<SpinningDive>()
            .ActivateOnEnter<DrenchingPulseSpread>()
            .ActivateOnEnter<CrashingPulseStack>()
            .ActivateOnEnter<TidalRoar>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 682, NameID = 8486)]
public class E03Leviathan(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20));
