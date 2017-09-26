using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RubikTimer
{
    public class Statistic : IComparable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void UpdateProperty(string propertyname) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname)); }

        private TimeSpan _solvetime;
        public TimeSpan SolveTime { get { return _solvetime; } set { _solvetime = value; UpdateProperty("SolveTime"); } }
        private string _info;
        public string Info { get { return _info; } set { _info = value; UpdateProperty("Info"); } }

        public int CompareTo(object obj) { return SolveTime.CompareTo((obj as Statistic).SolveTime); }

        public Statistic(TimeSpan solvetime, string info)
        {
            SolveTime = solvetime;
            Info = info;
        }

        public static Statistic operator +(Statistic s1, Statistic s2) { return new Statistic(s1.SolveTime + s2.SolveTime, ""); }
        public static Statistic operator +(Statistic s1, int k) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks + k), ""); }
        public static Statistic operator -(Statistic s1, Statistic s2) { return new Statistic(s1.SolveTime - s2.SolveTime, ""); }
        public static Statistic operator -(Statistic s1, int k) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks - k), ""); }
        public static Statistic operator *(Statistic s1, Statistic s2) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks * s2.SolveTime.Ticks), ""); }
        public static Statistic operator *(Statistic s1, int k) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks * k), ""); }
        public static Statistic operator /(Statistic s1, int k) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks / k), ""); }
        public static Statistic operator /(Statistic s1, Statistic s2) { return new Statistic(new TimeSpan(s1.SolveTime.Ticks / s2.SolveTime.Ticks), ""); }

        public static bool operator ==(Statistic s1, Statistic s2)
        {
            if ((object)s1 == null && (object)s2 == null) return true;
            if ((object)s1 == null || (object)s2 == null) return false;
            return s1.Info == s2.Info && s1.SolveTime.Ticks == s2.SolveTime.Ticks;
        }
        public static bool operator !=(Statistic s1, Statistic s2) { return !(s1 == s2); }
        public static bool operator <(Statistic s1, Statistic s2) { return s1.CompareTo(s2) < 0; }
        public static bool operator >(Statistic s1, Statistic s2) { return s1.CompareTo(s2) > 0; }
        public static bool operator <=(Statistic s1, Statistic s2) { return !(s1 > s2); }
        public static bool operator >=(Statistic s1, Statistic s2) { return !(s1 < s2); }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType() && this == (Statistic)obj;
        }

        public override int GetHashCode() { return SolveTime.GetHashCode() ^ Info.GetHashCode(); }
        public override string ToString() { return "Solved in " + SolveTime.ToString(@"h\:m\:s\.fff") + ((Info == "") ? "" : " - ") + Info; }
    }
}