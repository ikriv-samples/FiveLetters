using System;
using System.Collections.Generic;
using System.Linq;

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

        // Convert a word to a bitmask representing its letters.
        // Returns zero if the word is not of the right length.
        // If the word has 5 different letters, the mask would have 5 bits on
        // E.g. both 'dowry' and 'rowdy' would be represented as 
        // ------zyxwvutsrqponmlkjihgfedcba
        // 00000001010000100100000000001000
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

        // Insert a new item into a mapping from key to a list of items
        private static void Insert<T>(Dictionary<int, List<T>> mapIntToList, int key, T val)
        {
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

        // Create a mapping from bitmask to a list of words (see WordToMask)
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

        // N-sequence map is a map from a mask to a list of pairs (head_mask, tail_mask).
        // Each key represents a bitmask for a sequence of N non-colliding words.
        // The key is mapped to a list of pairs. Each pair contains a bitmask for the head word, and a bitmask for a tail of length N-1
        //
        // combined_mask->[(head_mask1, tail_mask1), (head_mask2, tail_mask2), ...]
        // For each i head_maski | tail_maski == combined_mask, and head_maski & tail_maski == 0
        //
        // The nand function takes an N-sequence map inputMap and returns an N+1-sequence map, 
        // in which head word masks are taken from wordMask, and tail masks are taken from the keys of inputMap.
        // This simulates adding a new non-collding word in front of an existing word sequence.
        private static MaskToPairMap Nand(MaskToPairMap inputMap, IEnumerable<int> wordMasks)
        {
            var outputMap = new MaskToPairMap();
            foreach (var inputMask in inputMap.Keys)
            {
                foreach (var wordMask in wordMasks)
                {
                    if ((inputMask & wordMask) == 0)
                    {
                        var output_mask = inputMask | wordMask;
                        Insert(outputMap, output_mask, new MaskPair(wordMask, inputMask));
                    }
                }
            }
            return outputMap;
        }

        // pairMaps argument is an a list of word sequence maps
        // pairMaps[0] is a dummy map
        // pairMaps[1] is a 1-sequence map
        // pairMaps[2] is a 2-sequence map
        // ...
        // pairMaps[5] is a 5-sequence map
        //
        // maskToWords arguemnt is a map from a bitmask to a list of words
        // prefix argument is a list of strings that contains known prefix of the non-colliding sequence, maybe empty.
        // mask argument is a bitmask of the suffix
        // 
        // If the prefix is of length 5, it contains the entire sequence, and the function simply prints it
        // If the prefix is shorter, the function looks up possible head words for the suffix in the values of pairMaps[n],
        // where n is the suffix length (aka "level").
        // It then appends the head word to the prefix and calls print_words_impl() recursively to handle the remaining suffix.
        //
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

        // Prints word sequences encoded in pairMaps
        // pairMaps argument is an a list of word sequence maps
        // pairMaps[0] is a dummy map
        // pairMaps[1] is a 1-sequence map
        // pairMaps[2] is a 2-sequence map
        // ...
        // pairMaps[5] is a 5-sequence map
        // maskToWords argument maps bitmasks to list of words
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

            // start with a dummy map
            var pairMaps = new List<MaskToPairMap> { new MaskToPairMap { { 0, new List<MaskPair> { (0, 0) } } } };

            // for each i, build an i-sequence word map (see comment for 'Nand()')
            for (int i=1; i<=NUM_WORDS; ++i)
            {
                var newMap = Nand(pairMaps[pairMaps.Count-1], maskToWords.Keys);
                Console.Error.WriteLine($"{newMap.Count} groups of size {i}");
                pairMaps.Add(newMap);
            }

            // Print the words encoded in the series of maps
            PrintWords(pairMaps, maskToWords);
        }
    }
}
