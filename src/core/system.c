#include "system.h"

void print(const char *format, ...) {
    assert(format);
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
    fflush(stdout);
}

void println(const char *format, ...) {
    assert(format);
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
    printf("\n");
    fflush(stdout);
}

void print_err(const char *format, ...) {
    assert(format);
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    fflush(stderr);
}

void println_err(const char *format, ...) {
    assert(format);
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    printf("\n");
    fflush(stderr);
}

char read_char(void) { return getchar(); }

WritableString read_line(void) {
    const String result = string_create(nullptr);
    char character;
    while ((character = (char) getchar()) != '\n' && character != EOF) {
        string_append(result, character);
    }
    return result;
}
