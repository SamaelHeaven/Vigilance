#pragma once

#include "array.h"

char *char_ptr_format(const char *format, ...);

bool char_ptr_is_empty(const char *char_ptr);

int32_t char_ptr_equals(const char *char_ptr, const char *other);

int32_t char_ptr_equals_ignore_case(const char *char_ptr, const char *other);

int32_t char_ptr_compare(const char *char_ptr, const char *other);

int32_t char_ptr_compare_ignore_case(const char *char_ptr, const char *other);

char *char_ptr_copy(const char *char_ptr);

bool char_ptr_starts_with(const char *char_ptr, const char *prefix);

bool char_ptr_ends_with(const char *char_ptr, const char *suffix);

bool char_ptr_contains(const char *char_ptr, const char *substr);

DECLARE_ARRAY(ArrayCharPtr, array_char_ptr, char *)

DECLARE_ARRAY(ArrayConstCharPtr, array_const_char_ptr, const char *)

WritableArrayCharPtr char_ptr_split(const char *char_ptr, const char *delim);
