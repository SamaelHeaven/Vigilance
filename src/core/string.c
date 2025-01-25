#include "string.h"

#include "array.h"

String string_create(const char *char_ptr) {
    Array_char string = array_char_create();
    if (char_ptr) {
        array_char_concat(&string, char_ptr, strlen(char_ptr));
    }
    array_char_add(&string, '\0');
    return *(String *) &string;
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

void string_destroy(String *string) { array_char_destroy((Array_char *) string); }

char string_char_at(const String *string, const int32_t index) {
    assert(index < string_length(string));
    return array_char_get((Array_char *) string, index);
}

void string_set_char(String *string, const int32_t index, const char character) {
    assert(index < string_length(string));
    array_char_set((Array_char *) string, index, character);
}

void string_replace(String *string, const char character, const char by) {
    array_char_replace((Array_char *) string, character, by);
    array_char_data((Array_char *) string)[string_length(string)] = '\0';
}

void string_append(String *string, const char character) {
    array_char_add((Array_char *) string, '\0');
    array_char_set((Array_char *) string, string_length(string) - 1, character);
}

void string_concat(String *string, const char *char_ptr) {
    assert(char_ptr);
    array_char_remove_at((Array_char *) string, string_length(string));
    array_char_concat((Array_char *) string, char_ptr, strlen(char_ptr));
    array_char_add((Array_char *) string, '\0');
}

void string_remove(String *string, const char element) {
    array_char_remove((Array_char *) string, element);
    array_char_data((Array_char *) string)[string_length(string)] = '\0';
}

void string_remove_at(String *string, const int32_t index) {
    assert(index < string_length(string));
    array_char_remove_at((Array_char *) string, index);
}

String string_substring(const String *string, const int32_t start, const int32_t end) {
    assert(end <= string_length(string));
    Array_char result = array_char_slice((Array_char *) string, start, end);
    array_char_add(&result, '\0');
    return *(String *) &result;
}

void string_reserve(String *string, const int32_t new_capacity) {
    array_char_reserve((Array_char *) string, new_capacity);
}

void string_shrink(String *string) { array_char_shrink((Array_char *) string); }

String string_trim(const String *string) {
    const int32_t len = string_length(string);
    int32_t start = 0;
    while (start < len && isspace(array_char_get((Array_char *) string, start))) {
        start++;
    }
    int32_t end = len - 1;
    while (end >= start && isspace(array_char_get((Array_char *) string, end))) {
        end--;
    }
    return start <= end ? string_substring(string, start, end + 1) : string_create("");
}

void string_lowercase(String *string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        array_char_set((Array_char *) string, i, tolower(array_char_get((Array_char *) string, i)));
    }
}

void string_uppercase(String *string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        array_char_set((Array_char *) string, i, toupper(array_char_get((Array_char *) string, i)));
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

bool string_is_empty(const String *string) { return array_char_is_empty((Array_char *) string); }

bool string_contains(const String *string, const char character) { return string_index_of(string, character) != -1; }

int32_t string_index_of(const String *string, const char character) {
    const int32_t result = array_char_index_of((Array_char *) string, character);
    return result == string_length(string) ? -1 : result;
}

void string_clear(String *string) { array_char_clear((Array_char *) string); }

int32_t string_equals(const String *string, const char *other) { return string_compare(string, other) == 0; }

int32_t string_equals_ignore_case(const String *string, const char *other) {
    return string_compare_ignore_case(string, other) == 0;
}

int32_t string_compare(const String *string, const char *other) {
    return strcmp(array_char_data((Array_char *) string), other);
}

int32_t string_compare_ignore_case(const String *string, const char *other) {
    const char *data_1 = array_char_data((Array_char *) string);
    const char *data_2 = other;
    if (data_1 == NULL || data_2 == NULL) {
        return data_1 == data_2 ? 0 : data_1 == NULL ? -1 : 1;
    }
    while (*data_1 && *data_2) {
        const unsigned char c1 = tolower(*data_1);
        const unsigned char c2 = tolower(*data_2);
        if (c1 != c2) {
            return c1 - c2;
        }
        data_1++;
        data_2++;
    }
    return (unsigned char) tolower(*data_1) - (unsigned char) tolower(*data_2);
}

String string_copy(const String *string) { return *(String *) &LVALUE(array_char_copy((Array_char *) string)); }

Array_char string_to_array(const String *string) { return array_char_copy((Array_char *) string); }

char *string_to_ptr(const String *string) { return array_char_to_ptr((Array_char *) string); }

char *string_data(const String *string) { return array_char_data((Array_char *) string); }

int32_t string_length(const String *string) { return array_char_size((Array_char *) string) - 1; }

int32_t string_capacity(const String *string) { return array_char_capacity((Array_char *) string); }
