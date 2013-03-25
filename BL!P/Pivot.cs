using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MBF;
using MBF.IO.GenBank;
using MBF.Web.Blast;
using System.Diagnostics;

namespace Blip
{
    public static class Pivot
    {
        private static string GetNCBIUrl(string blastProgram)
        {
            string url = "";
            if ((blastProgram == "blastn") || (blastProgram == "tblastn") || (blastProgram == "tblastx"))
            {
                url = "http://www.ncbi.nlm.nih.gov/nuccore/";
                    
            }
            else
            {
                url = "http://www.ncbi.nlm.nih.gov/protein/";
            }
            return url;
        }
        
        public static void CreateFacetCategories(UIParameters Up)
        {
            // Query information
            Up.FacetCategories.Add(new FacetCategory("InputOrder", "Number", "0", true));
            Up.FacetCategories.Add(new FacetCategory("QueryLen", "Number", "0", true));
            FacetCategory fc = new FacetCategory("QueryName", "String", false);
            fc.IsMetaDataVisible = false;
            fc.IsWordWheelVisible = true;
            Up.FacetCategories.Add(fc);
            FacetCategory fc_seq = new FacetCategory("QuerySequence", "String", false);
            fc.IsMetaDataVisible = false;
            fc.IsWordWheelVisible = false;
            Up.FacetCategories.Add(fc_seq);

            // BLAST alignment information
            Up.FacetCategories.Add(new FacetCategory("Rank", "Number", "0", true));
            Up.FacetCategories.Add(new FacetCategory("Annotated", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Score", "Number", "0.0", true));
            Up.FacetCategories.Add(new FacetCategory("EValue", "String", false));
            Up.FacetCategories.Add(new FacetCategory("Identity", "Number", "0.00", true));
            Up.FacetCategories.Add(new FacetCategory("Span", "Number", "0.00", true));
            Up.FacetCategories.Add(new FacetCategory("AlignLen", "Link", false));
            Up.FacetCategories.Add(new FacetCategory("NextScore", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Strand", "String", false));
            Up.FacetCategories.Add(new FacetCategory("SubjStart", "Number", "0", false));
            Up.FacetCategories.Add(new FacetCategory("SubjLen", "Number", "0", false));
            // Gene information
            Up.FacetCategories.Add(new FacetCategory("Gene", "Link", false));
            Up.FacetCategories.Add(new FacetCategory("GI", "Link", false));
            Up.FacetCategories.Add(new FacetCategory("Accession", "Link", false));
            Up.FacetCategories.Add(new FacetCategory("Definition", "LongString", false));
            Up.FacetCategories.Add(new FacetCategory("Product", "LongString", false));
            Up.FacetCategories.Add(new FacetCategory("Function", "LongString", false));
            // Organism information
            Up.FacetCategories.Add(new FacetCategory("Lineage", "LongString", false));
            Up.FacetCategories.Add(new FacetCategory("Organism", "LongString", false));
            Up.FacetCategories.Add(new FacetCategory("Species", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Genus", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Kingdom", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Phylum", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Class", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Order", "String", true));
            Up.FacetCategories.Add(new FacetCategory("Family", "String", true));
            // References
            Up.FacetCategories.Add(new FacetCategory("RefCount", "Number", "0", true));
            fc = new FacetCategory("References", "Link", false);
            fc.IsMetaDataVisible = true;
            fc.IsWordWheelVisible = false;
            Up.FacetCategories.Add(fc);
            Up.FacetCategories.Add(new FacetCategory("SubmissionDate", "DateTime", true));
        }

        public static void RecordPivotParameters(UIParameters Up, string collectionName, string collectionTitle)
        {
            //Up.CollectionName = collectionName;
            Up.CollectionName = "blip";
            Up.CollectionTitle = collectionTitle;
            //Up.CollectionUrl = @"file:///" + Up.CollectionPath + "\\" + Up.CollectionName + ".cxml";
            Up.CollectionUid = GetUid();
            Up.CollectionUrl = Up.CollectionPath + "\\" + Up.CollectionName + ".cxml";
            Up.CollectionImagePath = Up.CollectionPath + "\\" + Up.CollectionUid + "_images";
            Up.CollectionDeepzoomPath = Up.CollectionPath + "\\" + Up.CollectionUid + "_deepzoom";
        }
        
        private static string GetUid()
        {
            var random = new Random(System.DateTime.Now.Millisecond);
            int randomNumber = random.Next(1, 5000000);
            return randomNumber.ToString();
        }

        public static int[] GetBestAnnotatedIndex(UIParameters Up, int seqPos)
        {
            // BLAST reports are saved in individual files by query and 
            // numbered in the same order as they appear in the input FASTA file.
            int[] annotatedIndex = new int[2];
            annotatedIndex[0] = -1;
            annotatedIndex[1] = -1;

            string blastFile = Up.ProjectDir + "\\xml\\" + seqPos + ".xml";
            if (!File.Exists(blastFile))
            {
                throw new Exception("File does not exist.");
            }
            BlastXmlParser blastParser = new BlastXmlParser();
            IList<BlastResult> blastResults = blastParser.Parse(blastFile);
            GenBankParser gbParser = new GenBankParser();

            // iterate through the BLAST results.
            foreach (BlastResult blastResult in blastResults)
            {
                foreach (BlastSearchRecord record in blastResult.Records)
                {
                    int hitsProcessed = 0;
                    // If there are not hits in the BLAST result ...
                    int rank = 0;
                    if (record.Hits.Count() > 0)
                    {
                        // For each hit
                        for (int i = 0; i < record.Hits.Count(); i++)
                        {

                            Hit blastHit = record.Hits[i];
                            for (int j = 0; j < blastHit.Hsps.Count(); j++)
                            {

                                Hsp blastHsp = blastHit.Hsps[j];
                                double percentId = (blastHsp.IdentitiesCount / (double)blastHsp.AlignmentLength) * 100;
                                double queryCoverage = ((double)(blastHsp.QueryEnd - blastHsp.QueryStart + 1) / record.IterationQueryLength) * 100;
                                
                                if ((percentId >= Up.BlastMinPercentIdentity) &&
                                    (Up.BlastMaxEvalue >= blastHsp.EValue) &&
                                    (queryCoverage >= Up.BlastMinPercentQueryCoverage) &&
                                    (hitsProcessed < Up.BlastMaxNumHits))
                                {
                                    rank += 1;
                                    long gi = Convert.ToInt64(blastHit.Id.Split('|')[1]);
                                    GenBankItem gitem = new GenBankItem(gi, blastHsp.HitStart, blastHsp.HitEnd);
                                    string gbFile = Up.ProjectDir + "\\gb\\" + gitem.Id.ToString();
                                    gbFile += "_" + gitem.HitStart.ToString();
                                    gbFile += "_" + gitem.HitEnd.ToString();
                                    gbFile += ".gb";
                                    
                                    try
                                    {
                                        Console.WriteLine("GB OK: " + record.Hits[0].Id + " " + i.ToString() + " " + j.ToString());
                                        ISequence gbRecord = gbParser.ParseOne(gbFile);
                                        GenBankMetadata gbMeta = (GenBankMetadata)gbRecord.Metadata["GenBank"];
                                        IList<FeatureItem> features = gbMeta.Features.All;
                                        FeatureItem bestItem = getBestFeatureItem(features);
                                        if (bestItem != null)
                                        {
                                            annotatedIndex[0] = i;
                                            annotatedIndex[1] = j;
                                            return annotatedIndex;
                                        }
                                    }
                                    
                                    catch
                                    {
                                        Console.WriteLine("ISANNOTATED: " + record.Hits[0].Id + " " + i.ToString() + " " + j.ToString());
                                    }
                                    hitsProcessed += 1;
                                }
                            }
                        }
                    }
                }
            }
            
            return annotatedIndex;
        }
        
        public static int CreateItems(UIParameters Up, ISequence rec, int itemId, int seqPos, Collection collection)
        {
            
            string queryName = rec.DisplayID.ToString().Split(' ')[0];

            // BLAST reports are saved in individual files by query and 
            // numbered in the same order as they appear in the input FASTA file.
            string blastFile = Up.ProjectDir + "\\xml\\" + seqPos + ".xml";
            if (!File.Exists(blastFile))
            {
                throw new Exception("File does not exist.");
            }
            BlastXmlParser blastParser = new BlastXmlParser();
            IList<BlastResult> blastResults = blastParser.Parse(blastFile);
            GenBankParser gbParser = new GenBankParser();

            int[] annotatedIndex = GetBestAnnotatedIndex(Up, seqPos);
            
            // iterate through the BLAST results.
            foreach (BlastResult blastResult in blastResults)
            {
                foreach (BlastSearchRecord record in blastResult.Records)
                {
                    int hitsProcessed = 0;
                    // If there are not hits in the BLAST result ...
                    int rank = 0;
                    if (record.Hits.Count() > 0)
                    {
                        // For each hit
                        for (int i = 0; i < record.Hits.Count(); i++)
                        {

                            Hit blastHit = record.Hits[i];
                            // For each HSP
                            for (int j = 0; j < blastHit.Hsps.Count(); j++)
                            {
                                
                                Hsp blastHsp = blastHit.Hsps[j];
                                double percentId = (blastHsp.IdentitiesCount / (double)blastHsp.AlignmentLength) * 100;
                                double queryCoverage = ((double)(blastHsp.QueryEnd - blastHsp.QueryStart + 1) / record.IterationQueryLength) * 100;
                                string txt = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", percentId, Up.BlastMinPercentIdentity,
                                    Up.BlastMaxEvalue, blastHsp.EValue, queryCoverage, Up.BlastMinPercentQueryCoverage,
                                    hitsProcessed, Up.BlastMaxNumHits);
                                // if HSP passes user-defined thresholds
                                if ((percentId >= Up.BlastMinPercentIdentity) &&
                                    (Up.BlastMaxEvalue >= blastHsp.EValue) &&
                                    (queryCoverage >= Up.BlastMinPercentQueryCoverage) &&
                                    (hitsProcessed < Up.BlastMaxNumHits))
                                {
                                    rank += 1;
                                    string nextScore = "no";
                                    if ((i + 1) < record.Hits.Count())
                                    {
                                        if (blastHsp.Score > record.Hits[i + 1].Hsps[0].Score)
                                        {
                                            nextScore = "less than";
                                        }
                                        else
                                        {
                                            nextScore = "equal";
                                        }
                                    }
                                    else
                                    {
                                        nextScore = "non existent";
                                    }

                                    // parse GI numner from hit
                                    long gi = Convert.ToInt64(blastHit.Id.Split('|')[1]);
                                    GenBankItem gitem = new GenBankItem(gi, blastHsp.HitStart, blastHsp.HitEnd);
                                    string gbFile = Up.ProjectDir + "\\gb\\" + gitem.Id.ToString();
                                    gbFile += "_" + gitem.HitStart.ToString();
                                    gbFile += "_" + gitem.HitEnd.ToString();
                                    gbFile += ".gb";
                                    // init item
                                    string img = "#" + itemId.ToString();
                                    Item item = new Item(itemId, img);
                                    string[] headerTokens = parseFastaHeader(rec.DisplayID.ToString());
                                    item.Name = headerTokens[0];
                                    item.Description = headerTokens[1];

                                    // write pairwise alignment
                                    writePairwiseAlignment(Up, blastHit, j, itemId);

                                    // try to parse the GB record associated with the hit and set facet values to data from BLAST/GB record
                                    try
                                    {
                                        Console.WriteLine("GB OK: " + record.Hits[0].Id + " " + i.ToString() + " " + j.ToString());
                                        ISequence gbRecord = gbParser.ParseOne(gbFile);
                                        item.Href = GetNCBIUrl(Up.BlastProgram) + GetGenBankIdentifier(gbRecord);
                                        GenBankMetadata gbMeta = (GenBankMetadata)gbRecord.Metadata["GenBank"];
                                        CodingSequence bestCds = null;
                                        IList<FeatureItem> features = gbMeta.Features.All;
                                        FeatureItem bestItem = getBestFeatureItem(features);

                                        
                                        if (gbMeta.Features.CodingSequences.Count > 0)
                                        {
                                            bestCds = gbMeta.Features.CodingSequences[0];
                                        }
                                        
                                        for (int k = 1; k < gbMeta.Features.CodingSequences.Count; k++)
                                        {
                                            CodingSequence cds = gbMeta.Features.CodingSequences[k];
                                            //int bestSize = Math.Abs(bestCds.Location.End - bestCds.Location.Start);
                                            int bestSize = Math.Abs(bestItem.Location.End - bestItem.Location.Start);
                                            int cdsSize = Math.Abs(cds.Location.End - cds.Location.Start);
                                            if (cdsSize > bestSize)
                                            {
                                                bestCds = cds;
                                            }
                                        }
                                        foreach (FacetCategory f in Up.FacetCategories)
                                        {
                                            Facet facet = new Facet();
                                            switch (f.Name)
                                            {
                                                case "InputOrder":
                                                    facet = new Facet(f.Name, f.Type, seqPos);
                                                    break;
                                                case "QuerySequence":
                                                    facet = new Facet(f.Name, f.Type, rec.ToString());
                                                    break;
                                                case "NextScore":
                                                    facet = new Facet(f.Name, f.Type, nextScore);
                                                    break;
                                                case "Annotated":
                                                    string value = "na";
                                                    if ((annotatedIndex[0] == i) && (annotatedIndex[1] == j))
                                                    {
                                                        value = "top_annotated";
                                                    }
                                                    else
                                                    {
                                                        if ((i == 0) && (j == 0) && (annotatedIndex[0] == -1) && (annotatedIndex[1] == -1))
                                                        {
                                                            value = "top_unannotated";
                                                        }else{
                                                            if (bestItem != null)
                                                            {
                                                                value = "annotated";
                                                            }else{
                                                                value = "unannotated";
                                                            }
                                                        }
                                                    }
                                                    facet = new Facet(f.Name, f.Type, value);
                                                    break;
                                                default:
                                                    //facet = CreateFacet(f.Name, f.Type, record, i, j, gbRecord, item, GetNCBIUrl(Up.BlastProgram), bestCds, rank);
                                                    facet = CreateFacet(f.Name, f.Type, record, i, j, gbRecord, item, GetNCBIUrl(Up.BlastProgram), bestItem, rank);
                                                    break;
                                            }
                                            /*
                                            if (f.Name == "InputOrder")
                                            {
                                                facet = new Facet(f.Name, f.Type, seqPos);
                                            }

                                            else
                                            {
                                                facet = CreateFacet(f.Name, f.Type, record, i, j, gbRecord, item);
                                            }
                                            */
                                            item.Facets.Add(facet);
                                        }

                                    }
                                    //catch (System.NullReferenceException e) // if parsing failed init the item w/ default values (similar to 'no hit' above)
                                    catch
                                    {

                                        Console.WriteLine("GB ERROR: " + record.Hits[0].Id + " " + i.ToString() + " " + j.ToString());
                                        item.Href = "#";
                                        foreach (FacetCategory f in Up.FacetCategories)
                                        {
                                            Facet facet = new Facet();
                                            switch (f.Name)
                                            {
                                                case ("InputOrder"):
                                                    facet = new Facet(f.Name, f.Type, seqPos);
                                                    break;
                                                case "QuerySequence":
                                                    facet = new Facet(f.Name, f.Type, rec.ToString());
                                                    break;
                                                case ("NextScore"):
                                                    facet = new Facet(f.Name, f.Type, "no");
                                                    break;
                                                case "Annotated":
                                                    string value = "na";
                                                    if ((annotatedIndex[0] == i) && (annotatedIndex[1] == j))
                                                    {
                                                        value = "top_annotated";
                                                    }
                                                    else
                                                    {
                                                        if ((i == 0) && (j == 0) && (annotatedIndex[0] == -1) && (annotatedIndex[1] == -1))
                                                        {
                                                            value = "top_unannotated";
                                                        }
                                                        else
                                                        {
                                                            value = "unannotated";
                                                        }
                                                    }
                                                    facet = new Facet(f.Name, f.Type, value);
                                                    break;
                                                default:
                                                    facet = CreateGBErrorFacet(f.Name, f.Type, record, i, j, item, GetNCBIUrl(Up.BlastProgram), rank);
                                                    break;
                                            }
                                            item.Facets.Add(facet);
                                        }
                                        //throw (e);
                                    }
                                    // Add item to collection, increment to next item, 
                                    collection.Items.Add(item);
                                    hitsProcessed += 1;
                                    itemId += 1;
                                }
                            }
                        }
                    }
                    if ((record.Hits.Count()) == 0 || (hitsProcessed == 0))
                    {
                        // Init Pivot item
                        string img = "#" + itemId.ToString();
                        Item item = new Item(itemId, img);
                        item.Href = "#";
                        string[] headerTokens = parseFastaHeader(rec.DisplayID.ToString());
                        item.Name = headerTokens[0];
                        item.Description = headerTokens[1];

                        // Write pairwise alignment to file.
                        writePairwiseAlignment(Up, itemId);

                        // Set facet values for each facet category to default values
                        foreach (FacetCategory f in Up.FacetCategories)
                        {
                            Facet facet = new Facet();
                            switch (f.Name)
                            {
                                case ("InputOrder"):
                                    facet = new Facet(f.Name, f.Type, seqPos);
                                    break;
                                case ("QuerySequence"):
                                    facet = new Facet(f.Name, f.Type, rec.ToString());
                                    break;
                                default:
                                    facet = CreateFacet(f.Name, f.Type, record, item, 0);
                                    break;
                            }
                            item.Facets.Add(facet);
                        }

                        // Add item to collection, increment to next item, skip remaining code
                        collection.Items.Add(item);
                        itemId += 1;
                        hitsProcessed += 1;
                    }
                }
            }
            return itemId;
        }
        
        public static void writePairwiseAlignment(UIParameters Up, int itemId)
        {

            TextWriter tw = new StreamWriter(Up.ProjectDir + "\\txt\\" + itemId + ".txt");
            tw.Write("No alignments for this query sequence.");
            tw.Close();
        }

        public static void writePairwiseAlignment(UIParameters Up, Hit blastHit, int blastHspId, int itemId)
        {
            string pa = BlastUtil.ToString(blastHit, blastHspId);
            TextWriter tw = new StreamWriter(Up.ProjectDir + "\\txt\\" + itemId + ".txt");
            tw.Write(pa);
            tw.Close();
        }

        public static string[] parseFastaHeader(string header)
        {
            string identifier;
            string description = "No description available for this query sequence.";
            int x = header.IndexOf(' ');
            if (x > 0)
            {
                identifier = header.Substring(0, x);
                description = header.Substring(x + 1, (header.Length - x - 1));
            }
            else
            {
                identifier = header;
            }
            return new string[] { identifier, description };
        }

        public static string GetGenBankIdentifier(ISequence gb)
        {
            GenBankMetadata gbMeta = (GenBankMetadata)gb.Metadata["GenBank"];
            return gbMeta.Version.GINumber;
        }

        public static Facet CreateFacet(string fName, string fType, BlastSearchRecord rec, Item item, int rank)
        {
            switch (fName)
            {
                case "QueryName":
                    return new Facet(fName, fType, item.Name);
                case "QueryLen":
                    return new Facet(fName, fType, rec.IterationQueryLength);
                case "Rank":
                    return new Facet(fName, fType, rank);
                default:
                    if (fType == "Number")
                    {
                        return new Facet(fName, fType, 0);
                    }
                    else
                    {
                        if (fType == "Link")
                        {

                            return new Facet(fName, fType, "N/A", "about:none.");
                        }
                        else
                        {
                            if (fType == "DateTime")
                            {
                                Facet f = new Facet(fName, fType);
                                //Facet f = new Facet(fName, fType, dt.ToUniversalTime().ToString("o"));
                                //DateTime dt = new DateTime(1900, 1, 1);
                                return f;
                            }
                            else
                            {
                                return new Facet(fName, fType, "No Hit");
                            }
                        }
                    }
                //throw (new Exception("Facet category with name = " + fName + " does not exist."));
            }
        }

        public static string CreateReferenceURL(CitationReference r)
        {
            if (r.PubMed != null)
            {
                return "http://www.ncbi.nlm.nih.gov/pubmed/" + r.PubMed;
            }
            else
            {
                return "http://academic.research.microsoft.com/Search.aspx?query=" + System.Web.HttpUtility.UrlEncode(r.Title);
            }
        }

        public static string GetQualifierString(string featureItemKey, string qualifierKey, GenBankMetadata gbMeta)
        {
            Console.WriteLine("In GetQualifierString: " + featureItemKey + " " + qualifierKey);
            
            foreach (FeatureItem fi in gbMeta.Features.All)
            {
                if (fi.Key == featureItemKey)
                {
                    foreach (KeyValuePair<string, List<string>> q in fi.Qualifiers)
                    {
                        Console.WriteLine(featureItemKey + " " + q.Key + " " + q.Value[0]);
                        if (q.Key == qualifierKey)
                        {
                            return q.Value[0].Trim('"');
                        }
                    }
                }
            }
            return "N/A";
        }

        //public static string GetQualifierStringFromCDS(CodingSequence cds, string qualifierKey)
        public static string GetQualifierStringFromCDS(FeatureItem cds, string qualifierKey)
        {
            foreach (KeyValuePair<string, List<string>> q in cds.Qualifiers)
            {
                if (q.Key == qualifierKey)
                {
                    return q.Value[0].Trim('"');
                }
            }
            return "N/A";
        }

        public static FeatureItem getBestFeatureItem(IList<FeatureItem> features)
        {
            FeatureItem bestItem = null;
            foreach (FeatureItem feature in features)
            {
                //Console.WriteLine(feature.Key.ToString());
                if ((feature.Key != "source") && (feature.Qualifiers.Count > 0))
                {
                    bestItem = feature;
                    if (bestItem.Key == "CDS")
                    {
                        return bestItem;
                    }
                }
            }
            return bestItem;

        }

        public static string FrameToStrand(long frame)
        {
            switch (frame)
            {
                case 1:
                    return "+";
                case -1:
                    return "-";
                default:
                    return ".";
            }
        }

        //public static Facet CreateFacet(string fName, string fType, BlastSearchRecord rec, int hitId, int hspId, ISequence gb, Item item, string NCBIurl, CodingSequence bestCds, int rank)
        public static Facet CreateFacet(string fName, string fType, BlastSearchRecord rec, int hitId, int hspId, ISequence gb, Item item, string NCBIurl, FeatureItem bestItem, int rank)
        {
            Hit hit = rec.Hits[hitId];
            Hsp hsp = hit.Hsps[hspId];
            GenBankMetadata gbMeta = (GenBankMetadata)gb.Metadata["GenBank"];
            string[] classLevels;
            switch (fName)
            {
                case "QueryName":
                    return new Facet(fName, fType, item.Name);
                case "QueryLen":
                    return new Facet(fName, fType, rec.IterationQueryLength);
                case "Rank":
                    return new Facet(fName, fType, rank);
                case "Score":
                    return new Facet(fName, fType, Math.Round(hsp.BitScore, 1));
                case "Identity":
                    double pi = (hsp.IdentitiesCount / (double)hsp.AlignmentLength) * 100.0;
                    return new Facet(fName, fType, Math.Round(pi, 0));
                case "Span":
                    double sp = ((hsp.QueryEnd - hsp.QueryStart + 1) / (double)rec.IterationQueryLength) * 100.0;
                    return new Facet(fName, fType, Math.Round(sp, 0));
                case "SubjStart":
                    double subjStart = hsp.HitStart;
                    return new Facet(fName, fType, Math.Round(subjStart,0));
                case "SubjLen":
                    double subjLen = hit.Length;
                    return new Facet(fName, fType, Math.Round(subjLen, 0));
                
                case "Strand":
                    string strand = FrameToStrand(hsp.QueryFrame) + "/" + FrameToStrand(hsp.HitFrame);
                    return new Facet(fName, fType, strand);
                case "Species":
                    int index = gbMeta.Source.Organism.Species.IndexOf(" ", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        return new Facet(fName, fType, gbMeta.Source.Organism.Genus + " " + gbMeta.Source.Organism.Species.Substring(0, index));
                    }
                    else
                    {
                        return new Facet(fName, fType, gbMeta.Source.Organism.Genus + " " + gbMeta.Source.Organism.Species);
                    }
                case "Kingdom":
                    classLevels = gbMeta.Source.Organism.ClassLevels.Split(';');
                    if (classLevels.Length >= 1)
                    {
                        return new Facet(fName, fType, classLevels[0]);
                    }
                    else
                    {
                        return new Facet(fName, fType, "N/A");
                    }
                case "Phylum":
                    classLevels = gbMeta.Source.Organism.ClassLevels.Split(';');
                    if (classLevels.Length >= 2)
                    {
                        return new Facet(fName, fType, classLevels[1]);
                    }
                    else
                    {
                        return new Facet(fName, fType, "N/A");
                    }
                case "Class":
                    classLevels = gbMeta.Source.Organism.ClassLevels.Split(';');
                    if (classLevels.Length >= 3)
                    {
                        return new Facet(fName, fType, classLevels[2]);
                    }
                    else
                    {
                        return new Facet(fName, fType, "N/A");
                    }
                case "Order":
                    classLevels = gbMeta.Source.Organism.ClassLevels.Split(';');
                    if (classLevels.Length >= 4)
                    {
                        return new Facet(fName, fType, classLevels[3]);
                    }
                    else
                    {
                        return new Facet(fName, fType, "N/A");
                    }
                case "Family":
                    classLevels = gbMeta.Source.Organism.ClassLevels.Split(';');
                    if (classLevels.Length >= 5)
                    {
                        return new Facet(fName, fType, classLevels[4]);
                    }
                    else
                    {
                        return new Facet(fName, fType, "N/A");
                    }
                case "Lineage":
                    return new Facet(fName, fType, gbMeta.Source.Organism.ClassLevels.ToString());
                case "Organism":
                    return new Facet(fName, fType, gbMeta.Source.CommonName);
                    // return new Facet(fName, fType, gbMeta.Source.Organism.Genus + " " + gbMeta.Source.Organism.Species);
                case "Genus":
                    return new Facet(fName, fType, gbMeta.Source.Organism.ClassLevels.Split(';').Last().Trim().TrimEnd('.'));
                case "Gene":
                    string name = "N/A";
                    //if (bestCds != null)
                    if (bestItem != null)
                    {
                        //CodingSequence feature = bestCds;
                        FeatureItem feature = bestItem;
                        String geneSym = "N/A";
                        foreach (KeyValuePair<string,List<String>> qualifier in feature.Qualifiers){
                           if (qualifier.Key == "gene")
                            {
                            geneSym = qualifier.Value[0].ToString().Trim('"');
                            }   
                        }
                                                    
                        if (geneSym != "")
                        {
                            name = geneSym;
                            string url2 = System.Web.HttpUtility.HtmlEncode("http://www.ncbi.nlm.nih.gov/sites/entrez?cmd=search&db=gene&term=" + name + "%5Bsym%5D");
                            return new Facet(fName, fType, name, url2);
                        }
                    }
                    return new Facet(fName, fType, name, null);
                case "GI":
                    return new Facet(fName, fType, gbMeta.Version.GINumber, NCBIurl + gbMeta.Version.GINumber);
                case "Accession":
                    return new Facet(fName, fType, gbMeta.Version.CompoundAccession, NCBIurl + gbMeta.Version.CompoundAccession);
                case "Definition":
                    return new Facet(fName, fType, gbMeta.Definition);
                case "EValue":
                    return new Facet(fName, fType, String.Format("{0:#e+00}", hsp.EValue));
                case "AlignLen":
                    return new Facet(fName, fType, hsp.AlignmentLength, @"txt\" + item.Id + ".txt");
                case "RefCount":
                    int i = 0;
                    foreach (CitationReference r in gbMeta.References)
                    {
                        if ((r.Title != "Direct Submission") && (r.Journal != "Unpublished")) { i++; }
                    }
                    return new Facet(fName, fType, i);
                case "References":
                    if (gbMeta.References.Count() == 0)
                    {
                        return new Facet(fName, fType);
                    }

                    string url = CreateReferenceURL(gbMeta.References[0]);
                    Facet f = new Facet(fName, fType);

                    if (gbMeta.References.Count() > 0)
                    {
                        int j = 1;
                        foreach (CitationReference r in gbMeta.References)
                        {
                            if (r.Title != "Direct Submission" && (r.Journal != "Unpublished"))
                            {
                                url = CreateReferenceURL(r);
                                f.Add(new FacetValue(f.Type, String.Format("{0}. {1}. {2}.", j, r.Title, r.Journal), url));
                                j++;
                            }
                        }
                    }
                    return f;
                case "SubmissionDate":
                    DateTime dt = new DateTime(gbMeta.Locus.Date.Year, gbMeta.Locus.Date.Month, gbMeta.Locus.Date.Day);
                    return new Facet(fName, fType, dt.ToUniversalTime().ToString("o"));

                case "Product":
                    Facet productFacet = new Facet(fName, fType, GetQualifierString("Protein", "product", gbMeta));
                    
                    if (productFacet[0].Value == "N/A")
                    {
                        Console.WriteLine(productFacet[0].Value + "!!!!!!!!!!!!!!!!!!!!!!!!!!***********");
                        if (bestItem != null) { productFacet = new Facet(fName, fType, GetQualifierStringFromCDS(bestItem, "product")); }
                        Console.WriteLine(productFacet[0].Value + "!!!!!!!!!!!!!!!!!!!!!!!!&&&&&&&&&&&&&");
                    }
                    return productFacet;

                case "Function":
                    Facet funcFacet = new Facet(fName, fType, GetQualifierString("Protein", "function", gbMeta));
                    if (funcFacet[0].Value == "N/A")
                    {
                        if (bestItem != null) { funcFacet = new Facet(fName, fType, GetQualifierStringFromCDS(bestItem, "function")); }
                    }
                    return funcFacet;

                default:
                    throw (new Exception("Facet category with name = " + fName + " does not exist."));
            }
        }

    public static Facet CreateGBErrorFacet(string fName, string fType, BlastSearchRecord rec, int hitId, int hspId, Item item, string url, int rank)
        {
            Hit hit = rec.Hits[hitId];
            Hsp hsp = hit.Hsps[hspId];
            
            switch (fName)
            {
                case "QueryName":
                    return new Facet(fName, fType, item.Name);
                case "QueryLen":
                    return new Facet(fName, fType, rec.IterationQueryLength);
                case "Rank":
                    return new Facet(fName, fType, rank);
                case "Score":
                    return new Facet(fName, fType, Math.Round(hsp.BitScore, 1));
                case "Identity":
                    double pi = (hsp.IdentitiesCount / (double)hsp.AlignmentLength) * 100.0;
                    return new Facet(fName, fType, Math.Round(pi, 0));
                case "Span":
                    double sp = ((hsp.QueryEnd - hsp.QueryStart + 1) / (double)rec.IterationQueryLength) * 100.0;
                    return new Facet(fName, fType, Math.Round(sp, 0));
                case "SubjStart":
                    double subjStart = hsp.HitStart;
                    return new Facet(fName, fType, Math.Round(subjStart,0));
                case "SubjLen":
                    double subjLen = hit.Length;
                    return new Facet(fName, fType, Math.Round(subjLen, 0));
                case "Strand":
                    string strand = FrameToStrand(hsp.QueryFrame) + "/" + FrameToStrand(hsp.HitFrame);
                    return new Facet(fName, fType, strand);
                case "Species":
                    return new Facet(fName, fType, "N/A");
                case "Lineage":
                    return new Facet(fName, fType, "N/A");
                case "Kingdom":
                    return new Facet(fName, fType, "N/A");
                case "Phylum":
                    return new Facet(fName, fType, "N/A");
                case "Class":
                    return new Facet(fName, fType, "N/A");
                case "Order":
                    return new Facet(fName, fType, "N/A");
                case "Family":
                    return new Facet(fName, fType, "N/A");
                case "Organism":
                    return new Facet(fName, fType, "N/A");
                case "Genus":
                    return new Facet(fName, fType, "N/A");
                case "Gene":
                    return new Facet(fName, fType, "N/A", null);
                case "GI":
                    return new Facet(fName, fType, "N/A", url);
                case "Accession":
                    return new Facet(fName, fType, "N/A", url);
                case "Definition":
                    return new Facet(fName, fType, "N/A");
                case "EValue":
                    return new Facet(fName, fType, "N/A");
                case "AlignLen":
                    return new Facet(fName, fType, hsp.AlignmentLength, @"txt\" + item.Id + ".txt");
                case "RefCount":
                    int i = 0;
                    return new Facet(fName, fType, i);
                case "References":
                    return new Facet(fName, fType);
                case "SubmissionDate":
                    return new Facet(fName, fType);
                case "Product":
                    return new Facet(fName, fType, "N/A");
                case "Function":
                    return new Facet(fName, fType, "N/A");
                default:
                    throw (new Exception("Facet category with name = " + fName + " does not exist."));
            }
        }
        
    }
}
