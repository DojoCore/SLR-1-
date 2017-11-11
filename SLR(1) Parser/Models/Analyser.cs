using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLR_1__Parser.Models
{
    public class Analyser
    {
        public struct Block
        {
            public enum Kinds { NULL, ACTION_S, ACTION_R, GOTO, ACC };

            public Kinds Kind { get; set; }

            public int Value { get; set; }

            public override string ToString()
            {
                return $"{Kind.ToString()} {Value}";
            }
        }

        private Grammar grammar;

        private Grammar pGrammar;

        private List<Function> flist = new List<Function>();

        private List<State> slist = new List<State>();

        private Dictionary<State, Block[]> table = new Dictionary<State, Block[]>();

        public List<Rule> Rules { get { return grammar.Rules; } }

        public Dictionary<State, Block[]> Table { get { return table; } }

        public List<Symbol> Symbols { get { return grammar.Symbols; } }
        
        public Analyser(string filename)
        {
            grammar = new Grammar(filename);

            pGrammar = new Grammar(grammar);
        }

        public void Generate()
        {
            GenerateDFA();

            GenerateTable();
        }

        /// <summary>
        /// 将输入字符串转化成符号串
        /// </summary>
        /// <param name="input">符号串</param>
        /// <param name="v">输入字符串</param>
        private void Construct(Queue<Symbol> input, string v)
        {
            foreach (string s in v.Split(' '))
            {
                bool flag = true;
                foreach (var symbol in grammar.Symbols)
                {
                    if (symbol.Equals(s))
                    {
                        flag = false;
                        input.Enqueue(symbol);
                        break;
                    }
                }
                if (flag) throw new Exception("输入了未定义的符号");
            }
            input.Enqueue(Grammar.S);
        }

        /// <summary>
        /// 返回目标非终结符号的Follow集
        /// </summary>
        /// <param name="symbol">目标非终结符号</param>
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        private List<Symbol> Follow(Symbol symbol)
        {
            List<Symbol> result = new List<Symbol>();
            if (symbol == grammar.Symbols.First())
            {
                result.Add(Grammar.S);
            }
            else
            {
                foreach (var rule in grammar.Rules.Where(r => r.Expression.Contains(symbol)))
                {
                    var sIndex = rule.Expression.IndexOf(symbol);
                    if (++sIndex < rule.Expression.Count)
                    {
                        if (rule.Expression[sIndex].Kind == "O")
                        {
                            result.Add(rule.Expression[sIndex]);
                        }
                        else
                        {
                            result.AddRange(First(rule.Expression[sIndex]));
                        }
                    }
                    else
                    {
                        if (rule.Define != symbol)
                        {
                            result.AddRange(Follow(rule.Define));
                        }
                    }
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// 返回非终结符号的First集
        /// </summary>
        /// <param name="symbol">目标非终结符号</param>
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        private List<Symbol> First(Symbol symbol)
        {
            var result = new List<Symbol>();
            foreach (var rule in grammar.Rules.Where(r => r.Define.Equals(symbol)))
            {
                if (rule.Expression.First().Kind == "O")
                {
                    result.Add(rule.Expression.First());
                }
                else if (rule.Expression.First().Kind == "S" && !rule.Expression.First().Equals(symbol))
                {
                    result.AddRange(First(rule.Expression.First()));
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// 对目标文法生成DFA
        /// </summary>
        private void GenerateDFA()
        {
            foreach(var r in pGrammar.Rules)
            {
                r.Expression.Insert(0, Grammar.P);
            }

            int t = 1;
            var first = pGrammar.Rules.First();
            State state = Closure(new List<Rule>() { first });

            state.Name = "T0";
            slist.Add(state);
            for (int i = 0; i < slist.Count; i++)
            {
                List<Symbol> alist = new List<Symbol>();
                foreach (var rule in slist[i].Rules)
                {
                    var pIndex = rule.Expression.IndexOf(Grammar.P);
                    if (++pIndex < rule.Expression.Count)
                    {
                        alist.Add(rule.Expression[pIndex]);
                    }
                }

                foreach (var a in alist.Distinct())
                {
                    State newState = Move(slist[i], a);
                    if (newState.Rules.Count > 0)
                    {
                        if (slist.Contains(newState))
                        {
                            flist.Add(new Function()
                            {
                                From = slist[i],
                                To = slist.Where(s => s.Equals(newState)).Single(),
                                By = a
                            });
                        }
                        else
                        {
                            newState.Name = $"T{t}";
                            t++;
                            slist.Add(newState);
                            flist.Add(new Function()
                            {
                                From = slist[i],
                                To = newState,
                                By = a
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 对目标规则集求闭包，直至集合不再增大
        /// </summary>
        /// <param name="rules">目标规则集</param>
        /// <returns></returns>
        private State Closure(List<Rule> rules)
        {
            State state = new State();
            state.Rules.AddRange(rules);
            int i = -1;
            while (state.Rules.Count != i)
            {
                i = state.Rules.Count;
                foreach (var rule in state.Rules)
                {
                    var pIndex = rule.Expression.IndexOf(Grammar.P);
                    if (++pIndex < rule.Expression.Count)
                    {
                        var symbol = rule.Expression[pIndex];
                        if (symbol.Kind == "S")
                        {
                            List<Rule> temp = Expand(symbol)
                                .Where(t => !state.Rules.Contains(t)).ToList();
                            if (temp.Count > 0)
                            {
                                state.Rules.AddRange(temp);
                                break;
                            }
                        }
                    }
                }
            }
            return state;
        }

        /// <summary>
        /// 对目标非终结符号进行自展，直至集合不再增大
        /// </summary>
        /// <param name="symbol">目标非终结符号</param>
        /// <returns></returns>
        private List<Rule> Expand(Symbol symbol)
        {
            List<Rule> result = new List<Rule>();
            foreach (var rule in pGrammar.Rules.Where(r => r.Define.Equals(symbol)))
            {
                result.Add(rule);
            }
            return result;
        }

        /// <summary>
        /// 状态经由输入符号，生成转换后的状态
        /// </summary>
        /// <param name="state">源状态</param>
        /// <param name="a">输入符号</param>
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        public State Move(State state, Symbol a)
        {
            State result = new State();

            foreach (var rule in state.Rules)
            {
                var pIndex = rule.Expression.IndexOf(Grammar.P);
                if (++pIndex < rule.Expression.Count && rule.Expression[pIndex].Equals(a))
                {
                    var copyRule = new Rule();
                    var copy = new List<Symbol>(rule.Expression.ToArray());
                    copy.Insert(pIndex + 1, Grammar.P);
                    copy.Remove(Grammar.P);
                    copyRule.Define = rule.Define;
                    copyRule.Expression = copy;
                    result.Rules.AddRange(Closure(new List<Rule>() { copyRule }).Rules);
                }
            }

            var temp = result.Rules.Distinct().ToList();
            result.Rules.Clear();
            result.Rules.AddRange(temp);
            return result;
        }

        /// <summary>
        /// 生成分析表
        /// </summary>
        /// <param name="table">分析表</param>
        /// <param name="slist">DFA状态集</param>
        /// <param name="flist">DFA函数集</param>
        /// <param name="grammar">已插入[Grammar.P]的文法</param>
        /// <param name="copy">源文法</param>
        private void GenerateTable()
        {
            foreach (var s in slist)
            {
                table.Add(s, new Block[grammar.Symbols.Count]);
                if (s.Rules.All(r => r.Expression.Last() == Grammar.P))
                {
                    foreach (var rule in s.Rules)
                    {
                        var copyExpression = new List<Symbol>(rule.Expression.ToArray());
                        copyExpression.Remove(Grammar.P);
                        var index = grammar.Rules.FindIndex(
                            t => t.Expression.All(copyExpression.Contains) && t.Expression.Count == copyExpression.Count);
                        var follow = Follow(rule.Define);
                        foreach (var f in follow)
                        {
                            table[s][grammar.Symbols.IndexOf(f)].Kind = Block.Kinds.ACTION_R;
                            table[s][grammar.Symbols.IndexOf(f)].Value = index;
                        }
                    }
                }
            }
            foreach (var f in flist)
            {
                switch (f.By.Kind)
                {
                    case "O":
                        foreach (var r in f.From.Rules.Where(ru => ru.Expression.Last() != Grammar.P))
                        {
                            table[f.From][grammar.Symbols.IndexOf(f.By)].Kind = Block.Kinds.ACTION_S;
                            table[f.From][grammar.Symbols.IndexOf(f.By)].Value = slist.IndexOf(f.To);
                        }

                        foreach (var r in f.From.Rules.Where(ru => ru.Expression.Last() == Grammar.P))
                        {
                            var copyExpression = new List<Symbol>(r.Expression.ToArray());
                            copyExpression.Remove(Grammar.P);
                            var index = grammar.Rules.FindIndex(
                                t => t.Expression.All(copyExpression.Contains) && t.Expression.Count == copyExpression.Count);
                            List<Symbol> follow = Follow(r.Define);
                            foreach (var fs in follow)
                            {
                                if(table[f.From][grammar.Symbols.IndexOf(fs)].Kind != Block.Kinds.NULL)
                                {
                                    throw new Exception("该文法不是SLR(1)文法");
                                }
                                table[f.From][grammar.Symbols.IndexOf(fs)].Kind = Block.Kinds.ACTION_R;
                                table[f.From][grammar.Symbols.IndexOf(fs)].Value = index;
                            }
                        }
                        break;
                    case "S":
                        table[f.From][grammar.Symbols.IndexOf(f.By)].Kind = Block.Kinds.GOTO;
                        table[f.From][grammar.Symbols.IndexOf(f.By)].Value = slist.IndexOf(f.To);
                        break;
                    default:
                        table[f.From][grammar.Symbols.IndexOf(f.By)].Kind = Block.Kinds.NULL;
                        break;
                }
            }
            table[slist[1]][grammar.Symbols.IndexOf(Grammar.S)].Kind = Block.Kinds.ACC;
        }

        /// <summary>
        /// 分析输入串是否为文法句子
        /// </summary>
        /// <param name="v">输入串</param>
        /// <param name="output">输出规约顺序</param>
        /// <param name="grammar">文法</param>
        /// <param name="copy">文法副本</param>
        /// <param name="slist">状态集</param>
        /// <param name="table">分析表</param>
        /// <returns>分析是否成功</returns>
        public bool Analyse(string v, List<string> output)
        {
            Stack<Symbol> symbolStack = new Stack<Symbol>();
            Stack<State> stateStack = new Stack<State>();
            Queue<Symbol> input = new Queue<Symbol>();
            output.Add(String.Format("{0,-16}{1,-10}{2,10}", "State", "Analyser", "Input"));
            bool acc = false;

            symbolStack.Push(Grammar.S);
            stateStack.Push(slist.Find(t => t.Name == "T0"));
            try
            {
                Construct(input, v);
            }
            catch (Exception e)
            {
                output.Add(e.Message);
                return false;
            }

            while (true)
            {
                var sb = new StringBuilder();
                var inputSymbol = input.Peek();
                var topState = stateStack.Peek();
                var block = table[topState][grammar.Symbols.IndexOf(inputSymbol)];
                switch (block.Kind)
                {
                    case Block.Kinds.ACTION_S:
                        stateStack.Push(slist[block.Value]);
                        symbolStack.Push(inputSymbol);
                        input.Dequeue();
                        break;
                    case Block.Kinds.ACTION_R:
                        var rule = grammar.Rules[block.Value];
                        var toDelete = rule.Expression.Count;
                        while (toDelete-- > 0)
                        {
                            symbolStack.Pop();
                            stateStack.Pop();
                        }
                        symbolStack.Push(rule.Define);
                        var nextState = stateStack.Peek();
                        var nextSymbol = symbolStack.Peek();
                        var b = table[nextState][grammar.Symbols.IndexOf(nextSymbol)];
                        if (b.Kind != Block.Kinds.GOTO)
                        {
                            goto FinalCase;
                        }
                        else
                        {
                            stateStack.Push(slist[b.Value]);
                        }
                        break;
                    case Block.Kinds.ACC:
                        acc = true;
                        goto FinalCase;
                    default:
                        goto FinalCase;
                }
                
                foreach(var s in stateStack.Reverse())
                {
                    sb.Append(s.Name);
                }
                var stateStr = sb.ToString();
                sb.Clear();
                foreach(var s in symbolStack.Reverse())
                {
                    sb.Append(s);
                }
                var symbolStr = sb.ToString();
                sb.Clear();
                foreach(var s in input)
                {
                    sb.Append(s);
                }
                output.Add(String.Format("{0,-16}{1,-10}{2,10}", stateStr, symbolStr, sb.ToString()));
            }

            FinalCase:
            if (!acc)
            {
                output.Add("Failed");
                output.Add("输入不是该文法的句子");
            }
            else
            {
                output.Add("ACC");
                output.Add("输入是该文法的句子");
            }
            return acc;
        }
    }
}
