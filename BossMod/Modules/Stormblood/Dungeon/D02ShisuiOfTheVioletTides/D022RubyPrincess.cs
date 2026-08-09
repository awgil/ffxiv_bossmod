namespace BossMod.Stormblood.Dungeon.D02ShisuiOfTheVioletTides.D022RubyPrincess;

public enum OID : uint
{
    Boss = 0x1B0E, // R1.6
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Tornadogenesis = 8063, // Boss->self, no cast, range 8+R 120-degree cone
    Old = 8062, // Helper->self, no cast, range 4 circle, chest when polymorphing player
    Seduce = 8058, // Boss->self, 7.0s cast, range 50 circle
    CoriolisKick = 8059, // Boss->self, 5.0s cast, range 13 circle
    AbyssalVolcano = 8060, // Boss->self, 3.0s cast, range 7 circle
    GeothermalFlatulenceFirst = 9431, // Helper->location, 3.8s cast, range 4 circle
    GeothermalFlatulenceRest = 8061 // Helper->location, no cast, range 4 circle
}

public enum SID : uint
{
    Old = 1259, // none->player, extra=0x3D
    Seduced = 991 // Boss->player, extra=0xF

}

public enum IconID : uint
{
    ChasingAOE = 1 // player
}

class SeduceOld(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(2.5f);
    private bool active;
    private readonly List<Actor> chests = [with(4)];
    private readonly List<Circle> closedChests = [];
    private readonly List<Circle> openChests = [];
    private AOEShapeCustom? closedAOE;
    private BitMask old;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Old)
        {
            old.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Old)
        {
            old.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void Update()
    {
        if (closedAOE == null)
        {
            var helpers = Module.Enemies((uint)OID.Helper);
            var countH = helpers.Count;

            for (var i = 0; i < countH; ++i)
            {
                var c = helpers[i];
                if (c.NameID == 6274u)
                {
                    chests.Add(c);
                }
            }
            var count = chests.Count;
            for (var i = 0; i < count; ++i)
            {
                closedChests.Add(new(chests[i].Position, 2.5f));
            }
            closedAOE = new AOEShapeCustom(Arena.Center, [.. closedChests]);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = openChests.Count;
        var aoes = new AOEInstance[count + 1];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(circle, openChests[i].Center);
        }
        if (closedAOE is AOEShapeCustom aoe)
        {
            var isold = old[slot];
            aoe.InvertForbiddenZone = !isold && active;
            aoes[count] = new(aoe, Arena.Center, color: isold || !active ? default : Colors.SafeFromAOE);
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var count = chests.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = chests[i];
            if (c.Position.AlmostEqual(actor.Position, 5f))
            {
                if (state == 0x00040008u)
                {
                    var countC = closedChests.Count;
                    for (var j = 0; j < countC; ++j)
                    {
                        if (c.Position == closedChests[j].Center)
                        {
                            closedChests.RemoveAt(j);
                            openChests.Add(new(c.Position, 2.5f));
                            break;
                        }
                    }
                }
                else if (state == 0x00100020u)
                {
                    var countO = openChests.Count;
                    for (var j = 0; j < countO; ++j)
                    {
                        if (c.Position == openChests[j].Center)
                        {
                            openChests.RemoveAt(j);
                            closedChests.Add(new(c.Position, 2.5f));
                            break;
                        }
                    }
                }
                closedAOE = new AOEShapeCustom(Arena.Center, [.. closedChests]);
                return;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Seduce)
        {
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Seduce)
        {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var isOld = old[slot];
        if (isOld && active || !active)
        {
            var aoes = ActiveAOEs(slot, actor);
            ref readonly var aoe = ref aoes[0];
            if (aoe.Color != Colors.SafeFromAOE && aoe.Check(actor.Position))
            {
                hints.Add("GTFO from chests!");
            }
        }
        else if (!isOld && active)
        {
            hints.Add("Get morphed!");
        }
    }
}

sealed class SeduceCoriolisKick(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(13f);
    public AOEInstance[] AOE = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Seduce)
        {
            AOE = [new(circle, new WPos(-0.046f, -208.362f).Quantized(), default, Module.CastFinishAt(spell, 8d))];
        }
        else if (id == (uint)AID.CoriolisKick)
        {
            AOE = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CoriolisKick)
        {
            AOE = [];
        }
    }
}

sealed class AbyssalVolcano(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalVolcano, 7f);

sealed class GeothermalFlatulence(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.GeothermalFlatulenceFirst, (uint)AID.GeothermalFlatulenceRest, 3, 0.8d, 10, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (TargetsMask[slot])
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 18f), Activation);
        }
    }
}

sealed class Tornadogenesis(BossModule module) : Components.Cleave(module, (uint)AID.Tornadogenesis, new AOEShapeCone(9.6f, 60f.Degrees()))
{
    private readonly SeduceCoriolisKick _aoe = module.FindComponent<SeduceCoriolisKick>()!;
    private readonly GeothermalFlatulence _aoes = module.FindComponent<GeothermalFlatulence>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class D022RubyPrincessStates : StateMachineBuilder
{
    public D022RubyPrincessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SeduceOld>()
            .ActivateOnEnter<SeduceCoriolisKick>()
            .ActivateOnEnter<AbyssalVolcano>()
            .ActivateOnEnter<GeothermalFlatulence>()
            .ActivateOnEnter<Tornadogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 235u, NameID = 6241u)]
public sealed class D022RubyPrincess : BossModule
{
    public D022RubyPrincess(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D022RubyPrincess(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Circle(new(-0.046f, -208.362f), 20)], [new Rectangle(new(-0.4f, -187.4f), 20, 2.5f), new Rectangle(new(-20, -208), 1.5f, 20f)]);
        return (arena.Center, arena);
    }
}
