namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D033Svarbhanu;

public enum OID : uint
{
    Boss = 0x33EB, // R=7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AetherialDisruption = 25160, // Boss->self, 7.0s cast, single-target
    ChaoticPulse = 27489, // Boss->self, no cast, single-target
    ChaoticUndercurrentRedVisual = 25164, // Helper->self, no cast, single-target
    ChaoticUndercurrentBlueVisual = 25165, // Helper->self, no cast, single-target
    ChaoticUndercurrentRedRect = 25162, // Helper->self, no cast, range 40 width 10 rect
    ChaoticUndercurrentBlueRect = 25163, // Helper->self, no cast, range 40 width 10 rect
    CosmicKissVisual = 25161, // Boss->self, no cast, single-target
    CosmicKissCircle = 25167, // Helper->location, 3.0s cast, range 6 circle
    CosmicKissRect = 25374, // Helper->self, no cast, range 50 width 10 rect, 15 knockback, away from source
    CosmicKissSpread = 25168, // Helper->player, 8.0s cast, range 6 circle
    CosmicKiss = 25169, // Helper->location, 6.0s cast, range 100 circle, knockback 13, away from source
    CrumblingSky = 25166, // Boss->self, 3.0s cast, single-target
    FlamesOfDecay = 25170, // Boss->self, 5.0s cast, range 40 circle
    GnashingOfTeeth = 25171 // Boss->player, 5.0s cast, single-target
}

class ChaoticUndercurrent(BossModule module) : Components.GenericAOEs(module)
{
    private enum Pattern { None, BBRR, RRBB, BRRB, RBBR }
    private Pattern currentPattern;
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 5);
    private static readonly Angle rotation = 90.Degrees();
    private const int X = 280;
    private static readonly List<WPos> coords = [new(X, -142), new(X, -152), new(X, -162), new(X, -172)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        // index 0x08
        // red blue blue red => 0x00400020, red (-142, -172), blue (-152, -162)
        // blue blue red red => 0x00020001, red (-142, -152), blue (-162, -172) 
        // blue red red blue => 0x00100008, red (-152, -162), blue (-172, -142)
        // red red blue blue => 0x01000080, red (-162, -172), blue (-152, -142)
        if (index == 0x08)
        {
            currentPattern = state switch
            {
                0x00400020 => Pattern.RBBR,
                0x00020001 => Pattern.BBRR,
                0x00100008 => Pattern.BRRB,
                0x01000080 => Pattern.RRBB,
                _ => Pattern.None
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var activation = WorldState.FutureTime(7.7f);
        switch ((AID)spell.Action.ID)
        {
            case AID.ChaoticUndercurrentBlueVisual:
                AddAOEsForPattern(true, activation);
                break;
            case AID.ChaoticUndercurrentRedVisual:
                AddAOEsForPattern(false, activation);
                break;
            case AID.ChaoticUndercurrentRedRect:
            case AID.ChaoticUndercurrentBlueRect:
                _aoes.Clear();
                currentPattern = Pattern.None;
                break;
        }
    }

    private void AddAOEsForPattern(bool isBlue, DateTime activation)
    {
        switch (currentPattern)
        {
            case Pattern.RBBR:
                AddAOEs(isBlue ? (1, 2) : (0, 3), activation);
                break;
            case Pattern.BBRR:
                AddAOEs(isBlue ? (2, 3) : (0, 1), activation);
                break;
            case Pattern.BRRB:
                AddAOEs(isBlue ? (0, 3) : (1, 2), activation);
                break;
            case Pattern.RRBB:
                AddAOEs(isBlue ? (0, 1) : (2, 3), activation);
                break;
        }
    }

    private void AddAOEs((int, int) indices, DateTime activation)
    {
        _aoes.Add(new(rect, coords[indices.Item1], rotation, activation));
        _aoes.Add(new(rect, coords[indices.Item2], rotation, activation));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Module.FindComponent<CosmicKissKnockback>()!.Sources(slot, actor).FirstOrDefault();
        if (source != default)
        { } // remove forbidden zones while knockback is active to not confuse the AI
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class CosmicKissSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.CosmicKissSpread, 6);
class CosmicKissCircle(BossModule module) : Components.StandardAOEs(module, AID.CosmicKissCircle, 6);

class CosmicKissRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(50, 5);
    private static readonly Angle rotation = -90.Degrees();
    private const int X = 320;
    private static readonly List<WPos> coords = [new(X, -142), new(X, -152), new(X, -162), new(X, -172)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(3);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001 && _aoes.Count == 0)
        {
            var aoeSets = index switch
            {
                0x0A => // for set 09, 0A, 0C --> 1B, 1D, 1E --> 09, 0A, 0B
                [[0, 2, 3], [0, 1, 3], [1, 2, 3]],
                0x0B => // for set 09, 0B, 0C --> 1B, 1C, 1E --> 0A, 0B, 0C
                [[0, 1, 3], [0, 2, 3], [0, 1, 2]],
                _ => new List<int[]>()
            };

            if (aoeSets.Count > 0)
            {
                AddAOEs(aoeSets[0], 4.3f);
                AddAOEs(aoeSets[1], 9.5f);
                AddAOEs(aoeSets[2], 14.6f);
            }
        }
    }

    private void AddAOEs(IEnumerable<int> indices, float delay)
    {
        foreach (var index in indices)
            _aoes.Add(new(rect, coords[index], rotation, WorldState.FutureTime(delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.CosmicKissRect)
            _aoes.RemoveAt(0);
    }
}

class CosmicKissRaidwide(BossModule module) : Components.RaidwideCast(module, AID.CosmicKiss);

class CosmicKissKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.CosmicKiss, 13)
{
    private static readonly Angle a90 = 90.Degrees();
    private static readonly Angle a45 = 45.Degrees();
    private static readonly Angle a0 = 0.Degrees();
    private static readonly Angle a180 = 180.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<ChaoticUndercurrent>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, bool>>();
        var component = Module.FindComponent<ChaoticUndercurrent>()?.ActiveAOEs(slot, actor)?.ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (component != null && component.Count != 0 && source != default)
        {
            if (component!.Any(x => x.Origin.Z == -152) && component!.Any(x => x.Origin.Z == -162))
            {
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, a0, a45));
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, a180, a45));
            }
            else if (component!.Any(x => x.Origin.Z == -142) && component!.Any(x => x.Origin.Z == -172))
            {
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, a90, a45));
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, -a90, a45));
            }
            else if (component!.Any(x => x.Origin.Z == -142) && component!.Any(x => x.Origin.Z == -152))
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, a180, a90));
            else if (component!.Any(x => x.Origin.Z == -162) && component!.Any(x => x.Origin.Z == -172))
                forbidden.Add(ShapeContains.InvertedCone(Arena.Center, 7, a0, a90));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Any(f => f(p)), source.Activation);
        }
    }
}

class FlamesOfDecay(BossModule module) : Components.RaidwideCast(module, AID.FlamesOfDecay);
class GnashingOfTeeth(BossModule module) : Components.SingleTargetCast(module, AID.GnashingOfTeeth);

class D033SvarbhanuStates : StateMachineBuilder
{
    public D033SvarbhanuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChaoticUndercurrent>()
            .ActivateOnEnter<CosmicKissSpread>()
            .ActivateOnEnter<CosmicKissCircle>()
            .ActivateOnEnter<CosmicKissRect>()
            .ActivateOnEnter<CosmicKissKnockback>()
            .ActivateOnEnter<CosmicKissRaidwide>()
            .ActivateOnEnter<FlamesOfDecay>()
            .ActivateOnEnter<GnashingOfTeeth>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10719)]
public class D033Svarbhanu(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, -157), new ArenaBoundsSquare(20));
