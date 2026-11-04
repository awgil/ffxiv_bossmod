namespace BossMod.Endwalker.Trial.T07Zeromus;

public enum OID : uint
{
    Boss = 0x404E, // R20.000, x1
    Comet = 0x40AB, // R2.250, x22
    ToxicBubble = 0x40AC, // R1.700, spawn during fight
    FlowOfTheAbyss = 0x4110, // R1.000, x1
    Helper = 0x233C, // R0.500, x30, 523 type
    FlareTower = 0x1EB94E, // R0.500, EventObj type, spawn during fight
    FlareRay = 0x1EB94F, // R0.500, EventObj type, spawn during fight
    BlackHole = 0x1EB94C, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 35912, // Boss->player, no cast, single-target

    AbyssalNox = 35575, // Boss->self, 5.0s cast, range 60 circle, doom on everyone
    AbyssalEchoesVisualCardinal = 35576, // Helper->self, 9.0s cast, single-target, visual
    AbyssalEchoesVisualIntercardinal = 35577, // Helper->self, 9.0s cast, single-target, visual
    AbyssalEchoes = 35578, // Helper->self, 16.0s cast, range 12 circle

    SableThread = 35566, // Boss->self, 5.0s cast, single-target, visual (line stack)
    SableThreadTarget = 35567, // Helper->player, no cast, single-target, visual (target selection)
    SableThreadVisualHitIntermediate = 35568, // Boss->self, no cast, single-target, visual
    SableThreadVisualHitLast = 35569, // Boss->self, no cast, single-target, visual
    SableThreadAOE = 35570, // Helper->self, no cast, range 60 width 12 rect line stack

    DarkMatter = 35638, // Boss->self, 4.0s cast, single-target, visual (tankbuster)
    DarkMatterAOE = 35639, // Helper->player, no cast, range 8 circle tankbuster, 3 hits

    VisceralWhirlR = 35579, // Boss->self, 8.0s cast, single-target, visual (two lines with right safespot)
    VisceralWhirlRAOE1 = 35580, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlRAOE2 = 35581, // Helper->self, 8.8s cast, range 60 width 28 rect
    VisceralWhirlL = 35582, // Boss->self, 8.0s cast, single-target, visual (two lines with left safespot)
    VisceralWhirlLAOE1 = 35583, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlLAOE2 = 35584, // Helper->self, 8.8s cast, range 60 width 28 rect

    Flare = 35602, // Boss->self, 7.0s cast, single-target, visual (tower)
    FlareAOE = 35603, // Helper->self, 8.0s cast, range 5 circle tower (full party)
    FlareScald = 35765, // Helper->self, no cast, range 5 circle (tower aftereffect, damage + vuln)
    FlareKill = 35605, // Helper->self, 5.0s cast, range 5 circle (tower aftereffect, last hit)
    ProminenceSpine = 35606, // Helper->self, 5.0s cast, range 60 width 10 rect (tower aftereffect, ray)
    Nox = 36135, // Boss->self, 4.0s cast, single-target, visual (chasing aoe)
    NoxAOEFirst = 36137, // Helper->self, 5.0s cast, range 10 circle
    NoxAOERest = 36131, // Helper->self, no cast, range 10 circle

    VoidBio = 35607, // Boss->self, 5.0s cast, single-target, visual (spawn toxic bubbles)
    VoidBioBurst = 35608, // ToxicBubble->player, no cast, single-target (bubble touch)

    BigBang = 35587, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigBangAOE = 35589, // Helper->location, 8.0s cast, range 5 circle puddle
    BigCrunch = 35588, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigCrunchAOE = 36144, // Helper->location, 8.0s cast, range 5 circle puddle
    AccelerationBomb = 35590, // ?, no cast, single-target
    DarkDivides = 35593, // Helper->player, no cast, range 5 circle spread
    DarkBeckons = 35594, // Helper->player, no cast, range 6 circle stack

    MeteorImpact = 35595, // Boss->self, 11.0s cast, single-target, visual (tethered meteors)
    VoidMeteor = 35596, // Boss->self, 4.8s cast, single-target, visual (placed meteors)
    MeteorImpactChargeNormal = 35597, // Comet->location, no cast, width 4 rect charge
    MeteorImpactChargeClipping = 35598, // Comet->location, no cast, width 4 rect charge (clipped a placed meteor)
    MeteorImpactAppearClipping = 35599, // Helper->location, no cast, range 60 circle
    MeteorImpactMassiveExplosion = 35600, // Comet->self, no cast, range 60 circle
    MeteorImpactProximity = 35601, // Comet->self, 6.0s cast, range 40 circle with falloff
    MeteorImpactAppearNormal = 36024, // Helper->location, no cast, range 2 circle
    MeteorImpactExplosion = 36147, // Comet->self, 5.0s cast, range 10 circle

    BlackHole = 36133, // Boss->self, 5.0s cast, single-target, visual (black hole + laser)
    BlackHoleAOE = 36134, // Helper->location, 5.7s cast, range 4 circle (black hole drop spot)
    FracturedEventideEW = 35571, // Boss->self, 8.0s cast, single-target, visual (laser E to W)
    FracturedEventideWE = 35572, // Boss->self, 8.0s cast, single-target, visual (laser W to E)
    FracturedEventideAOEFirstEW = 35573, // Helper->self, 8.5s cast, range 60 width 8 rect
    FracturedEventideAOEFirstWE = 35910, // Helper->self, 8.5s cast, range 60 width 8 rect
    FracturedEventideAOERest = 35574, // Helper->self, no cast, range 60 width 8 rect

    RendTheRift = 35609, // Boss->self, 6.0s cast, range 60 circle, raidwide
    Nostalgia = 35610, // Boss->self, 5.0s cast, single-target, visual (multiple raidwides)
    NostalgiaBury1 = 35612, // Helper->self, 0.7s cast, range 60 circle
    NostalgiaBury2 = 35613, // Helper->self, 1.7s cast, range 60 circle
    NostalgiaBury3 = 35614, // Helper->self, 2.7s cast, range 60 circle
    NostalgiaBury4 = 35615, // Helper->self, 3.7s cast, range 60 circle
    NostalgiaRoar1 = 35616, // Helper->self, 5.7s cast, range 60 circle
    NostalgiaRoar2 = 35617, // Helper->self, 6.7s cast, range 60 circle
    NostalgiaPrimalRoar = 35618, // Helper->self, 9.7s cast, range 60 circle
    DimensionalSurgePuddle = 35633, // Helper->location, 4.0s cast, range 5 circle
    DimensionalSurgeStarCenter = 35634, // Helper->location, 4.0s cast, range 2 circle
    DimensionalSurgeStarLine = 35635, // Helper->self, 4.0s cast, range 60 width 2 rect (3 per star)

    FlowOfTheAbyss = 36089, // Boss->self, 7.0s cast, single-target, visual (spreads + unsafe line)
    FlowOfTheAbyssDimensionalSurge = 35637, // Helper->self, 9.0s cast, range 60 width 14 rect
    AkhRhaiFirst = 35619, // Helper->location, 3.0s cast, range 5 circle (at spread targets)
    AkhRhaiRest = 35620, // Helper->location, no cast, range 5 circle (10 repeats)
    ChasmicNails = 35622, // Boss->self, 7.0s cast, single-target, visual (pizzas)
    ChasmicNailsVisual1 = 35623, // Helper->self, 1.5s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual2 = 35624, // Helper->self, 3.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual3 = 35625, // Helper->self, 4.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual4 = 35626, // Helper->self, 5.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual5 = 35627, // Helper->self, 6.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsAOE1 = 35628, // Helper->self, 7.7s cast, range 60 40-degree cone
    ChasmicNailsAOE2 = 35629, // Helper->self, 8.4s cast, range 60 40-degree cone
    ChasmicNailsAOE3 = 35630, // Helper->self, 9.1s cast, range 60 40-degree cone
    ChasmicNailsAOE4 = 35631, // Helper->self, 9.8s cast, range 60 40-degree cone
    ChasmicNailsAOE5 = 35632, // Helper->self, 10.5s cast, range 60 40-degree cone
}

public enum SID : uint
{
    Doom = 1769, // Boss->player, extra=0x0
    //HPPenalty = 1089, // none->player, extra=0x0
    AccelerationBomb = 2657, // none->player, extra=0x0
    DivisiveDark = 3762, // none->player, extra=0x0, spread when it expires
    BeckoningDark = 3763, // none->player, extra=0x0, stack when it expires
    //BigBounce = 3761, // Boss->player, extra=0x0, dot after big bang/crunch
    //Bind = 2518, // none->player, extra=0x0
    //BluntResistanceDown = 2248, // Comet->player, extra=0x0
    //Pollen = 1507, // ToxicBubble->player, extra=0x0
}

public enum IconID : uint
{
    DarkMatter = 364, // player
    DivisiveDark = 376, // player
    BeckoningDark = 100, // player
    AccelerationBomb = 267, // player
    Nox = 197, // player
    AkhRhai = 23, // player
    FracturedEventideEW = 501, // Boss
    FracturedEventideWE = 502, // Boss
}

public enum TetherID : uint
{
    VoidMeteorCloseClipping = 252, // Comet->player
    VoidMeteorCloseGood = 253, // Comet->player
    VoidMeteorStretchedClipping = 254, // Comet->player
    VoidMeteorStretchedGood = 255, // Comet->player
    FlowOfTheAbyss = 265, // FlowOfTheAbyss->Boss
}

class AbyssalEchoes(BossModule module) : Components.StandardAOEs(module, AID.AbyssalEchoes, new AOEShapeCircle(12));
class BigBangPuddle(BossModule module) : Components.StandardAOEs(module, AID.BigBangAOE, 5);
class BigCrunchPuddle(BossModule module) : Components.StandardAOEs(module, AID.BigCrunchAOE, 5);

class AbyssalNoxDoom(BossModule module) : BossComponent(module)
{
    private BitMask _doomed;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_doomed.Any())
            hints.Add("Heal doomed players to full!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed[slot] && actor.HPMP.CurHP < actor.HPMP.MaxHP)
            hints.Add("Get healed to full!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _doomed[playerSlot] && player.HPMP.CurHP < player.HPMP.MaxHP ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class SableThread(BossModule module) : Components.GenericWildCharge(module, 6, AID.SableThreadAOE, 60)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.SableThreadTarget)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}

class DarkMatter(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(8);

    public int RemainingCasts { get; private set; }

    public override void Update()
    {
        if (CurrentBaits.RemoveAll(b => b.Target.IsDestroyed || b.Target.IsDead) > 0 && CurrentBaits.Count == 0)
            RemainingCasts = 0;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DarkMatter)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            RemainingCasts = 3;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkMatterAOE)
        {
            ++NumCasts;
            if (--RemainingCasts <= 0)
                CurrentBaits.Clear();
        }
    }
}

class VisceralWhirl(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeNormal = new(29, 14);
    private static readonly AOEShapeRect _shapeOffset = new(60, 14);

    public bool Active => _aoes.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VisceralWhirlRAOE1:
            case AID.VisceralWhirlLAOE1:
                _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.VisceralWhirlRAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.VisceralWhirlLAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.VisceralWhirlRAOE1 or AID.VisceralWhirlRAOE2 or AID.VisceralWhirlLAOE1 or AID.VisceralWhirlLAOE2)
            _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
    }
}

class VoidBio(BossModule module) : Components.Voidzone(module, 2.2f, OID.ToxicBubble, a => a.IsDead, 6);

class FlareTower(BossModule module) : Components.CastTowers(module, AID.FlareAOE, 4, 1, 8);

class FlareScald(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlareAOE:
                _aoes.Add(new(_shape, caster.Position, default, WorldState.FutureTime(2)));
                break;
            case AID.FlareScald:
            case AID.FlareKill:
                ++NumCasts;
                break;
        }
    }
}

class ProminenceSpine(BossModule module) : Components.StandardAOEs(module, AID.ProminenceSpine, new AOEShapeRect(60, 5));

class Nox : Components.StandardChasingAOEs
{
    public Nox(BossModule module) : base(module, new AOEShapeCircle(10), AID.NoxAOEFirst, AID.NoxAOERest, 2.5f, 1.55f, 3)
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Chasers.Where(c => c.Target != actor))
            foreach (var (pos, activation) in FutureDrops(c))
                hints.AddForbiddenZone(Shape.Distance(pos, default), activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var c in Chasers)
            foreach (var (pos, _) in FutureDrops(c))
                Shape.Outline(Arena, pos, default, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }

    private IEnumerable<(WPos pos, DateTime activation)> FutureDrops(Chaser chaser)
    {
        var pos = chaser.PredictedPosition();
        for (var i = 1; i < chaser.NumRemaining; ++i)
        {
            var offset = chaser.Target.Position - pos;
            var distance = offset.Length();
            pos = distance > MoveDistance ? pos + MoveDistance * offset / distance : chaser.Target.Position;
            yield return (pos, chaser.NextActivation.AddSeconds(i * SecondsBetweenActivations));
        }
    }
}

class NoxBait(BossModule module) : Components.UniformStackSpread(module, 0, 10, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Nox)
            AddSpread(actor, WorldState.FutureTime(10));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NoxAOEFirst)
            Spreads.Clear();
    }
}

class AccelerationBomb(BossModule module) : Components.StayMove(module, 3)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
            SetState(Raid.FindSlot(actor.InstanceID), new(Requirement.Stay, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

class DarkBeckons : Components.UniformStackSpread
{
    public DarkBeckons(BossModule module) : base(module, 6, 0)
    {
        foreach (var p in Raid.WithoutSlot(true))
            if (p.FindStatus(SID.BeckoningDark) is var s && s != null)
                AddStack(p, s.Value.ExpireAt);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.BeckoningDark)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkBeckons)
            Stacks.Clear();
    }
}

class DarkDivides : Components.UniformStackSpread
{
    public DarkDivides(BossModule module) : base(module, 0, 5, alwaysShowSpreads: true)
    {
        foreach (var p in Raid.WithoutSlot(true))
            if (p.FindStatus(SID.DivisiveDark) is var s && s != null)
                AddSpread(p, s.Value.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkDivides)
            Spreads.Clear();
    }
}

class MeteorImpactProximity(BossModule module) : Components.StandardAOEs(module, AID.MeteorImpactProximity, new AOEShapeCircle(10)); // TODO: verify falloff

class MeteorImpactCharge(BossModule module) : BossComponent(module)
{
    struct PlayerState
    {
        public Actor? TetherSource;
        public bool Stretched;
        public bool NonClipping;
        public List<RelTriangle>? DangerZone;
    }

    public int NumCasts { get; private set; }
    public bool Active => _playerStates.Any(s => s.TetherSource != null);
    private readonly List<WPos> _meteors = [];
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private DateTime _chargeDeadline;

    private const float _radius = 2;
    private const float _minStretch = 18;
    private const int _ownThickness = 2;
    private const int _otherThickness = 1;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerStates[slot].TetherSource != null)
        {
            if (!_playerStates[slot].NonClipping)
                hints.Add("Avoid placed meteors!");
            if (!_playerStates[slot].Stretched)
                hints.Add("Stretch the tether!");
        }

        if (IsClippedByOthers(actor))
            hints.Add("GTFO from charges!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_playerStates[slot].TetherSource is var source && source != null)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(source.Position, _minStretch + 0.5f), _chargeDeadline);
            foreach (var m in _meteors)
            {
                var toMeteor = m - source.Position;
                hints.AddForbiddenZone(ShapeDistance.Cone(source.Position, 60, Angle.FromDirection(toMeteor), Angle.Asin(_radius * 2 / toMeteor.Length())), _chargeDeadline);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _playerStates[playerSlot].TetherSource != null ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_meteors.Count > 0 && _playerStates[pcSlot].TetherSource is var source && source != null)
        {
            ref var state = ref _playerStates.AsSpan()[pcSlot];
            state.DangerZone ??= BuildShadowZone(source.Position - Module.Center);
            Arena.Zone(state.DangerZone, ArenaColor.AOE);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in _meteors)
            Arena.AddCircle(m, _radius, ArenaColor.Object);

        foreach (var (slot, target) in Raid.WithSlot(true))
        {
            if (_playerStates[slot].TetherSource is var source && source != null)
            {
                var thickness = slot == pcSlot ? _ownThickness : _otherThickness;
                var norm = (target.Position - source.Position).Normalized().OrthoL() * 2;
                var rot = Angle.FromDirection(target.Position - source.Position);
                Arena.PathArcTo(target.Position, 2, (rot + 90.Degrees()).Rad, (rot - 90.Degrees()).Rad);
                Arena.PathLineTo(source.Position - norm);
                Arena.PathLineTo(source.Position + norm);
                Arena.PathStroke(true, _playerStates[slot].NonClipping ? ArenaColor.Safe : ArenaColor.Danger, thickness);
                Arena.AddLine(source.Position, target.Position, _playerStates[slot].Stretched ? ArenaColor.Safe : ArenaColor.Danger, thickness);
            }
        }

        if (_playerStates[pcSlot].TetherSource is var pcSource && pcSource != null)
            Arena.AddCircle(pcSource.Position, _minStretch, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorImpactProximity)
        {
            _meteors.Add(caster.Position);
        }
        else if ((AID)spell.Action.ID is AID.MeteorImpactChargeNormal or AID.MeteorImpactChargeClipping)
        {
            ++NumCasts;
            var (closestSlot, closestPlayer) = Raid.WithSlot(true).Closest(spell.TargetXZ);
            if (closestPlayer != null)
                _playerStates[closestSlot].TetherSource = null;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.VoidMeteorCloseClipping or TetherID.VoidMeteorCloseGood or TetherID.VoidMeteorStretchedClipping or TetherID.VoidMeteorStretchedGood && Raid.TryFindSlot(tether.Target, out var slot))
        {
            ref var state = ref _playerStates[slot];
            state.TetherSource = source;
            state.Stretched = (TetherID)tether.ID is TetherID.VoidMeteorStretchedClipping or TetherID.VoidMeteorStretchedGood;
            state.NonClipping = (TetherID)tether.ID is TetherID.VoidMeteorCloseGood or TetherID.VoidMeteorStretchedGood;
            if (_chargeDeadline == default)
                _chargeDeadline = WorldState.FutureTime(12);
        }
    }

    private IEnumerable<WDir> BuildShadowPolygon(WDir sourceOffset, WDir meteorOffset)
    {
        var toMeteor = meteorOffset - sourceOffset;
        var dirToMeteor = Angle.FromDirection(toMeteor);
        var halfAngle = Angle.Asin(_radius * 2 / toMeteor.Length());
        var dirFromMeteor = dirToMeteor + 180.Degrees();
        var halfAngleFromMeteor = 90.Degrees() - halfAngle;
        foreach (var off in CurveApprox.CircleArc(_radius * 2, dirFromMeteor + halfAngleFromMeteor, dirFromMeteor - halfAngleFromMeteor, 0.2f))
            yield return meteorOffset + off;
        yield return sourceOffset + 100 * (dirToMeteor + halfAngle).ToDirection();
        yield return sourceOffset + 100 * (dirToMeteor - halfAngle).ToDirection();
    }

    private List<RelTriangle> BuildShadowZone(WDir sourceOffset)
    {
        PolygonClipper.Operand set = new();
        foreach (var m in _meteors)
            set.AddContour(BuildShadowPolygon(sourceOffset, m - Module.Center));
        var simplified = Arena.Bounds.Clipper.Simplify(set, Clipper2Lib.FillRule.NonZero);
        return Arena.Bounds.ClipAndTriangulate(simplified);
    }

    private static bool IsClipped(WPos source, WPos target, WPos position) => position.InCircle(target, _radius) || position.InRect(source, target - source, _radius);

    private bool IsClippedByOthers(Actor player)
    {
        foreach (var (i, p) in Raid.WithSlot(true).Exclude(player))
            if (_playerStates[i].TetherSource is var src && src != null && IsClipped(src.Position, p.Position, player.Position))
                return true;
        return false;
    }
}

class MeteorImpactExplosion(BossModule module) : Components.StandardAOEs(module, AID.MeteorImpactExplosion, new AOEShapeCircle(10));

class BlackHoleDrop(BossModule module) : Components.StandardAOEs(module, AID.BlackHoleAOE, 4);

class BlackHole(BossModule module) : BossComponent(module)
{
    public Actor? Voidzone;
    public WPos? SafeCorner { get; private set; }
    private DateTime _growthStart;

    private static readonly WPos _cornerE = new(117, 83.5f);
    private static readonly WPos _cornerW = new(83, 83.5f);
    private const float _cornerRadius = 3;

    private const float _startingRadius = 4;
    private const float _maxRadius = 35;
    private const float _growthPerSecond = 3.3f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Voidzone != null && actor.Position.InCircle(Voidzone.Position, CurrentRadius))
            hints.Add("GTFO from black hole!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Voidzone != null)
            hints.AddForbiddenZone(ShapeDistance.Circle(Voidzone.Position, CurrentRadius + 1));
        if (SafeSpot is { } spot)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(spot, _cornerRadius, 5));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (SafeSpot is { } spot)
            Arena.ZoneCircle(spot, _cornerRadius, ArenaColor.SafeFromAOE);
        if (Voidzone != null)
            Arena.ZoneCircle(Voidzone.Position, CurrentRadius, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (SafeSpot is { } spot)
            Arena.AddCircle(spot, _cornerRadius, ArenaColor.Safe, 2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FracturedEventideAOEFirstWE:
                SafeCorner = _cornerE;
                break;
            case AID.FracturedEventideAOEFirstEW:
                SafeCorner = _cornerW;
                break;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BlackHole)
            Voidzone = actor;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor == Voidzone)
        {
            switch (state)
            {
                case 0x00100008:
                    _growthStart = WorldState.CurrentTime;
                    break;
                case 0x00040020:
                    Voidzone = null;
                    _growthStart = default;
                    SafeCorner = null;
                    break;
            }
        }
    }

    private WPos? SafeSpot => Module.FindComponent<FracturedEventide>()?.Active == true ? null : SafeCorner;

    private float CurrentRadius => _growthStart == default ? _startingRadius : Math.Min(_maxRadius, _startingRadius + _growthPerSecond * (float)(WorldState.CurrentTime - _growthStart).TotalSeconds);
}

class FracturedEventide(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;

    private static readonly AOEShapeRect _shape = new(60, 4, 10);
    private static readonly AOEShapeRect _shapeVisual = new(60, 4);
    private const int _maxCasts = 21;
    private const float _stepDuration = 0.45f;
    private const float _startRadius = 3;
    private static readonly WDir _startOffset = new(-13.1f, 4.6f);

    public bool Active => _source != null && NumCasts < _maxCasts;

    private float Elapsed => (float)(WorldState.CurrentTime - _startingActivation).TotalSeconds;
    private Angle CurrentRotation => _startingRotation + Math.Clamp(Elapsed / _stepDuration, 0, _maxCasts - 1) * _increment;

    private WPos StartSpot => _source!.Position + new WDir(_increment.Rad > 0 ? _startOffset.X : -_startOffset.X, _startOffset.Z);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source == null)
            yield break;

        for (var i = NumCasts; i < _maxCasts; ++i)
            yield return new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(_stepDuration * i));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!Active)
            return;
        if (Elapsed < 0)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(StartSpot, _startRadius, 5));
        else if (Module.FindComponent<BlackHole>()?.SafeCorner is { } corner)
            hints.GoalZones.Add(AIHints.GoalProximity(corner, 30, 1));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            _shapeVisual.Draw(Arena, _source!.Position, CurrentRotation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FracturedEventideAOEFirstWE or AID.FracturedEventideAOEFirstEW)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0 ? -7.Degrees() : 7.Degrees();
            _startingActivation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FracturedEventideAOEFirstWE or AID.FracturedEventideAOEFirstEW or AID.FracturedEventideAOERest)
            ++NumCasts;
    }
}

class DimensionalSurgePuddle(BossModule module) : Components.StandardAOEs(module, AID.DimensionalSurgePuddle, 5);
class DimensionalSurgeStarCenter(BossModule module) : Components.StandardAOEs(module, AID.DimensionalSurgeStarCenter, 2);
class DimensionalSurgeStarLine(BossModule module) : Components.StandardAOEs(module, AID.DimensionalSurgeStarLine, new AOEShapeRect(60, 1));

class Nostalgia(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NostalgiaBury1 or AID.NostalgiaBury2 or AID.NostalgiaBury3 or AID.NostalgiaBury4 or AID.NostalgiaRoar1 or AID.NostalgiaRoar2 or AID.NostalgiaPrimalRoar)
            ++NumCasts;
    }
}

class FlowOfTheAbyssDimensionalSurge(BossModule module) : Components.StandardAOEs(module, AID.FlowOfTheAbyssDimensionalSurge, new AOEShapeRect(60, 7));

class FlowOfTheAbyssSpread(BossModule module) : Components.UniformStackSpread(module, 0, 5, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AkhRhai)
            AddSpread(actor, WorldState.FutureTime(5));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhaiFirst)
            Spreads.Clear();
    }
}

class FlowOfTheAbyssAkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhaiFirst)
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhaiRest && ++NumCasts >= _aoes.Count * 10)
            _aoes.Clear();
    }
}

class ChasmicNails(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Angle rot, DateTime activation)> _angles = [];

    private static readonly AOEShapeCone _shape = new(60, 20.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var a in _angles.Skip(1).Take(2))
            yield return new(_shape, Module.PrimaryActor.Position, a.rot, a.activation);
        if (_angles.Count > 0)
            yield return new(_shape, Module.PrimaryActor.Position, _angles[0].rot, _angles[0].activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChasmicNailsAOE1 or AID.ChasmicNailsAOE2 or AID.ChasmicNailsAOE3 or AID.ChasmicNailsAOE4 or AID.ChasmicNailsAOE5)
        {
            _angles.Add((spell.Rotation, Module.CastFinishAt(spell)));
            _angles.SortBy(a => a.activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChasmicNailsAOE1 or AID.ChasmicNailsAOE2 or AID.ChasmicNailsAOE3 or AID.ChasmicNailsAOE4 or AID.ChasmicNailsAOE5)
        {
            ++NumCasts;
            if (_angles.Count > 0)
                _angles.RemoveAt(0);
        }
    }
}

class T07ZeromusStates : StateMachineBuilder
{
    public T07ZeromusStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsDead || Module.PrimaryActor.HPMP.CurHP <= 1 || (Module.PrimaryActor.CastInfo?.IsSpell(AID.RendTheRift) ?? false);
        DeathPhase(1, Phase2);
    }

    private void Phase1(uint id)
    {
        AbyssalNoxEchoesSableThread(id, 6.2f, 6.2f);
        VisceralWhirl(id + 0x10000, 8.4f);
        DarkMatter(id + 0x20000, 6.3f);
        FlareNox(id + 0x30000, 5.3f);
        VoidBioVisceralWhirl(id + 0x40000, 7.0f);
        BigBang(id + 0x50000, 4.4f);
        MeteorImpactTethers(id + 0x60000, 6.4f);
        DarkMatter(id + 0x70000, 8.2f);
        BlackHole(id + 0x80000, 6.3f);
        VoidMeteor(id + 0x90000, 4.0f);
        BlackHole(id + 0xA0000, 1.9f);
        BigCrunch(id + 0xB0000, 4.1f);
        AbyssalNoxEchoesSableThread(id + 0xC0000, 10.2f, 3.0f);
        FlareNox(id + 0xD0000, 7.3f);
        DarkMatter(id + 0xE0000, 13.1f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Phase2(uint id)
    {
        RendTheRift(id, 0);
        DimensionalSurgeNostalgia(id + 0x10000, 8.2f);
        FlowOfTheAbyss(id + 0x20000, 7.4f);
        DimensionalSurgeNostalgia(id + 0x30000, 5.7f);
        FlowOfTheAbyss(id + 0x40000, 7.3f);
        DimensionalSurgeNostalgia(id + 0x50000, 5.8f);
        for (uint i = 0; i < 8; ++i)
        {
            FlowOfTheAbyss(id + 0x60000 + i * 0x20000, 7.3f);
            DimensionalSurgeNostalgia(id + 0x70000 + i * 0x20000, 5.8f);
        }

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void AbyssalNoxEchoesSableThread(uint id, float delay, float sableDelay)
    {
        Cast(id, AID.AbyssalNox, delay, 5, "Doom")
            .ActivateOnEnter<AbyssalNoxDoom>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AbyssalEchoes>(id + 0x10, 0.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<AbyssalEchoes>();
        ComponentCondition<AbyssalEchoes>(id + 0x20, 15.9f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<AbyssalEchoes>();

        CastStart(id + 0x100, AID.SableThread, sableDelay)
            .ActivateOnEnter<SableThread>();
        CastEnd(id + 0x101, 5)
            .DeactivateOnExit<AbyssalNoxDoom>();
        ComponentCondition<SableThread>(id + 0x110, 0.7f, comp => comp.NumCasts > 0, "Line stack start");
        ComponentCondition<SableThread>(id + 0x120, 6.1f, comp => comp.NumCasts >= 5, "Line stack resolve")
            .DeactivateOnExit<SableThread>();
    }

    private void DarkMatter(uint id, float delay)
    {
        CastStart(id, AID.DarkMatter, delay)
            .ActivateOnEnter<DarkMatter>();
        CastEnd(id + 1, 4);
        ComponentCondition<DarkMatter>(id + 0x10, 0.8f, comp => comp.NumCasts >= 1, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DarkMatter>(id + 0x11, 1.6f, comp => comp.NumCasts >= 2, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DarkMatter>(id + 0x12, 1.5f, comp => comp.NumCasts >= 3, "Tankbuster 3")
            .DeactivateOnExit<DarkMatter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State VisceralWhirl(uint id, float delay)
    {
        CastMulti(id, [AID.VisceralWhirlR, AID.VisceralWhirlL], delay, 8)
            .ActivateOnEnter<VisceralWhirl>();
        return ComponentCondition<VisceralWhirl>(id + 2, 0.8f, comp => !comp.Active, "Lines")
            .DeactivateOnExit<VisceralWhirl>();
    }

    private void FlareNox(uint id, float delay)
    {
        Cast(id, AID.Flare, delay, 7)
            .ActivateOnEnter<FlareTower>();
        ComponentCondition<FlareTower>(id + 2, 1.1f, comp => comp.NumCasts > 0, "Tower")
            .ActivateOnEnter<FlareScald>()
            .DeactivateOnExit<FlareTower>();

        CastStart(id + 0x10, AID.Nox, 1.2f)
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<NoxBait>();
        ComponentCondition<ProminenceSpine>(id + 0x11, 0.9f, comp => comp.Casters.Count > 0, "AOEs at tower")
            .ActivateOnEnter<ProminenceSpine>();
        CastEnd(id + 0x12, 3);
        ComponentCondition<ProminenceSpine>(id + 0x20, 2, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<FlareScald>()
            .DeactivateOnExit<ProminenceSpine>();

        ComponentCondition<Nox>(id + 0x30, 4, comp => comp.NumCasts > 0, "Chaser start")
            .DeactivateOnExit<NoxBait>();
        ComponentCondition<Nox>(id + 0x40, 3.2f, comp => comp.Chasers.Count == 0, "Chaser resolve")
            .DeactivateOnExit<Nox>();
    }

    private void VoidBioVisceralWhirl(uint id, float delay)
    {
        Cast(id, AID.VoidBio, delay, 5, "Bubbles spawn")
            .ActivateOnEnter<VoidBio>();
        VisceralWhirl(id + 0x100, 6.1f)
            .DeactivateOnExit<VoidBio>();
    }

    private void BigBang(uint id, float delay)
    {
        Cast(id, AID.BigBang, delay, 10, "Raidwide")
            .ActivateOnEnter<BigBangPuddle>()
            .ActivateOnEnter<AccelerationBomb>()
            .DeactivateOnExit<BigBangPuddle>()
            .SetHint(StateMachine.StateHint.Raidwide);
        VisceralWhirl(id + 0x100, 10.2f)
            .DeactivateOnExit<AccelerationBomb>();
        ComponentCondition<DarkBeckons>(id + 0x200, 8, comp => !comp.Active, "Stack")
            .ActivateOnEnter<DarkBeckons>()
            .DeactivateOnExit<DarkBeckons>();
        ComponentCondition<DarkDivides>(id + 0x210, 5, comp => !comp.Active, "Spread")
            .ActivateOnEnter<DarkDivides>()
            .DeactivateOnExit<DarkDivides>();
    }

    private void BigCrunch(uint id, float delay)
    {
        Cast(id, AID.BigCrunch, delay, 10, "Raidwide")
            .ActivateOnEnter<BigCrunchPuddle>()
            .DeactivateOnExit<BigCrunchPuddle>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MeteorImpactTethers(uint id, float delay)
    {
        Cast(id, AID.MeteorImpact, delay, 11, "Tethers")
            .ActivateOnEnter<MeteorImpactCharge>();
        ComponentCondition<MeteorImpactCharge>(id + 0x10, 2.1f, comp => comp.NumCasts > 0, "Charges start");
        ComponentCondition<MeteorImpactCharge>(id + 0x11, 1, comp => !comp.Active, "Charges resolve")
            .DeactivateOnExit<MeteorImpactCharge>();
    }

    private void VoidMeteor(uint id, float delay)
    {
        Cast(id, AID.VoidMeteor, delay, 4.8f)
            .ActivateOnEnter<MeteorImpactProximity>()
            .ActivateOnEnter<MeteorImpactCharge>();
        ComponentCondition<MeteorImpactProximity>(id + 2, 1.3f, comp => comp.NumCasts > 0, "Proximity")
            .DeactivateOnExit<MeteorImpactProximity>();
        Cast(id + 0x10, AID.MeteorImpact, 0.9f, 11, "Tethers");
        ComponentCondition<MeteorImpactCharge>(id + 0x20, 2.1f, comp => comp.NumCasts > 0, "Charges start");
        ComponentCondition<MeteorImpactCharge>(id + 0x21, 1.2f, comp => !comp.Active, "Charges resolve")
            .DeactivateOnExit<MeteorImpactCharge>();
        ComponentCondition<MeteorImpactExplosion>(id + 0x30, 2.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<MeteorImpactExplosion>();
        ComponentCondition<MeteorImpactExplosion>(id + 0x31, 5, comp => comp.NumCasts > 0, "Explosions")
            .DeactivateOnExit<MeteorImpactExplosion>();
    }

    private void BlackHole(uint id, float delay)
    {
        Cast(id, AID.BlackHole, delay, 5)
            .ActivateOnEnter<BlackHoleDrop>()
            .ActivateOnEnter<BlackHole>();
        CastStartMulti(id + 0x10, [AID.FracturedEventideWE, AID.FracturedEventideEW], 2.1f)
            .ActivateOnEnter<FracturedEventide>();
        ComponentCondition<BlackHoleDrop>(id + 0x11, 4.4f, comp => comp.NumCasts > 0, "Black hole")
            .DeactivateOnExit<BlackHoleDrop>();
        CastEnd(id + 0x12, 3.6f);
        ComponentCondition<FracturedEventide>(id + 0x13, 0.5f, comp => comp.NumCasts > 0, "Laser start");
        ComponentCondition<FracturedEventide>(id + 0x20, 9, comp => comp.NumCasts >= 21, "Laser end")
            .DeactivateOnExit<FracturedEventide>();
        ComponentCondition<BlackHole>(id + 0x30, 2.7f, comp => comp.Voidzone == null, "Black hole resolve")
            .DeactivateOnExit<BlackHole>();
    }

    private void RendTheRift(uint id, float delay)
    {
        Cast(id, AID.RendTheRift, delay, 6, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DimensionalSurgeNostalgia(uint id, float delay)
    {
        ComponentCondition<DimensionalSurgePuddle>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<DimensionalSurgePuddle>()
            .ActivateOnEnter<DimensionalSurgeStarCenter>()
            .ActivateOnEnter<DimensionalSurgeStarLine>();
        CastStart(id + 0x10, AID.Nostalgia, 4.1f, "First puddles");
        ComponentCondition<DimensionalSurgePuddle>(id + 0x20, 2, comp => comp.Casters.Count == 0, "Last puddles")
            .DeactivateOnExit<DimensionalSurgePuddle>()
            .DeactivateOnExit<DimensionalSurgeStarCenter>()
            .DeactivateOnExit<DimensionalSurgeStarLine>();
        CastEnd(id + 0x30, 3);
        ComponentCondition<Nostalgia>(id + 0x40, 0.7f, comp => comp.NumCasts >= 1, "Raidwides start")
            .ActivateOnEnter<Nostalgia>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x41, 1, comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x42, 1, comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x43, 1, comp => comp.NumCasts >= 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x44, 2, comp => comp.NumCasts >= 5)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x45, 1, comp => comp.NumCasts >= 6)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x46, 3, comp => comp.NumCasts >= 7, "Raidwides end")
            .DeactivateOnExit<Nostalgia>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FlowOfTheAbyss(uint id, float delay)
    {
        CastStart(id, AID.FlowOfTheAbyss, delay)
            .ActivateOnEnter<FlowOfTheAbyssDimensionalSurge>()
            .ActivateOnEnter<FlowOfTheAbyssSpread>();
        CastEnd(id + 1, 7);
        ComponentCondition<FlowOfTheAbyssSpread>(id + 2, 1.1f, comp => !comp.Active, "Spreads")
            .ActivateOnEnter<FlowOfTheAbyssAkhRhai>()
            .DeactivateOnExit<FlowOfTheAbyssSpread>();
        ComponentCondition<FlowOfTheAbyssDimensionalSurge>(id + 3, 0.9f, comp => comp.NumCasts > 0, "Line")
            .DeactivateOnExit<FlowOfTheAbyssDimensionalSurge>();
        ComponentCondition<FlowOfTheAbyssAkhRhai>(id + 4, 2.3f, comp => comp.NumCasts > 0, "Puddles start");

        Cast(id + 0x10, AID.ChasmicNails, 4.9f, 7)
            .ActivateOnEnter<ChasmicNails>()
            .DeactivateOnExit<FlowOfTheAbyssAkhRhai>();
        ComponentCondition<ChasmicNails>(id + 0x20, 0.8f, comp => comp.NumCasts >= 1, "Pizza start");
        ComponentCondition<ChasmicNails>(id + 0x21, 0.6f, comp => comp.NumCasts >= 2);
        ComponentCondition<ChasmicNails>(id + 0x22, 0.8f, comp => comp.NumCasts >= 3);
        ComponentCondition<ChasmicNails>(id + 0x23, 0.7f, comp => comp.NumCasts >= 4);
        ComponentCondition<ChasmicNails>(id + 0x24, 0.6f, comp => comp.NumCasts >= 5, "Pizza resolve")
            .DeactivateOnExit<ChasmicNails>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 964, NameID = 12586)]
public class T07Zeromus(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
