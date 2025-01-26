#include "string.h"

#include "array.h"

String string_create(const char *char_ptr) {
    CharArray string = char_array_create();
    const int32_t len = char_ptr ? strlen(char_ptr) : 0;
    char_array_reserve(&string, len + 1);
    if (char_ptr) {
        char_array_concat(&string, char_ptr, len);
    }
    char_array_add(&string, '\0');
    return CAST(String, string);
}

String string_format(const char *format, ...) {
    va_list args;
    va_start(args, format);
    const int32_t len = vsnprintf(NULL, 0, format, args);
    char *buffer = malloc(len + 1);
    vsnprintf(buffer, len + 1, format, args);
    va_end(args);
    buffer[len] = '\0';
    const String result = string_create(buffer);
    free(buffer);
    return result;
}

void string_destroy(String *string) { char_array_destroy((CharArray *) string); }

char string_char_at(const String *string, const int32_t index) {
    assert(index < string_length(string));
    return char_array_get((CharArray *) string, index);
}

void string_set_char(String *string, const int32_t index, const char character) {
    assert(index < string_length(string));
    char_array_set((CharArray *) string, index, character);
}

void string_replace(String *string, const char character, const char by) {
    char_array_replace((CharArray *) string, character, by);
    char_array_set((CharArray *) string, string_length(string), '\0');
}

void string_append(String *string, const char character) {
    char_array_add((CharArray *) string, '\0');
    char_array_set((CharArray *) string, string_length(string) - 1, character);
}

void string_concat(String *string, const char *char_ptr) {
    assert(char_ptr);
    char_array_remove_at((CharArray *) string, string_length(string));
    char_array_concat((CharArray *) string, char_ptr, strlen(char_ptr));
    char_array_add((CharArray *) string, '\0');
}

void string_remove(String *string, const char element) {
    char_array_remove((CharArray *) string, element);
    char_array_set((CharArray *) string, string_length(string), '\0');
}

void string_remove_at(String *string, const int32_t index) {
    assert(index < string_length(string));
    char_array_remove_at((CharArray *) string, index);
}

void string_reverse(String *string) {
    char_array_remove_at((CharArray *) string, string_length(string));
    char_array_reverse((CharArray *) string);
    char_array_add((CharArray *) string, '\0');
}

String string_substring(const String *string, const int32_t start, const int32_t end) {
    assert(end <= string_length(string));
    CharArray result = char_array_slice((CharArray *) string, start, end);
    char_array_add(&result, '\0');
    return CAST(String, result);
}

void string_reserve(String *string, const int32_t new_capacity) {
    char_array_reserve((CharArray *) string, new_capacity);
}

void string_shrink(String *string) { char_array_shrink((CharArray *) string); }

String string_trim(const String *string) {
    const int32_t len = string_length(string);
    int32_t start = 0;
    while (start < len && isspace(char_array_get((CharArray *) string, start))) {
        start++;
    }
    int32_t end = len - 1;
    while (end >= start && isspace(char_array_get((CharArray *) string, end))) {
        end--;
    }
    return start <= end ? string_substring(string, start, end + 1) : string_create(NULL);
}

void string_lowercase(String *string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        char_array_set((CharArray *) string, i, tolower(char_array_get((CharArray *) string, i)));
    }
}

void string_uppercase(String *string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        char_array_set((CharArray *) string, i, toupper(char_array_get((CharArray *) string, i)));
    }
}

String string_to_uppercase(const String *string) {
    String result = string_copy(string);
    string_uppercase(&result);
    return result;
}

String string_to_lowercase(const String *string) {
    String result = string_copy(string);
    string_lowercase(&result);
    return result;
}

bool string_is_empty(const String *string) { return char_array_is_empty((CharArray *) string); }

bool string_contains(const String *string, const char character) { return string_index_of(string, character) != -1; }

int32_t string_index_of(const String *string, const char character) {
    const int32_t result = char_array_index_of((CharArray *) string, character);
    return result == string_length(string) ? -1 : result;
}

void string_clear(String *string) { char_array_clear((CharArray *) string); }

int32_t string_equals(const String *string, const char *other) { return string_compare(string, other) == 0; }

int32_t string_equals_ignore_case(const String *string, const char *other) {
    return string_compare_ignore_case(string, other) == 0;
}

int32_t string_compare(const String *string, const char *other) {
    return strcmp(char_array_data((CharArray *) string), other);
}

int32_t string_compare_ignore_case(const String *string, const char *other) {
    const char *data = char_array_data((CharArray *) string);
    if (data == NULL || other == NULL) {
        return data == other ? 0 : data == NULL ? -1 : 1;
    }
    while (*data && *other) {
        const unsigned char c1 = tolower(*data);
        const unsigned char c2 = tolower(*other);
        if (c1 != c2) {
            return c1 - c2;
        }
        data++;
        other++;
    }
    return (unsigned char) tolower(*data) - (unsigned char) tolower(*other);
}

String string_copy(const String *string) { return *(String *) &LVALUE(char_array_copy((CharArray *) string)); }

CharArray string_to_array(const String *string) { return char_array_copy((CharArray *) string); }

char *string_to_ptr(const String *string) { return char_array_to_ptr((CharArray *) string); }

const char *string_data(const String *string) { return char_array_data((CharArray *) string); }

int32_t string_length(const String *string) { return char_array_size((CharArray *) string) - 1; }

int32_t string_capacity(const String *string) { return char_array_capacity((CharArray *) string); }
