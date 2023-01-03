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
  

def insert(map_int_to_list, key, val):
    existing_list = map_int_to_list.get(key)
    if existing_list:
      existing_list.append(val)
    else:
      map_int_to_list[key] = [val]

def build_word_map(words):
  mask_to_words = {}
  for word in words:
    insert(mask_to_words, word_to_mask(word), word)
  return mask_to_words


def print_invalid_words(map):
  if 0 not in map:
    return
  print_stderr("Ivnalid words:")
  for word in map[0]:
    print_stderr(word)
  print_stderr("End invalid words")

def nand(input_map, word_masks):
  output_map = {}
  for input_mask in input_map:
    for word_mask in word_masks:
      if input_mask & word_mask == 0:
        output_mask = input_mask | word_mask
        insert(output_map, output_mask, (word_mask, input_mask))
  return output_map


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
  
  pair_maps = [{ 0 : [(0,0)] }]

  for i in range(1, NUM_WORDS+1):
    new_map = nand(pair_maps[-1], mask_to_words.keys())
    print_stderr(f"{len(new_map)} groups of size {i}")
    pair_maps.append(new_map)

  print_words(pair_maps, mask_to_words)

  
if __name__ == "__main__":
  main()
