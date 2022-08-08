// FiveWords.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <vector>
#include <map>
#include <algorithm>
#include <array>
#include <limits.h>

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

int word_to_int(string& word) {
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

void build_words_map(vector<string>& words, multimap<int, string>& words_map) {
	for (auto word : words) {
		if (word.length() == 0) continue;
		words_map.insert(make_pair(word_to_int(word), word));
	}
}

void print_invalid_words(multimap<int, string> const& words_map) {
	if (words_map.count(0) == 0) return;

	cerr << "Invalid words: " << endl;
	auto bad_words = words_map.equal_range(0);
	for_each(bad_words.first, bad_words.second, [](pair<int, string> const& entry) {
		cerr << entry.second << endl;
		});
	cerr << "End invalid words" << endl;
}

typedef __int64 int64;

void get_sorted_keys(multimap<int, string> const& words_map, vector<int>& keys) {
	for (auto entry : words_map) {
		keys.push_back(entry.first);
	}
	sort(keys.begin(), keys.end());
}
typedef vector<array<int, NUM_WORDS>> group_vector;

void keys_to_groups(vector<int> const& keys, group_vector& groups) {
	groups.reserve(keys.size());
	for (auto key : keys) {
		array<int,NUM_WORDS> group = { key };
		groups.push_back(group);
	}
}

void find_groups(group_vector const& input_groups, int input_group_size, vector<int> const& keys, group_vector& output_groups) {
	for (array<int,5> const& input_group : input_groups) {
		int last = input_group[input_group_size - 1];
		for (auto key : keys) {
			if (key <= last) continue;
			bool good_key = true;
			for (int i = 0; i < input_group_size; ++i) {
				if ((key & input_group[i]) != 0) {
					good_key = false;
					break;
				}
			}
			if (good_key) {
				array<int, NUM_WORDS> new_group;
				for (int i = 0; i < input_group_size; ++i) {
					new_group[i] = input_group[i];
				}
				new_group[input_group_size] = key;
				output_groups.push_back(new_group);
			}
		}
	}
}

int main()
{
	auto int_bits = sizeof(int64) * CHAR_BIT;
	if (int_bits < 64) {
		cerr << "At least 64-bit integers are required, " << int_bits << " bits detected" << endl;
		return 1;
	}
	vector<string> words;
	read_input(cin, words);
	cerr << words.size() << " words" << endl;

	multimap<int, string> words_map;
	build_words_map(words, words_map);

	print_invalid_words(words_map);

	vector<int> keys;
	get_sorted_keys(words_map, keys);

	group_vector groups;
	keys_to_groups(keys, groups);

	try {
		for (int group_size = 1; group_size < NUM_WORDS; ++group_size) {
			group_vector new_groups;
			find_groups(groups, group_size, keys, new_groups);
			cerr << new_groups.size() << " groups of size " << group_size + 1 << endl;
			groups = new_groups;
		}
	}
	catch (exception const& e)
	{
		cerr << "Error: " << e.what();
		return 2;
	}

	return 0;
}
