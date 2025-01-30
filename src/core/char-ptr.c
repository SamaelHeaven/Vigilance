#include "char-ptr.h"

#include "gc.h"

char *char_ptr_format(const char *format, ...) {
    ASSERT(format);
    va_list args;
    va_start(args, format);
    const int32_t len = vsnprintf(nullptr, 0, format, args);
    char *buffer = gc_malloc(len + 1);
    vsnprintf(buffer, len + 1, format, args);
    va_end(args);
    buffer[len] = '\0';
    return buffer;
}

bool char_ptr_is_empty(const char *char_ptr) {
    ASSERT(char_ptr);
    return *char_ptr == '\0';
}

int32_t char_ptr_equals(const char *char_ptr, const char *other) { return char_ptr_compare(char_ptr, other) == 0; }

int32_t char_ptr_equals_ignore_case(const char *char_ptr, const char *other) {
    return char_ptr_compare_ignore_case(char_ptr, other) == 0;
}

int32_t char_ptr_compare(const char *char_ptr, const char *other) {
    if (char_ptr == nullptr || other == nullptr) {
        return char_ptr == other ? 0 : char_ptr == nullptr ? -1 : 1;
    }
    return strcmp(char_ptr, other);
}

int32_t char_ptr_compare_ignore_case(const char *char_ptr, const char *other) {
    if (char_ptr == nullptr || other == nullptr) {
        return char_ptr == other ? 0 : char_ptr == nullptr ? -1 : 1;
    }
    while (*char_ptr && *other) {
        const unsigned char c1 = tolower(*char_ptr);
        const unsigned char c2 = tolower(*other);
        if (c1 != c2) {
            return c1 - c2;
        }
        char_ptr++;
        other++;
    }
    return (unsigned char) tolower(*char_ptr) - (unsigned char) tolower(*other);
}

char *char_ptr_copy(const char *char_ptr) {
    ASSERT(char_ptr);
    const int32_t len = strlen(char_ptr);
    char *buffer = gc_malloc(len + 1);
    memcpy(buffer, char_ptr, len);
    buffer[len] = '\0';
    return buffer;
}

bool char_ptr_starts_with(const char *char_ptr, const char *prefix) {
    ASSERT(char_ptr && prefix);
    const int32_t prefix_len = strlen(prefix);
    const int32_t str_len = strlen(char_ptr);
    if (prefix_len > str_len) {
        return false;
    }
    for (int32_t i = 0; i < prefix_len; ++i) {
        if (char_ptr[i] != prefix[i]) {
            return false;
        }
    }
    return true;
}

bool char_ptr_ends_with(const char *char_ptr, const char *suffix) {
    ASSERT(char_ptr && suffix);
    const int32_t suffix_len = strlen(suffix);
    const int32_t str_len = strlen(char_ptr);
    if (suffix_len > str_len) {
        return false;
    }
    for (int32_t i = 0; i < suffix_len; ++i) {
        if (char_ptr[str_len - suffix_len + i] != suffix[i]) {
            return false;
        }
    }
    return true;
}

bool char_ptr_contains(const char *char_ptr, const char *substr) {
    ASSERT(char_ptr && substr);
    return strstr(char_ptr, substr) != nullptr;
}

WritableArrayCharPtr char_ptr_split(const char *char_ptr, const char *delim) {
    ASSERT(delim);
    const ArrayCharPtr result = array_char_ptr_create();
    char *token = strtok(char_ptr_copy(char_ptr), delim);
    while (token) {
        array_char_ptr_add(result, token);
        token = strtok(nullptr, delim);
    }
    return result;
}
