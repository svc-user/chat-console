import sys
import tiktoken

encoding = tiktoken.get_encoding("cl100k_base")


def main(input):
    tokenized = encoding.encode(input)
    [print("(" + str(token) + ", \"" + encoding.decode_single_token_bytes(token).decode('latin1') + "\")", end=', ') for token in tokenized]
    print()
    print(tokenized)


if __name__ == "__main__":
    main(sys.argv[1])
