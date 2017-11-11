using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SLR_1__Parser.Models;


namespace SLR_1__Parser
{
    /// <summary>
    /// WinTable.xaml 的交互逻辑
    /// </summary>
    public partial class WinTable : Window
    {
        public WinTable()
        {
            InitializeComponent();
            ShowTable();
        }

        public void ShowTable()
        {
            if(MainWindow.analyser == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("    ");
            foreach(var symbol in MainWindow.analyser.Symbols)
            {
                sb.Append($"{symbol,-4}");
            }
            sb.AppendLine();
            foreach (var key in MainWindow.analyser.Table.Keys)
            {
                sb.Append($"{key.Name,-4}");
                foreach (var val in MainWindow.analyser.Table[key])
                {
                    switch(val.Kind)
                    {
                        case Analyser.Block.Kinds.ACC:
                            sb.AppendFormat("{0, -4}", "ACC");
                            break;
                        case Analyser.Block.Kinds.ACTION_R:
                            sb.Append($"r{val.Value,-3}");
                            break;
                        case Analyser.Block.Kinds.ACTION_S:
                            sb.Append($"S{val.Value,-3}");
                            break;
                        case Analyser.Block.Kinds.GOTO:
                            sb.Append($"{val.Value,-4}");
                            break;
                        case Analyser.Block.Kinds.NULL:
                            sb.Append("    ");
                            break;
                    }
                }
                sb.AppendLine();
            }

            txtTable.Text = sb.ToString();
        }
    }
}
