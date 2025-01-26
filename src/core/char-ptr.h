#pragma once

#include "../vigilance.h"

char *char_ptr_format(const char *format, ...);

int32_t char_ptr_equals(const char *char_ptr, const char *other);

int32_t char_ptr_equals_ignore_case(const char *char_ptr, const char *other);

int32_t char_ptr_compare(const char *char_ptr, const char *other);

int32_t char_ptr_compare_ignore_case(const char *char_ptr, const char *other);

char *char_ptr_copy(const char *char_ptr);
