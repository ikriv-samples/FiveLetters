using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWordsCs
{
    using WordMap = Dictionary<int, List<string>>;
    using MaskPair = ValueTuple<int,int>;
    using MaskToPairMap = Dictionary<int, List<(int,int)>>;


    class Program
    {
        const int EXPECTED_LENGTH = 5;
        const int LETTERS_IN_ALPHABET = 26;
        const int NUM_WORDS = 5;



        private static int ToNumber(char c)
        {
            return c - 'a';
        }

        private static bool IsFiveLetterWord(string word)
        {
            if (word.Length != EXPECTED_LENGTH) return false;
            foreach (char c in word)
            {
                int b = ToNumber(c);
                if (b < 0 || b >= LETTERS_IN_ALPHABET) return false;
            }
            return true;
        }

        private static IEnumerable<string> ReadInput()
        {
            while (true)
            {
                string line = Console.ReadLine();
                if (line == null) yield break;
                if (!IsFiveLetterWord(line)) continue;
                yield return line;
            }
        }

        private static int WordToMask(string word)
        {
            if (word.Length != EXPECTED_LENGTH) return 0;
            int result = 0;

            foreach (char c in word)
            {
                int bit = 1 << ToNumber(c);
                if ((result & bit) != 0) return 0; // a letter occurred twice
                result |= bit;
            }
            return result;
        }

        private static void Insert<T>(Dictionary<int, List<T>> mapIntToList, int key, T val)
        {
            //var existing_list = 
            mapIntToList.TryGetValue(key, out List<T> existingList);
            if (existingList != null)
            {
                existingList.Add(val);
            }
            else
            {
                mapIntToList.Add(key, new List<T> { val });
            }
        }

        private static WordMap BuildWordMap(IEnumerable<string> words)
        {
            var maskToWords = new Dictionary<int, List<string>>();
            foreach (string word in words)
            {
                Insert(maskToWords, WordToMask(word), word);
            }
            return maskToWords;
        }

        private static void PrintInvalidWords(WordMap map)
        {
            if (map.TryGetValue(0, out var invalidWords))
            {
                Console.Error.WriteLine("Invalid words:");
                foreach (var word in invalidWords)
                {
                    Console.Error.WriteLine(word);
                }
                Console.Error.WriteLine("End invalid words");
            }
        }

        private static MaskToPairMap Nand(MaskToPairMap inputMap, IEnumerable<int> wordMasks)
        {
            var outputMap = new MaskToPairMap();
            foreach (var input_mask in inputMap.Keys)
            {
                foreach (var word_mask in wordMasks)
                {
                    if ((input_mask & word_mask) == 0)
                    {
                        var output_mask = input_mask | word_mask;
                        Insert(outputMap, output_mask, new MaskPair(word_mask, input_mask));
                    }
                }
            }
            return outputMap;
        }

        private static void PrintWordsImpl(List<MaskToPairMap> pairMaps, WordMap maskToWords, List<string> prefix, int mask)
        {
            int level = NUM_WORDS - prefix.Count;
            if (level == 0)
            {
                // print all the words in the list
                Console.WriteLine(String.Join(" ", prefix));
            }
            else
            {
                var pairs = pairMaps[level][mask];
                foreach (var pair in pairs)
                {
                    var (word_mask, suffix_mask) = pair;
                    var words = maskToWords[word_mask];
                    string lastWordOfPrefix = prefix.Count > 0 ? prefix[prefix.Count - 1] : "";
                    foreach (var word in words)
                    {
                        if (String.CompareOrdinal(word, lastWordOfPrefix) <= 0) continue;
                        prefix.Add(word);
                        PrintWordsImpl(pairMaps, maskToWords, prefix, suffix_mask);
                        prefix.RemoveAt(prefix.Count - 1);
                    }
                }
            }
        }

        private static void PrintWords(List<MaskToPairMap> pairMaps, WordMap maskToWords)
        {
            var prefix = new List<string>();
            foreach (var mask in pairMaps[pairMaps.Count - 1].Keys)
            {
                PrintWordsImpl(pairMaps, maskToWords, prefix, mask);
            }
        }

        public static void Main(string[] args)
        {
            var words = ReadInput().ToList();
            Console.Error.WriteLine($"{words.Count} words");
            var maskToWords = BuildWordMap(words);
            PrintInvalidWords(maskToWords);

            maskToWords.Remove(0);
            Console.Error.WriteLine($"{maskToWords.Count} unique keys");

            var pairMaps = new List<MaskToPairMap> { new MaskToPairMap { { 0, new List<MaskPair> { (0, 0) } } } };

            for (int i=1; i<=NUM_WORDS; ++i)
            {
                var newMap = Nand(pairMaps[pairMaps.Count-1], maskToWords.Keys);
                Console.Error.WriteLine($"{newMap.Count} groups of size {i}");
                pairMaps.Add(newMap);
            }

            PrintWords(pairMaps, maskToWords);
        }
    }
}
