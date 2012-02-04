using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using LLCompiler;
using LLCompiler.SemanticAnalyzer;
using LLCompiler.Lexer;
using LLCompiler.Parser;
using LLCompiler.CodeGenerator;

namespace LL_Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void bQuitSobSob_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        bool isCompiled = false;
        bool isSaved = false;


        string FileToCompile = "";
        string compiledString = "";



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isCompiled && !isSaved)
            {
                var res = MessageBox.Show("You have unsaved file opened. Do you wanna fix that?", "LLC", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.Yes:
                        e.Cancel = ! TrySaveMeDude();
                        return;
                    default:
                        return;
                }
            }
            else
            {
                var res = MessageBox.Show("Do you reeeeeallly wanna quit?", "LLC", MessageBoxButton.YesNo, MessageBoxImage.Hand);
                if (res == MessageBoxResult.No) e.Cancel = true;
            }
        }

        bool TrySaveMeDude()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();

            sfd.Filter = "C++ Source File (*.cpp)|*.cpp|All Files (*.*)|*.*";
            var res = sfd.ShowDialog();

            if (res != true) return false;

            System.IO.File.WriteAllText(sfd.FileName, "#include <lili/lilib.h>\n\n");

            System.IO.File.AppendAllText(sfd.FileName, compiledString);

            var p = new Process();
            p.StartInfo.Arguments = "-A2SNYUfpOk2W2 " + sfd.FileName;
            p.StartInfo.FileName = "astyle.exe";
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();

            return true;

        }

        private void bOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Filter = "Little Lisp Source File (*.ll)|*.ll|All Files (*.*)|*.*";
            var res = ofd.ShowDialog();

            if (res != true) return;

            System.IO.File.ReadAllText(ofd.FileName);

            FileToCompile = ofd.FileName;
            
            bCompile.IsEnabled = true;
            bSave.IsEnabled = false;
        }                                                                 

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            TrySaveMeDude();
            isSaved = true;                       
        }

        private void bCompile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SemanticAnalyzer an = new SemanticAnalyzer();

                var st = System.IO.File.ReadAllText(FileToCompile);                                                 
                var tokens = Lexer.ProcessString(st);
                var values = Parser.ProcessTokens(tokens);

                an.CreateSymbolTable(values);
                an.DeriveTypes();
                an.ValidateFuncCalls();

                CodeGenerator cg = new CodeGenerator(an.FuncTable);
                cg.GenerateCFunctions();
                compiledString = cg.WriteCFunctionsToString();

                ErrorMessage.Text = "";
                isCompiled = true;
                bCompile.IsEnabled = true;
                bSave.IsEnabled = true;
            }
            catch (Exception ee)
            {
                ErrorMessage.Text = ee.Message;
            }
        }
    }
}
