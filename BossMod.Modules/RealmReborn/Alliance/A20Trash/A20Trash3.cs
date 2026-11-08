namespace BossMod.RealmReborn.Alliance.A20Trash3;

public enum OID : uint
{
    Azer = 0xBD3, // R1.560, x3
    FlyingRedDragon = 0xBD1, // R7.000, x1, untargetable, flies over the north edge until it leaves
    RedDragon = 0xBD2, // R7.000, x1, lands once the flying one leaves
    Haokah = 0xBD5, // R1.000, x2, spawn as the dragon lands
}

public enum AID : uint
{
    AutoAttack = 872, // Azer/Haokah->player, no cast, single-target
    AutoAttackDragon = 870, // RedDragon->player, no cast, single-target
    DoubleSmash = 618, // Azer->self, 2.5s cast, range 6+R 120-degree cone
    Fireball = 2343, // FlyingRedDragon->player, no cast, range 4 circle
    Cauterize = 2342, // FlyingRedDragon->self, no cast, range 48+R width 20 rect, aimed at the icon target
    Touchdown = 564, // RedDragon->self, no cast, range 10 circle, as it lands
    Roast = 2341, // RedDragon->self, no cast, range 30+R width 8 rect
    LowPeal = 2448, // Haokah->self, 3.0s cast, range 3+R circle
}

public enum IconID : uint
{
    Cauterize = 18, // player, ~5.2s before the flying dragon's cauterize is aimed at them
}

class DoubleSmash(BossModule module) : Components.StandardAOEs(module, AID.DoubleSmash, new AOEShapeCone(7.56f, 60.Degrees()));
class LowPeal(BossModule module) : Components.StandardAOEs(module, AID.LowPeal, new AOEShapeCircle(4));

class Roast(BossModule module) : Components.Cleave(module, AID.Roast, new AOEShapeRect(37, 4), (uint)OID.RedDragon)
{
    private DateTime _start;

    private bool Due => _start != default && WorldState.CurrentTime >= _start;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Touchdown)
        {
            _start = WorldState.FutureTime(13);
            NextExpected = WorldState.FutureTime(15.7f);
        }
        else if (NumCasts >= 2)
        {
            _start = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Due)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Due)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Due)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class Cauterize(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(55, 10), (uint)IconID.Cauterize, AID.Cauterize, 5.2f)
{
    public override Actor? BaitSource(Actor target) => Module.Enemies(OID.FlyingRedDragon).FirstOrDefault();
}

class Touchdown(BossModule module) : Components.GenericAOEs(module, AID.Touchdown)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && Module.Enemies(OID.RedDragon).FirstOrDefault() is { } dragon)
            yield return new(new AOEShapeCircle(10), dragon.Position, default, _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Cauterize:
                _activation = WorldState.FutureTime(3.1f);
                break;
            case AID.Touchdown:
                _activation = default;
                break;
        }
    }
}

class A20Trash3States : StateMachineBuilder
{
    public A20Trash3States(A20Trash3 module) : base(module)
    {
        var haokahSeen = false;
        TrivialPhase()
            .ActivateOnEnter<DoubleSmash>()
            .ActivateOnEnter<LowPeal>()
            .ActivateOnEnter<Roast>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>()
            .Raw.Update = () =>
            {
                haokahSeen |= module.Enemies(OID.Haokah).Count > 0;
                return haokahSeen && A20Trash3.Pack.All(oid => module.Enemies(oid).All(e => e.IsDead));
            };
    }
}

[ModuleInfo(Contributors = "Nonu", PrimaryActorOID = (uint)OID.Azer, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2792, SortOrder = 6)]
public class A20Trash3(ModuleInit init) : BossModule(init, new(0, -200), new ArenaBoundsCircle(29))
{
    public static readonly OID[] Pack = [OID.Azer, OID.RedDragon, OID.Haokah];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var oid in Pack)
            Arena.Actors(Enemies(oid), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FlyingRedDragon), ArenaColor.Object, true);
    }
}
