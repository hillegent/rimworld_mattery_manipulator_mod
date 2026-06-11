using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MatterManipulator
{
    internal static class MatterManipulatorText
    {
        public static string T(string key)
        {
            return key.Translate().ToString();
        }

        public static string T(string key, params object[] args)
        {
            return string.Format(key.Translate().ToString(), args);
        }
    }

    public class MatterManipulatorMod : Mod
    {
        public static MatterManipulatorSettings Settings;
        private Vector2 scrollPosition;

        public MatterManipulatorMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MatterManipulatorSettings>();
            Settings.Clamp();
            ApplySettingsToDefs();
        }

        public override string SettingsCategory()
        {
            return MatterManipulatorText.T("MatterManipulator.ModName");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect, ref scrollPosition);
        }

        public override void WriteSettings()
        {
            Settings.Clamp();
            base.WriteSettings();
            ApplySettingsToDefs();
        }

        public static void ApplySettingsToDefs()
        {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("MatterManipulator");
            if (def != null)
            {
                def.drawerType = DrawerType.MapMeshAndRealTime;
                def.drawGUIOverlay = false;
            }

            var powerProps = def?.GetCompProperties<CompProperties_Power>();
            if (powerProps != null)
            {
                var field = typeof(CompProperties_Power).GetField("basePowerConsumption", BindingFlags.Instance | BindingFlags.NonPublic);
                field?.SetValue(powerProps, Settings?.powerConsumptionWatts ?? MatterManipulatorSettings.DefaultPowerConsumptionWatts);
            }

            var researchDef = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("MatterManipulation");
            if (researchDef != null)
            {
                researchDef.baseCost = Settings?.researchCost ?? MatterManipulatorSettings.DefaultResearchCost;
            }
        }
    }

    public class MatterManipulatorSettings : ModSettings
    {
        public const float DefaultInputMassFactor = 2f;
        public const float DefaultHoursPerKg = 8f;
        public const float DefaultPowerConsumptionWatts = 10000f;
        public const float DefaultResearchCost = 10000f;
        public const float DefaultAwfulMultiplier = 0.3f;
        public const float DefaultPoorMultiplier = 0.5f;
        public const float DefaultNormalMultiplier = 0.7f;
        public const float DefaultGoodMultiplier = 0.85f;
        public const float DefaultExcellentMultiplier = 1f;
        public const float DefaultMasterworkMultiplier = 1.2f;
        public const float DefaultLegendaryMultiplier = 1.5f;

        public float inputMassFactor = DefaultInputMassFactor;
        public float hoursPerKg = DefaultHoursPerKg;
        public float powerConsumptionWatts = DefaultPowerConsumptionWatts;
        public float researchCost = DefaultResearchCost;
        public float awfulMultiplier = DefaultAwfulMultiplier;
        public float poorMultiplier = DefaultPoorMultiplier;
        public float normalMultiplier = DefaultNormalMultiplier;
        public float goodMultiplier = DefaultGoodMultiplier;
        public float excellentMultiplier = DefaultExcellentMultiplier;
        public float masterworkMultiplier = DefaultMasterworkMultiplier;
        public float legendaryMultiplier = DefaultLegendaryMultiplier;

        private string inputMassBuffer;
        private string hoursPerKgBuffer;
        private string powerBuffer;
        private string researchCostBuffer;
        private string awfulBuffer;
        private string poorBuffer;
        private string normalBuffer;
        private string goodBuffer;
        private string excellentBuffer;
        private string masterworkBuffer;
        private string legendaryBuffer;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inputMassFactor, "inputMassFactor", DefaultInputMassFactor);
            Scribe_Values.Look(ref hoursPerKg, "hoursPerKg", DefaultHoursPerKg);
            Scribe_Values.Look(ref powerConsumptionWatts, "powerConsumptionWatts", DefaultPowerConsumptionWatts);
            Scribe_Values.Look(ref researchCost, "researchCost", DefaultResearchCost);
            Scribe_Values.Look(ref awfulMultiplier, "awfulMultiplier", DefaultAwfulMultiplier);
            Scribe_Values.Look(ref poorMultiplier, "poorMultiplier", DefaultPoorMultiplier);
            Scribe_Values.Look(ref normalMultiplier, "normalMultiplier", DefaultNormalMultiplier);
            Scribe_Values.Look(ref goodMultiplier, "goodMultiplier", DefaultGoodMultiplier);
            Scribe_Values.Look(ref excellentMultiplier, "excellentMultiplier", DefaultExcellentMultiplier);
            Scribe_Values.Look(ref masterworkMultiplier, "masterworkMultiplier", DefaultMasterworkMultiplier);
            Scribe_Values.Look(ref legendaryMultiplier, "legendaryMultiplier", DefaultLegendaryMultiplier);
            Clamp();
        }

        public void DoWindowContents(Rect inRect, ref Vector2 scrollPosition)
        {
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, 700f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label(MatterManipulatorText.T("MatterManipulator.Settings.General"));
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Settings.PowerConsumption"), ref powerConsumptionWatts, ref powerBuffer, 0f, 100000f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Settings.InputMassFactor"), ref inputMassFactor, ref inputMassBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Settings.HoursPerKg"), ref hoursPerKg, ref hoursPerKgBuffer, 0.01f, 1000f);

            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Settings.ResearchCost"), ref researchCost, ref researchCostBuffer, 1f, 1000000f);

            listing.GapLine();
            listing.Label(MatterManipulatorText.T("MatterManipulator.Settings.QualityMultipliers"));
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Awful"), ref awfulMultiplier, ref awfulBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Poor"), ref poorMultiplier, ref poorBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Normal"), ref normalMultiplier, ref normalBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Good"), ref goodMultiplier, ref goodBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Excellent"), ref excellentMultiplier, ref excellentBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Masterwork"), ref masterworkMultiplier, ref masterworkBuffer, 0.01f, 100f);
            NumericField(listing, MatterManipulatorText.T("MatterManipulator.Quality.Legendary"), ref legendaryMultiplier, ref legendaryBuffer, 0.01f, 100f);

            listing.GapLine();
            if (listing.ButtonText(MatterManipulatorText.T("MatterManipulator.Settings.ResetDefaults")))
            {
                ResetToDefaults();
                ResetBuffers();
            }

            listing.End();
            Widgets.EndScrollView();
            Clamp();
        }

        public void Clamp()
        {
            powerConsumptionWatts = Mathf.Clamp(powerConsumptionWatts, 0f, 100000f);
            inputMassFactor = Mathf.Clamp(inputMassFactor, 0.01f, 100f);
            hoursPerKg = Mathf.Clamp(hoursPerKg, 0.01f, 1000f);
            researchCost = Mathf.Clamp(researchCost, 1f, 1000000f);
            awfulMultiplier = Mathf.Clamp(awfulMultiplier, 0.01f, 100f);
            poorMultiplier = Mathf.Clamp(poorMultiplier, 0.01f, 100f);
            normalMultiplier = Mathf.Clamp(normalMultiplier, 0.01f, 100f);
            goodMultiplier = Mathf.Clamp(goodMultiplier, 0.01f, 100f);
            excellentMultiplier = Mathf.Clamp(excellentMultiplier, 0.01f, 100f);
            masterworkMultiplier = Mathf.Clamp(masterworkMultiplier, 0.01f, 100f);
            legendaryMultiplier = Mathf.Clamp(legendaryMultiplier, 0.01f, 100f);
        }

        public void ResetToDefaults()
        {
            inputMassFactor = DefaultInputMassFactor;
            hoursPerKg = DefaultHoursPerKg;
            powerConsumptionWatts = DefaultPowerConsumptionWatts;
            researchCost = DefaultResearchCost;
            awfulMultiplier = DefaultAwfulMultiplier;
            poorMultiplier = DefaultPoorMultiplier;
            normalMultiplier = DefaultNormalMultiplier;
            goodMultiplier = DefaultGoodMultiplier;
            excellentMultiplier = DefaultExcellentMultiplier;
            masterworkMultiplier = DefaultMasterworkMultiplier;
            legendaryMultiplier = DefaultLegendaryMultiplier;
        }

        public float MultiplierFor(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Awful:
                    return awfulMultiplier;
                case QualityCategory.Poor:
                    return poorMultiplier;
                case QualityCategory.Normal:
                    return normalMultiplier;
                case QualityCategory.Good:
                    return goodMultiplier;
                case QualityCategory.Excellent:
                    return excellentMultiplier;
                case QualityCategory.Masterwork:
                    return masterworkMultiplier;
                case QualityCategory.Legendary:
                    return legendaryMultiplier;
                default:
                    return 1f;
            }
        }

        private static void NumericField(Listing_Standard listing, string label, ref float value, ref string buffer, float min, float max)
        {
            listing.TextFieldNumericLabeled(label, ref value, ref buffer, min, max);
            listing.Gap(4f);
        }

        private void ResetBuffers()
        {
            inputMassBuffer = null;
            hoursPerKgBuffer = null;
            powerBuffer = null;
            researchCostBuffer = null;
            awfulBuffer = null;
            poorBuffer = null;
            normalBuffer = null;
            goodBuffer = null;
            excellentBuffer = null;
            masterworkBuffer = null;
            legendaryBuffer = null;
        }
    }

    public class Building_MatterManipulator : Building
    {
        private const int RandomQualityMode = -1;
        private const int DefaultBatchCount = 1;
        private static readonly int[] BatchCountChoices = { 1, 5, 10, 100 };
        private static readonly QualityCategory[] QualityChoices =
        {
            QualityCategory.Awful,
            QualityCategory.Poor,
            QualityCategory.Normal,
            QualityCategory.Good,
            QualityCategory.Excellent,
            QualityCategory.Masterwork,
            QualityCategory.Legendary
        };
        private static readonly Material ProgressBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.24f, 0.82f, 1f));
        private static readonly Material ProgressBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.08f, 0.09f, 0.1f));
        private static readonly IntVec3[] CardinalDirections =
        {
            new IntVec3(0, 0, 1),
            new IntVec3(1, 0, 0),
            new IntVec3(0, 0, -1),
            new IntVec3(-1, 0, 0)
        };

        private static List<ThingDef> cachedProductDefs;

        private ThingDef targetDef;
        private ThingDef targetStuff;
        private int progressTicks;
        private float feedstockMass;
        private int selectedQuality = RandomQualityMode;
        private int cycleQuality = RandomQualityMode;
        private int batchCount = DefaultBatchCount;
        private bool repeatProduction = true;
        private CompPowerTrader powerComp;

        private bool Powered => powerComp == null || powerComp.PowerOn;
        private float ProductMass => targetDef == null ? 0f : MassForProduct(targetDef, targetStuff) * ActiveBatchCount;
        private float RequiredFeedstockMass => ProductMass * InputMassFactor;
        private int BaseRequiredWorkTicks => targetDef == null ? 0 : Mathf.Max(1, Mathf.CeilToInt(ProductMass * WorkTicksPerKg * CurrentStuffWorkMultiplier));
        private int RequiredWorkTicks => targetDef == null ? 0 : Mathf.Max(1, Mathf.CeilToInt(BaseRequiredWorkTicks * CurrentQualityMultiplier));
        private float ProgressPct => RequiredWorkTicks <= 0 ? 0f : Mathf.Clamp01(progressTicks / (float)RequiredWorkTicks);
        private int ActiveBatchCount => ClampBatchCount(targetDef, batchCount);
        private bool TargetSupportsBatching => SupportsBatching(targetDef);
        private bool TargetSupportsBatchSelection => TargetSupportsBatching && BatchCountModesFor(targetDef).Skip(1).Any();
        private bool TargetSupportsQuality => targetDef != null && SupportsQuality(targetDef);
        private bool TargetSupportsStuff => targetDef != null && targetDef.MadeFromStuff && targetStuff != null;
        private float CurrentQualityMultiplier => TargetSupportsQuality ? QualityMultiplier(CurrentCycleQualityForTiming()) : 1f;
        private float CurrentStuffWorkMultiplier => StuffWorkMultiplier(targetStuff);
        private static MatterManipulatorSettings Settings => MatterManipulatorMod.Settings;
        private static float InputMassFactor => Settings?.inputMassFactor ?? MatterManipulatorSettings.DefaultInputMassFactor;
        private static int WorkTicksPerKg => Mathf.CeilToInt(GenDate.TicksPerHour * (Settings?.hoursPerKg ?? MatterManipulatorSettings.DefaultHoursPerKg));
        public int SelectedQualityMode => selectedQuality;

        public static List<ThingDef> ProductDefs
        {
            get
            {
                if (cachedProductDefs == null)
                {
                    cachedProductDefs = DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(CanProduce)
                        .OrderBy(def => def.label)
                        .ThenBy(def => def.defName)
                        .ToList();
                }

                return cachedProductDefs;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            MatterManipulatorMod.ApplySettingsToDefs();
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref targetDef, "targetDef");
            Scribe_Defs.Look(ref targetStuff, "targetStuff");
            Scribe_Values.Look(ref progressTicks, "progressTicks", 0);
            Scribe_Values.Look(ref feedstockMass, "feedstockMass", 0f);
            Scribe_Values.Look(ref selectedQuality, "selectedQuality", RandomQualityMode);
            Scribe_Values.Look(ref cycleQuality, "cycleQuality", RandomQualityMode);
            Scribe_Values.Look(ref batchCount, "batchCount", DefaultBatchCount);
            Scribe_Values.Look(ref repeatProduction, "repeatProduction", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (targetDef != null && targetDef.MadeFromStuff)
                {
                    targetStuff = ValidStuffFor(targetDef, targetStuff);
                }
                batchCount = ClampBatchCount(targetDef, batchCount);
            }
        }

        public override void TickRare()
        {
            base.TickRare();

            if (targetDef == null || !Spawned || !Powered)
            {
                return;
            }

            AbsorbHopperFeedstock();

            if (feedstockMass + 0.0001f < RequiredFeedstockMass)
            {
                return;
            }

            EnsureCycleQuality();
            progressTicks += GenTicks.TickRareInterval;
            if (progressTicks >= RequiredWorkTicks && TryFinishProduct())
            {
                feedstockMass = Mathf.Max(0f, feedstockMass - RequiredFeedstockMass);
                progressTicks = 0;
                cycleQuality = RandomQualityMode;
                if (!repeatProduction)
                {
                    ClearTarget();
                    return;
                }

                AbsorbHopperFeedstock();
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (ShouldDrawProgressBar())
            {
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = ProgressBarCenter(drawLoc),
                    size = new Vector2(1.6f, 0.18f),
                    fillPercent = ProgressPct,
                    filledMat = ProgressBarFilledMat,
                    unfilledMat = ProgressBarUnfilledMat,
                    margin = 0.04f,
                    rotation = Rot4.North
                });
            }
        }

        public override string GetInspectString()
        {
            var baseText = base.GetInspectString();
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(baseText))
            {
                sb.AppendLine(baseText.TrimEndNewlines());
            }

            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.Target", TargetLabel()));

            if (targetDef == null)
            {
                sb.Append(MatterManipulatorText.T("MatterManipulator.Status.SelectItem"));
                return sb.ToString();
            }

            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.RequiredMass", RequiredFeedstockMass.ToString("0.##")));
            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.MassBuffer", feedstockMass.ToString("0.##")));
            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.CycleTime", WorkTimeLabel()));
            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.Mode", ProductionModeLabel()));
            if (TargetSupportsBatchSelection)
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.BatchSize", ActiveBatchCount.ToString()));
            }
            if (TargetSupportsQuality)
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.Quality", SelectedQualityLabel()));
                if (cycleQuality != RandomQualityMode)
                {
                    var quality = (QualityCategory)cycleQuality;
                    sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.CycleQuality", QualityLabel(quality), QualityMultiplier(quality).ToString("0.##")));
                }
            }
            sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Inspect.Hoppers", AdjacentHoppers().Count().ToString()));

            if (!Powered)
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Status.NoPower"));
            }
            else if (!AdjacentHoppers().Any())
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Status.NeedsHopper"));
            }
            else if (feedstockMass + 0.0001f < RequiredFeedstockMass)
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Status.WaitingResources", Mathf.Max(0f, RequiredFeedstockMass - feedstockMass).ToString("0.##")));
            }
            else
            {
                sb.AppendLine(MatterManipulatorText.T("MatterManipulator.Status.Working", (ProgressPct * 100f).ToString("0")));
            }

            return sb.ToString().TrimEndNewlines();
        }

        private bool ShouldDrawProgressBar()
        {
            return Spawned &&
                targetDef != null &&
                Powered &&
                feedstockMass + 0.0001f >= RequiredFeedstockMass &&
                progressTicks > 0 &&
                progressTicks < RequiredWorkTicks;
        }

        private float VisibleProgressPct()
        {
            if (feedstockMass + 0.0001f < RequiredFeedstockMass)
            {
                return RequiredFeedstockMass <= 0f ? 0f : Mathf.Clamp01(feedstockMass / RequiredFeedstockMass);
            }

            return ProgressPct;
        }

        private string ProgressBarLabel()
        {
            if (!Powered)
            {
                return MatterManipulatorText.T("MatterManipulator.Progress.NoPower");
            }

            if (feedstockMass + 0.0001f < RequiredFeedstockMass)
            {
                return MatterManipulatorText.T("MatterManipulator.Progress.Feedstock", (VisibleProgressPct() * 100f).ToString("0"));
            }

            return $"{ProgressPct * 100f:0}%";
        }

        private static Vector3 ProgressBarCenter(Vector3 drawLoc)
        {
            var center = drawLoc + new Vector3(0f, 0f, 1.85f);
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            return center;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.SelectProduct"),
                defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.SelectProductDesc"),
                icon = CommandIcon(),
                action = () => Find.WindowStack.Add(new Dialog_SelectMatterProduct(this))
            };

            var hopperDef = DefDatabase<ThingDef>.GetNamedSilentFail("MatterHopper");
            if (hopperDef != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.BuildHopper"),
                    defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.BuildHopperDesc"),
                    icon = hopperDef.uiIcon,
                    action = () => Find.DesignatorManager.Select(new Designator_Build(hopperDef))
                };
            }

            if (targetDef != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.Mode", ProductionModeLabel()),
                    defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.ModeDesc"),
                    icon = CommandIcon(),
                    action = ToggleProductionMode
                };

                if (TargetSupportsBatchSelection)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.BatchSize", ActiveBatchCount.ToString()),
                        defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.BatchSizeDesc"),
                        icon = CommandIcon(),
                        action = ToggleBatchCount
                    };
                }

                if (TargetSupportsStuff)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.Material", targetStuff.LabelCap),
                        defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.MaterialDesc"),
                        icon = targetStuff.uiIcon,
                        action = () => Find.WindowStack.Add(new Dialog_SelectMatterStuff(this, targetDef))
                    };
                }

                if (TargetSupportsQuality)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.Quality", SelectedQualityLabel()),
                        defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.QualityDesc"),
                        icon = CommandIcon(),
                        action = () => Find.WindowStack.Add(new Dialog_SelectMatterQuality(this))
                    };
                }

                yield return new Command_Action
                {
                    defaultLabel = MatterManipulatorText.T("MatterManipulator.Command.ClearTarget"),
                    defaultDesc = MatterManipulatorText.T("MatterManipulator.Command.ClearTargetDesc"),
                    icon = CommandIcon(),
                    action = ClearTarget
                };
            }
        }

        public void SetTarget(ThingDef def)
        {
            SetTarget(def, BestStuffFor(def));
        }

        public void SetTarget(ThingDef def, ThingDef stuff)
        {
            targetDef = def;
            targetStuff = def != null && def.MadeFromStuff ? ValidStuffFor(def, stuff) : null;
            progressTicks = 0;
            selectedQuality = RandomQualityMode;
            cycleQuality = RandomQualityMode;
            batchCount = DefaultBatchCount;
            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.ProductSelected", TargetLabel()), this, MessageTypeDefOf.TaskCompletion, false);
        }

        public bool IsCurrentTarget(ThingDef def)
        {
            return targetDef == def;
        }

        public void SetStuffForCurrentTarget(ThingDef stuff)
        {
            if (targetDef == null || !targetDef.MadeFromStuff)
            {
                return;
            }

            targetStuff = ValidStuffFor(targetDef, stuff);
            progressTicks = 0;
            cycleQuality = RandomQualityMode;
            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.MaterialSelected", targetStuff?.LabelCap ?? MatterManipulatorText.T("MatterManipulator.NoneSelected")), this, MessageTypeDefOf.TaskCompletion, false);
        }

        private void ToggleProductionMode()
        {
            repeatProduction = !repeatProduction;
            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.ModeSelected", ProductionModeLabel()), this, MessageTypeDefOf.TaskCompletion, false);
        }

        private string ProductionModeLabel()
        {
            return repeatProduction ? MatterManipulatorText.T("MatterManipulator.Mode.Repeat") : MatterManipulatorText.T("MatterManipulator.Mode.MakeOne");
        }

        private void ToggleBatchCount()
        {
            var choices = BatchCountModesFor(targetDef).ToList();
            if (choices.Count <= 1)
            {
                batchCount = DefaultBatchCount;
                return;
            }

            var current = ActiveBatchCount;
            var index = choices.IndexOf(current);
            batchCount = choices[(index + 1 + choices.Count) % choices.Count];
            progressTicks = 0;
            cycleQuality = RandomQualityMode;
            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.BatchSelected", ActiveBatchCount.ToString()), this, MessageTypeDefOf.TaskCompletion, false);
        }

        public void SetQualityMode(int qualityMode)
        {
            if (qualityMode != RandomQualityMode && !QualityChoices.Contains((QualityCategory)qualityMode))
            {
                return;
            }

            if (selectedQuality == qualityMode)
            {
                return;
            }

            selectedQuality = qualityMode;
            cycleQuality = RandomQualityMode;
            progressTicks = 0;
            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.QualitySelected", SelectedQualityLabel()), this, MessageTypeDefOf.TaskCompletion, false);
        }

        private void ClearTarget()
        {
            targetDef = null;
            targetStuff = null;
            progressTicks = 0;
            selectedQuality = RandomQualityMode;
            cycleQuality = RandomQualityMode;
            batchCount = DefaultBatchCount;
        }

        private void AbsorbHopperFeedstock()
        {
            if (targetDef == null || feedstockMass + 0.0001f >= RequiredFeedstockMass)
            {
                return;
            }

            var heldThings = AdjacentHoppers()
                .Where(hopper => hopper.slotGroup != null)
                .SelectMany(hopper => hopper.slotGroup.HeldThings)
                .ToList();

            for (var i = 0; i < heldThings.Count && feedstockMass + 0.0001f < RequiredFeedstockMass; i++)
            {
                var thing = heldThings[i];
                if (thing == null || thing.Destroyed)
                {
                    continue;
                }

                var massPerUnit = MassPerUnit(thing);
                if (massPerUnit <= 0f)
                {
                    continue;
                }

                var needed = RequiredFeedstockMass - feedstockMass;
                var countToConsume = Mathf.Min(thing.stackCount, Mathf.CeilToInt(needed / massPerUnit));
                if (countToConsume <= 0)
                {
                    continue;
                }

                Consume(thing, countToConsume);
                feedstockMass += massPerUnit * countToConsume;
            }
        }

        private IEnumerable<Building_Storage> AdjacentHoppers()
        {
            if (!Spawned)
            {
                yield break;
            }

            var seen = new HashSet<Building_Storage>();
            var occupiedRect = this.OccupiedRect();
            foreach (var occupiedCell in occupiedRect)
            {
                for (var i = 0; i < CardinalDirections.Length; i++)
                {
                    var cell = occupiedCell + CardinalDirections[i];
                    if (!cell.InBounds(Map) || occupiedRect.Contains(cell))
                    {
                        continue;
                    }

                    var things = cell.GetThingList(Map);
                    for (var j = 0; j < things.Count; j++)
                    {
                        if (things[j] is Building_Storage storage &&
                            storage.def?.building != null &&
                            storage.def.building.isHopper &&
                            seen.Add(storage))
                        {
                            yield return storage;
                        }
                    }
                }
            }
        }

        private static void Consume(Thing thing, int count)
        {
            if (count >= thing.stackCount)
            {
                thing.Destroy(DestroyMode.Vanish);
                return;
            }

            var split = thing.SplitOff(count);
            split.Destroy(DestroyMode.Vanish);
        }

        private bool TryFinishProduct()
        {
            var productQuality = TargetSupportsQuality ? (QualityCategory?)CurrentCycleQualityForTiming() : null;
            var product = MakeProduct(targetDef, targetStuff, productQuality, ActiveBatchCount);
            if (product == null)
            {
                Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.MakeFailed"), this, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            var outputCell = InteractionCell.IsValid && InteractionCell.InBounds(Map) ? InteractionCell : Position;
            if (!GenPlace.TryPlaceThing(product, outputCell, Map, ThingPlaceMode.Near))
            {
                product.Destroy(DestroyMode.Vanish);
                Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.NoOutputSpace"), this, MessageTypeDefOf.RejectInput, false);
                return false;
            }

            Messages.Message(MatterManipulatorText.T("MatterManipulator.Message.Created", product.LabelShortCap), product, MessageTypeDefOf.TaskCompletion, false);
            return true;
        }

        private static Thing MakeProduct(ThingDef def, ThingDef stuff, QualityCategory? qualityToSet, int stackCount)
        {
            try
            {
                var product = ThingMaker.MakeThing(def, def.MadeFromStuff ? stuff : null);
                product.stackCount = Mathf.Clamp(stackCount, DefaultBatchCount, Mathf.Max(DefaultBatchCount, def.stackLimit));

                var quality = product.TryGetComp<CompQuality>();
                if (quality != null && qualityToSet.HasValue)
                {
                    quality.SetQuality(qualityToSet.Value, ArtGenerationContext.Outsider);
                }

                return product;
            }
            catch (Exception ex)
            {
                Log.Warning($"[MatterManipulator] Failed to make product {def?.defName}: {ex.Message}");
                return null;
            }
        }

        private string TargetLabel()
        {
            if (targetDef == null)
            {
                return MatterManipulatorText.T("MatterManipulator.NoneSelected");
            }

            if (targetStuff != null)
            {
                return $"{targetStuff.label} {targetDef.label}".CapitalizeFirst();
            }

            return targetDef.LabelCap;
        }

        private void EnsureCycleQuality()
        {
            if (!TargetSupportsQuality)
            {
                cycleQuality = RandomQualityMode;
                return;
            }

            if (cycleQuality != RandomQualityMode)
            {
                return;
            }

            if (selectedQuality != RandomQualityMode)
            {
                cycleQuality = selectedQuality;
                return;
            }

            cycleQuality = (int)QualityChoices[Rand.Range(0, QualityChoices.Length)];
        }

        private QualityCategory CurrentCycleQualityForTiming()
        {
            if (!TargetSupportsQuality)
            {
                return QualityCategory.Normal;
            }

            if (cycleQuality != RandomQualityMode)
            {
                return (QualityCategory)cycleQuality;
            }

            if (selectedQuality != RandomQualityMode)
            {
                return (QualityCategory)selectedQuality;
            }

            return QualityCategory.Normal;
        }

        private string WorkTimeLabel()
        {
            if (!TargetSupportsQuality)
            {
                return MatterManipulatorText.T("MatterManipulator.Time.Hours", TicksToHours(BaseRequiredWorkTicks).ToString("0.#"));
            }

            if (selectedQuality == RandomQualityMode && cycleQuality == RandomQualityMode)
            {
                var minTicks = Mathf.CeilToInt(BaseRequiredWorkTicks * QualityMultiplier(QualityCategory.Awful));
                var maxTicks = Mathf.CeilToInt(BaseRequiredWorkTicks * QualityMultiplier(QualityCategory.Legendary));
                return MatterManipulatorText.T("MatterManipulator.Time.HourRange", TicksToHours(minTicks).ToString("0.#"), TicksToHours(maxTicks).ToString("0.#"));
            }

            return MatterManipulatorText.T("MatterManipulator.Time.HoursMultiplier", TicksToHours(RequiredWorkTicks).ToString("0.#"), CurrentQualityMultiplier.ToString("0.##"));
        }

        private string SelectedQualityLabel()
        {
            return QualityLabelForMode(selectedQuality);
        }

        public static IEnumerable<int> QualityModes()
        {
            yield return RandomQualityMode;
            for (var i = 0; i < QualityChoices.Length; i++)
            {
                yield return (int)QualityChoices[i];
            }
        }

        public static IEnumerable<int> BatchCountModesFor(ThingDef def)
        {
            if (!SupportsBatching(def))
            {
                yield return DefaultBatchCount;
                yield break;
            }

            for (var i = 0; i < BatchCountChoices.Length; i++)
            {
                if (BatchCountChoices[i] <= def.stackLimit)
                {
                    yield return BatchCountChoices[i];
                }
            }
        }

        public static string QualityLabelForMode(int qualityMode)
        {
            return qualityMode == RandomQualityMode ? MatterManipulatorText.T("MatterManipulator.Quality.Random") : QualityLabel((QualityCategory)qualityMode);
        }

        public static string QualityMultiplierLabelForMode(int qualityMode)
        {
            return qualityMode == RandomQualityMode ? MatterManipulatorText.T("MatterManipulator.Quality.RandomEveryCycle") : $"x{QualityMultiplier((QualityCategory)qualityMode):0.##}";
        }

        public static float BaseHoursForMass(float mass)
        {
            return mass * (Settings?.hoursPerKg ?? MatterManipulatorSettings.DefaultHoursPerKg);
        }

        public static float HoursForMassAndStuff(float mass, ThingDef stuff)
        {
            return BaseHoursForMass(mass) * StuffWorkMultiplier(stuff);
        }

        public static float StuffWorkMultiplier(ThingDef stuff)
        {
            var factors = stuff?.stuffProps?.statFactors;
            if (factors == null)
            {
                return 1f;
            }

            for (var i = 0; i < factors.Count; i++)
            {
                if (factors[i].stat == StatDefOf.WorkToMake)
                {
                    return Mathf.Max(0.01f, factors[i].value);
                }
            }

            return 1f;
        }

        public static List<ThingDef> AllowedStuffsForProduct(ThingDef def)
        {
            if (def == null || !def.MadeFromStuff)
            {
                return new List<ThingDef>();
            }

            return GenStuff.AllowedStuffsFor(def)
                .Where(stuff => stuff != null)
                .OrderBy(stuff => stuff.label)
                .ThenBy(stuff => stuff.defName)
                .ToList();
        }

        public static string ProductLabelFor(ThingDef def, ThingDef stuff)
        {
            if (def == null)
            {
                return "";
            }

            if (stuff != null && def.MadeFromStuff)
            {
                return $"{stuff.label} {def.label}".CapitalizeFirst();
            }

            return def.LabelCap;
        }

        private static bool SupportsQuality(ThingDef def)
        {
            return def != null && def.HasComp(typeof(CompQuality));
        }

        private static bool SupportsBatching(ThingDef def)
        {
            return def != null && def.stackLimit > DefaultBatchCount;
        }

        private static int ClampBatchCount(ThingDef def, int count)
        {
            if (!SupportsBatching(def))
            {
                return DefaultBatchCount;
            }

            var best = DefaultBatchCount;
            for (var i = 0; i < BatchCountChoices.Length; i++)
            {
                if (BatchCountChoices[i] <= def.stackLimit && BatchCountChoices[i] <= count)
                {
                    best = BatchCountChoices[i];
                }
            }

            return best;
        }

        private static string QualityLabel(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Awful:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Awful");
                case QualityCategory.Poor:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Poor");
                case QualityCategory.Normal:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Normal");
                case QualityCategory.Good:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Good");
                case QualityCategory.Excellent:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Excellent");
                case QualityCategory.Masterwork:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Masterwork");
                case QualityCategory.Legendary:
                    return MatterManipulatorText.T("MatterManipulator.Quality.Legendary");
                default:
                    return quality.ToString();
            }
        }

        private static float QualityMultiplier(QualityCategory quality)
        {
            if (Settings != null)
            {
                return Settings.MultiplierFor(quality);
            }

            switch (quality)
            {
                case QualityCategory.Awful:
                    return MatterManipulatorSettings.DefaultAwfulMultiplier;
                case QualityCategory.Poor:
                    return MatterManipulatorSettings.DefaultPoorMultiplier;
                case QualityCategory.Normal:
                    return MatterManipulatorSettings.DefaultNormalMultiplier;
                case QualityCategory.Good:
                    return MatterManipulatorSettings.DefaultGoodMultiplier;
                case QualityCategory.Excellent:
                    return MatterManipulatorSettings.DefaultExcellentMultiplier;
                case QualityCategory.Masterwork:
                    return MatterManipulatorSettings.DefaultMasterworkMultiplier;
                case QualityCategory.Legendary:
                    return MatterManipulatorSettings.DefaultLegendaryMultiplier;
                default:
                    return 1f;
            }
        }

        private static float MassPerUnit(Thing thing)
        {
            try
            {
                return Mathf.Max(0f, thing.GetStatValue(StatDefOf.Mass));
            }
            catch
            {
                return Mathf.Max(0f, thing.def.BaseMass);
            }
        }

        private static float MassForProduct(ThingDef def, ThingDef stuff)
        {
            return Mathf.Max(0.01f, def.BaseMass);
        }

        private static float TicksToHours(int ticks)
        {
            return ticks / (float)GenDate.TicksPerHour;
        }

        private static Texture2D CommandIcon()
        {
            return ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", false) ?? BaseContent.BadTex;
        }

        private static bool CanProduce(ThingDef def)
        {
            if (def == null || def.category != ThingCategory.Item || def.label.NullOrEmpty() || def.BaseMass <= 0f)
            {
                return false;
            }

            if (def.IsBlueprint || def.IsFrame || def.destroyOnDrop || def.stackLimit <= 0)
            {
                return false;
            }

            var thingClass = def.thingClass;
            if (thingClass != null &&
                (typeof(Corpse).IsAssignableFrom(thingClass) ||
                 typeof(MinifiedThing).IsAssignableFrom(thingClass) ||
                 typeof(UnfinishedThing).IsAssignableFrom(thingClass)))
            {
                return false;
            }

            if (def.MadeFromStuff && BestStuffFor(def) == null)
            {
                return false;
            }

            return true;
        }

        private static ThingDef BestStuffFor(ThingDef def)
        {
            if (def == null || !def.MadeFromStuff)
            {
                return null;
            }

            var defaultStuff = GenStuff.DefaultStuffFor(def);
            if (defaultStuff != null)
            {
                return defaultStuff;
            }

            return AllowedStuffsForProduct(def).FirstOrDefault();
        }

        private static ThingDef ValidStuffFor(ThingDef def, ThingDef stuff)
        {
            if (def == null || !def.MadeFromStuff)
            {
                return null;
            }

            var allowedStuffs = AllowedStuffsForProduct(def);
            if (stuff != null && allowedStuffs.Contains(stuff))
            {
                return stuff;
            }

            return allowedStuffs.FirstOrDefault();
        }
    }

    public class Dialog_SelectMatterQuality : Window
    {
        private const float RowHeight = 42f;
        private Vector2 scrollPosition;
        private readonly Building_MatterManipulator manipulator;

        public Dialog_SelectMatterQuality(Building_MatterManipulator manipulator)
        {
            this.manipulator = manipulator;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(560f, 520f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), MatterManipulatorText.T("MatterManipulator.Dialog.Quality.Title"));

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 42f, inRect.width, 44f), MatterManipulatorText.T("MatterManipulator.Dialog.Quality.Description"));

            var modes = Building_MatterManipulator.QualityModes().ToList();
            var outRect = new Rect(0f, 96f, inRect.width, inRect.height - 96f);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(outRect.height, modes.Count * RowHeight));

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            for (var i = 0; i < modes.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight - 4f);
                DrawQualityRow(row, modes[i]);
            }
            Widgets.EndScrollView();
        }

        private void DrawQualityRow(Rect row, int mode)
        {
            if (Mouse.IsOver(row))
            {
                Widgets.DrawHighlight(row);
            }

            var selected = manipulator.SelectedQualityMode == mode;
            var labelRect = new Rect(row.x + 4f, row.y + 6f, row.width * 0.42f, row.height - 8f);
            var multiplierRect = new Rect(labelRect.xMax + 8f, row.y + 6f, row.width * 0.28f, row.height - 8f);
            var buttonRect = new Rect(row.xMax - 112f, row.y + 4f, 108f, row.height - 8f);

            Widgets.Label(labelRect, Building_MatterManipulator.QualityLabelForMode(mode));
            Widgets.Label(multiplierRect, Building_MatterManipulator.QualityMultiplierLabelForMode(mode));

            if (selected)
            {
                Widgets.Label(buttonRect, MatterManipulatorText.T("MatterManipulator.Button.Selected"));
                return;
            }

            if (Widgets.ButtonText(buttonRect, MatterManipulatorText.T("MatterManipulator.Button.Select")))
            {
                manipulator.SetQualityMode(mode);
                Close();
            }
        }
    }

    public class Dialog_SelectMatterStuff : Window
    {
        private const float RowHeight = 38f;
        private readonly Building_MatterManipulator manipulator;
        private readonly ThingDef productDef;
        private string search = "";
        private Vector2 scrollPosition;

        public Dialog_SelectMatterStuff(Building_MatterManipulator manipulator, ThingDef productDef)
        {
            this.manipulator = manipulator;
            this.productDef = productDef;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(720f, 760f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), MatterManipulatorText.T("MatterManipulator.Dialog.Stuff.Title"));

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 36f, inRect.width, 24f), productDef?.LabelCap ?? "");
            search = Widgets.TextField(new Rect(0f, 66f, inRect.width, 32f), search ?? "");

            var stuffs = FilteredStuffs().Take(500).ToList();
            Widgets.Label(new Rect(0f, 104f, inRect.width, 24f), MatterManipulatorText.T("MatterManipulator.Dialog.Stuff.Found", stuffs.Count.ToString()));

            var outRect = new Rect(0f, 134f, inRect.width, inRect.height - 134f);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(outRect.height, stuffs.Count * RowHeight));

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            for (var i = 0; i < stuffs.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight - 2f);
                DrawStuffRow(row, stuffs[i]);
            }
            Widgets.EndScrollView();
        }

        private IEnumerable<ThingDef> FilteredStuffs()
        {
            var query = (search ?? "").Trim().ToLowerInvariant();
            foreach (var stuff in Building_MatterManipulator.AllowedStuffsForProduct(productDef))
            {
                if (query.Length == 0 || $"{stuff.label} {stuff.defName}".ToLowerInvariant().Contains(query))
                {
                    yield return stuff;
                }
            }
        }

        private void DrawStuffRow(Rect row, ThingDef stuff)
        {
            if (Mouse.IsOver(row))
            {
                Widgets.DrawHighlight(row);
            }

            var labelRect = new Rect(row.x + 4f, row.y + 4f, row.width * 0.42f, row.height - 8f);
            var infoRect = new Rect(labelRect.xMax + 8f, row.y + 4f, row.width * 0.32f, row.height - 8f);
            var buttonRect = new Rect(row.xMax - 96f, row.y + 4f, 92f, row.height - 8f);
            var mass = Mathf.Max(0.01f, productDef.BaseMass);
            var multiplier = Building_MatterManipulator.StuffWorkMultiplier(stuff);
            var hours = Building_MatterManipulator.HoursForMassAndStuff(mass, stuff);

            Widgets.Label(labelRect, Building_MatterManipulator.ProductLabelFor(productDef, stuff));
            Widgets.Label(infoRect, MatterManipulatorText.T("MatterManipulator.Dialog.Stuff.Info", multiplier.ToString("0.##"), hours.ToString("0.#")));

            if (Widgets.ButtonText(buttonRect, MatterManipulatorText.T("MatterManipulator.Button.Select")))
            {
                if (manipulator.IsCurrentTarget(productDef))
                {
                    manipulator.SetStuffForCurrentTarget(stuff);
                }
                else
                {
                    manipulator.SetTarget(productDef, stuff);
                }
                Close();
            }
        }
    }

    public class Dialog_SelectMatterProduct : Window
    {
        private const float RowHeight = 38f;
        private static readonly Dictionary<ThingDef, float> productMassCache = new Dictionary<ThingDef, float>();

        private readonly Building_MatterManipulator manipulator;
        private string search = "";
        private Vector2 scrollPosition;

        public Dialog_SelectMatterProduct(Building_MatterManipulator manipulator)
        {
            this.manipulator = manipulator;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(720f, 760f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), MatterManipulatorText.T("MatterManipulator.Dialog.Product.Title"));

            Text.Font = GameFont.Small;
            search = Widgets.TextField(new Rect(0f, 42f, inRect.width, 32f), search ?? "");

            var products = FilteredProducts().Take(300).ToList();
            var infoRect = new Rect(0f, 80f, inRect.width, 24f);
            Widgets.Label(infoRect, MatterManipulatorText.T("MatterManipulator.Dialog.Product.Found", products.Count.ToString()));

            var outRect = new Rect(0f, 110f, inRect.width, inRect.height - 110f);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(outRect.height, products.Count * RowHeight));

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            for (var i = 0; i < products.Count; i++)
            {
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight - 2f);
                DrawProductRow(row, products[i]);
            }
            Widgets.EndScrollView();
        }

        private IEnumerable<ThingDef> FilteredProducts()
        {
            var query = (search ?? "").Trim().ToLowerInvariant();
            foreach (var def in Building_MatterManipulator.ProductDefs)
            {
                if (query.Length == 0 || SearchText(def).Contains(query))
                {
                    yield return def;
                }
            }
        }

        private static string SearchText(ThingDef def)
        {
            var stuff = GetStuffLabel(def);
            return $"{def.label} {def.defName} {stuff}".ToLowerInvariant();
        }

        private static string GetStuffLabel(ThingDef def)
        {
            if (def == null || !def.MadeFromStuff)
            {
                return "";
            }

            var stuff = GenStuff.DefaultStuffFor(def) ?? Building_MatterManipulator.AllowedStuffsForProduct(def).FirstOrDefault();
            return stuff?.label ?? "";
        }

        private void DrawProductRow(Rect row, ThingDef def)
        {
            if (Mouse.IsOver(row))
            {
                Widgets.DrawHighlight(row);
            }

            var labelRect = new Rect(row.x + 4f, row.y + 4f, row.width * 0.48f, row.height - 8f);
            var infoRect = new Rect(labelRect.xMax + 8f, row.y + 4f, row.width * 0.28f, row.height - 8f);
            var buttonRect = new Rect(row.xMax - 96f, row.y + 4f, 92f, row.height - 8f);

            Widgets.Label(labelRect, ProductLabel(def));

            var stuff = GenStuff.DefaultStuffFor(def) ?? Building_MatterManipulator.AllowedStuffsForProduct(def).FirstOrDefault();
            var mass = def.MadeFromStuff ? ProductMass(def, stuff) : ProductMass(def, null);
            Widgets.Label(infoRect, MatterManipulatorText.T("MatterManipulator.Dialog.Product.Info", mass.ToString("0.##"), Building_MatterManipulator.HoursForMassAndStuff(mass, stuff).ToString("0.#")));

            var buttonLabel = def.MadeFromStuff ? MatterManipulatorText.T("MatterManipulator.Button.Material") : MatterManipulatorText.T("MatterManipulator.Button.Select");
            if (Widgets.ButtonText(buttonRect, buttonLabel))
            {
                if (def.MadeFromStuff)
                {
                    Find.WindowStack.Add(new Dialog_SelectMatterStuff(manipulator, def));
                }
                else
                {
                    manipulator.SetTarget(def);
                }
                Close();
            }
        }

        private static string ProductLabel(ThingDef def)
        {
            return def.LabelCap;
        }

        private static float ProductMass(ThingDef def, ThingDef stuff)
        {
            if (productMassCache.TryGetValue(def, out var cachedMass))
            {
                return cachedMass;
            }

            var mass = Mathf.Max(0.01f, def.BaseMass);
            productMassCache[def] = mass;
            return mass;
        }
    }
}
