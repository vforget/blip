namespace Blip
{
    /// <summary>
    /// The BlastQueue represents an array of BlastJobs. 
    /// NCBI BLAST documentation suggests to limit the number 
    /// of threads (see http://www.ncbi.nlm.nih.gov/BLAST/Doc/node60.html).
    /// The Length property can be modified change the queue length.
    /// RequestDelay and SubmitDelay specified the time (ms) that the
    /// program waits between Requests and Submits of a job. 
    /// </summary>
    public class BlastQueue
    {
        // Blast queue parameters and constants
        public const int Length = 10;
        public const int RequestDelay = 8000 / Length;
        public const int SubmitDelay = 1000;

        // The blast queue
        public BlastJob[] blastQueue { get; set; }

        // Initialize BlastQueue to empty jobs
        /// <summary>
        /// Instatiates a BlastJob for each position in the BlastQueue
        /// </summary>
        public BlastQueue()
        {
            blastQueue = new BlastJob[Length];
            for (int i = 0; i < Length; i++)
            {
                blastQueue[i] = new BlastJob();
            }
        }

        // Create indexer for blast queue
        public BlastJob this[int index]
        {
            get { return blastQueue[index]; }
            set { blastQueue[index] = value; }
        }

        public bool isBlastQueueBusy()
        {
            for (int i = 0; i < Length; i++)
            {
                if (this[i].JobStatus == BlastJob.BUSY)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
