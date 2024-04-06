namespace BossMod.Global.MaskedCarnivale.Stage15;

public enum OID : uint
{
    Boss = 0x26F9, // R=2.3
    Shabti = 0x26FA, //R=1.1
    Serpent = 0x26FB, //R=1.2
    Helper = 0x233C, //R=0.5
};

public enum AID : uint
{
    HighVoltage = 14890, // 26F9->self, 7,0s cast, range 50+R circle, paralysis + summon add
    Summon = 14897, // 26F9->self, no cast, range 50 circle
    Ballast0 = 14893, // 26F9->self, 1,0s cast, single-target
    Ballast1 = 14955, // 26F9->self, no cast, single-target
    Ballast2 = 14894, // 233C->self, 3,0s cast, range 5+R 270-degree cone, knockback dist 15
    Ballast3 = 14895, // 233C->self, 3,0s cast, range 10+R 270-degree cone, knockback dist 15
    Ballast4 = 14896, // 233C->self, 3,0s cast, range 15+R 270-degree cone, knockback dist 15
    PiercingLaser = 14891, // 26F9->self, 3,0s cast, range 30+R width 8 rect
    AutoAttack = 6497, // 26FA/26FB->player, no cast, single-target
    RepellingCannons = 14892, // 26F9->self, 3,0s cast, range 10+R circle
    Spellsword = 14968, // 26FA->self, 3,5s cast, range 6+R 120-degree cone
    Superstorm = 14971, // 26F9->self, 3,5s cast, single-target
    Superstorm2 = 14970, // 233C->self, 3,5s cast, range 8-20 donut
    Disseminate = 14899, // 26FB->self, 2,0s cast, range 6+R circle, casts on death of serpents
};

class HighVoltage : Components.CastHint
{
    public HighVoltage() : base(ActionID.MakeSpell(AID.HighVoltage), "Interrupt!") { }
}

class Ballast : Components.GenericAOEs
{
    private bool casting2;
    private bool casting3;
    private bool casting4;
    private Angle _rotation;
    private DateTime _activation1;
    private DateTime _activation2;
    private DateTime _activation3;

    private static readonly AOEShapeCone cone1 = new(5.5f, 135.Degrees());
    private static readonly AOEShapeDonutSector cone2 = new(5.5f, 10.5f, 135.Degrees());
    private static readonly AOEShapeDonutSector cone3 = new(10.5f, 15.5f, 135.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (casting2)
        {
            yield return new(cone1, module.PrimaryActor.Position, _rotation, _activation1, ArenaColor.Danger);
            yield return new(cone2, module.PrimaryActor.Position, _rotation, _activation2);
            yield return new(cone3, module.PrimaryActor.Position, _rotation, _activation3);
        }
        if (casting3 && !casting2)
        {
            yield return new(cone2, module.PrimaryActor.Position, _rotation, _activation2, ArenaColor.Danger);
            yield return new(cone3, module.PrimaryActor.Position, _rotation, _activation3);
        }
        if (casting4 && !casting3)
            yield return new(cone3, module.PrimaryActor.Position, _rotation, _activation3, ArenaColor.Danger);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Ballast0)
        {
            casting2 = true;
            casting3 = true;
            casting4 = true;
            _rotation = module.PrimaryActor.Rotation;
            _activation1 = module.WorldState.CurrentTime.AddSeconds(4.6f);
            _activation2 = module.WorldState.CurrentTime.AddSeconds(5.2f);
            _activation3 = module.WorldState.CurrentTime.AddSeconds(5.8f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Ballast2)
            casting2 = false;
        if ((AID)spell.Action.ID == AID.Ballast3)
            casting3 = false;
        if ((AID)spell.Action.ID == AID.Ballast4)
            casting4 = false;
    }
}

class PiercingLaser : Components.SelfTargetedAOEs
{
    public PiercingLaser() : base(ActionID.MakeSpell(AID.PiercingLaser), new AOEShapeRect(32.3f, 4)) { }
}

class RepellingCannons : Components.SelfTargetedAOEs
{
    public RepellingCannons() : base(ActionID.MakeSpell(AID.RepellingCannons), new AOEShapeCircle(12.3f)) { }
}

class Superstorm : Components.SelfTargetedAOEs
{
    public Superstorm() : base(ActionID.MakeSpell(AID.Superstorm2), new AOEShapeDonut(8, 20)) { }
}

class Spellsword : Components.SelfTargetedAOEs
{
    public Spellsword() : base(ActionID.MakeSpell(AID.Spellsword), new AOEShapeCone(7.1f, 60.Degrees())) { }
}

class Disseminate : Components.SelfTargetedAOEs
{
    public Disseminate() : base(ActionID.MakeSpell(AID.Disseminate), new AOEShapeCircle(7.2f)) { }
}

class BallastKB : Components.Knockback //actual knockbacks are 0.274s after snapshot
{
    private bool casting2;
    private bool casting3;
    private bool casting4;
    private DateTime _activation1;
    private DateTime _activation2;
    private DateTime _activation3;
    private Angle _rotation;

    private static readonly AOEShapeCone cone1 = new(5.5f, 135.Degrees());
    private static readonly AOEShapeDonutSector cone2 = new(5.5f, 10.5f, 135.Degrees());
    private static readonly AOEShapeDonutSector cone3 = new(10.5f, 15.5f, 135.Degrees());

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        if (casting2)
            yield return new(module.PrimaryActor.Position, 20, _activation1, cone1, _rotation);
        if (casting3)
            yield return new(module.PrimaryActor.Position, 20, _activation2, cone2, _rotation);
        if (casting4)
            yield return new(module.PrimaryActor.Position, 20, _activation3, cone3, _rotation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Ballast0)
        {
            casting2 = true;
            casting3 = true;
            casting4 = true;
            _rotation = module.PrimaryActor.Rotation;
            _activation1 = module.WorldState.CurrentTime.AddSeconds(4.6f);
            _activation2 = module.WorldState.CurrentTime.AddSeconds(5.2f);
            _activation3 = module.WorldState.CurrentTime.AddSeconds(5.8f);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Ballast2)
            casting2 = false;
        if ((AID)spell.Action.ID == AID.Ballast3)
            casting3 = false;
        if ((AID)spell.Action.ID == AID.Ballast4)
            casting4 = false;
    }
}

class Hints : BossComponent
{
    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add("For this stage Flying Sardine and Acorn Bomb are highly recommended.\nUse Flying Sardine to interrupt High Voltage.\nUse Acorn Bomb to put Shabtis to sleep until their buff runs out.");
    }
}

class Stage15States : StateMachineBuilder
{
    public Stage15States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighVoltage>()
            .ActivateOnEnter<Ballast>()
            .ActivateOnEnter<BallastKB>()
            .ActivateOnEnter<PiercingLaser>()
            .ActivateOnEnter<RepellingCannons>()
            .ActivateOnEnter<Superstorm>()
            .ActivateOnEnter<Spellsword>()
            .ActivateOnEnter<Disseminate>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 625, NameID = 8109)]
public class Stage15 : BossModule
{
    public Stage15(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Shabti))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.Serpent))
            Arena.Actor(s, ArenaColor.Object);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Shabti => 2, //TODO: ideally AI would use Acorn Bomb to put it to sleep until buff runs out instead of attacking them directly
                OID.Serpent => 1,
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
