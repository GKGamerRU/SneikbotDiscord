using SneikbotDiscord.Sneik;
using SneikbotDiscord.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SneikbotDiscord.Markov
{
    public class MarkovChain
    {
        public Dictionary<string, List<string>> chain = new Dictionary<string, List<string>>();
        public ulong GuildID { get; set; }

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
            if (SneikBot.Guilds[GuildID].MarkovConfiguration.isSuperMarkov)
            {
                int wordsInKey = SneikBot.Guilds[GuildID].MarkovConfiguration.WordsInKey;
                for (int size = words.Length > wordsInKey ? wordsInKey : words.Length - 1; size > 0; size--)
                {
                    //int corrector = size == 0 ? 1 : 0;
                    for (int i = 0; i < words.Length - size; i++)
                    {
                        string superWord = "";
                        for (int x = 0; x < size; x++)
                        {
                            if (x == 0)
                                superWord += words[i + x];
                            else
                                superWord += $" {words[i + x]}";
                        }

                        if (superWord.ContainsEnds() == false)
                        {
                            if (!chain.ContainsKey(superWord))
                            {
                                chain[superWord] = new List<string>();
                            }

                            chain[superWord].Add(words[i + size]);
                        }
                        else
                        {
                            words[i + 1] = words[i + 1].ToFirstLetterUp();
                        }
                    }
                }
            }
            else
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
        }
        //TODO: Сделать рандом для продолжения слова
        public string GenerateSentence(string startWords, int length)
        {
            if (SneikBot.Guilds[GuildID].MarkovConfiguration.isSuperMarkov)
            {
                List<string> result = new List<string>(startWords.Split());
                //string currentWord = startWords.Last();

                for (int i = 0; i < length - 1; i++)
                {
                    bool isTriggered = false;
                    int wordsInKey = SneikBot.Guilds[GuildID].MarkovConfiguration.WordsInKey;
                    for (int size = result.Count > wordsInKey ? wordsInKey : result.Count; size > 0; size--)
                    {
                        string currentWord = "";

                        for (int x = result.Count - size; x < result.Count; x++)
                        {
                            if (x == result.Count - size)
                                currentWord += result[x];
                            else
                                currentWord += $" {result[x]}";
                        }

                        if (chain.ContainsKey(currentWord))
                        {
                            currentWord = chain[currentWord][random.Next(chain[currentWord].Count)];
                            result.Add(currentWord);
                            isTriggered = true;
                            break;
                        }
                    }
                    if (isTriggered == false) break;
                }

                return string.Join(" ", result);
            }
            else
            {
                List<string> result = new List<string> { startWords };
                string currentWord = startWords;

                for (int i = 0; i < length - 1; i++)
                {
                    if (!chain.ContainsKey(currentWord) || chain[currentWord].Count == 0)
                        break;
                    if (i != 0 && (SneikBot.Guilds[GuildID].MarkovConfiguration.isRandomCut && new Random().Next(0, 10) == 0))
                        continue;

                    currentWord = chain[currentWord][random.Next(chain[currentWord].Count)];
                    result.Add(currentWord);
                }

                return string.Join(" ", result);
            }
        }
    }
}
