using Microsoft.Win32;
using SLR_1__Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SLR_1__Parser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Analyser analyser = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            analyser = null;

            lstGrammar.Items.Clear();

            dialog.Multiselect = false;

            dialog.ShowDialog();

            txtFile.Text = dialog.SafeFileName;
            
            try
            {
                analyser = new Analyser(dialog.FileName);
                foreach (var rule in analyser.Rules)
                {
                    lstGrammar.Items.Add(new ListViewItem() { Content = rule });
                }
            }
            catch(Exception ex)
            {
                analyser = null;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            lstAnalyse.Items.Clear();

            if(analyser == null)
            {
                MessageBox.Show("未成功生成文法分析");
                return;
            }

            var v = txtInput.Text;

            var output = new List<string>();

            var success = analyser.Analyse(v, output);

            foreach (var s in output.Take(output.Count - 1))
            {
                lstAnalyse.Items.Add(new ListViewItem() { Content = s });
            }

            MessageBox.Show(output.Last());
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if(analyser == null)
            {
                MessageBox.Show("未能成功生成文法分析");
                return;
            }

            try
            {
                analyser.Generate();
                MessageBox.Show("分析完成");
            }
            catch(StackOverflowException)
            {
                analyser = null;
                MessageBox.Show("文法不为SLR(1)文法");
            }
            catch(Exception ex)
            {
                analyser = null;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTable_Click(object sender, RoutedEventArgs e)
        {
            WinTable win = new WinTable();
            win.Show();
        }
    }
}
