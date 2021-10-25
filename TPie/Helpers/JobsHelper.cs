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

        private static int CompareActionsByName(LuminaAction a, LuminaAction b)
        {
            return a.Name.ToString().CompareTo(b.Name.ToString());
        }

        private bool ClassJobCategoryContainsJob(uint key, uint jobId)
        {
            switch (key)
            {
                case 1: return true;
                case 2: return jobId == JobIds.GLD;
                case 3: return jobId == JobIds.PGL;
                case 4: return jobId == JobIds.MRD;
                case 5: return jobId == JobIds.LNC;
                case 6: return jobId == JobIds.ARC;
                case 7: return jobId == JobIds.CNJ;
                case 8: return jobId == JobIds.THM;
                case 9: return jobId == JobIds.CRP;
                case 10: return jobId == JobIds.BSM;
                case 11: return jobId == JobIds.ARM;
                case 12: return jobId == JobIds.GSM;
                case 13: return jobId == JobIds.LTW;
                case 14: return jobId == JobIds.WVR;
                case 15: return jobId == JobIds.ALC;
                case 16: return jobId == JobIds.CUL;
                case 17: return jobId == JobIds.MIN;
                case 18: return jobId == JobIds.BTN;
                case 19: return jobId == JobIds.FSH;
                case 20: return jobId == JobIds.PLD;
                case 21: return jobId == JobIds.MNK;
                case 22: return jobId == JobIds.WAR;
                case 23: return jobId == JobIds.DRG;
                case 24: return jobId == JobIds.BRD;
                case 25: return jobId == JobIds.WHM;
                case 26: return jobId == JobIds.BLM;
                case 27: return jobId == JobIds.ACN;
                case 28: return jobId == JobIds.SMN;
                case 29: return jobId == JobIds.SCH;
                case 38: return jobId == JobIds.PLD;
                case 41: return jobId == JobIds.MNK;
                case 44: return jobId == JobIds.WAR;
                case 47: return jobId == JobIds.DRG;
                case 50: return jobId == JobIds.BRD;
                case 53: return jobId == JobIds.WHM;
                case 55: return jobId == JobIds.BLM;
                case 59: return jobId == JobIds.PLD || jobId == JobIds.WAR || jobId == JobIds.DRK || jobId == JobIds.GNB;
                case 61: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 63: return jobId == JobIds.BLM || jobId == JobIds.SMN || jobId == JobIds.RDM || jobId == JobIds.BLU;
                case 64: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 66: return jobId == JobIds.BRD || jobId == JobIds.MCH || jobId == JobIds.DNC;
                case 68: return jobId == JobIds.SCH || jobId == JobIds.SMN;
                case 69: return jobId == JobIds.SMN;
                case 73: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 89: return jobId == JobIds.BLM || jobId == JobIds.SMN || jobId == JobIds.RDM || jobId == JobIds.BLU;
                case 91: return jobId == JobIds.ROG;
                case 92: return jobId == JobIds.NIN;
                case 93: return jobId == JobIds.NIN;
                case 94: return jobId == JobIds.NIN;
                case 95: return jobId == JobIds.NIN;
                case 96: return jobId == JobIds.MCH;
                case 98: return jobId == JobIds.DRK;
                case 99: return jobId == JobIds.AST;
                case 103: return jobId == JobIds.NIN;
                case 106: return jobId == JobIds.BRD;
                case 111: return jobId == JobIds.SAM;
                case 112: return jobId == JobIds.RDM;
                case 113: return jobId == JobIds.PLD || jobId == JobIds.WAR || jobId == JobIds.DRK || jobId == JobIds.GNB;
                case 114: return jobId == JobIds.MNK || jobId == JobIds.DRG || jobId == JobIds.NIN || jobId == JobIds.SAM;
                case 115: return jobId == JobIds.BRD || jobId == JobIds.MCH || jobId == JobIds.DNC;
                case 116: return jobId == JobIds.BLM || jobId == JobIds.SMN || jobId == JobIds.RDM || jobId == JobIds.BLU;
                case 117: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 118: return jobId == JobIds.MNK || jobId == JobIds.DRG || jobId == JobIds.NIN || jobId == JobIds.SAM;
                case 121: return jobId == JobIds.PLD || jobId == JobIds.WAR || jobId == JobIds.DRK || jobId == JobIds.GNB;
                case 122: return jobId == JobIds.MNK || jobId == JobIds.DRG || jobId == JobIds.NIN || jobId == JobIds.SAM;
                case 123: return jobId == JobIds.BRD || jobId == JobIds.MCH || jobId == JobIds.DNC;
                case 125: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 129: return jobId == JobIds.BLU;
                case 133: return jobId == JobIds.WHM || jobId == JobIds.SCH || jobId == JobIds.AST;
                case 134: return jobId == JobIds.PLD || jobId == JobIds.WAR || jobId == JobIds.DRK || jobId == JobIds.GNB;
                case 139: return jobId == JobIds.BRD || jobId == JobIds.MCH || jobId == JobIds.DNC;
                case 147: return jobId == JobIds.BLM || jobId == JobIds.SMN || jobId == JobIds.RDM || jobId == JobIds.BLU;
                case 148: return jobId == JobIds.MNK || jobId == JobIds.DRG || jobId == JobIds.NIN || jobId == JobIds.SAM;
                case 149: return jobId == JobIds.GNB;
                case 150: return jobId == JobIds.DNC;
                case 160: return jobId == JobIds.SCH;
            }

            return false;
        }
    }

    internal static class JobIds
    {
        public const uint GLD = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;

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

        public const uint CRP = 8;
        public const uint BSM = 9;
        public const uint ARM = 10;
        public const uint GSM = 11;
        public const uint LTW = 12;
        public const uint WVR = 13;
        public const uint ALC = 14;
        public const uint CUL = 15;

        public const uint MIN = 16;
        public const uint BTN = 17;
        public const uint FSH = 18;
    }
}
