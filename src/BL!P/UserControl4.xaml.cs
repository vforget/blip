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
using MBF;
using MBF.IO.Fasta;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl4.xaml
    /// </summary>
    public partial class UserControl4 : UserControl
    {
        public double MinPercentId { get; set; }
        public double MinQueryCoverage { get; set; }
        public double MaxEvalue { get; set; }
        public int MaxNumHits { get; set; }
        private String[] proteinDatabases = new string[] {"nr", "refseq_protein", "swissprot", "pataa", "pdbaa", "env_nr"};
        private String[] nucleotideDatabases = new string[] {"nt", "refseq_rna", "refseq_genomic", "est", "est_human", "est_mouse", "est_others", "gss", "htgs", "patnt", "pdbnt", "month", "dbsts", "chromosome", "wgs", "env_nt" };
        private String[] proteinAlgorithms = new String[] { "plain", "psi", "phi" };
        private String[] nucleotideAlgorithms = new String[] { "plain" }; //, "megablast" };

        UIParameters Up = new UIParameters();
        
        public UserControl4(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl4";
            Up = up;
            BlastDatabase.ItemsSource = proteinDatabases;
            BlastAlgorithm.ItemsSource = proteinAlgorithms;
            BlastDatabase.SelectedIndex = 0;
            BlastAlgorithm.SelectedIndex = 0;
            
        }

        // Set BLAST parameters to the defaults from the UI.
        private void UserControl4_Loaded(object sender, RoutedEventArgs e)
        {
            ISequence s = Up.QuerySequences[0];
            if (s.Alphabet.Name == Alphabets.DNA.Name)
            {
                BlastProgram.SelectedIndex = 0;
            }
            if (s.Alphabet.Name == Alphabets.Protein.Name)
            {
                BlastProgram.SelectedIndex = 1;
            }

            Up.BlastMaxEvalue = Math.Pow(10, EvalueSlider.Value);
            Up.BlastMinPercentIdentity = PercentIdSlider.Value;
            Up.BlastMinPercentQueryCoverage = QueryCoverageSlider.Value;
            Up.BlastMaxNumHits = Convert.ToInt32(NumTopHitsSlider.Value);
        }

        // Disable genetic code if BLASTp is selected
        private void BlastProgram_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BlastGeneticCode != null){
                string blastProgram = ((ComboBoxItem)BlastProgram.SelectedItem).Content.ToString();

                ISequence s = Up.QuerySequences[0];
                if ((s.Alphabet.Name == Alphabets.DNA.Name) && ((blastProgram == "blastp") || (blastProgram == "tblastn")))
                {
                    MessageBox.Show("The selected BLAST program does not allow DNA query sequences. Reverting to BLASTn.");
                    BlastProgram.SelectedIndex = 0;
                }
                if ((s.Alphabet.Name == Alphabets.Protein.Name) && ((blastProgram == "blastn") || (blastProgram == "blastx") || (blastProgram == "tblastx")))
                {
                    MessageBox.Show("The selected BLAST program does not allow protein query sequences. Reverting to BLASTp.");
                    BlastProgram.SelectedIndex = 1;
                }
                
                
                switch (blastProgram)
                {
                    case "blastn":
                        BlastGeneticCode.IsEnabled = false;
                        BlastDatabase.ItemsSource = nucleotideDatabases;
                        BlastAlgorithm.ItemsSource = nucleotideAlgorithms;
                        BlastDatabase.SelectedIndex = 0;
                        BlastAlgorithm.SelectedIndex = 0;
                        break;
                    case "blastp":
                        BlastGeneticCode.IsEnabled = false;
                        BlastDatabase.ItemsSource = proteinDatabases;
                        BlastAlgorithm.ItemsSource = proteinAlgorithms;
                        BlastDatabase.SelectedIndex = 0;
                        BlastAlgorithm.SelectedIndex = 0;
                        break;
                    case "blastx":
                        BlastGeneticCode.IsEnabled = true;
                        BlastDatabase.ItemsSource = proteinDatabases;
                        BlastAlgorithm.ItemsSource = proteinAlgorithms;
                        BlastDatabase.SelectedIndex = 0;
                        BlastAlgorithm.SelectedIndex = 0;
                        break;
                    case "tblastn":
                        BlastGeneticCode.IsEnabled = false;
                        BlastDatabase.ItemsSource = nucleotideDatabases;
                        BlastAlgorithm.ItemsSource = nucleotideAlgorithms;
                        BlastDatabase.SelectedIndex = 0;
                        BlastAlgorithm.SelectedIndex = 0;
                        break;
                    case "tblastx":
                        BlastGeneticCode.IsEnabled = true;
                        BlastDatabase.ItemsSource = nucleotideDatabases;
                        BlastAlgorithm.ItemsSource = nucleotideAlgorithms;
                        BlastDatabase.SelectedIndex = 0;
                        BlastAlgorithm.SelectedIndex = 0;
                        break;
                    default:
                        break;

                }
            }
        }

        
    }
}
