﻿using System;
using System.Collections.Generic;

namespace JinxMod.Modules
{
    internal static class Helpers
    {
        internal const string agilePrefix = "<style=cIsUtility>Agile.</style> ";

        internal const string shockingPrefix = "<style=cIsDamage>Shocking.</style> ";

        internal static string ScepterDescription(string desc)
        {
            return "\n<color=#d299ff>SCEPTER: " + desc + "</color>";
        }

        public static T[] Append<T>(ref T[] array, List<T> list)
        {
            var orig = array.Length;
            var added = list.Count;
            Array.Resize<T>(ref array, orig + added);
            list.CopyTo(array, orig);
            return array;
        }
        public static Func<T[], T[]> AppendDel<T>(List<T> list) => (r) => Append(ref r, list);

        internal static string colorText(string v1, string v2)
        {
            throw new NotImplementedException();
        }
    }
}