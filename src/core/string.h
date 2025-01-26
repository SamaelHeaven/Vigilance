#pragma once

#include "../vigilance.h"

typedef struct String {
    struct Handle *handle;
} String;

typedef String WritableString;

typedef String ReadonlyString;

WritableString string_create(const char *char_ptr);

WritableString string_format(const char *format, ...);

void string_destroy(WritableString string);

ReadonlyString string_readonly(String string);

bool string_is_readonly(String string);

void string_assert_writable(String string);

char string_char_at(String string, int32_t index);

void string_set_char(WritableString string, int32_t index, char character);

void string_replace(WritableString string, char character, char by);

void string_append(WritableString string, char character);

void string_concat(WritableString string, const char *char_ptr);

void string_remove(WritableString string, char element);

void string_remove_at(WritableString string, int32_t index);

void string_remove_if(WritableString string, bool (*predicate)(char element));

void string_reverse(WritableString string);

WritableString string_substring(String string, int32_t start, int32_t end);

void string_reserve(WritableString string, int32_t new_capacity);

void string_shrink(WritableString string);

WritableString string_trim(String string);

void string_lowercase(WritableString string);

void string_uppercase(WritableString string);

WritableString string_to_uppercase(String string);

WritableString string_to_lowercase(String string);

bool string_contains(String string, char character);

int32_t string_index_of(String string, char character);

bool string_is_empty(String string);

void string_clear(WritableString string);

int32_t string_equals(String string, const char *other);

int32_t string_equals_ignore_case(String string, const char *other);

int32_t string_compare(String string, const char *other);

int32_t string_compare_ignore_case(String string, const char *other);

WritableString string_copy(String string);

struct ArrayChar string_to_array(String string);

char *string_to_ptr(String string);

const char *string_data(String string);

int32_t string_length(String string);

int32_t string_capacity(String string);
