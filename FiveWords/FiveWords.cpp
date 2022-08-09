// FiveWords.cpp : This file contains the 'main' function
// The FiveWords program takes a list of 5-letter English words (or alleged words) without repeating letters
// and prints list of 5-word combinations in which no letter is used more than once, for the total of 25
// distinct letters in each combination
// NOTE: 64-bit version performs slightly faster than 32-bit version


#include <iostream>
#include <vector>
#include <unordered_map>

using namespace std;

int const EXPECTED_LENGTH = 5;
int const LETTERS_IN_ALPHABET = 26;
int const NUM_WORDS = 5;

void read_input(istream& input, vector<string>& words) {
	// can do it prettier using STL transformers, but this simple code will do
	while (input) {
		string word;
		input >> word;
		words.push_back(word);
	}
}

int word_to_mask(string& word) {
	if (word.length() != EXPECTED_LENGTH) return 0;
	int result = 0;
	for (auto c : word) {
		int letter = c - 'a';
		if (letter < 0 || letter >= LETTERS_IN_ALPHABET) {
			return 0; // invalid letter
		}
		auto bit = (1 << letter);
		if (result & bit) return 0;
		result |= bit;
	}

	return result;
}

typedef unordered_map<int, vector<string>> word_map;

template<typename T>
void insert(unordered_map<int, vector<T>>& map, int key, T const& value) {
	auto it = map.find(key);
	if (it != map.end()) {
		it->second.push_back(value);
	}
	else {
		map.insert({ key, {value} });
	}
}

void build_words_map(vector<string>& words, word_map& mask_to_words) {
	for (auto word : words) {
		if (word.length() == 0) continue;
		insert(mask_to_words, word_to_mask(word), word);
	}
}

void print_invalid_words(word_map const& map) {
	auto it = map.find(0);
	if (it == map.end()) return;

	cerr << "Invalid words: " << endl;
	for (auto word : it->second) {
		cerr << word << endl;
	}
	cerr << "End invalid words" << endl;
}

template<typename T>
void get_unique_keys(unordered_map<int, T> const& map, vector<int>& keys) {
	if (map.size() == 0) return;
	for (auto entry : map) {
		keys.push_back(entry.first);
	}
}

typedef pair<int, int> mask_pair;
typedef unordered_map<int, vector<mask_pair>> mask_to_pair_map;

void nand(mask_to_pair_map const& input_map, vector<int> const& word_masks, mask_to_pair_map& output_map) {
	for (auto entry : input_map) {
		const int input_mask = entry.first;
		for (auto word_mask : word_masks) {
			if ((input_mask & word_mask) == 0) {
				const int output_mask = input_mask | word_mask;
				insert(output_map, output_mask, make_pair(word_mask, input_mask));
			}
		}
	}
}

void print_words_impl(mask_to_pair_map* pair_maps, word_map const& mask_to_words, vector<string>& prefix, int mask) {
	const int level = NUM_WORDS - int(prefix.size());
	if (level == 0) {
		// print all the words in the vector
		bool is_first = true;
		for (auto word : prefix) {
			if (is_first) {
				is_first = false;
			}
			else {
				cout << " ";
			}
			cout << word;
		}
		cout << endl;
	}
	else {
		auto it = pair_maps[level].find(mask);
		if (it == pair_maps[level].end()) {
			throw logic_error("Mask not found in the pair map");
		}

		for (auto mask_pair : it->second) {
			auto word_mask = mask_pair.first;
			auto suffix_mask = mask_pair.second;

			auto word_it = mask_to_words.find(word_mask);
			if (word_it == mask_to_words.end()) {
				throw logic_error("Mask not found in the word map");
			}
			auto const last_word_of_prefix = (prefix.size() > 0) ? prefix[prefix.size() - 1] : "";
			for (auto word : word_it->second) {
				if (word <= last_word_of_prefix) continue;
				prefix.push_back(word);
				print_words_impl(pair_maps, mask_to_words, prefix, suffix_mask);
				prefix.pop_back();
			}
		}
	}
}

void print_words(mask_to_pair_map* pair_maps, word_map const& mask_to_words) {
	vector<string> prefix;
	for (auto entry : pair_maps[NUM_WORDS]) {
		print_words_impl(pair_maps, mask_to_words, prefix, entry.first);
	}
}

int main()
{
	vector<string> words;
	read_input(cin, words);
	cerr << words.size() << " words" << endl;

	word_map mask_to_words;
	build_words_map(words, mask_to_words);
	print_invalid_words(mask_to_words);
	mask_to_words.erase(0);

	vector<int> keys;
	get_unique_keys(mask_to_words, keys);
	cerr << keys.size() << " unique keys" << endl;

	mask_to_pair_map pair_maps[NUM_WORDS+1];
	pair_maps[0].insert({ 0,{{0,0}} });
	for (int i = 1; i <= NUM_WORDS; ++i) {
		nand(pair_maps[i-1], keys, pair_maps[i]);
		cerr << pair_maps[i].size() << " groups of size " << i << endl;
	}

	print_words(pair_maps, mask_to_words);

	return 0;
}
