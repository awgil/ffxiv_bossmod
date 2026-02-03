namespace BossMod.RealmReborn.Novice.NoviceTactical;

public enum OID : uint
{
    Boss = 0x463B, // R0.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    MasterOfTrials = 0x100B76,
    NA03Bomb = 0x4640, // R1.500, x0 (spawn during fight)

}
public enum AID : uint
{
    // NA01 React to Attack Markers
    NA01Glaciate = 40679, // 463B->self, 4.5+0.5s cast, single-target
    NA01Glaciate1 = 40680, // 233C->player/463D/463E/463F, 5.0s cast, range 6 circle
    NA01Glaciate2 = 40691, // 463B->self, 6.5+0.5s cast, single-target
    NA01Glaciate3 = 40692, // 233C->player/463D/463E/463F, 7.0s cast, range 6 circle
    NA01FrigidRing = 40694, // 233C->self, 7.0s cast, range 10-20 donut
    NA01BitterChill = 40693, // 233C->self, 11.0s cast, range 10 circle

    NA01A1 = 40677, // 233C->location, no cast, range 20 circle --- Pretty sure this is the Heal

    NA01SpectralBlazeVisual1 = 40682, // 463B->self, 5.5+0.5s cast, single-target
    NA01SpectralBlazeVisual2 = 40695, // 463B->self, 6.5+0.5s cast, single-target
    NA01SpectralBlaze1 = 40683, // 233C->463E, 6.0s cast, range 6 circle
    NA01SpectralBlaze2 = 40696, // 233C->463D, 7.0s cast, range 6 circle
    NA01Fireflood = 40697, // 233C->location, 7.0s cast, range 15 circle

    NA01ScorchingStreakVisual1 = 40685, // 463B->player, 6.5+0.5s cast, single-target
    NA01ScorchingStreakVisual2 = 40698, // 463B->player, 6.5+0.5s cast, single-target
    NA01ScorchingStreak1 = 40686, // 233C->self, no cast, range 44 width 10 rect
    NA01ScorchingStreak2 = 40699, // 233C->self, no cast, range 44 width 10 rect
    NA01BlazingRing = 40700, // 233C->self, 7.0s cast, range 8-25 donut

    NA01PiercingStone = 40688, // 463B->self, 4.5+0.5s cast, single-target
    NA01PiercingStone1 = 40689, // 233C->location, 4.0s cast, range 4 circle
    NA01PiercingStone2 = 40690, // 233C->location, 1.5s cast, range 4 circle
    NA01PiercingStone3 = 40701, // 463B->self, 4.5+0.5s cast, single-target
    NA01PiercingStone4 = 40702, // 233C->location, 4.0s cast, range 4 circle
    NA01PiercingStone5 = 40703, // 233C->location, 1.5s cast, range 4 circle

    // NA02 React to Floor Markers
    NA02RogueWave = 40705, // Boss->self, 5.5+0,5s cast, single-target
    NA02RogueWave1 = 40706, // Helper->self, 6.0s cast, range 20 circle

    NA02Windage = 40708, // Boss->self, 4.5+0,5s cast, single-target
    NA02Windage1 = 40710, // Helper->self, 5.0s cast, range 20 circle
    NA02Windage2 = 40709, // Helper->location, 5.0s cast, range 5 circle
    NA02FuriousFlare = 40712, // Boss->self, 5.5+0,5s cast, single-target
    NA02FuriousFlare1 = 40713, // Helper->location, 6.0s cast, range 40 circle
    NA02ThunderingPillar = 40715, // Boss->self, 5.5+0,5s cast, single-target
    NA02ThunderingPillar1 = 40716, // Helper->self, 6.0s cast, range 4 circle
    NA02FervidSurge = 40718, // Boss->self, 2.5+0,5s cast, single-target
    NA02Explosion = 40719, // Helper->self, no cast, range 4 circle
    NA02RogueWave2 = 40721, // Boss->self, 5.5+0,5s cast, single-target
    NA02RogueWave3 = 40722, // Helper->self, 6.0s cast, range 20 circle
    NA02WaterDrop = 40723, // Helper->location, 7.5s cast, range 10 circle
    NA02Upwell = 40724, // Boss->self, 6.5+0,5s cast, single-target
    NA02Upwell1 = 40726, // Helper->self, 7.0s cast, range 40 circle
    NA02Upwell2 = 40725, // Helper->location, 7.0s cast, range 5 circle
    NA02ThunderingPillar2 = 40727, // Helper->self, 16.0s cast, range 4 circle
    NA02FuriousFlare2 = 40729, // Boss->self, 6.5+0,5s cast, single-target
    FuriousFlare3 = 40730, // Helper->location, 7.0s cast, range 40 circle
    NA02BlazingSurge = 40731, // Boss->self, 4.5+0,5s cast, single-target
    NA02BlazingSurge1 = 40732, // Helper->self, 5.0s cast, range 20 180-degree cone
    NA02BlazingSurge2 = 40733, // Helper->self, 8.0s cast, range 20 180-degree cone

    // NA03 React to Advanced Visual Indicators
    NA03BurningPillar = 40735, // Boss->self, 5.5+0,5s cast, single-target
    NA03BurningPillar1 = 40736, // Helper->player/463C/463D/463F, no cast, single-target
    NA03GreatBallOfFire = 40738, // Boss->self, 5.5+0,5s cast, single-target
    NA03GreatBallOfFire1 = 40739, // Helper->player, no cast, single-target
    NA03PetrifyingLight = 40741, // Boss->self, 11.5+0,5s cast, single-target
    NA03PetrifyingLight1 = 40742, // Boss->self, 4.5+0,5s cast, single-target
    NA03PetrifyingLight2 = 40743, // Helper->self, 5.0s cast, range 60 circle
    NA03Shackles = 40744, // Boss->player, no cast, single-target
    NA03Shackles1 = 40745, // Boss->player, no cast, single-target
    NA03Glaciate = 40746, // Boss->self, 9.5+0,5s cast, single-target
    NA03Glaciate1 = 40747, // Helper->self, 10.0s cast, range 6 circle
    NA03FanOfFlames = 40749, // Boss->self, 5.5+0,5s cast, single-target
    NA03BurningPillar2 = 40750, // Helper->player/463C/463F/463D, no cast, single-target
    NA03FanOfFlames1 = 40752, // Helper->self, 6.0s cast, range 60 45-degree cone
    NA03FanOfFlames2 = 40753, // Helper->self, 9.0s cast, range 60 45-degree cone
    NA03GreatBallOfFire2 = 40754, // Boss->self, 6.5+0,5s cast, single-target
    NA03GreatBallOfFire3 = 40755, // Helper->player/463C/463D/463F, no cast, single-target
    NA03BitterChill = 40758, // Boss->self, 2.5+0,5s cast, single-target
    NA03BitterChill1 = 40759, // Helper->self, 10.0s cast, range 10 circle
    NA03PetrifyingLight3 = 40756, // Boss->self, 7.5+0,5s cast, single-target
    NA03PetrifyingLight4 = 40757, // Helper->self, 8.0s cast, range 60 circle
    NA03FrigidRing = 40760, // Helper->self, 13.5s cast, range 10-20 donut
    NA03Withdraw = 40761, // Boss->self, 3.5+0,5s cast, single-target
    NA03Withdraw1 = 40762, // 46B2->self, 4.0s cast, range 80 circle
    NA03Fireflood = 40763, // Boss->self, 18.5+0,5s cast, single-target
    NA03Fireflood1 = 40764, // Helper->location, 19.0s cast, range 15 circle
}
public enum IconID : uint
{
    NA01GlaciateSpread1 = 139,
    NA01SpectralBlazeStack1 = 318,
    NA01GlaciateSpread2 = 375,
    NA01SpectralBlazeStack2 = 317,
    NA01ScorchingStreakTankbuster = 412,
}

public enum TetherID : uint
{
    NA03Tether = 57, // Boss/463D/463C/463F->player/Boss
    NA03Tether2 = 84, // Boss/46B3->4640/player/463F/463C/463D
}

class StartingPositions(BossModule module) : BossComponent(module)
{
    private WPos? readyPos;
    private float radius = 1f;
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            readyPos = Module.WorldState.CurrentCFCID switch
            {
                1012 => // NA01 React to Attack Markers
                    index switch
                    {
                        0 => new WPos(-3.3f, 0f),
                        1 => new WPos(-9.9f, 9.5f),
                        2 => new WPos(-7.7f, -0.1f),
                        3 => new WPos(-14.8f, 0f),
                        4 => new WPos(-3.2f, -0.1f),
                        _ => null
                    },
                1013 => // NA02 React to Floor Markers
                    index switch
                    {
                        0 => new WPos(-10.070f, 10.198f),
                        1 => new WPos(-2.152f, 2.652f),
                        2 => new WPos(-3.050f, 0.260f),
                        3 => new WPos(-10.248f, 9.636f),
                        4 => new WPos(-11.719f, 2.587f),
                        5 => new WPos(-2.815f, 2.137f),
                        0x10 => new WPos(-1.511f, 4.501f),
                        0x18 => new WPos(1.999f, 5.020f),
                        _ => null
                    },
                1014 => // NA03 React to Advanced Visual Indicators
                    index switch
                    {
                        0 => new WPos(-5, 0f),
                        1 => new WPos(-4.709f, -7.366f),
                        2 => new WPos(-5.637f, -5.024f),
                        3 => new WPos(-12.223f, 0.224f),
                        4 => new WPos(-4.729f, 0.106f),
                        _ => null
                    },
                _ => null
            };
            if (Module.WorldState.CurrentCFCID == 1013 && index == 0x18)
                radius = 4f;
        }

        if (state == 0x00080004)
        {
            readyPos = null;
            radius = 1f;
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (readyPos is WPos { } pos)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(pos, radius));
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (readyPos is WPos { } pos)
            Arena.AddCircle(pos, radius, ArenaColor.Safe);
    }
}
class Interact(BossModule module) : BossComponent(module)
{
    public Actor? InteractNPC => Module.Enemies(OID.MasterOfTrials).FirstOrDefault();
    public bool interactReady;
    public bool trialComplete;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (InteractNPC != null && InteractNPC.IsTargetable && (interactReady || trialComplete))
        {
            hints.InteractWithOID(WorldState, OID.MasterOfTrials); // We are missing the logic to complete the NPC interaction - We need to be able to crawl through dialogue, select "Commence the final challenge.", and then select "Yes" in the following YesNo Addon.
        }
    }
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        base.OnEventDirectorUpdate(updateID, param1, param2, param3, param4);

        switch (Module.WorldState.CurrentCFCID)
        {
            case 1012:                                                                                          // NA01 React to Attack Markers
                if (updateID == 0x80000015 && param1 == 0x4 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //NPC Asks to speak
                {
                    interactReady = true;
                }

                if (updateID == 0x80000016 && param1 == 0x5 && param2 == 0x1 && param3 == 0x0 && param4 == 0x0) //Spoken to NPC - Break loop
                {
                    interactReady = false;
                }

                if (updateID == 0x00000001 && param1 == 0x0 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //Duty has been Completed
                {
                    trialComplete = true;
                }
                break;
            case 1013:
                if (updateID == 0x80000015 && param1 == 0x5 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //NPC Asks to speak
                {
                    interactReady = true;
                }

                if (updateID == 0x80000016 && param1 == 0x6 && param2 == 0x1 && param3 == 0x0 && param4 == 0x0) //Spoken to NPC - Break loop
                {
                    interactReady = false;
                }

                if (updateID == 0x00000001 && param1 == 0x0 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //Duty has been Completed
                {
                    trialComplete = true;
                }
                break;
            case 1014:
                if (updateID == 0x80000015 && param1 == 0x4 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //NPC Asks to speak
                {
                    interactReady = true;
                }

                if (updateID == 0x80000016 && param1 == 0x5 && param2 == 0x1 && param3 == 0x0 && param4 == 0x0) //Spoken to NPC - Break loop
                {
                    interactReady = false;
                }

                if (updateID == 0x00000001 && param1 == 0x0 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //Duty has been Completed
                {
                    trialComplete = true;
                }
                break;
        }
    }
}

#region NA01

class NA01Glaciate1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.NA01GlaciateSpread1, AID.NA01Glaciate1, 6f, 0, true);
class NA01Glaciate2(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.NA01GlaciateSpread2, AID.NA01Glaciate3, 6f, 0, true);
class NA01SpectralBlaze1(BossModule module) : Components.StackWithIcon(module, (uint)IconID.NA01SpectralBlazeStack1, AID.NA01SpectralBlazeVisual1, 6f, 0, 2)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NA01SpectralBlazeVisual1 or AID.NA01SpectralBlazeVisual2 or AID.NA01SpectralBlaze1 or AID.NA01SpectralBlaze2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
    }
}
class NA01SpectralBlaze2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.NA01SpectralBlazeStack2, AID.NA01SpectralBlazeVisual2, 6f, 0, 2)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NA01SpectralBlazeVisual1 or AID.NA01SpectralBlazeVisual2 or AID.NA01SpectralBlaze1 or AID.NA01SpectralBlaze2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
    }
}
class NA01ScorchingStreak1(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(44, 5), (uint)IconID.NA01ScorchingStreakTankbuster, AID.NA01ScorchingStreakVisual1, centerAtTarget: false, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count > 0)
        {
            if (actor.Role is Role.Tank)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0f, -15f), 3));
            }
            else
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0f, 15f), 3));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NA01ScorchingStreakVisual1 or AID.NA01ScorchingStreakVisual2 or AID.NA01ScorchingStreak1 or AID.NA01ScorchingStreak2)
        {
            CurrentBaits.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}
class NA01PiercingStone1(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4f), AID.NA01PiercingStone1, AID.NA01PiercingStone2, 5f, 3, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Chasers.Count > 0)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(new WPos(0f, 13f), 30));
        }
    }
}
class NA01PiercingStone2(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4f), AID.NA01PiercingStone4, AID.NA01PiercingStone5, 5f, 3, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Chasers.Count > 0)
        {
            hints.AddForbiddenZone(ShapeContains.DonutSector(new WPos(0f, 0f), 13, 20, 45.Degrees(), 135.Degrees()));
        }
    }
}
class NA01FrigidRing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private static readonly AOEShapeDonut donut = new(10, 20);
    private static readonly AOEShapeCircle circ = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count > 0)
        {
            foreach (var e in aoes.Take(1))
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NA01FrigidRing)
        {
            aoes.Add(new(donut, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
        if ((AID)spell.Action.ID is AID.NA01BitterChill)
        {
            aoes.Add(new(circ, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (aoes.Count > 0 && (AID)spell.Action.ID is AID.NA01FrigidRing or AID.NA01BitterChill)
        {
            aoes.RemoveAt(0);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (aoes.Count > 1)
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0, 0), 9f));
        }
        else if (aoes.Count == 1)
            hints.AddForbiddenZone(ShapeContains.Circle(new WPos(0, 0), 11f));
    }
}
class NA01Fireflood(BossModule module) : Components.StandardAOEs(module, AID.NA01Fireflood, 15f);
class NA01BlazingRing(BossModule module) : Components.StandardAOEs(module, AID.NA01BlazingRing, new AOEShapeDonut(8, 25));
#endregion NA01

#region NA02

class NA02RogueWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.NA02RogueWave1, 15f, shape: new AOEShapeCircle(25f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0)
        {
            foreach (var caster in Casters)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(caster.Position, 3));
            }
        }
    }
}

class NA02RogueWave2(BossModule module) : Components.KnockbackFromCastTarget(module, AID.NA02RogueWave3, 15f, shape: new AOEShapeCircle(25f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0)
        {
            foreach (var caster in Casters)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCone(caster.Position, 3, 90.Degrees(), 40.Degrees()));
            }
        }
    }
}

class NA02Border(BossModule module) : Components.StandardAOEs(module, null!, new AOEShapeDonut(18, 25))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.WorldState.CurrentCFCID == 1013 && ((AID?)Module.PrimaryActor.CastInfo?.Action.ID) is AID.NA02RogueWave or AID.NA02RogueWave2 or AID.NA02Windage or AID.NA02Upwell)
            yield return new(Shape, new WPos(0, 0));
    }
}

class NA02Windage(BossModule module) : Components.StandardAOEs(module, AID.NA02Windage1, 5f);
class NA02WindageKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.NA02Windage2, 10f);
class NA02FuriousFlare(BossModule module) : Components.StandardAOEs(module, AID.NA02FuriousFlare1, 18f);
class NA02FuriousFlare2(BossModule module) : Components.StandardAOEs(module, AID.FuriousFlare3, 20f);
class NA02ThunderingPillar(BossModule module) : Components.CastTowers(module, AID.NA02ThunderingPillar1, 4f);
class NA02ThunderingPillar2(BossModule module) : Components.CastTowers(module, AID.NA02ThunderingPillar2, 4f);
class NA02WaterDrop(BossModule module) : Components.StandardAOEs(module, AID.NA02WaterDrop, 10f);
class NA02Upwell(BossModule module) : Components.StandardAOEs(module, AID.NA02Upwell2, 5f);

class NA02UpwellKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.NA02Upwell1, 20f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0)
        {
            foreach (var caster in Casters)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCone(caster.Position, 8, 270.Degrees(), 30.Degrees()));
            }
        }
    }
}
class NA02BlazingSurge(BossModule module) : Components.StandardAOEs(module, AID.NA02BlazingSurge1, new AOEShapeCone(20f, 90.Degrees()));
class NA02BlazingSurge2(BossModule module) : Components.StandardAOEs(module, AID.NA02BlazingSurge2, new AOEShapeCone(20f, 90.Degrees()));
#endregion

#region NA03

class NA03Tether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(19f), (uint)TetherID.NA03Tether)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any())
        {
            hints.AddForbiddenZone(new AOEShapeCircle(19f), Module.Center);
        }
    }
}

class NA03TetherIntercept(BossModule module) : BossComponent(module)
{
    public uint TID { get; init; } = (uint)TetherID.NA03Tether2;
    private readonly List<Actor> tethers = [];

    public bool Active => tethers.Count == 1;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Active)
        {
            var bomb = tethers.First();

            var distance = Module.PrimaryActor.Position - bomb.Position;
            var halfLength = distance.Length() / 2 / 3 * 2;

            hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.Center - distance.Scaled(0.5f), distance.ToAngle(), halfLength, halfLength, 1f));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;
        hints.Add("Grab the tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        var bomb = WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.NA03Bomb);

        if (bomb == null)
            return;
        Arena.AddLine(Module.Center, bomb.Position, ArenaColor.Safe, 2f);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target?.OID == (uint)OID.NA03Bomb)
                tethers.Add(target);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target?.OID == (uint)OID.NA03Bomb)
                tethers.Remove(target);
        }
    }
}

class NA03Glaciate(BossModule module) : Components.StandardAOEs(module, AID.NA03Glaciate1, 6f);
class NA03FanOfFlames(BossModule module) : Components.StandardAOEs(module, AID.NA03FanOfFlames1, new AOEShapeCone(60f, 23.Degrees(), 0.Degrees()));
class NA03FanOfFlames2(BossModule module) : Components.StandardAOEs(module, AID.NA03FanOfFlames2, new AOEShapeCone(60f, 23.Degrees(), 0.Degrees()));
class NA03BitterChill(BossModule module) : Components.StandardAOEs(module, AID.NA03BitterChill1, 10f);
class NA03PetrifyingLight(BossModule module) : Components.CastGaze(module, AID.NA03PetrifyingLight2);
class NA03PetrifyingLight2(BossModule module) : Components.CastGaze(module, AID.NA03PetrifyingLight4);
class NA03FrigidRing(BossModule module) : Components.StandardAOEs(module, AID.NA03FrigidRing, new AOEShapeDonut(10, 20));
class NA03Withdraw(BossModule module) : Components.RaidwideCast(module, AID.NA03Withdraw);
class NA03Fireflood(BossModule module) : Components.StandardAOEs(module, AID.NA03Fireflood, 15f);
#endregion NA03

class NoviceTacticalStates : StateMachineBuilder
{
    public NoviceTacticalStates(BossModule module) : base(module)
    {
        TrivialPhase()
           .ActivateOnEnter<StartingPositions>()
           .ActivateOnEnter<Interact>()

           .ActivateOnEnter<NA01Glaciate1>()
           .ActivateOnEnter<NA01Glaciate2>()
           .ActivateOnEnter<NA01SpectralBlaze1>()
           .ActivateOnEnter<NA01SpectralBlaze2>()
           .ActivateOnEnter<NA01ScorchingStreak1>()
           .ActivateOnEnter<NA01PiercingStone1>()
           .ActivateOnEnter<NA01PiercingStone2>()
           .ActivateOnEnter<NA01FrigidRing>()
           .ActivateOnEnter<NA01Fireflood>()
           .ActivateOnEnter<NA01BlazingRing>()

           .ActivateOnEnter<NA02Border>()
           .ActivateOnEnter<NA02RogueWave>()
           .ActivateOnEnter<NA02RogueWave2>()
           .ActivateOnEnter<NA02Windage>()
           .ActivateOnEnter<NA02WindageKnockback>()
           .ActivateOnEnter<NA02FuriousFlare>()
           .ActivateOnEnter<NA02FuriousFlare2>()
           .ActivateOnEnter<NA02ThunderingPillar>()
           .ActivateOnEnter<NA02ThunderingPillar2>()
           .ActivateOnEnter<NA02WaterDrop>()
           .ActivateOnEnter<NA02Upwell>()
           .ActivateOnEnter<NA02UpwellKnockback>()
           .ActivateOnEnter<NA02BlazingSurge>()
           .ActivateOnEnter<NA02BlazingSurge2>()

           .ActivateOnEnter<NA03Tether>()
           .ActivateOnEnter<NA03TetherIntercept>()
           .ActivateOnEnter<NA03Glaciate>()
           .ActivateOnEnter<NA03FanOfFlames>()
           .ActivateOnEnter<NA03FanOfFlames2>()
           .ActivateOnEnter<NA03BitterChill>()
           .ActivateOnEnter<NA03Fireflood>()
           .ActivateOnEnter<NA03FrigidRing>()
           .ActivateOnEnter<NA03PetrifyingLight>()
           .ActivateOnEnter<NA03PetrifyingLight2>()
           .ActivateOnEnter<NA03Withdraw>();
    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala, erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1012, NameID = 13616)]
public class NoviceTactical(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => !PrimaryActor.IsDeadOrDestroyed;
}

