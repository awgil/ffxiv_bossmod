namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D70ForgivenZeal;

public enum OID : uint
{
    Boss = 0x484C, // R7.000, x1
    Helper = 0x233C, // R0.500, x8, Helper type
    HolySphere = 0x484D, // R0.700, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    Jump = 43416, // Boss->location, no cast, single-target
    ZealousGlowerClose = 43406, // Boss->self, 3.0s cast, single-target
    ZealousGlowerClose1 = 43407, // Helper->self, 4.0s cast, range 5 width 10 rect
    ZealousGlowerClose2 = 43408, // Helper->self, 4.5s cast, range 5 width 10 rect
    ZealousGlowerClose3 = 43409, // Helper->self, 5.0s cast, range 5 width 10 rect
    ZealousGlowerClose4 = 43410, // Helper->self, 5.5s cast, range 5 width 10 rect
    ZealousGlowerFar = 43411, // Boss->self, 3.0s cast, single-target
    ZealousGlowerFar1 = 43412, // Helper->self, 4.0s cast, range 5 width 10 rect
    ZealousGlowerFar2 = 43413, // Helper->self, 4.5s cast, range 5 width 10 rect
    ZealousGlowerFar3 = 43414, // Helper->self, 5.0s cast, range 5 width 10 rect
    ZealousGlowerFar4 = 43415, // Helper->self, 5.5s cast, range 5 width 10 rect
    BrutalHalo = 43417, // 484D->self, 2.0s cast, range 3-20 donut
    ArdorousEyeCW = 43418, // Boss->self, 6.0s cast, single-target
    ArdorousEyeCW1 = 43419, // Helper->self, 7.5s cast, range 5-10 90-degree donut
    ArdorousEyeCW2 = 43420, // Helper->self, 7.8s cast, range 5-10 90-degree donut
    ArdorousEyeCW3 = 43421, // Helper->self, 8.1s cast, range 5-10 90-degree donut
    ArdorousEyeCW4 = 43422, // Helper->self, 8.4s cast, range 5-10 90-degree donut
    ArdorousEyeCCW = 43423, // Boss->self, 6.0s cast, single-target
    ArdorousEyeCCW1 = 43424, // Helper->self, 7.5s cast, range 5-10 90-degree donut
    ArdorousEyeCCW2 = 43425, // Helper->self, 7.8s cast, range 5-10 90-degree donut
    ArdorousEyeCCW3 = 43426, // Helper->self, 8.1s cast, range 5-10 90-degree donut
    ArdorousEyeCCW4 = 43427, // Helper->self, 8.4s cast, range 5-10 90-degree donut
    DisorientingGroanCast = 43430, // Boss->self, 6.0s cast, single-target
    DisorientingGroan = 43431, // Helper->self, 7.0+1.0s cast, distance 7 knockback
    OctupleSwipeIndicator = 43437, // Helper->self, 1.0s cast, range 40 90-degree cone
    OctupleSwipeCast = 43432, // Boss->self, 10.0s cast, single-target
    OctupleSwipe1 = 43433, // Boss->self, no cast, range 40 90-degree cone
    OctupleSwipe2 = 43434, // Boss->self, no cast, range 40 90-degree cone
    OctupleSwipe3 = 43435, // Boss->self, no cast, range 40 90-degree cone
    OctupleSwipe4 = 43436, // Boss->self, no cast, range 40 90-degree cone
    W2000MinaSwingCast = 43428, // Boss->self, 6.0s cast, single-target
    W2000MinaSwing = 43429, // Helper->self, 7.0+1.0s cast, range 8 circle
}

class ZealousGlower(BossModule module) : Components.GroupedAOEs(module, [AID.ZealousGlowerClose1, AID.ZealousGlowerClose2, AID.ZealousGlowerClose3, AID.ZealousGlowerClose4, AID.ZealousGlowerFar1, AID.ZealousGlowerFar2, AID.ZealousGlowerFar3, AID.ZealousGlowerFar4], new AOEShapeRect(5, 5, 0));

class ArdorousEye(BossModule module) : Components.GroupedAOEs(module, [AID.ArdorousEyeCCW1, AID.ArdorousEyeCCW2, AID.ArdorousEyeCCW3, AID.ArdorousEyeCCW4, AID.ArdorousEyeCW1, AID.ArdorousEyeCW2, AID.ArdorousEyeCW3, AID.ArdorousEyeCW4], new AOEShapeDonutSector(5, 10, 45.Degrees()));

class W2000MinaSwing(BossModule module) : Components.StandardAOEs(module, AID.W2000MinaSwing, 8);

class BrutalHalo(BossModule module) : Components.GenericAOEs(module)
{
    private class Line(int cap, DateTime first)
    {
        public int Capacity = cap;
        public List<(Actor, DateTime)> Orbs = [];
        public DateTime Start = first;

        public bool Add(Actor orb)
        {
            if (Orbs.Count >= Capacity)
                return false;

            Orbs.Add((orb, Start.AddSeconds(2.5f * Orbs.Count)));
            return true;
        }
    }

    private readonly List<Line> _lines = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var l in _lines.Take(1))
            foreach (var (o, t) in l.Orbs.Take(1))
                yield return new(new AOEShapeDonut(3, 20), o.Position, default, t);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ZealousGlowerClose:
            case AID.ZealousGlowerFar:
                _lines.Add(new(5, Module.CastFinishAt(spell, 8.3f)));
                break;
            case AID.ArdorousEyeCCW:
            case AID.ArdorousEyeCW:
                _lines.Add(new(8, Module.CastFinishAt(spell, 6.7f)));
                break;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.HolySphere)
            foreach (var l in _lines)
                if (l.Add(actor))
                    break;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrutalHalo)
        {
            NumCasts++;
            if (_lines.Count > 0)
            {
                if (_lines[0].Orbs.Count > 0)
                    _lines[0].Orbs.RemoveAt(0);
                if (_lines[0].Orbs.Count == 0)
                    _lines.RemoveAt(0);
            }
        }
    }
}

class DisorientingGroan(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DisorientingGroan, 7)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(ShapeContains.Donut(Arena.Center, 3, 20), src.Activation);
    }
}

class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imm = true;
        foreach (var p in _predicted.Skip(NumCasts).Take(2))
        {
            yield return p with { Color = imm ? ArenaColor.Danger : ArenaColor.AOE };
            imm = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OctupleSwipeIndicator)
            _predicted.Add(new(new AOEShapeCone(40, 45.Degrees()), Arena.Center, spell.Rotation, WorldState.FutureTime(8.3f + 2 * _predicted.Count)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OctupleSwipe1 or AID.OctupleSwipe2 or AID.OctupleSwipe3 or AID.OctupleSwipe4)
        {
            NumCasts++;
            if (NumCasts >= 8)
            {
                NumCasts = 0;
                _predicted.Clear();
            }
        }
    }
}

class D70ForgivenZealStates : StateMachineBuilder
{
    public D70ForgivenZealStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrutalHalo>()
            .ActivateOnEnter<ZealousGlower>()
            .ActivateOnEnter<ArdorousEye>()
            .ActivateOnEnter<DisorientingGroan>()
            .ActivateOnEnter<W2000MinaSwing>()
            .ActivateOnEnter<OctupleSwipe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1038, NameID = 13971)]
public class D70ForgivenZeal(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(10, MapResolution: 0.25f));

