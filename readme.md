# Five Letters
The task: find groups of 5 English words in which each letter appears at most once, i.e. 25 letters total. The important part is that these must be "real" English words from the dictionary.
For example:
`fjord gucks nymph vibex waltz`

Of course, the "reality" of these words is sometimes questionable, e.g. the dictionary contains "words" like "hdqrs" and "pbxes", but that's beyond the point, our job is to write the algorithm.

The dictionary is taken from https://github.com/dwyl/english-words/blob/master/words_alpha.txt. There are other dictionaries out there, but this seems to be contain the largest percentage of 'real' words.

We divide the problem into three parts:
- `FiveUniqueLetters`: a program that takes a dictionary and selects 5-letter words with no repeating letters.
- `FiveWords`: a program that takes a list of 5 letter words, and outputs 5-word groups with no repeating letters. There is some diagnostics output to stderr

`go.bat` runs both programs in a pipe, and also measures the execution time.