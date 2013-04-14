using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBF;
using MBF.Web;
using MBF.Web.Blast;

namespace Blip
{
    class BlastUtil
    {

        public static void RecordBlastThresholds(UIParameters Up, UserControl4 uc4, string blastProgram, string blastDb, string blastAlgo)
        {
            Up.BlastProgram = blastProgram;
            Up.BlastDatabase = blastDb;
            Up.BlastAlgorithm = blastAlgo;
            Up.BlastGeneticCode = (uc4.BlastDatabase.SelectedIndex + 1).ToString();
            Up.BlastMinPercentQueryCoverage = uc4.QueryCoverageSlider.Value;
            Up.BlastMinPercentIdentity = uc4.PercentIdSlider.Value;
            Up.BlastMaxEvalue = Math.Pow(10, uc4.EvalueSlider.Value);
            Up.BlastMaxNumHits = Convert.ToInt32(uc4.NumTopHitsSlider.Value);
            
        }
        
        public static string ToString(Hit blastHit, int blastHspId)
        {
            Hsp hsp = blastHit.Hsps[blastHspId];
            string pa = "";
            pa += String.Format(">{0} {1}\r\n", blastHit.Id, blastHit.Def);
            pa += String.Format("Length={0}\r\n\r\n", blastHit.Length);
            pa += String.Format(" Score = {0} bits ({1}), Expect = {2}\r\n", hsp.BitScore, hsp.Score, hsp.EValue);
            int percentId = (int)Math.Round(((hsp.IdentitiesCount / (double)hsp.AlignmentLength) * 100.0), 0);
            int percentPos = (int)Math.Round(((hsp.PositivesCount / (double)hsp.AlignmentLength) * 100.0), 0);
            int percentGap = (int)Math.Round(((hsp.Gaps / (double)hsp.AlignmentLength) * 100.0), 0);
            pa += String.Format(" Identities = {0}/{1} ({2}%), Positives = {3}/{4} ({5}%), Gaps = {6}/{7} ({8}%)\r\n", hsp.IdentitiesCount,
                                                                                                                        hsp.AlignmentLength,
                                                                                                                        percentId,
                                                                                                                        hsp.PositivesCount,
                                                                                                                        hsp.AlignmentLength,
                                                                                                                        percentPos,
                                                                                                                        hsp.Gaps,
                                                                                                                        hsp.AlignmentLength,
                                                                                                                        percentGap);
            pa += String.Format(" Frame = {0}\r\n", hsp.HitFrame);
            pa += "\r\n";

            for (int k = 0; k < hsp.AlignmentLength; k += 60)
            {
                long tl = 60;
                if ((k + tl) > hsp.AlignmentLength)
                {
                    tl = hsp.AlignmentLength - k;
                }

                pa += String.Format("Query  {1,-5}  {0}  {2,-5}\r\n", hsp.QuerySequence.Substring(k, (int)tl), hsp.QueryStart + k, hsp.QueryStart + k + tl - 1);
                pa += String.Format("       {1,-5}  {0}  {2,-5}\r\n", hsp.Midline.Substring(k, (int)tl), "", "");
                pa += String.Format("Sbjct  {1,-5}  {0}  {2,-5}\r\n", hsp.HitSequence.Substring(k, (int)tl), hsp.HitStart + k, hsp.HitStart + k + tl - 1);
                pa += "\r\n";
            }
            return pa;
        }

        public static List<GenBankItem> filter(IList<BlastResult> blastResults, int maxHits = 1, double maxEvalue = 10, double minPercentId = 0.0, double minQueryCoverage = 0.0)
        {

            List<GenBankItem> giList = new List<GenBankItem>();
            
            foreach (BlastResult blastResult in blastResults)
            {
                foreach (BlastSearchRecord record in blastResult.Records)
                {
                    int numHits = 0;
                    for (int i = 0; i < record.Hits.Count; i++)
                    {
                        Hit blastHit = record.Hits[i];
                        long gi = Convert.ToInt64(blastHit.Id.Split('|')[1]);
                        for (int j = 0; j < blastHit.Hsps.Count; j++)
                        {

                            Hsp blastHsp = blastHit.Hsps[j];
                            double percentId = (blastHsp.IdentitiesCount / (double)blastHsp.AlignmentLength) * 100;
                            double queryCoverage = ((blastHsp.QueryEnd - blastHsp.QueryStart + 1) / (double)record.IterationQueryLength) * 100;
                            //double queryCoverage = (blastHsp.AlignmentLength / (double)queryLen) * 100;

                            if ((percentId >= minPercentId) && (maxEvalue >= blastHsp.EValue) && (queryCoverage >= minQueryCoverage))
                            {
                                if (numHits < maxHits)
                                {
                                        GenBankItem gitem = new GenBankItem(gi, blastHsp.HitStart, blastHsp.HitEnd);
                                        giList.Add(gitem);
                                }
                                else
                                {
                                    break;
                                }
                                numHits += 1;
                            }
                        }
                    }
                }
            }
            return giList;
        }
    }
}
