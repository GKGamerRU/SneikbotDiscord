using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Markov
{
    public class MarkovConfiguration
    {
        public int WordsInKey { get; set; } = 2;
        public bool isRandomCut { get; set; } = false;
        public bool isSuperMarkov { get; set; } = true;
    }
}
