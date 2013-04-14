using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blip
{
    class GenBankItem
    {
        public long Id { get; set;}
        public long HitStart { get; set;}
        public long HitEnd { get; set;}


        public GenBankItem(long id, long hitEnd, long hitStart)
        {
            Id = id;
            if (hitStart > hitEnd)
            {
                HitStart = hitEnd;
                HitEnd = hitStart;
            }
            else
            {
                HitStart = hitStart;
                HitEnd = hitEnd;
            }
        }
    }
}
