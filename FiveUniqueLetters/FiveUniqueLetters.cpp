// Input list of words, either from stdin or from a file
// Output only words with 5 letters, all letters being different
//
// Usage:
// FiveUniqueLetters (reads from stdin)
// FiveUniqueLetters input (reads from input)
//
#include <iostream>
#include <fstream>

using namespace std;
const int VALID_LENGTH = 5;
const int LETTERS_IN_ALPHABET = 26;

bool is_valid_word(string const& word) {
    if (word.length() != VALID_LENGTH) return false;
    bool letters[LETTERS_IN_ALPHABET] = { 0 };
    bool is_good_word = true;
    for (auto c : word) {
        int index = c - 'a';
        if (index < 0 || index >= LETTERS_IN_ALPHABET) {
            cerr << "Invalid chars: " << word << endl;
            is_good_word = false;
            break;
        }
        if (letters[index]) {
            is_good_word = false;
            break;
        }
        letters[index] = true;
    }

    return is_good_word;
}

void process_input(istream& f) {
    while (f) {
        string line;
        f >> line;
        if (is_valid_word(line)) {
            cout << line << endl;
        }
    }
}

int main(int argc, char** argv)
{
    if (argc == 1) {
        process_input(cin);
    }
    else {
        for (int n = 1; n < argc; ++n) {
            ifstream input(argv[n]);
            process_input(input);
        }
    }
}
