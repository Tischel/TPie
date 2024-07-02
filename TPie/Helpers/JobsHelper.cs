using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Helpers
{
    internal class JobsHelper
    {
        #region Singleton
        private JobsHelper()
        {
            FindActions();
        }

        public static void Initialize() { Instance = new JobsHelper(); }

        public static JobsHelper Instance { get; private set; } = null!;

        ~JobsHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        private List<LuminaAction> _playerActions = new List<LuminaAction>();

        private void FindActions()
        {
            ExcelSheet<LuminaAction>? sheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();
            _playerActions = sheet?.Where(row => row.IsPlayerAction && !row.IsPvP).ToList() ?? _playerActions;
            _playerActions.Sort(CompareActionsByName);
        }

        public List<LuminaAction> ActionsForJobId(uint jobId)
        {
            return _playerActions.Where(row => ClassJobCategoryContainsJob(row.ClassJobCategory.Row, jobId)).ToList();
        }

        public bool IsActionValid(uint jobId, uint actionId)
        {
            return ActionsForJobId(jobId).Any(row => row.RowId == actionId);
        }

        private static int CompareActionsByName(LuminaAction a, LuminaAction b) => String.Compare(a.Name.ToString(), b.Name.ToString(), StringComparison.Ordinal);

        public bool ClassJobCategoryContainsJob(uint key, uint jobId)
        {
            switch (key)
            {
                case 1: return true;
                case 2: return jobId == JobIDs.GLA;
                case 3: return jobId == JobIDs.PGL;
                case 4: return jobId == JobIDs.MRD;
                case 5: return jobId == JobIDs.LNC;
                case 6: return jobId == JobIDs.ARC;
                case 7: return jobId == JobIDs.CNJ;
                case 8: return jobId == JobIDs.THM;
                case 9: return jobId == JobIDs.CRP;
                case 10: return jobId == JobIDs.BSM;
                case 11: return jobId == JobIDs.ARM;
                case 12: return jobId == JobIDs.GSM;
                case 13: return jobId == JobIDs.LTW;
                case 14: return jobId == JobIDs.WVR;
                case 15: return jobId == JobIDs.ALC;
                case 16: return jobId == JobIDs.CUL;
                case 17: return jobId == JobIDs.MIN;
                case 18: return jobId == JobIDs.BOT;
                case 19: return jobId == JobIDs.FSH;
                case 20: return jobId == JobIDs.PLD;
                case 21: return jobId == JobIDs.MNK;
                case 22: return jobId == JobIDs.WAR;
                case 23: return jobId == JobIDs.DRG;
                case 24: return jobId == JobIDs.BRD;
                case 25: return jobId == JobIDs.WHM;
                case 26: return jobId == JobIDs.BLM;
                case 27: return jobId == JobIDs.ACN;
                case 28: return jobId == JobIDs.SMN;
                case 29: return jobId == JobIDs.SCH;
                case 38: return jobId == JobIDs.PLD;
                case 41: return jobId == JobIDs.MNK;
                case 42: return jobId == JobIDs.VPR;
                case 43: return jobId == JobIDs.PCT;
                case 44: return jobId == JobIDs.WAR;
                case 47: return jobId == JobIDs.DRG;
                case 50: return jobId == JobIDs.BRD;
                case 53: return jobId == JobIDs.WHM;
                case 55: return jobId == JobIDs.BLM;
                case 59: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 61: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 63: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU;
                case 64: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 66: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 68: return jobId == JobIDs.SCH || jobId == JobIDs.SMN;
                case 69: return jobId == JobIDs.SMN;
                case 73: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 89: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU;
                case 91: return jobId == JobIDs.ROG;
                case 92: return jobId == JobIDs.NIN;
                case 93: return jobId == JobIDs.NIN;
                case 94: return jobId == JobIDs.NIN;
                case 95: return jobId == JobIDs.NIN;
                case 96: return jobId == JobIDs.MCH;
                case 98: return jobId == JobIDs.DRK;
                case 99: return jobId == JobIDs.AST;
                case 103: return jobId == JobIDs.NIN;
                case 106: return jobId == JobIDs.BRD;
                case 111: return jobId == JobIDs.SAM;
                case 112: return jobId == JobIDs.RDM;
                case 113: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 114: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM || jobId == JobIDs.RPR || jobId == JobIDs.VPR;
                case 115: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 116: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU || jobId == JobIDs.PCT;
                case 117: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 118: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM || jobId == JobIDs.RPR || jobId == JobIDs.VPR;
                case 121: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 122: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM || jobId == JobIDs.VPR;
                case 123: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 125: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 129: return jobId == JobIDs.BLU;
                case 133: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST || jobId == JobIDs.SGE;
                case 134: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 139: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 147: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU || jobId == JobIDs.PCT;
                case 148: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM || jobId == JobIDs.RPR || jobId == JobIDs.VPR;
                case 149: return jobId == JobIDs.GNB;
                case 150: return jobId == JobIDs.DNC;
                case 160: return jobId == JobIDs.SCH;
                case 180: return jobId == JobIDs.RPR;
                case 181: return jobId == JobIDs.SGE;
            }

            return false;
        }

        public static Dictionary<JobRoles, List<uint>> JobsByRole = new Dictionary<JobRoles, List<uint>>()
        {
            // tanks
            [JobRoles.Tank] = new List<uint>() {
                JobIDs.GLA,
                JobIDs.MRD,
                JobIDs.PLD,
                JobIDs.WAR,
                JobIDs.DRK,
                JobIDs.GNB,
            },

            // healers
            [JobRoles.Healer] = new List<uint>()
            {
                JobIDs.CNJ,
                JobIDs.WHM,
                JobIDs.SCH,
                JobIDs.AST,
                JobIDs.SGE
            },

            // melee dps
            [JobRoles.DPSMelee] = new List<uint>() {
                JobIDs.PGL,
                JobIDs.LNC,
                JobIDs.ROG,
                JobIDs.MNK,
                JobIDs.DRG,
                JobIDs.NIN,
                JobIDs.SAM,
                JobIDs.RPR,
                JobIDs.VPR
            },

            // ranged phys dps
            [JobRoles.DPSRanged] = new List<uint>()
            {
                JobIDs.ARC,
                JobIDs.BRD,
                JobIDs.MCH,
                JobIDs.DNC,
            },

            // ranged magic dps
            [JobRoles.DPSCaster] = new List<uint>()
            {
                JobIDs.THM,
                JobIDs.ACN,
                JobIDs.BLM,
                JobIDs.SMN,
                JobIDs.RDM,
                JobIDs.BLU,
                JobIDs.PCT,
            },

            // crafters
            [JobRoles.Crafter] = new List<uint>()
            {
                JobIDs.CRP,
                JobIDs.BSM,
                JobIDs.ARM,
                JobIDs.GSM,
                JobIDs.LTW,
                JobIDs.WVR,
                JobIDs.ALC,
                JobIDs.CUL,
            },

            // gatherers
            [JobRoles.Gatherer] = new List<uint>()
            {
                JobIDs.MIN,
                JobIDs.BOT,
                JobIDs.FSH,
            },

            // unknown
            [JobRoles.Unknown] = new List<uint>()
        };

        public static Dictionary<JobRoles, string> RoleNames = new Dictionary<JobRoles, string>()
        {
            [JobRoles.Tank] = "Tank",
            [JobRoles.Healer] = "Healer",
            [JobRoles.DPSMelee] = "Melee",
            [JobRoles.DPSRanged] = "Ranged",
            [JobRoles.DPSCaster] = "Caster",
            [JobRoles.Crafter] = "Crafter",
            [JobRoles.Gatherer] = "Gatherer",
            [JobRoles.Unknown] = "Unknown"
        };

        public static Dictionary<uint, string> JobNames = new Dictionary<uint, string>()
        {
            [JobIDs.ACN] = "ACN",
            [JobIDs.ALC] = "ALC",
            [JobIDs.ARC] = "ARC",
            [JobIDs.ARM] = "ARM",
            [JobIDs.AST] = "AST",

            [JobIDs.BLM] = "BLM",
            [JobIDs.BLU] = "BLU",
            [JobIDs.BRD] = "BRD",
            [JobIDs.BSM] = "BSM",
            [JobIDs.BOT] = "BOT",

            [JobIDs.CNJ] = "CNJ",
            [JobIDs.CRP] = "CRP",
            [JobIDs.CUL] = "CUL",

            [JobIDs.DNC] = "DNC",
            [JobIDs.DRG] = "DRG",
            [JobIDs.DRK] = "DRK",

            [JobIDs.FSH] = "FSH",

            [JobIDs.GLA] = "GLA",
            [JobIDs.GNB] = "GNB",
            [JobIDs.GSM] = "GSM",

            [JobIDs.MRD] = "MRD",
            [JobIDs.PLD] = "PLD",
            [JobIDs.WAR] = "WAR",

            [JobIDs.LNC] = "LNC",
            [JobIDs.LTW] = "LTW",

            [JobIDs.MCH] = "MCH",
            [JobIDs.MIN] = "MIN",
            [JobIDs.MNK] = "MNK",

            [JobIDs.NIN] = "NIN",

            [JobIDs.PCT] = "PCT",
            [JobIDs.PGL] = "PGL",

            [JobIDs.RDM] = "RDM",
            [JobIDs.RPR] = "RPR",
            [JobIDs.ROG] = "ROG",

            [JobIDs.SAM] = "SAM",
            [JobIDs.SCH] = "SCH",
            [JobIDs.SGE] = "SGE",
            [JobIDs.SMN] = "SMN",

            [JobIDs.THM] = "THM",

            [JobIDs.VPR] = "VPR",

            [JobIDs.WVR] = "WVR",

            [JobIDs.WHM] = "WHM",
        };
    }

    public enum JobRoles
    {
        Tank = 0,
        Healer = 1,
        DPSMelee = 2,
        DPSRanged = 3,
        DPSCaster = 4,
        Crafter = 5,
        Gatherer = 6,
        Unknown
    }

    internal static class JobIDs
    {
        public const uint GLA = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;
        public const uint SGE = 40;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;
        public const uint RPR = 39;
        public const uint VPR = 41;

        public const uint ARC = 5;
        public const uint BRD = 23;
        public const uint MCH = 31;
        public const uint DNC = 38;

        public const uint THM = 7;
        public const uint ACN = 26;
        public const uint BLM = 25;
        public const uint SMN = 27;
        public const uint RDM = 35;
        public const uint BLU = 36;
        public const uint PCT = 42;

        public const uint CRP = 8;
        public const uint BSM = 9;
        public const uint ARM = 10;
        public const uint GSM = 11;
        public const uint LTW = 12;
        public const uint WVR = 13;
        public const uint ALC = 14;
        public const uint CUL = 15;

        public const uint MIN = 16;
        public const uint BOT = 17;
        public const uint FSH = 18;
    }
}
