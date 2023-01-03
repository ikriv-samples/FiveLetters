import sys

EXPECTED_LENGTH = 5
LETTERS_IN_ALPHABET = 26
NUM_WORDS = 5


def print_stderr(message):
   print(message, file=sys.stderr)
   

def to_number(c):
  return ord(c) - ord('a')


def is_five_letter_word(word):
  if len(word) != EXPECTED_LENGTH:
    return False
  for c in word:
    b = to_number(c)
    if b < 0 or b >= LETTERS_IN_ALPHABET:
      return False
  return True
  

def read_input():
  lines = sys.stdin.readlines()
  words = (line.rstrip() for line in lines)
  return [word for word in words if is_five_letter_word(word)]


# Convert a word to a bitmask representing its letters.
# Returns zero if the word is not of the right length.
# If the word has 5 different letters, the mask would have 5 bits on
# E.g. both 'dowry' and 'rowdy' would be represented as 
# ------zyxwvutsrqponmlkjihgfedcba
# 00000001010000100100000000001000
def word_to_mask(word):
  if len(word) != EXPECTED_LENGTH:
    return 0
  result = 0;
  for c in word:
    bit = 1 << to_number(c)
    if result & bit:
      return 0 # a letter occurred twice
    result |= bit
  return result
  

# Insert a new item into a mapping from key to a list of items
def insert(map_int_to_list, key, val):
    existing_list = map_int_to_list.get(key)
    if existing_list:
      existing_list.append(val)
    else:
      map_int_to_list[key] = [val]

# Create a mapping from bitmask to a list of words (see word_to_mask)
def build_word_map(words):
  mask_to_words = {}
  for word in words:
    insert(mask_to_words, word_to_mask(word), word)
  return mask_to_words

def print_invalid_words(map):
  if 0 not in map:
    return
  print_stderr("Invalid words:")
  for word in map[0]:
    print_stderr(word)
  print_stderr("End invalid words")

# N-sequence map is a map from a mask to a list of pairs (head_mask, tail_mask).
# Each key represents a bitmask for a sequence of N non-colliding words.
# The key is mapped to a list of pairs. Each pair contains a bitmask for the head word, and a bitmask for a tail of length N-1
#
# combined_mask->[(head_mask1, tail_mask1), (head_mask2, tail_mask2), ...]
# For each i head_maski | tail_maski == combined_mask, and head_maski & tail_maski == 0
#
# The nand function takes an N-sequence map input_map and returns an N+1-sequence map, 
# in which head word masks are taken from word_mask, and tail masks are taken from the keys of input_map.
# This simulates adding a new non-collding word in front of an existing word sequence.
def nand(input_map, word_masks):
  output_map = {}
  for input_mask in input_map:
    for word_mask in word_masks:
      if input_mask & word_mask == 0:
        output_mask = input_mask | word_mask
        insert(output_map, output_mask, (word_mask, input_mask))
  return output_map

# pair_maps argument is an a list of word sequence maps
# pair_maps[0] is a dummy map
# pair_maps[1] is a 1-sequence map
# pair_maps[2] is a 2-sequence map
# ...
# pair_maps[5] is a 5-sequence map
#
# mask_to_words arguemnt is a map from a bitmask to a list of words
# prefix argument is a list of strings that contains known prefix of the non-colliding sequence, maybe empty.
# mask argument is a bitmask of the suffix
# 
# If the prefix is of length 5, it contains the entire sequence, and the function simply prints it
# If the prefix is shorter, the function looks up possible head words for the suffix in the values of pair_maps[n],
# where n is the suffix length (aka "level").
# It then appends the head word to the prefix and calls print_words_impl() recursively to handle the remaining suffix.
#
def print_words_impl(pair_maps, mask_to_words, prefix, mask):
  level = NUM_WORDS - len(prefix)
  if level == 0:
    # print all the words in the vector
    print(' '.join(prefix))
  else:
    pairs = pair_maps[level][mask]
    for (word_mask, suffix_mask) in pairs:
      words = mask_to_words[word_mask]
      last_word_of_prefix = prefix[-1] if prefix else ""
      for word in words:
        if word <= last_word_of_prefix:
          continue;
        prefix.append(word)
        print_words_impl(pair_maps, mask_to_words, prefix, suffix_mask)
        prefix.pop()

# Prints word sequences encoded in pair_maps
# pair_maps argument is an a list of word sequence maps
# pair_maps[0] is a dummy map
# pair_maps[1] is a 1-sequence map
# pair_maps[2] is a 2-sequence map
# ...
# pair_maps[5] is a 5-sequence map
# mask_to_words argument maps bitmasks to list of words
def print_words(pair_maps, mask_to_words):
  prefix = []
  for mask in pair_maps[-1]:
    print_words_impl(pair_maps, mask_to_words, prefix, mask)


def main():
  words = read_input()
  print_stderr(f"{len(words)} words")
  mask_to_words = build_word_map(words)
  print_invalid_words(mask_to_words)
  
  if 0 in mask_to_words:
    del mask_to_words[0]
  print_stderr(f"{len(mask_to_words)} unique keys")
  
  # start with a dummy map
  pair_maps = [{ 0 : [(0,0)] }]

  # for each i, build an i-sequence word map (see comment for 'nand()')
  for i in range(1, NUM_WORDS+1):
    new_map = nand(pair_maps[-1], mask_to_words.keys())
    print_stderr(f"{len(new_map)} groups of size {i}")
    pair_maps.append(new_map)

  # Print the words encoded in the series of maps
  print_words(pair_maps, mask_to_words)

  
if __name__ == "__main__":
  main()
