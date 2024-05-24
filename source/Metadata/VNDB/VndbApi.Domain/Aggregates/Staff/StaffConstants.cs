using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VndbApi.Domain.StaffAggregate
{
    /// <summary>
    /// Constants for staff-related fields.
    /// </summary>
    public static class StaffConstants
    {
        public static class Gender
        {
            public const string Male = "m";
            public const string Female = "f";
        }

        public static class Role
        {
            public const string Seiyuu = "seiyuu";
            public const string Scenario = "scenario";
            public const string Director = "director";
            public const string CharacterDesign = "chardesign";
            public const string Artist = "art";
            public const string Composer = "music";
            public const string Songs = "songs";
            public const string Translator = "translator";
            public const string Editor = "editor";
            public const string QualityAssurance = "qa";
            public const string Staff = "staff";
        }
    }
}