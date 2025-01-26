#include "console.h"

void console_print(const char *format, ...) {
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
    fflush(stdout);
}

void console_println(const char *format, ...) {
    va_list args;
    va_start(args, format);
    vprintf(format, args);
    va_end(args);
    printf("\n");
    fflush(stdout);
}

void console_print_err(const char *format, ...) {
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    fflush(stderr);
}

void console_println_err(const char *format, ...) {
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    printf("\n");
    fflush(stderr);
}

char console_read_char() { return getchar(); }

String console_read_line() {
    String result = string_create(NULL);
    char character;
    while ((character = (char) getchar()) != '\n' && character != EOF) {
        string_append(&result, character);
    }
    return result;
}
