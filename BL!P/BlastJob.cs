using System;
using MBF;
using MBF.Web;
using MBF.Web.Blast;

namespace Blip
{
    /// <summary>
    /// Represents a job submitted to NCBI BLAST.
    /// </summary>
    public class BlastJob
    {
        public const string AVAILABLE = "AVAILABLE";
        public const string BUSY = "BUSY";
        public const string FAILED = "FAILED";
        public string JobId { get; set; }
        public NCBIBlastHandler BlastService { get; set; }
        public BlastParameters SearchParams { get; set; }
        public ConfigParameters ConfigParams { get; set; }
        public string JobStatus { get; set; }
        public ISequence Query { get; set; }
        public int Position { get; set; }

        public BlastJob()
        {
            JobStatus = AVAILABLE;
        }
    }
}
