﻿namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D091SeekerOfSolitude;

public enum AID : uint
{
    _AutoAttack_ = 18280, // Boss->player, no cast, single-target
    _Spell_Shadowbolt = 18281, // Boss->player, 4.0s cast, single-target
    _Spell_ImmortalAnathema = 18851, // Boss->self, 4.0s cast, range 60 circle
    _Spell_Tribulation1 = 18283, // Boss->self, 3.0s cast, single-target
    _Spell_Tribulation = 18852, // Helper->location, 3.0s cast, range 3 circle
    _Spell_DarkShock = 18286, // Boss->self, 3.0s cast, single-target
    _Spell_DarkShock1 = 18287, // Helper->location, 3.0s cast, range 6 circle
    _Ability_Sweep = 18288, // Helper->player, no cast, single-target
    _Ability_DeepClean = 18289, // Helper->player, no cast, single-target
    _Spell_DarkPulse = 18282, // Boss->players, 5.0s cast, range 6 circle
    _Spell_DarkWell = 18285, // Helper->player, 5.0s cast, range 5 circle
    _Spell_DarkWell1 = 18284, // Boss->self, no cast, single-target
    _Spell_MovementMagick = 18713, // Boss->self, 3.0s cast, single-target
}

public enum OID : uint
{
    Boss = 0x2C1A,
    Helper = 0x233C,
    MagickedBroom = 0x2C1B,
    DirtPile = 0x1EAEAE
}

class Tribulation(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_Tribulation), 3);
class ImmortalAnathema(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_ImmortalAnathema));
class DarkPulse(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID._Spell_DarkPulse), 6);
class DarkWell(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_DarkWell), 5);
class DarkShock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_DarkShock1), 6);
class Shadowbolt(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID._Spell_Shadowbolt));

// not sure about radius, sweep trigger is incredibly janky
// filter out brooms who are too far outside the arena since they don't affect players and the AOE lingering on minimap is annoying
class Sweep(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.MagickedBroom).Where(b => MathF.Abs(b.Position.X) <= 23.5f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Sources(Module))
            hints.AddForbiddenZone(ShapeDistance.Capsule(t.Position, t.Rotation, 2, 4));
    }
}

class DeepClean(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly float[] CleaningTime = [11f, 12.5f, 15.5f, 17f, 18.5f];
    private const float CleanLinger = 2f; // estimate
    private readonly List<DirtPile> Cleanings = [];

    private record DirtPile(Actor Actor, DateTime CleaningPredicted, DateTime? CleanedAt)
    {
        public DateTime? CleanedAt = CleanedAt;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Cleanings.RemoveAll(c => c.CleanedAt is DateTime dt && dt.AddSeconds(CleanLinger) < WorldState.CurrentTime);

        return Cleanings.Select(p => new AOEInstance(new AOEShapeCircle(6), p.Actor.Position, default, p.CleaningPredicted))
            .Where(a => (a.Activation - WorldState.CurrentTime).TotalSeconds < 5);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DirtPile && Module.Enemies(OID.DirtPile).Count == 5)
            ScheduleAOEs();
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (modelState == 27 && animState1 == 1)
        {
            var c = Cleanings.MinBy(e => (e.Actor.Position - actor.Position).Length());
            if (c != null)
                c.CleanedAt = WorldState.CurrentTime;
        }
    }

    private void ScheduleAOEs()
    {
        var dirtOrdered = Module.Enemies(OID.DirtPile).OrderBy(d => d.Position.Z).ToList();
        // deep cleans always trigger in order (from north to south) 1 2 4 5 3
        // so just take actor #3 and stick it at the end of the list
        dirtOrdered.Add(dirtOrdered[2]);
        dirtOrdered.RemoveAt(2);
        foreach (var (dirt, delay) in dirtOrdered.Zip(CleaningTime))
            Cleanings.Add(new(dirt, WorldState.FutureTime(delay), null));
    }
}

class SeekerOfSolitudeStates : StateMachineBuilder
{
    public SeekerOfSolitudeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeepClean>()
            .ActivateOnEnter<Sweep>()
            .ActivateOnEnter<DarkPulse>()
            .ActivateOnEnter<DarkWell>()
            .ActivateOnEnter<Tribulation>()
            .ActivateOnEnter<ImmortalAnathema>()
            .ActivateOnEnter<DarkShock>()
            .ActivateOnEnter<Shadowbolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9041)]
public class SeekerOfSolitude(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 187), new ArenaBoundsRect(20.5f, 14.5f));
