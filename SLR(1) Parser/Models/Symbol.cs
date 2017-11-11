using Newtonsoft.Json;
using System.Collections.Generic;

namespace SLR_1__Parser.Models
{
    public partial class Symbol
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj.GetType() == typeof(Symbol))
            {
                return ((Symbol)obj).Value == this.Value;
            }
            if (obj.GetType() == typeof(string))
            {
                return (string)obj == this.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1484969029;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Kind);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public override string ToString()
        {
            if (this.Value == "[Sharp]") return "#";
            return this.Value;
        }
    }
}
