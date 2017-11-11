using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLR_1__Parser.Models
{
    public class Grammar
    {
        public static Symbol P { get; } = new Symbol() { Kind = "O", Value = "[Point]" };

        public static Symbol S { get; } = new Symbol() { Kind = "O", Value = "[Sharp]" };

        public List<Rule> Rules { get; private set; } = new List<Rule>();

        public List<Symbol> Symbols { get; private set; } = new List<Symbol>();

        public Grammar(string filename)
        {
            var json = File.ReadAllText(filename);
            Rules.AddRange(Rule.FromJson(json));

            List<Symbol> temp = new List<Symbol>();
            foreach (var rule in Rules)
            {
                temp.Add(rule.Define);
                temp.AddRange(rule.Expression);
            }
            Symbols.AddRange(temp.Distinct().ToList());
            Symbols.Add(Grammar.S);
        }

        public Grammar(Grammar grammar)
        {
            foreach (var rule in grammar.Rules)
            {
                this.Rules.Add(new Rule(rule));
            }
            this.Symbols.AddRange(grammar.Symbols);
        }
    }
}
