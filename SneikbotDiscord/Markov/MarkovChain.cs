using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneikbotDiscord.Markov
{
    public class MarkovChain
    {
        public Dictionary<string, List<string>> chain = new Dictionary<string, List<string>>();
        private Random random = new Random();
        public string GetRandomStartWord()
        {
            var index = random.Next(0,chain.Count);
            return chain.ElementAt(index).Key;
        }

        public void ApplyWords(Dictionary<string, List<string>> words)
        {
            chain = words;
        }

        public void AddWords(string[] words)
        {
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].ContainsEnds() == false)
                {
                    if (!chain.ContainsKey(words[i]))
                    {
                        chain[words[i]] = new List<string>();
                    }

                    //if (words[i + 1].isFirstLetterUp() == false)
                    chain[words[i]].Add(words[i + 1]);
                }
                else
                {
                    words[i + 1] = words[i + 1].ToFirstLetterUp();
                }
            }
        }
        //TODO: Сделать рандом для продолжения слова
        public string GenerateSentence(string startWord, int length)
        {
            List<string> result = new List<string> { startWord };
            string currentWord = startWord;

            for (int i = 0; i < length - 1; i++)
            {
                if (!chain.ContainsKey(currentWord) || chain[currentWord].Count == 0)
                    break;
                if (i != 0 && new Random().Next(0, 10) == 0)
                    continue;

                currentWord = chain[currentWord][random.Next(chain[currentWord].Count)];
                result.Add(currentWord);
            }

            return string.Join(" ", result);
        }
    }
}
