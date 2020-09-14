using System;
using System.Collections.Generic;
using System.Linq;
using StdOttStandard;

namespace TimetableFH.Models
{
    public struct EventBase
    {
        /* Names: BETRGL, CommEng, DATSTR, DBDESIGN, DBDESIGN_G.AP147.BZ7, "Einf�hrung, Hausf�hrung",
         *        INFO, KONFIG, KONFIG_Repetitorium, MATH1, MATH2, MATH2_A bis M, MATH2_G.AP147.BZ7, 
         *        MATH2_N bis Z, Mentoring BSD, PMuAR, PROFENG, PROG1, PROG2_A bis M, PROG2_N bis Z, 
         *        PROG2_objektorientierte Progr., PROG2_objektorientierte Program, "Recruiting Day, Audimax", 
         *        RelDB, WEBTECH, WEBTECH_A bis M, WEBTECH_N bis Z, �KOGL
         */
        // Gropus: PA, PAM, B1, B2, M1, M2, VO, UE, SO, PAB

        public DateTime Begin { get; }

        public DateTime End { get; }

        public string Name { get; }

        public string Professor { get; }

        public string Room { get; }

        public string Group { get; }

        public EventBase(Dictionary<string, string> dict)
        {
            string[] betreffs = dict["Betreff"].Split(" / ").Select(p => p.Trim()).ToArray();
            DateTime beginDate = DateTime.Parse(dict["Beginnt am"]);
            TimeSpan beginTime = TimeSpan.Parse(dict["Beginnt um"]);
            DateTime endDate = DateTime.Parse(dict["Endet am"]);
            TimeSpan endTime = TimeSpan.Parse(dict["Endet um"]);
            string room = dict["Ort"];

            Begin = beginDate.Add(beginTime);
            End = endDate.Add(endTime);
            Name = betreffs[0];
            Professor = betreffs[1];
            Group = betreffs[2];
            Room = room;
        }
    }
}
