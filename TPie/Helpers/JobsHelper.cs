﻿using Lumina.Excel;
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

        private static int CompareActionsByName(LuminaAction a, LuminaAction b)
        {
            return a.Name.ToString().CompareTo(b.Name.ToString());
        }

        public bool ClassJobCategoryContainsJob(uint key, uint jobId)
        {
            switch (key)
            {
                case 1: return true;
                case 2: return jobId == JobIDs.GLD;
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
                case 18: return jobId == JobIDs.BTN;
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
                case 44: return jobId == JobIDs.WAR;
                case 47: return jobId == JobIDs.DRG;
                case 50: return jobId == JobIDs.BRD;
                case 53: return jobId == JobIDs.WHM;
                case 55: return jobId == JobIDs.BLM;
                case 59: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 61: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
                case 63: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU;
                case 64: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
                case 66: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 68: return jobId == JobIDs.SCH || jobId == JobIDs.SMN;
                case 69: return jobId == JobIDs.SMN;
                case 73: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
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
                case 114: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM;
                case 115: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 116: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU;
                case 117: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
                case 118: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM;
                case 121: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 122: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM;
                case 123: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 125: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
                case 129: return jobId == JobIDs.BLU;
                case 133: return jobId == JobIDs.WHM || jobId == JobIDs.SCH || jobId == JobIDs.AST;
                case 134: return jobId == JobIDs.PLD || jobId == JobIDs.WAR || jobId == JobIDs.DRK || jobId == JobIDs.GNB;
                case 139: return jobId == JobIDs.BRD || jobId == JobIDs.MCH || jobId == JobIDs.DNC;
                case 147: return jobId == JobIDs.BLM || jobId == JobIDs.SMN || jobId == JobIDs.RDM || jobId == JobIDs.BLU;
                case 148: return jobId == JobIDs.MNK || jobId == JobIDs.DRG || jobId == JobIDs.NIN || jobId == JobIDs.SAM;
                case 149: return jobId == JobIDs.GNB;
                case 150: return jobId == JobIDs.DNC;
                case 160: return jobId == JobIDs.SCH;
            }

            return false;
        }

        public static Dictionary<uint, string> JobNames = new Dictionary<uint, string>()
        {
            // tanks
            [JobIDs.GLD] = "GLD",
            [JobIDs.MRD] = "MRD",
            [JobIDs.PLD] = "PLD",
            [JobIDs.WAR] = "WAR",
            [JobIDs.DRK] = "DRK",
            [JobIDs.GNB] = "GNB",

            // melee dps
            [JobIDs.PGL] = "PGL",
            [JobIDs.LNC] = "LNC",
            [JobIDs.ROG] = "ROG",
            [JobIDs.MNK] = "MNK",
            [JobIDs.DRG] = "DRG",
            [JobIDs.NIN] = "NIN",
            [JobIDs.SAM] = "SAM",

            // ranged phys dps
            [JobIDs.ARC] = "ARC",
            [JobIDs.BRD] = "BRD",
            [JobIDs.MCH] = "MCH",
            [JobIDs.DNC] = "DNC",

            // ranged magic dps
            [JobIDs.THM] = "THM",
            [JobIDs.ACN] = "ACN",
            [JobIDs.BLM] = "BLM",
            [JobIDs.SMN] = "SMN",
            [JobIDs.RDM] = "RDM",
            [JobIDs.BLU] = "BLU",

            // healers
            [JobIDs.CNJ] = "CNJ",
            [JobIDs.WHM] = "WHM",
            [JobIDs.SCH] = "SCH",
            [JobIDs.AST] = "AST",

            // crafters
            [JobIDs.CRP] = "CRP",
            [JobIDs.BSM] = "BSM",
            [JobIDs.ARM] = "ARM",
            [JobIDs.GSM] = "GSM",
            [JobIDs.LTW] = "LTW",
            [JobIDs.WVR] = "WVR",
            [JobIDs.ALC] = "ALC",
            [JobIDs.CUL] = "CUL",

            // gatherers
            [JobIDs.MIN] = "MIN",
            [JobIDs.BTN] = "BTN",
            [JobIDs.FSH] = "FSH",
        };
    }

    internal static class JobIDs
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