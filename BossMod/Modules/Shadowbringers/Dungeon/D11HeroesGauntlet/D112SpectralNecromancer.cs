namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D112SpectralNecromancer;
public enum OID : uint
{
    Boss = 0x2DF1, // R2.300, x? 
    Necrobomb1 = 0x2DF2, // R0.750, x?
    Necrobomb2 = 0x2DF3, // R0.750, x?
    Necrobomb3 = 0x2DF4, // R0.750, x?
    Necrobomb4 = 0x2DF5, // R0.750, x?
    Necrobomb5 = 0x2DF6, // R0.750, x?
    Necrobomb6 = 0x2DF7, // R0.750, x?
    Necrobomb7 = 0x2DF8, // R0.750, x?
    Necrobomb8 = 0x2DF9, // R0.750, x?
    Voidzone = 0x1EB02C,
    FetterZombieObject = 0x1EB07A,
    Helper = 0x233C,

}
public enum AID : uint
{
    FellForces = 20305, // 2DF1->player, no cast, single-target
    AbsoluteDarkII = 20321, // 2DF1->self, 5.0s cast, range 40 120-degree cone
    TwistedTouch = 20318, // 2DF1->player, 4.0s cast, single-target
    PainMire = 20387, // 2DF1->self, no cast, single-target
    PainMireLocation = 20388, // 233C->location, 5.5s cast, range 9 circle
    ChaosStorm = 20320, // 2DF1->self, 4.0s cast, range 40 circle
    DarkDeluge = 20316, // 2DF1->self, 4.0s cast, single-target
    DarkDelugeLocation = 20317, // 233C->location, 5.0s cast, range 5 circle

    Attack = 6499, // 2DF3/2DF2/2DF5/2DF4->player, no cast, single-target
    Necromancy = 20311, // 2DF1->self, 3.0s cast, single-target
    Necromancy2 = 20312, // 2DF1->self, 3.0s cast, single-target
    DeathThroes = 20323, // 2DF9/2DF6/2DF7/2DF8->player, no cast, single-target
    Necroburst = 20313, // 2DF1->self, 4.3s cast, single-target
    Necroburst2 = 20314, // 2DF1->self, 4.3s cast, single-target
    Burst1 = 20322, // 2DF2->self, 4.0s cast, range 8 circle
    Burst2 = 21429, // 2DF3->self, 4.0s cast, range 8 circle
    Burst3 = 21430, // 2DF4->self, 4.0s cast, range 8 circle
    Burst4 = 21431, // 2DF5->self, 4.0s cast, range 8 circle
    Burst5 = 20324, // 2DF6->self, 4.0s cast, range 8 circle
    Burst6 = 21432, // 2DF7->self, 4.0s cast, range 8 circle
    Burst7 = 21433, // 2DF8->self, 4.0s cast, range 8 circle
    Burst8 = 21434, // 2DF9->self, 4.0s cast, range 8 circle
}
public enum SID : uint
{
    ZombieActive = 2056,
}
public enum IconID : uint
{
    ZombieFetter = 23,
    Tankbuster = 198,
}
public enum TetherID : uint
{
    ZombieChase = 17,
    ZombieFetterTether = 79,
}
class AbsoluteDarkII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteDarkII), new AOEShapeCone(40, 60.Degrees()));
class PainMireLocation(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PainMireLocation), 9);
class PainMireVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9, ActionID.MakeSpell(AID.PainMireLocation), m => m.Enemies(OID.Voidzone).Where(z => z.EventState == 7), 0.5f);
class ChaosStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ChaosStorm));
class DarkDelugeLocation(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkDelugeLocation), 5);
class Necromancy(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _necroAOEs = [];
    private DateTime delay;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_necroAOEs.Count > 0)
        {
            for (var i = 0; i < _necroAOEs.Count; i++)
            {
                yield return _necroAOEs[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_necroAOEs.Count > 0)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(WorldState.Party.Player()!.Position, 4), delay);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Necromancy2)
        {
            delay = Module.CastFinishAt(spell).AddSeconds(8);
            _necroAOEs.Add(new AOEInstance(new AOEShapeCircle(4f), WorldState.Party.Player()!.Position, default, delay));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Necromancy2)
        {
            _necroAOEs.Clear();
        }
    }
}
class NecroFetter(BossModule module) : Components.UniformStackSpread(module, 0, 8f, 0, 0, true)
{
    private readonly List<Actor> activeChasers = [];
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ZombieChase)
        {
            activeChasers.Add(source);
            return;
        }
        else if (tether.ID is (uint)TetherID.ZombieFetterTether)
        {
            if (activeChasers.Count == 0)
            {
                foreach (var player in WorldState.Party.WithoutSlot())
                {
                    Spreads.Add(new(player, 8f));
                }
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ZombieChase)
        {
            activeChasers.Remove(source);
            return;
        }
        else if (tether.ID is (uint)TetherID.ZombieFetterTether)
        {
            Spreads.Clear();
        }
    }
}
class NecroBurst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime delay;
    private IEnumerable<Actor> ChaserZombies
    {
        get
        {
            return Module.Enemies(OID.Necrobomb1)
                   .Concat(Module.Enemies(OID.Necrobomb2))
                   .Concat(Module.Enemies(OID.Necrobomb3))
                   .Concat(Module.Enemies(OID.Necrobomb4))
                   .Where(e => !e.IsTargetable && !e.IsDead);
        }
    }
    private readonly IEnumerable<AID> Bursts = [AID.Burst1, AID.Burst2, AID.Burst3, AID.Burst4];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            for (var i = 0; i < _aoes.Count; i++)
            {
                yield return _aoes[i] with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Bursts.Contains((AID)spell.Action.ID))
        {
            delay = Module.CastFinishAt(spell).AddSeconds(4);
            foreach (var e in ChaserZombies)
            {
                _aoes.Add(new AOEInstance(new AOEShapeCircle(10f), e.Position, default, delay));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Bursts.Contains((AID)spell.Action.ID))
            _aoes.Clear();
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ZombieChase)
        {
            foreach (var e in ChaserZombies)
            {
                _aoes.Add(new AOEInstance(new AOEShapeCircle(10f), e.Position, default, delay));
            }
        }
    }
}
class D112SpectralNecromancerStates : StateMachineBuilder
{
    public D112SpectralNecromancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteDarkII>()
            .ActivateOnEnter<PainMireLocation>()
            .ActivateOnEnter<PainMireVoidzone>()
            .ActivateOnEnter<ChaosStorm>()
            .ActivateOnEnter<DarkDelugeLocation>()
            .ActivateOnEnter<Necromancy>()
            .ActivateOnEnter<NecroBurst>()
            .ActivateOnEnter<NecroFetter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9511)]
public class D112SpectralNecromancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-449.6f, -531.6f), new ArenaBoundsCircle(17));
