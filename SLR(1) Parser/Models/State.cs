using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLR_1__Parser.Models
{
    public class State
    {
        public string Name { get; set; }

        public List<Rule> Rules { get; private set; } = new List<Rule>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"State:{Name}");
            foreach (var r in Rules)
            {
                sb.AppendLine($"  {r.ToString()}");
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj.GetType() == typeof(State))
            {
                return ((State)obj).Rules.All(this.Rules.Contains) && ((State)obj).Rules.Count == this.Rules.Count;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 2045102464 + EqualityComparer<List<Rule>>.Default.GetHashCode(Rules);
        }
    }
}
