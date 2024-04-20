namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

// note: apparently there's a slight overlap between aoes in the center, which looks ugly, but at least that's the truth...
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
                _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, spell.NPCFinishAt));
                break;
            case AID.VisceralWhirlRAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.NPCFinishAt));
                break;
            case AID.VisceralWhirlLAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, spell.NPCFinishAt));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.VisceralWhirlRAOE1 or AID.VisceralWhirlRAOE2 or AID.VisceralWhirlLAOE1 or AID.VisceralWhirlLAOE2)
            _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
    }
}

class MiasmicBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MiasmicBlast), new AOEShapeCross(60, 5));

class VoidBio(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _bubbles = module.Enemies(OID.ToxicBubble);

    private static readonly AOEShapeCircle _shape = new(2); // TODO: verify explosion radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _bubbles.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

class BondsOfDarkness(BossModule module) : BossComponent(module)
{
    public int NumTethers { get; private set; }
    private readonly int[] _partners = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partners[slot] >= 0)
            hints.Add("Break tether!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _partners[pcSlot] == playerSlot ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Raid[_partners[pcSlot]];
        if (partner != null)
            Arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BondsOfDarkness)
        {
            var slot1 = Raid.FindSlot(source.InstanceID);
            var slot2 = Raid.FindSlot(tether.Target);
            if (slot1 >= 0 && slot2 >= 0)
            {
                ++NumTethers;
                _partners[slot1] = slot2;
                _partners[slot2] = slot1;
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BondsOfDarkness)
        {
            var slot1 = Raid.FindSlot(source.InstanceID);
            var slot2 = Raid.FindSlot(tether.Target);
            if (slot1 >= 0 && slot2 >= 0)
            {
                --NumTethers;
                _partners[slot1] = -1;
                _partners[slot2] = -1;
            }
        }
    }
}

class DarkDivides(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DivisiveDark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkDivides)
            Spreads.Clear();
    }
}
