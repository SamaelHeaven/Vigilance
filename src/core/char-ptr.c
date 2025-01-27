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
