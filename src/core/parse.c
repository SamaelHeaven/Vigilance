#include "parse.h"

#include "char-ptr.h"

bool parse_bool(const char *char_ptr) {
    assert(char_ptr);
    return char_ptr_equals_ignore_case(char_ptr, "true");
}

bool parse_int8(const char *char_ptr, int8_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const int32_t value = strtol(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value < INT8_MIN || value > INT8_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_int16(const char *char_ptr, int16_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const int32_t value = strtol(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value < INT16_MIN || value > INT16_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_int32(const char *char_ptr, int32_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const int32_t value = strtol(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value < INT32_MIN || value > INT32_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_int64(const char *char_ptr, int64_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const int64_t value = strtoll(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_uint8(const char *char_ptr, uint8_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const uint32_t value = strtoul(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value > UINT8_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_uint16(const char *char_ptr, uint16_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const uint32_t value = strtoul(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value > UINT16_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_uint32(const char *char_ptr, uint32_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const uint32_t value = strtoul(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end || value > UINT32_MAX) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_uint64(const char *char_ptr, uint64_t *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const uint64_t value = strtoull(char_ptr, &end, 10);
    if (char_ptr == end || errno == ERANGE || *end) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_float(const char *char_ptr, float *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const float value = strtof(char_ptr, &end);
    if (char_ptr == end || errno == ERANGE || *end) {
        return false;
    }
    *out = value;
    return true;
}

bool parse_double(const char *char_ptr, double *out) {
    assert(char_ptr && out);
    errno = 0;
    char *end;
    const double value = strtod(char_ptr, &end);
    if (char_ptr == end || errno == ERANGE || *end) {
        return false;
    }
    *out = value;
    return true;
}
