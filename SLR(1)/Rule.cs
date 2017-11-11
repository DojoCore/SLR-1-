using System.Collections.Generic;

using Newtonsoft.Json;
using System.Text;

namespace SLR_1_
{
    public partial class Rule
    {
        [JsonProperty("define")]
        public Symbol Define { get; set; }

        [JsonProperty("as")]
        public List<Symbol> Expression { get; set; }

        public Rule() { }

        public Rule(Rule rule)
        {
            this.Define = rule.Define;
            this.Expression = new List<Symbol>(rule.Expression.ToArray());
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if(obj.GetType() == typeof(Rule))
            {
                var rule = (Rule)obj;
                if (rule.Define == this.Define)
                {
                    if(rule.Expression.Count == this.Expression.Count)
                    {
                        for (int i = 0; i < rule.Expression.Count; i++)
                        {
                            if (rule.Expression[i] != this.Expression[i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Define} → ");
            foreach(var s in Expression)
            {
                sb.Append($"{s}");
            }
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 915180621;
            hashCode = hashCode * -1521134295 + EqualityComparer<Symbol>.Default.GetHashCode(Define);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Symbol>>.Default.GetHashCode(Expression);
            return hashCode;
        }
    }

    public partial class Symbol
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if(obj.GetType() == typeof(Symbol))
            {
                return ((Symbol)obj).Value == this.Value;
            }
            if(obj.GetType() == typeof(string))
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
            return this.Value;
        }
    }

    public partial class Rule
    {
        public static List<Rule> FromJson(string json) => JsonConvert.DeserializeObject<List<Rule>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Grammar> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
