#pragma once

#include "../vigilance.h"

bool parse_bool(const char *char_ptr);

bool parse_char(const char *char_ptr, char *out);

bool parse_int8(const char *char_ptr, int8_t *out);

bool parse_int16(const char *char_ptr, int16_t *out);

bool parse_int32(const char *char_ptr, int32_t *out);

bool parse_int64(const char *char_ptr, int64_t *out);

bool parse_uint8(const char *char_ptr, uint8_t *out);

bool parse_uint16(const char *char_ptr, uint16_t *out);

bool parse_uint32(const char *char_ptr, uint32_t *out);

bool parse_uint64(const char *char_ptr, uint64_t *out);

bool parse_float(const char *char_ptr, float *out);

bool parse_double(const char *char_ptr, double *out);
