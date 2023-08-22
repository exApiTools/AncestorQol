using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;

namespace AncestorQol;

public class AncestorQolSettings : ISettings
{
    private static readonly IReadOnlyList<(string Id, string Name)> UnitTypes = new List<(string, string)>
    {
        ("Turtle", "Titanic Shell"),
        ("KuniKuni", "Consuming Kunekune"),
        ("Thunderbird", "Thunderbird"),
        ("Tuatara", "Tuatara"),
        ("KaruiSpear", "Bloodbound Warrior"),
        ("JadeHulk", "Jade Hulk"),
        ("KaruiDrunk", "Frenzymonger"),
        ("SpiritCaster", "Honoured Sage"),
        ("KaruiHorn", "Hinekora's Horn"),
        ("MoltenHulk", "Caldera Ravager"),
        ("Deathcaster", "Death's Guide"),
        ("ColdTurtle", "Lunar Turtle"),
        ("ForestTuatara", "Camouflaged Tuatara"),
        ("Icecaster", "Sunset Sage"),
        ("BuffHorn", "Warcaller"),
        ("FishingWhipMace", "Riptide"),
        ("FishingTaiaha", "Trawler"),
        ("ColdPoi", "Moon Dancer"),
        ("ForestKuniKuni", "Enraged Kunekune"),
        ("KaruiMooncaster", "Tidecaller"),
        ("ChieftainHakuTest", "Haku"),
        ("NgamahuDrunk", "Firebreather"),
        ("KaruiShieldAxe", "Storm Guard"),
        ("Druid", "Tawhoa Shaman"),
        ("IceArcher", "Autumnal Archer"),
        ("JadeCaster", "Jadecrafter"),
        ("ShieldWaller", "Fieldmaster"),
        ("MoltenDestroyer", "Blackbark Demolisher"),
        ("SpearRope", "Spearfisher"),
        ("DeathProphet", "Mystic Prophet"),
        ("LightningCaster", "Storm Conduit"),
        ("Stunner", "Goliath of Night"),
        ("JadeCutter", "Jade Shaman"),
        ("SpearAgile", "Spear Dancer"),
        ("FreezingWave", "Freezing Wave Idol"),
        ("Cremation", "Volcanic Idol"),
        ("MultiHook", "Hook Trap Idol"),
        ("TotemRevival", "Tribal Reconstruction Idol"),
        ("TotemRevivalSingle", "Reconstruction Idol"),
        ("HealingAura", "Ritual of Life Idol"),
        ("HealAll", "Burst of Life Idol"),
        ("FeastOnFlesh", "Feasting Ritual Idol"),
        ("CircleOfJade", "Jadecrafter Idol"),
        ("Invulnerability", "Invulnerability Idol"),
        ("WoodenWall", "Great Barrier Idol"),
        ("TidalWave", "Tidal Wave Idol"),
        ("DivineIre", "Lightning Idol"),
        ("Ikiaho", "Ikiaho"),
        ("Tawhanuku", "Tawhanuku"),
        ("ColdTuatara", "Tattooed Tuatara"),
        ("Kahuturoa", "Kahuturoa"),
        ("Kaom", "Kaom"),
        ("TasalioTuatara", "Hooked Tuatara"),
        ("NavaliShieldAxe", "Honoured Warrior"),
        ("Kiloava", "Kiloava"),
        ("Ahuana", "Ahuana"),
        ("Akoya", "Akoya"),
        ("Maata", "Maata"),
        ("Rakiata", "Rakiata"),
        ("Utula", "Utula"),
        ("TotemHook", "Regrouping Idol"),
        ("TasalioBlessing", "Tasalio's Blessing Idol"),
        ("ArohonguiBlessing", "Arohongui's Blessing Idol"),
        ("ShockField", "Raging Storm Idol"),
        ("RockSmash", "Quake Idol"),
        ("TotemImmunity", "Nightcloak Idol"),
        ("TeamMartyr", "Ngamahu's Vengeance Idol"),
        ("AllyReturn", "Storm's Retreat Idol"),
        ("ProximityShield", "Bulwark Idol"),
        ("Cleanse", "Tawhoa's Blessing Idol"),
        ("BoarStampede", "Stampede Idol"),
        ("TotemLeeching", "Kitava's Blessing Idol"),
        ("SappingSpirit", "Lani Hua's Gift Idol"),
        ("TailwindTornado", "Ramako's Inspiration Idol"),

        ("NgamahuIgnite", "Umu Coals"),
        ("NgamahuRighteousFire", "Firebreather Mead"),
        ("NgamahuChanceForNoRespawnTimer", "Volcanic Emblem"),
        ("NgamahuFireExplode", "Dying Roar"),
        ("TasalioVines", "Ichthyic Bolas"),
        ("TasalioStealth", "Inky Infusion"),
        ("TasalioShroudWalker", "Sudden Fog"),
        ("TasalioChannellingInterruptDuration", "Sea Charm"),
        ("ArohonguiEnergyShield", "Silver Ward"),
        ("ArohonguiChilledGround", "Moon's Path"),
        ("ArohonguiColdAilmentImmunity", "Lunar Grace"),
        ("ArohonguiFreeze", "Dance of the Seasons"),
        ("ValakoChargesForDefensiveTeammates", "Lightning Rod"),
        ("ValakoProximityShield", "Whaletooth Bangle"),
        ("ValakoLightningThorns", "Carved Horns"),
        ("ValakoBlock", "Tidal Charm"),
        ("HinekoraFasterRespawn", "Birthing Spoon"),
        ("HinekoraCooldownRecovery", "Cyclic Bauble"),
        ("HinekoraChannellingDoesntPreventRespawnTimer", "Death Chimes"),
        ("HinekoraHinder", "Fate's Grasp"),
        ("TawhoaLifeRegeneration", "Amber Burgeon"),
        ("TawhoaTotemLifeRegeneration", "Spiritual Growth"),
        ("TawhoaMaximumLifeOnRespawn", "Bone Juju"),
        ("TawhoaNearbyPlayerFlaskCharges", "Dream Catcher"),
        ("KitavaLifeOnKill", "Victory Feast"),
        ("KitavaDamageTakenRecoveredByTotem", "Utula's Promise"),
        ("KitavaLifeRegenerationWhileChannelling", "Jawclamp Earrings"),
        ("KitavaNearbyEnemiesNoLifeRecovery", "Noxious Fumes"),
        ("RamakoMovementSpeed", "Eternal Pursuit"),
        ("RamakoTailwind", "Queen's Inspiration"),
        ("RamakoExtraProjectiles", "Sun-blot Band"),
        ("RamakoSpeedWhileEnemiesInCloseRange", "Sniper's Gambit"),
        ("TukohamaBonusesOnLowLife", "Ancestral Defiance"),
        ("TukohamaTotemDamageTakenWhileChannelling", "Massive Chisel"),
        ("TukohamaAdrenalineForOffensiveTeammates", "War Venom"),
        ("TukohamaDebilitate", "Jade Blade"),
        ("RongokuraiCannotBeSlowed", "Turtle Trinket"),
        ("RongokuraiStunOnRespawn", "Emerging Bellow"),
        ("RongokuraiKnockback", "Goliath Knuckle"),
        ("RongokuraiTotemLife", "Nightguard"),
        ("NoTribeColdResistance", "Summer Charm"),
        ("NoTribeLife", "Tough Snack"),
        ("NoTribeDamage", "Barbed Dagger"),
        ("NoTribeRespawnRate", "Battle Call"),
        ("NoTribeTotemLife", "Albino Weta"),
    };

    public AncestorQolSettings()
    {
        var unitFilter = "";
        Units = new CustomNode
        {
            DrawDelegate = () =>
            {
                ImGui.Text("Unit tiers");
                ImGui.InputTextWithHint("##CurrencyFilter", "Filter", ref unitFilter, 100);
                foreach (var (id, name) in UnitTypes.Where(t => t.Name.Contains(unitFilter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var currentValue = GetUnitTier(id);
                    if (ImGui.SliderInt($"{name}###{id}", ref currentValue, 1, 3))
                    {
                        UnitTiers[id] = currentValue;
                    }
                }
            }
        };
    }


    public int GetUnitTier(string type)
    {
        return UnitTiers.GetValueOrDefault(type ?? "", 2);
    }

    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    public RangeNode<int> FrameThickness { get; set; } = new RangeNode<int>(2, 0, 10);

    public ColorNode Tier1Color { get; set; } = new(Color.Green);
    public ColorNode Tier2Color { get; set; } = new(Color.White);
    public ColorNode Tier3Color { get; set; } = new(Color.Red);

    [JsonIgnore]
    public CustomNode Units { get; }

    public Dictionary<string, int> UnitTiers = new()
    {
    };
}