#pragma once

#include "../vigilance.h"

typedef struct String {
    struct Handle *handle;
} String;

String string_create(const char *char_ptr);

String string_format(const char* format, ...);

void string_destroy(String *string);

char string_char_at(const String *string, int32_t index);

void string_set_char(String *string, int32_t index, char character);

void string_replace(String *string, char character, char by);

void string_append(String *string, char character);

void string_append_all(String *string, const char *char_ptr);

void string_concat(String *string, const String *other);

void string_remove(String *string, char element);

void string_remove_at(String *string, int32_t index);

String string_substring(const String *string, int32_t start, int32_t end);

void string_reserve(String *string, int32_t new_capacity);

void string_shrink(String *string);

String string_trim(const String *string);

void string_lowercase(String *string);

void string_uppercase(String *string);

String string_to_uppercase(const String *string);

String string_to_lowercase(const String *string);

bool string_contains(const String *string, char character);

int32_t string_index_of(const String *string, char character);

bool string_is_empty(const String *string);

void string_clear(String *string);

int32_t string_equals(const String *string, const String *other);

int32_t string_equals_ignore_case(const String *string, const String *other);

int32_t string_compare(const String *string, const String *other);

int32_t string_compare_ignore_case(const String *string, const String *other);

String string_copy(const String *string);

struct Array_char string_to_array(const String *string);

char *string_to_ptr(const String *string);

char *string_data(const String *string);

int32_t string_length(const String *string);

int32_t string_capacity(const String *string);
