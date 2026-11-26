using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.ActionTweaks.CustomCombos;

public enum CustomComboGaugeKind
{
    Number,
    Bool,
    Choice,
}

public sealed record class CustomComboGaugeField(string Key, string Label, CustomComboGaugeKind Kind, Func<WorldState, float> Read)
{
    public float Max { get; init; } // upper bound for number inputs, 0 if unbounded
    public string Unit { get; init; } = "";
    public IReadOnlyList<(string Label, int Value)> Options { get; init; } = [];

    public string Format(float value) => Kind switch
    {
        CustomComboGaugeKind.Bool => value != 0 ? "yes" : "no",
        CustomComboGaugeKind.Choice => Options.FirstOrDefault(o => o.Value == (int)value).Label ?? value.ToString(),
        _ => $"{value:0.#}{Unit}"
    };
}

public static class CustomComboGauge
{
    private static readonly Dictionary<Class, List<CustomComboGaugeField>> _fields = BuildFields();

    public static List<CustomComboGaugeField> Fields(Class job) => _fields.GetValueOrDefault(job) ?? [];
    public static CustomComboGaugeField? FindField(Class job, string key) => Fields(job).Find(f => f.Key == key);
    public static float? Read(WorldState ws, Class job, string key) => FindField(job, key)?.Read(ws);
    public static CustomComboGaugeField? FindAnyField(string key) => _fields.Values.SelectMany(l => l).FirstOrDefault(f => f.Key == key);

    private sealed class Builder<T> where T : unmanaged
    {
        public readonly List<CustomComboGaugeField> Fields = [];

        public Builder<T> Number(string key, string label, float max, Func<T, float> get)
        {
            Fields.Add(new(key, label, CustomComboGaugeKind.Number, ws => get(ws.Client.GetGauge<T>())) { Max = max });
            return this;
        }

        public Builder<T> Timer(string key, string label, Func<T, float> getMs)
        {
            Fields.Add(new(key, label, CustomComboGaugeKind.Number, ws => getMs(ws.Client.GetGauge<T>()) * 0.001f) { Unit = "s" });
            return this;
        }

        public Builder<T> Bool(string key, string label, Func<T, bool> get)
        {
            Fields.Add(new(key, label, CustomComboGaugeKind.Bool, ws => get(ws.Client.GetGauge<T>()) ? 1 : 0));
            return this;
        }

        public Builder<T> Choice(string key, string label, Func<T, int> get, params (string Label, int Value)[] options)
        {
            Fields.Add(new(key, label, CustomComboGaugeKind.Choice, ws => get(ws.Client.GetGauge<T>())) { Options = options });
            return this;
        }
    }

    private static Dictionary<Class, List<CustomComboGaugeField>> BuildFields()
    {
        var pld = new Builder<PaladinGauge>()
            .Number("oath", "Oath gauge", 100, g => g.OathGauge)
            .Fields;
        var war = new Builder<WarriorGauge>()
            .Number("beast", "Beast gauge", 100, g => g.BeastGauge)
            .Fields;
        var drk = new Builder<DarkKnightGauge>()
            .Number("blood", "Blood gauge", 100, g => g.Blood)
            .Timer("darkside", "Darkside time left", g => g.DarksideTimer)
            .Bool("darkarts", "Dark Arts", g => g.DarkArtsState != 0)
            .Timer("shadow", "Living Shadow time left", g => g.ShadowTimer)
            .Fields;
        var gnb = new Builder<GunbreakerGauge>()
            .Number("ammo", "Cartridges", 3, g => g.Ammo)
            .Number("ammocombo", "Cartridge combo step", 0, g => g.AmmoComboStep)
            .Fields;
        var whm = new Builder<WhiteMageGauge>()
            .Number("lily", "Lilies", 3, g => g.Lily)
            .Number("bloodlily", "Blood Lily", 3, g => g.BloodLily)
            .Timer("lilytimer", "Time since last lily", g => g.LilyTimer)
            .Fields;
        var sch = new Builder<ScholarGauge>()
            .Number("aetherflow", "Aetherflow", 3, g => g.Aetherflow)
            .Number("fairy", "Fae Aether", 100, g => g.FairyGauge)
            .Timer("seraph", "Seraph time left", g => g.SeraphTimer)
            .Bool("dismissed", "Fairy dismissed", g => g.DismissedFairy != 0)
            .Fields;
        (string, int)[] cards = [("none", 0), ("The Balance", 1), ("The Bole", 2), ("The Arrow", 3), ("The Spear", 4), ("The Ewer", 5), ("The Spire", 6), ("Lord of Crowns", 7), ("Lady of Crowns", 8)];
        var ast = new Builder<AstrologianGauge>()
            .Choice("draw", "Next draw", g => (int)g.CurrentDraw, ("Astral Draw", 0), ("Umbral Draw", 1))
            .Choice("card1", "Card 1", g => (int)g.Card1, cards)
            .Choice("card2", "Card 2", g => (int)g.Card2, cards)
            .Choice("card3", "Card 3", g => (int)g.Card3, cards)
            .Choice("arcana", "Minor Arcana", g => (int)g.CurrentArcana, cards)
            .Fields;
        var sge = new Builder<SageGauge>()
            .Number("addersgall", "Addersgall", 3, g => g.Addersgall)
            .Number("addersting", "Addersting", 3, g => g.Addersting)
            .Timer("addersgalltimer", "Time since last Addersgall", g => g.AddersgallTimer)
            .Bool("eukrasia", "Eukrasia active", g => g.EukrasiaActive)
            .Fields;
        (string, int)[] chakra = [("none", 0), ("Opo-opo", 1), ("Raptor", 2), ("Coeurl", 3)];
        var mnk = new Builder<MonkGauge>()
            .Number("chakra", "Chakra", 10, g => g.Chakra)
            .Number("beastchakras", "Beast Chakra count", 3, g => g.BeastChakraStacks)
            .Choice("beastchakra1", "Beast Chakra 1", g => (int)g.BeastChakra1, chakra)
            .Choice("beastchakra2", "Beast Chakra 2", g => (int)g.BeastChakra2, chakra)
            .Choice("beastchakra3", "Beast Chakra 3", g => (int)g.BeastChakra3, chakra)
            .Bool("lunarnadi", "Lunar Nadi", g => g.Nadi.HasFlag(NadiFlags.Lunar))
            .Bool("solarnadi", "Solar Nadi", g => g.Nadi.HasFlag(NadiFlags.Solar))
            .Number("opoopofury", "Opo-opo's Fury", 1, g => g.OpoOpoStacks)
            .Number("raptorfury", "Raptor's Fury", 1, g => g.RaptorStacks)
            .Number("coeurlfury", "Coeurl's Fury", 2, g => g.CoeurlStacks)
            .Timer("blitz", "Masterful Blitz time left", g => g.BlitzTimeRemaining)
            .Fields;
        var drg = new Builder<DragoonGauge>()
            .Number("focus", "Firstminds' Focus", 2, g => g.FirstmindsFocusCount)
            .Number("eyes", "Dragon eyes", 2, g => g.EyeCount)
            .Bool("lotd", "Life of the Dragon", g => g.LotdState != 0)
            .Timer("lotdtimer", "Life of the Dragon time left", g => g.LotdTimer)
            .Fields;
        var nin = new Builder<NinjaGauge>()
            .Number("ninki", "Ninki", 100, g => g.Ninki)
            .Number("kazematoi", "Kazematoi", 5, g => g.Kazematoi)
            .Fields;
        var sam = new Builder<SamuraiGauge>()
            .Number("kenki", "Kenki", 100, g => g.Kenki)
            .Number("meditation", "Meditation", 3, g => g.MeditationStacks)
            .Bool("setsu", "Setsu", g => g.SenFlags.HasFlag(SenFlags.Setsu))
            .Bool("getsu", "Getsu", g => g.SenFlags.HasFlag(SenFlags.Getsu))
            .Bool("ka", "Ka", g => g.SenFlags.HasFlag(SenFlags.Ka))
            .Number("sen", "Sen count", 3, g => BitOperations.PopCount((uint)g.SenFlags))
            .Choice("kaeshi", "Tsubame-gaeshi ready for", g => (int)g.Kaeshi, ("nothing", 0), ("Higanbana", 1), ("Goken", 2), ("Setsugekka", 3), ("Namikiri", 4))
            .Fields;
        var rpr = new Builder<ReaperGauge>()
            .Number("soul", "Soul gauge", 100, g => g.Soul)
            .Number("shroud", "Shroud gauge", 100, g => g.Shroud)
            .Number("lemure", "Lemure Shroud", 5, g => g.LemureShroud)
            .Number("void", "Void Shroud", 5, g => g.VoidShroud)
            .Timer("enshroud", "Enshroud time left", g => g.EnshroudedTimeRemaining)
            .Fields;
        var vpr = new Builder<ViperGauge>()
            .Number("coils", "Rattling Coils", 3, g => g.RattlingCoilStacks)
            .Number("offering", "Serpent Offering", 100, g => g.SerpentOffering)
            .Number("tribute", "Anguine Tribute", 5, g => g.AnguineTribute)
            .Timer("reawaken", "Reawaken time left", g => g.ReawakenedTimer)
            .Choice("dread", "Coil/Den combo", g => (int)g.DreadCombo, ("none", 0), ("after Vicewinder", 1), ("after Hunter's Coil", 2), ("after Swiftskin's Coil", 3), ("after Vicepit", 4), ("after Hunter's Den", 5), ("after Swiftskin's Den", 6))
            .Choice("serpent", "Serpent's Tail ready for", g => (int)g.SerpentCombo, ("nothing", 0), ("Death Rattle", 1), ("Last Lash", 2), ("First Legacy", 3), ("Second Legacy", 4), ("Third Legacy", 5), ("Fourth Legacy", 6))
            .Fields;
        var brd = new Builder<BardGauge>()
            .Choice("song", "Current song", g => (int)g.SongFlags & 3, ("none", 0), ("Mage's Ballad", 1), ("Army's Paeon", 2), ("Wanderer's Minuet", 3))
            .Timer("songtimer", "Song time left", g => g.SongTimer)
            .Number("repertoire", "Repertoire", 4, g => g.Repertoire)
            .Number("soulvoice", "Soul Voice", 100, g => g.SoulVoice)
            .Number("coda", "Coda count", 3, g => BitOperations.PopCount((uint)g.SongFlags & 0x70))
            .Fields;
        var mch = new Builder<MachinistGauge>()
            .Number("heat", "Heat", 100, g => g.Heat)
            .Number("battery", "Battery", 100, g => g.Battery)
            .Bool("overheated", "Overheated", g => g.OverheatTimeRemaining > 0)
            .Timer("overheattimer", "Overheat time left", g => g.OverheatTimeRemaining)
            .Bool("queen", "Automaton Queen active", g => g.SummonTimeRemaining > 0)
            .Timer("queentimer", "Automaton Queen time left", g => g.SummonTimeRemaining)
            .Fields;
        var dnc = new Builder<DancerGauge>()
            .Number("feathers", "Fourfold Feathers", 4, g => g.Feathers)
            .Number("esprit", "Esprit", 100, g => g.Esprit)
            .Number("steps", "Dance steps done", 4, g => g.StepIndex)
            .Choice("nextstep", "Next dance step", g => (int)g.CurrentStep, ("finish", 0), ("Emboite", 1), ("Entrechat", 2), ("Jete", 3), ("Pirouette", 4))
            .Fields;
        var blm = new Builder<BlackMageGauge>()
            .Number("astralfire", "Astral Fire", 3, g => g.AstralStacks)
            .Number("umbralice", "Umbral Ice", 3, g => g.UmbralStacks)
            .Number("hearts", "Umbral Hearts", 3, g => g.UmbralHearts)
            .Number("polyglot", "Polyglot", 3, g => g.PolyglotStacks)
            .Number("astralsoul", "Astral Soul", 6, g => g.AstralSoulStacks)
            .Bool("enochian", "Enochian active", g => g.EnochianActive)
            .Bool("paradox", "Paradox ready", g => g.ParadoxActive)
            .Fields;
        var smn = new Builder<SummonerGauge>()
            .Number("aetherflow", "Aetherflow", 2, g => (int)(g.AetherFlags & AetherFlags.Aetherflow))
            .Bool("ifrit", "Ifrit ready", g => g.AetherFlags.HasFlag(AetherFlags.IfritReady))
            .Bool("titan", "Titan ready", g => g.AetherFlags.HasFlag(AetherFlags.TitanReady))
            .Bool("garuda", "Garuda ready", g => g.AetherFlags.HasFlag(AetherFlags.GarudaReady))
            .Choice("attunement", "Attuned to", g => g.AttunementType, ("nothing", 0), ("Ifrit", 1), ("Titan", 2), ("Garuda", 3))
            .Number("attunementcount", "Attunement stacks", 4, g => g.AttunementCount)
            .Timer("attunementtimer", "Attunement time left", g => g.AttunementTimer)
            .Timer("summontimer", "Demi-summon time left", g => g.SummonTimer)
            .Fields;
        var rdm = new Builder<RedMageGauge>()
            .Number("white", "White Mana", 100, g => g.WhiteMana)
            .Number("black", "Black Mana", 100, g => g.BlackMana)
            .Number("stacks", "Mana Stacks", 3, g => g.ManaStacks)
            .Fields;
        var pct = new Builder<PictomancerGauge>()
            .Number("palette", "Palette gauge", 100, g => g.PalleteGauge)
            .Number("paint", "White/Black Paint", 5, g => g.Paint)
            .Bool("creaturemotif", "Creature motif drawn", g => g.CreatureMotifDrawn)
            .Bool("weaponmotif", "Weapon motif drawn", g => g.WeaponMotifDrawn)
            .Bool("landscapemotif", "Landscape motif drawn", g => g.LandscapeMotifDrawn)
            .Bool("moogle", "Moogle portrait ready", g => g.MooglePortraitReady)
            .Bool("madeen", "Madeen portrait ready", g => g.MadeenPortraitReady)
            .Fields;

        // base classes use their job's gauge
        return new()
        {
            [Class.PLD] = pld,
            [Class.GLA] = pld,
            [Class.WAR] = war,
            [Class.MRD] = war,
            [Class.DRK] = drk,
            [Class.GNB] = gnb,
            [Class.WHM] = whm,
            [Class.CNJ] = whm,
            [Class.SCH] = sch,
            [Class.AST] = ast,
            [Class.SGE] = sge,
            [Class.MNK] = mnk,
            [Class.PGL] = mnk,
            [Class.DRG] = drg,
            [Class.LNC] = drg,
            [Class.NIN] = nin,
            [Class.ROG] = nin,
            [Class.SAM] = sam,
            [Class.RPR] = rpr,
            [Class.VPR] = vpr,
            [Class.BRD] = brd,
            [Class.ARC] = brd,
            [Class.MCH] = mch,
            [Class.DNC] = dnc,
            [Class.BLM] = blm,
            [Class.THM] = blm,
            [Class.SMN] = smn,
            [Class.ACN] = smn,
            [Class.RDM] = rdm,
            [Class.PCT] = pct,
        };
    }
}
