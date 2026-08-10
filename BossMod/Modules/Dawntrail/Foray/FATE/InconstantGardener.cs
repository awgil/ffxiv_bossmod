namespace BossMod.Dawntrail.Foray.FATE.InconstantGardener;

public enum OID : uint {
    Iambe = 0x4C41,
    Helper = 0x233C,
    Iambe1 = 0x4C42, // R1.000, x0 (spawn during fight)
    WinsomeSeed = 0x4C43, // R0.240-0.528, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50855, // Iambe->player, no cast, single-target
    DirectSeeding = 48029, // Iambe->self, 3.0s cast, single-target
    GardenersHymnCast = 48031, // Iambe->self, 2.5s cast, single-target
    GardenersHymn = 48032, // 4C42->location, 6.0s cast, range 5 circle
    Burst = 48033, // 4C43->self, 2.0s cast, range 15 circle
    OdeOfTheUnderfoot = 48037, // Iambe->self, 5.0s cast, range 10 circle
    IambicMarch = 48035, // Iambe->self, 3.0s cast, range 40 circle
}

public enum SID : uint {
    ForwardMarch = 5142, // Iambe->player, extra=0x0
    AboutFace = 5143, // Iambe->player, extra=0x0
    ForcedMarch = 1257, // Iambe->player, extra=0x1/0x2
    Gen = 5106, // 4C42->4C43, extra=0x1
    Gen1 = 5107, // 4C42->4C43, extra=0x1
}

sealed class GardenersHymn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GardenersHymn, 5f);
sealed class OdeOfTheUnderfoot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdeOfTheUnderfoot, 10f);

sealed class IambicMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 2.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, default, default) {
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0) {
            return;
        }

        var move0 = state.PendingMoves[0];
        var requiredFacing = Angle.FromDirection((actor.Position - Module.PrimaryActor.Position).Normalized()) - move0.dir;
        hints.ForbiddenDirections.Add((requiredFacing + 180.0f.Degrees(), 170.0f.Degrees(), move0.activation));
    }
}

sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 15.0f, riskyWithSecondsLeft: 6.0f) {
    private readonly List<Actor> seeds = [];

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.WinsomeSeed) {
            seeds.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.WinsomeSeed) {
            seeds.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.GardenersHymn) {
            foreach (var seed in seeds) {
                if (seed.Position.InCircle(spell.LocXZ, 5.0f)) {
                    Casters.Add(new(Shape, seed.Position, default, Module.CastFinishAt(spell, 3.5f), actorID: seed.InstanceID,
                        shapeDistance: Shape.Distance(seed.Position, default)));
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class InconstantGardenerStates : StateMachineBuilder {
    public InconstantGardenerStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<GardenersHymn>()
            .ActivateOnEnter<OdeOfTheUnderfoot>()
            .ActivateOnEnter<IambicMarch>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(InconstantGardenerStates),
    ConfigType = null, // replace null with typeof(IambeConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Iambe,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2079u,
    SortOrder = 8,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class InconstantGardener(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
