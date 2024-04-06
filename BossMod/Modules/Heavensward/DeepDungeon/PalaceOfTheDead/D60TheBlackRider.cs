namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D60TheBlackRider;

public enum OID : uint
{
    Boss = 0x1814, // R3.920, x1
    Voidzone = 0x1E858E, // R0.500, EventObj type, spawn during fight
    VoidsentDiscarnate = 0x18E6, // R1.000, spawn during fight
    Helper = 0x233C, // R0.500, x12, 523 type
};

public enum AID : uint
{
    AutoAttack = 7179, // Boss->player, no cast, range 8+R 90-degree cone
    Geirrothr = 7087, // Boss->self, no cast, range 6+R 90-degree cone, 5.1s after pull, 7.1s after Valfodr + 8.1s after every 2nd HallofSorrow
    HallOfSorrow = 7088, // Boss->location, no cast, range 9 circle
    Infaturation = 7157, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
    Valfodr = 7089, // Boss->player, 4.0s cast, width 6 rect charge, knockback 25, dir forward
};

class CleaveAuto : Components.Cleave
{
    public CleaveAuto() : base(ActionID.MakeSpell(AID.AutoAttack), new AOEShapeCone(11.92f, 45.Degrees())) { }
}

class Geirrothr : Components.GenericAOEs
{
    private DateTime _activation;
    private static readonly AOEShapeCone cone = new(9.92f, 45.Degrees());
    private bool Pulled;

    public override void Update(BossModule module)
    {
        if (!Pulled)
        {
            _activation = module.WorldState.CurrentTime.AddSeconds(5.1f);
            Pulled = true;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Valfodr) // boss can move after cast started, so we can't use aoe instance, since that would cause outdated position data to be used
            _activation = module.WorldState.CurrentTime.AddSeconds(7.1f);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Geirrothr)
            _activation = default;
        if ((AID)spell.Action.ID == AID.HallOfSorrow)
        {
            ++NumCasts;
            if (NumCasts % 2 == 0)
                _activation = module.WorldState.CurrentTime.AddSeconds(8.1f);
        }
    }
}

class Infaturation : Components.SelfTargetedAOEs
{
    public Infaturation() : base(ActionID.MakeSpell(AID.Infaturation), new AOEShapeCircle(7)) { }
}

class HallOfSorrow : Components.PersistentVoidzone
{
    public HallOfSorrow() : base(9, m => m.Enemies(OID.Voidzone).Where(z => z.EventState != 7)) { }
}

class Valfodr : Components.BaitAwayChargeCast
{
    public Valfodr() : base(ActionID.MakeSpell(AID.Valfodr), 3) { }
}

class ValfodrKB : Components.Knockback //note actual knockback is delayed by upto 1.2s in replay
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (module.FindComponent<Valfodr>()?.CurrentBaits.Count > 0)
            yield return new(module.PrimaryActor.Position, 25, _activation, module.FindComponent<Valfodr>()!.CurrentBaits[0].Shape, Angle.FromDirection(module.FindComponent<Valfodr>()!.CurrentBaits[0].Target.Position - module.PrimaryActor.Position), Kind: Kind.DirForward);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Valfodr)
        {
            _activation = spell.NPCFinishAt;
            StopAtWall = true;
        }
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => (module.FindComponent<HallOfSorrow>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (module.FindComponent<Infaturation>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
}

class D60TheBlackRiderStates : StateMachineBuilder
{
    public D60TheBlackRiderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Geirrothr>()
            .ActivateOnEnter<Infaturation>()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>()
            .ActivateOnEnter<CleaveAuto>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 204, NameID = 5309)]
public class D60TheBlackRider : BossModule
{
    public D60TheBlackRider(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300, -220), 25)) { }
}
