using StdOttStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace TimetableFH
{
    public class EventBase : INotifyPropertyChanged
    {
        /* Names: BETRGL, CommEng, DATSTR, DBDESIGN, DBDESIGN_G.AP147.BZ7, "Einf�hrung, Hausf�hrung",
         *        INFO, KONFIG, KONFIG_Repetitorium, MATH1, MATH2, MATH2_A bis M, MATH2_G.AP147.BZ7, 
         *        MATH2_N bis Z, Mentoring BSD, PMuAR, PROFENG, PROG1, PROG2_A bis M, PROG2_N bis Z, 
         *        PROG2_objektorientierte Progr., PROG2_objektorientierte Program, "Recruiting Day, Audimax", 
         *        RelDB, WEBTECH, WEBTECH_A bis M, WEBTECH_N bis Z, �KOGL
         */
        // Gropus: PA, PAM, B1, B2, M1, M2, VO, UE, SO, PAB

        private DateTime begin, end;
        private string name, professor, room, group;

        public long BeginTicks
        {
            get { return Begin.Ticks; }
            set { Begin = new DateTime(value); }
        }

        [XmlIgnore]
        public DateTime Begin
        {
            get { return begin; }
            set
            {
                if (value == begin) return;

                begin = value;
                OnPropertyChanged(nameof(Begin));
            }
        }

        public long EndTicks
        {
            get { return End.Ticks; }
            set { End = new DateTime(value); }
        }

        public DateTime End
        {
            get { return end; }
            set
            {
                if (value == end) return;

                end = value;
                OnPropertyChanged(nameof(End));
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;

                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Professor
        {
            get { return professor; }
            set
            {
                if (value == professor) return;

                professor = value;
                OnPropertyChanged(nameof(Professor));
            }
        }

        public string Room
        {
            get { return room; }
            set
            {
                if (value == room) return;

                room = value;
                OnPropertyChanged(nameof(Room));
            }
        }

        public string Group
        {
            get { return group; }
            set
            {
                if (value == group) return;

                group = value;
                OnPropertyChanged(nameof(Group));
            }
        }

        public EventBase()
        {
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
