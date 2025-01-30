#pragma once

#include "array.h"

char *char_ptr_format(const char *format, ...);

int32_t char_ptr_equals(const char *char_ptr, const char *other);

int32_t char_ptr_equals_ignore_case(const char *char_ptr, const char *other);

int32_t char_ptr_compare(const char *char_ptr, const char *other);

int32_t char_ptr_compare_ignore_case(const char *char_ptr, const char *other);

char *char_ptr_copy(const char *char_ptr);

DECLARE_ARRAY(ArrayCharPtr, array_char_ptr, char *)

DECLARE_ARRAY(ArrayConstCharPtr, array_const_char_ptr, const char *)
