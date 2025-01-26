#include "string.h"

#include "array.h"
#include "char-ptr.h"

String string_create(const char *char_ptr) {
    ArrayChar string = array_char_create();
    const int32_t len = char_ptr ? strlen(char_ptr) : 0;
    array_char_reserve(string, len + 1);
    if (char_ptr) {
        array_char_concat(string, char_ptr, len);
    }
    array_char_add(string, '\0');
    return *(String *) &string;
}

String string_format(const char *format, ...) {
    va_list args;
    va_start(args, format);
    const int32_t len = vsnprintf(nullptr, 0, format, args);
    char *buffer = malloc(len + 1);
    vsnprintf(buffer, len + 1, format, args);
    va_end(args);
    buffer[len] = '\0';
    const String result = string_create(buffer);
    free(buffer);
    return result;
}

void string_destroy(const String string) { array_char_destroy(*(ArrayChar *) &string); }

ReadonlyString string_readonly(String string) {
    return CAST(ReadonlyString, array_char_readonly(*(ArrayChar *) &string));
}

bool string_is_readonly(String string) { return array_char_is_readonly(*(ArrayChar *) &string); }

char string_char_at(const String string, const int32_t index) {
    assert(index < string_length(string));
    return array_char_get(*(ArrayChar *) &string, index);
}

void string_set_char(const String string, const int32_t index, const char character) {
    assert(index < string_length(string));
    array_char_set(*(ArrayChar *) &string, index, character);
}

void string_replace(const String string, const char character, const char by) {
    array_char_replace(*(ArrayChar *) &string, character, by);
    array_char_set(*(ArrayChar *) &string, string_length(string), '\0');
}

void string_append(const String string, const char character) {
    array_char_add(*(ArrayChar *) &string, '\0');
    array_char_set(*(ArrayChar *) &string, string_length(string) - 1, character);
}

void string_concat(const String string, const char *char_ptr) {
    assert(char_ptr);
    array_char_remove_at(*(ArrayChar *) &string, string_length(string));
    array_char_concat(*(ArrayChar *) &string, char_ptr, strlen(char_ptr));
    array_char_add(*(ArrayChar *) &string, '\0');
}

void string_remove(const String string, const char element) {
    array_char_remove(*(ArrayChar *) &string, element);
    array_char_set(*(ArrayChar *) &string, string_length(string), '\0');
}

void string_remove_at(const String string, const int32_t index) {
    assert(index < string_length(string));
    array_char_remove_at(*(ArrayChar *) &string, index);
}

void string_remove_if(const String string, bool (*predicate)(char element)) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        if (predicate(array_char_get(*(ArrayChar *) &string, i))) {
            array_char_remove_at(*(ArrayChar *) &string, i);
            i--;
        }
    }
}

void string_reverse(const String string) {
    array_char_remove_at(*(ArrayChar *) &string, string_length(string));
    array_char_reverse(*(ArrayChar *) &string);
    array_char_add(*(ArrayChar *) &string, '\0');
}

String string_substring(const String string, const int32_t start, const int32_t end) {
    assert(end <= string_length(string));
    ArrayChar result = array_char_slice(*(ArrayChar *) &string, start, end);
    array_char_add(result, '\0');
    return *(String *) &result;
}

void string_reserve(const String string, const int32_t new_capacity) {
    array_char_reserve(*(ArrayChar *) &string, new_capacity);
}

void string_shrink(const String string) { array_char_shrink(*(ArrayChar *) &string); }

String string_trim(const String string) {
    const int32_t len = string_length(string);
    int32_t start = 0;
    while (start < len && isspace(array_char_get(*(ArrayChar *) &string, start))) {
        start++;
    }
    int32_t end = len - 1;
    while (end >= start && isspace(array_char_get(*(ArrayChar *) &string, end))) {
        end--;
    }
    return start <= end ? string_substring(string, start, end + 1) : string_create(nullptr);
}

void string_lowercase(const String string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        array_char_set(*(ArrayChar *) &string, i, tolower(array_char_get(*(ArrayChar *) &string, i)));
    }
}

void string_uppercase(const String string) {
    for (int32_t i = 0; i < string_length(string); ++i) {
        array_char_set(*(ArrayChar *) &string, i, toupper(array_char_get(*(ArrayChar *) &string, i)));
    }
}

String string_to_uppercase(const String string) {
    const String result = string_copy(string);
    string_uppercase(result);
    return result;
}

String string_to_lowercase(const String string) {
    const String result = string_copy(string);
    string_lowercase(result);
    return result;
}

bool string_is_empty(const String string) { return array_char_is_empty(*(ArrayChar *) &string); }

bool string_contains(const String string, const char character) { return string_index_of(string, character) != -1; }

int32_t string_index_of(const String string, const char character) {
    const int32_t result = array_char_index_of(*(ArrayChar *) &string, character);
    return result == string_length(string) ? -1 : result;
}

void string_clear(const String string) { array_char_clear(*(ArrayChar *) &string); }

int32_t string_equals(const String string, const char *other) { return string_compare(string, other) == 0; }

int32_t string_equals_ignore_case(const String string, const char *other) {
    return string_compare_ignore_case(string, other) == 0;
}

int32_t string_compare(const String string, const char *other) {
    return char_ptr_compare(array_char_data(*(ArrayChar *) &string), other);
}

int32_t string_compare_ignore_case(const String string, const char *other) {
    return char_ptr_compare_ignore_case(array_char_data(*(ArrayChar *) &string), other);
}

String string_copy(const String string) { return CAST(String, array_char_copy(*(ArrayChar *) &string)); }

ArrayChar string_to_array(const String string) { return array_char_copy(*(ArrayChar *) &string); }

char *string_to_ptr(const String string) { return array_char_to_ptr(*(ArrayChar *) &string); }

const char *string_data(const String string) { return array_char_data(*(ArrayChar *) &string); }

int32_t string_length(const String string) { return array_char_size(*(ArrayChar *) &string) - 1; }

int32_t string_capacity(const String string) { return array_char_capacity(*(ArrayChar *) &string); }
