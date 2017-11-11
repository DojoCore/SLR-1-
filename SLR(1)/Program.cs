using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLR_1_
{
    /// <summary>
    /// DFA状态单元
    /// </summary>
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
                //if (((State)obj).Rules.Count != this.Rules.Count)
                //{
                //    return false;
                //}
                //else
                //{
                //    for (int i = 0; i < this.Rules.Count; i++)
                //    {
                //        if (this.Rules[i] != ((State)obj).Rules[i])
                //        {
                //            return false;
                //        }
                //    }
                //    return true;
                //}
                return ((State)obj).Rules.All(this.Rules.Contains) && ((State)obj).Rules.Count == this.Rules.Count;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 2045102464 + EqualityComparer<List<Rule>>.Default.GetHashCode(Rules);
        }
    }

    /// <summary>
    /// DFA关系单元
    /// </summary>
    public class Function
    {
        public State From { get; set; }
        public State To { get; set; }
        public Symbol By { get; set; }

        public override string ToString()
        {
            return From.Name + " --" + By.Value + "-> " + To.Name;
        }
    }

    /// <summary>
    /// 分析表项
    /// </summary>
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

    class Program
    {
        static void Main(string[] args)
        {
            #region 读取，显示文法
            Console.WriteLine("文法文件：");
            var filename = Console.ReadLine();

            Grammar grammar = new Grammar(filename);
            Grammar copy = new Grammar(grammar);

            Console.WriteLine("文法：");
            Console.WriteLine("符号:");
            foreach (var symbol in grammar.Symbols)
            {
                Console.Write($"{symbol}  ");
            }
            Console.WriteLine("\n规则:");
            foreach (var rule in grammar.Rules)
            {
                Console.WriteLine(rule);
                rule.Expression.Insert(0, Grammar.P);
            }
            Console.WriteLine();
            #endregion

            #region DFA
            Console.WriteLine("DFA:");
            var flist = new List<Function>();
            var slist = new List<State>();

            GenerateDFA(grammar, flist, slist);

            foreach (var f in flist)
            {
                Console.WriteLine(f);
            }
            Console.WriteLine();
            #endregion

            #region 生成分析表
            Console.WriteLine("SLR(1)分析表:");
            var table = new Dictionary<State, Block[]>();
            foreach (var s in slist)
            {
                table.Add(s, new Block[grammar.Symbols.Count]);
                if (s.Rules.All(r => r.Expression.Last() == Grammar.P))
                {
                    foreach (var rule in s.Rules)
                    {
                        var copyExpression = new List<Symbol>(rule.Expression.ToArray());
                        copyExpression.Remove(Grammar.P);
                        var index = copy.Rules.FindIndex(
                            t => t.Expression.All(copyExpression.Contains) && t.Expression.Count == copyExpression.Count);
                        var follow = Follow(rule.Define, copy);
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
                            var index = copy.Rules.FindIndex(
                                t => t.Expression.All(copyExpression.Contains) && t.Expression.Count == copyExpression.Count);
                            List<Symbol> follow = Follow(r.Define, copy);
                            foreach (var fs in follow)
                            {
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

            Console.Write("   |");
            foreach (var s in grammar.Symbols)
            {
                Console.Write("{0,10}|", s);
            }
            Console.WriteLine();
            foreach (var s in slist)
            {
                Console.Write("{0,3}|", s.Name);
                foreach (var b in table[s])
                {
                    Console.Write("{0,8}{1,2}|", b.Kind, b.Value);
                }
                Console.WriteLine();
            }
            #endregion

            #region 语法分析
            while (true)
            {
                Console.Write("\n输入：");
                var v = Console.ReadLine();
                if (v == "")
                    break;

                Stack<Symbol> symbolStack = new Stack<Symbol>();
                Stack<State> stateStack = new Stack<State>();
                Queue<Symbol> input = new Queue<Symbol>();

                symbolStack.Push(Grammar.S);
                stateStack.Push(slist.Find(t => t.Name == "T0"));
                try
                {
                    Construct(input, grammar, v);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                bool acc = false;
                while (true)
                {
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
                            var rule = copy.Rules[block.Value];
                            var toDelete = rule.Expression.Count;
                            var tempStack = new Stack<Symbol>();
                            while (toDelete-- > 0)
                            {
                                tempStack.Push(symbolStack.Pop());
                                stateStack.Pop();
                            }
                            while (tempStack.Count > 0)
                            {
                                Console.Write($"{tempStack.Pop()}");
                            }
                            symbolStack.Push(rule.Define);
                            Console.WriteLine($" -> {rule.Define}");
                            var nextState = stateStack.Peek();
                            var nextSymbol = symbolStack.Peek();
                            var b = table[nextState][grammar.Symbols.IndexOf(nextSymbol)];
                            if (b.Kind != Block.Kinds.GOTO)
                            {
                                goto FailCase;
                            }
                            else
                            {
                                stateStack.Push(slist[b.Value]);
                            }
                            break;
                        case Block.Kinds.ACC:
                            acc = true;
                            goto FailCase;
                        default:
                            goto FailCase;
                    }
                }

                FailCase:
                if (!acc)
                {
                    Console.WriteLine("输入不是该文法的句子");
                }
                else
                {
                    Console.WriteLine("输入是该文法的句子");
                }
            }

            #endregion

            Console.ReadKey();
        }

        /// <summary>
        /// 将输入字符串转化成符号串
        /// </summary>
        /// <param name="input">符号串</param>
        /// <param name="grammar">文法</param>
        /// <param name="v">输入字符串</param>
        private static void Construct(Queue<Symbol> input, Grammar grammar, string v)
        {
            var words = v.Split(' ');
            foreach(var word in words)
            {
                if(!grammar.Symbols.Exists(s => s.Value == word))
                {
                    throw new Exception("输入了未定义的符号");
                }
                var w = grammar.Symbols.Find(s => s.Value == word);
                input.Enqueue(w);
            }
            input.Enqueue(Grammar.S);
        }

        /// <summary>
        /// 返回某一非终结符号的Follow集
        /// </summary>
        /// <param name="symbol">非终结符号</param>
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        private static List<Symbol> Follow(Symbol symbol, Grammar grammar)
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
                        if(rule.Expression[sIndex].Kind == "O")
                        {
                            result.Add(rule.Expression[sIndex]);
                        }
                        else
                        {
                            result.AddRange(First(rule.Expression[sIndex], grammar));
                        }
                    }
                    else
                    {
                        if (rule.Define != symbol)
                        {
                            result.AddRange(Follow(rule.Define, grammar));
                        }
                    }
                }
            }
            return result.Distinct().ToList();
        }

        private static List<Symbol> First(Symbol symbol, Grammar grammar)
        {
            var result = new List<Symbol>();
            foreach(var rule in grammar.Rules.Where(r => r.Define.Equals(symbol)))
            {
                if(rule.Expression.First().Kind == "O")
                {
                    result.Add(rule.Expression.First());
                }
                else if(rule.Expression.First().Kind == "S" && !rule.Expression.First().Equals(symbol))
                {
                    result.AddRange(First(rule.Expression.First(), grammar));
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// 对目标文法生成DFA
        /// </summary>
        /// <param name="grammar">目标文法</param>
        /// <param name="functions">DFA函数集</param>
        /// <param name="states">DFA状态集</param>
        static void GenerateDFA(Grammar grammar, List<Function> functions, List<State> states)
        {
            int t = 1;
            var first = grammar.Rules.First();
            State state = Closure(new List<Rule>() { first }, grammar);

            state.Name = "T0";
            states.Add(state);
            for (int i = 0; i < states.Count; i++)
            {
                List<Symbol> alist = new List<Symbol>();
                foreach (var rule in states[i].Rules)
                {
                    var pIndex = rule.Expression.IndexOf(Grammar.P);
                    if (++pIndex < rule.Expression.Count)
                    {
                        alist.Add(rule.Expression[pIndex]);
                    }
                }

                foreach (var a in alist.Distinct())
                {
                    State newState = Move(states[i], a, grammar);
                    if (newState.Rules.Count > 0)
                    {
                        if (states.Contains(newState))
                        {
                            functions.Add(new Function()
                            {
                                From = states[i],
                                To = states.Where(s => s.Equals(newState)).Single(),
                                By = a
                            });
                        }
                        else
                        {
                            newState.Name = $"T{t}";
                            t++;
                            states.Add(newState);
                            functions.Add(new Function()
                            {
                                From = states[i],
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
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        private static State Closure(List<Rule> rules, Grammar grammar)
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
                            List<Rule> temp = Expand(symbol, grammar)
                                .Where(t => !state.Rules.Contains(t)).ToList();
                            if (temp.Count > 0)
                            {
                                state.Rules.AddRange(temp);
                                break;
                            }
                            //state.Rules.AddRange(temp);
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
        /// <param name="grammar">文法</param>
        /// <returns></returns>
        private static List<Rule> Expand(Symbol symbol, Grammar grammar)
        {
            List<Rule> result = new List<Rule>();
            foreach (var rule in grammar.Rules.Where(r => r.Define.Equals(symbol)))
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
        private static State Move(State state, Symbol a, Grammar grammar)
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
                    result.Rules.AddRange(Closure(new List<Rule>() { copyRule }, grammar).Rules);
                }
            }

            var temp = result.Rules.Distinct().ToList();
            result.Rules.Clear();
            result.Rules.AddRange(temp);
            return result;
        }
    }
}
