namespace BossMod.Shadowbringers.Dungeon.D01Holminser.D013Philia;

public enum OID : uint
{
    Boss = 0x278C, // R9.800, x1
    IronChain = 0x2895, // R1.000, spawn during fight
    SludgeVoidzone = 0x1EABFA,
    Helper = 0x233C, // x3
}

public enum AID : uint
{
    AutoAttack = 872, // 278C->player, no cast, single-target
    ScavengersDaughter = 15832, // 278C->self, 4.0s cast, range 40 circle
    HeadCrusher = 15831, // 278C->player, 4.0s cast, single-target
    Pendulum = 16777, // 278C->self, 5.0s cast, single-target, cast to jump
    PendulumAOE1 = 16790, // 278C->location, no cast, range 40 circle, jump to target
    PendulumAOE2 = 15833, // 278C->location, no cast, range 40 circle, jump back to center
    PendulumAOE3 = 16778, // Helper->location, 4.5s cast, range 40 circle, damage fall off AOE visual
    ChainDown = 17052, // 278C->self, 5.0s cast, single-target 
    Aethersup = 15848, // 278C->self, 15.0s cast, range 21 120-degree cone
    Aethersup2 = 15849, // Helper->self, no cast, range 24+R 120-degree cone
    RightKnout = 15846, // 278C->self, 5.0s cast, range 24 210-degree cone
    LeftKnout = 15847, // 278C->self, 5.0s cast, range 24 210-degree cone
    Taphephobia = 15842, // 278C->self, 4.5s cast, single-target
    Taphephobia2 = 16769, // Helper->player, 5.0s cast, range 6 circle
    IntoTheLight = 15844, // Helper->player, no cast, single-target, line stack
    IntoTheLight1 = 17232, // 278C->self, 5.0s cast, single-target
    IntoTheLight2 = 15845, // 278C->self, no cast, range 50 width 8 rect
    FierceBeating1 = 15834, // 278C->self, 5.0s cast, single-target
    FierceBeating2 = 15836, // 278C->self, no cast, single-target
    FierceBeating3 = 15835, // 278C->self, no cast, single-target
    FierceBeating4 = 15837, // Helper->self, 5.0s cast, range 4 circle
    FierceBeating5 = 15839, // Helper->location, no cast, range 4 circle
    FierceBeating6 = 15838, // Helper->self, no cast, range 4 circle
    CatONineTails = 15840, // 278C->self, no cast, single-target
    CatONineTails2 = 15841, // Helper->self, 2.0s cast, range 25 120-degree cone
}

public enum IconID : uint
{
    Tankbuster = 198, // player 
    SpreadFlare = 87, // player
    ChainTarget = 92, // player
    Spread = 139, // player
    RotateCW = 167, // Boss
}

public enum SID : uint
{
    Fetters = 1849, // none->player, extra=0xEC4
    DownForTheCount = 783, // none->player, extra=0xEC7
    Sludge = 287, // none->player, extra=0x0
}

class SludgeVoidzone(BossModule module) : Components.PersistentVoidzone(module, 9.8f, m => m.Enemies(OID.SludgeVoidzone).Where(z => z.EventState != 7));
class ScavengersDaughter(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScavengersDaughter));
class HeadCrusher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeadCrusher));

class Chains(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus(SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && !chainsactive)
            hints.Add($"{chaintarget.Name} is about to be chained!");
        if (chaintarget != null && chainsactive)
            hints.Add($"Destroy chains on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
            foreach (var e in hints.PotentialTargets)
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.IronChain => 1,
                    OID.Boss => -1,
                    _ => 0
                };
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.ChainTarget)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChainDown)
            casting = false;
    }
}

class Aethersup(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private Angle _rotation;
    private static readonly AOEShapeCone cone = new(24, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, Module.PrimaryActor.Position, _rotation, _activation, Risky: Module.Enemies(OID.IronChain).All(x => x.IsDead));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Aethersup)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
    }


    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aethersup:
            case AID.Aethersup2:
                if (++NumCasts == 4)
                {
                    _activation = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class PendulumFlare(BossModule module) : Components.GenericBaitAway(module)
{
    private bool targeted;
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.SpreadFlare)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(20)));
            targeted = true;
            target = actor;
            CenterAtTarget = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PendulumAOE1)
        {
            CurrentBaits.Clear();
            targeted = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor && targeted)
            hints.AddForbiddenZone(ShapeDistance.Rect(Module.Center, target.Position, 18));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor && targeted)
            hints.Add("Bait away!");
    }
}

class PendulumAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PendulumAOE3), 15);
class LeftKnout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftKnout), new AOEShapeCone(24, 105.Degrees()));
class RightKnout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightKnout), new AOEShapeCone(24, 105.Degrees()));
class Taphephobia(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Taphephobia2), 6);

// TODO: create and use generic 'line stack' component
class IntoTheLight(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? target;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IntoTheLight)
        {
            target = WorldState.Actors.Find(spell.MainTargetID);
            CurrentBaits.Add(new(Module.PrimaryActor, target!, new AOEShapeRect(50, 4)));
        }
        if ((AID)spell.Action.ID == AID.IntoTheLight2)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0 && actor != target)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 50, 0, 4));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (!actor.Position.InRect(Module.PrimaryActor.Position, (target!.Position - Module.PrimaryActor.Position).Normalized(), 50, 0, 4))
                hints.Add("Stack!");
            else
                hints.Add("Stack!", false);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in CurrentBaits)
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

class CatONineTails(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(25, 60.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating1)
            Sequences.Add(new(_shape, Module.Center, spell.Rotation + 180.Degrees(), -45.Degrees(), spell.NPCFinishAt, 2, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CatONineTails2 && Sequences.Count > 0)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class FierceBeating(BossModule module) : Components.Exaflare(module, 4)
{
    private readonly List<WPos> _casters = [];
    private int linesstartedcounttotal;
    private int linesstartedcount1;
    private int linesstartedcount2;
    private static readonly AOEShapeCircle circle = new(4);
    private DateTime _activation;
    private const float RadianConversion = 45 * (MathF.PI / 180);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor);
        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
        if (Lines.Count > 0 && linesstartedcount1 < 8)
            yield return new(circle, CalculateCirclePosition(linesstartedcount1, Module.Center, _casters[0]), default, _activation.AddSeconds(linesstartedcount1 * 3.7f));
        if (Lines.Count > 1 && linesstartedcount2 < 8)
            yield return new(circle, CalculateCirclePosition(linesstartedcount2, Module.Center, _casters[1]), default, _activation.AddSeconds(linesstartedcount2 * 3.7f));
    }

    private static WPos CalculateCirclePosition(int count, WPos origin, WPos caster)
    {
        float x = MathF.Cos(count * RadianConversion) * (caster.X - origin.X) - MathF.Sin(count * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(count * RadianConversion) * (caster.X - origin.X) + MathF.Cos(count * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }

    public override void Update()
    {
        if (linesstartedcount1 != 0 && Lines.Count == 0)
        {
            linesstartedcounttotal = 0;
            linesstartedcount1 = 0;
            linesstartedcount2 = 0;
            _casters.Clear();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating4)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 2.5f * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1, ExplosionsLeft = 7, MaxShownExplosions = 3 });
            _activation = spell.NPCFinishAt;
            ++linesstartedcounttotal;
            ++NumCasts;
            _casters.Add(caster.Position);
            if (linesstartedcounttotal % 2 != 0)
                ++linesstartedcount1;
            else
                ++linesstartedcount2;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating6)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 2.5f * caster.Rotation.ToDirection(), NextExplosion = WorldState.FutureTime(1), TimeToMove = 1, ExplosionsLeft = 7, MaxShownExplosions = 3 });
            ++linesstartedcounttotal;
            if (linesstartedcounttotal % 2 != 0)
                ++linesstartedcount1;
            else
                ++linesstartedcount2;
        }
        if (Lines.Count > 0)
        {
            if ((AID)spell.Action.ID is AID.FierceBeating4 or AID.FierceBeating6)
            {
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
            if ((AID)spell.Action.ID == AID.FierceBeating5)
            {
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(spell.TargetXZ, 1));
                AdvanceLine(Lines[index], spell.TargetXZ);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}

class D013PhiliaStates : StateMachineBuilder
{
    public D013PhiliaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScavengersDaughter>()
            .ActivateOnEnter<HeadCrusher>()
            .ActivateOnEnter<PendulumFlare>()
            .ActivateOnEnter<PendulumAOE>()
            .ActivateOnEnter<Aethersup>()
            .ActivateOnEnter<Chains>()
            .ActivateOnEnter<SludgeVoidzone>()
            .ActivateOnEnter<LeftKnout>()
            .ActivateOnEnter<RightKnout>()
            .ActivateOnEnter<Taphephobia>()
            .ActivateOnEnter<IntoTheLight>()
            .ActivateOnEnter<CatONineTails>()
            .ActivateOnEnter<FierceBeating>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8301)]
public class D013Philia(WorldState ws, Actor primary) : BossModule(ws, primary, new(134, -465), new ArenaBoundsCircle(19.5f));
