#pragma once

#include "../vigilance.h"
#include "string.h"

typedef struct Array {
    struct Handle *handle;
} Array;

Array array_create(int32_t element_size);

void array_destroy(Array *array);

void array_add(Array *array, const void *element);

void array_add_at(Array *array, int32_t index, const void *element);

void array_add_all(Array *dest, const Array *src);

void array_concat(Array *dest, const void *elements, int32_t count);

void array_remove(Array *array, const void *element);

void array_remove_at(Array *array, int32_t index);

void array_remove_all(Array *dest, const Array *src);

void array_remove_if(Array *array, bool (*predicate)(const void *element));

bool array_contains(const Array *array, const void *element);

bool array_is_empty(const Array *array);

int32_t array_index_of(const Array *array, const void *element);

void array_clear(Array *array);

void *array_get(const Array *array, int32_t index);

void array_set(Array *array, int32_t index, const void *element);

void array_replace(Array *array, const void *element, const void *by);

void array_reserve(Array *array, int32_t new_capacity);

void array_shrink(Array *array);

Array array_copy(const Array *array);

Array array_slice(const Array *array, int32_t begin, int32_t end);

void array_reverse(Array *array);

void array_sort(Array *array, int32_t (*comparator)(const void *a, const void *b));

void array_stable_sort(Array *array, int32_t (*comparator)(const void *a, const void *b));

void array_for_each(const Array *array, void (*callback)(void *element));

void *array_to_ptr(const Array *array);

void *array_data(const Array *array);

int32_t array_size(const Array *array);

int32_t array_capacity(const Array *array);

int32_t array_element_size(const Array *array);

#define ARRAY_DEFINE(type, name)                                                                                       \
                                                                                                                       \
    typedef struct Array_##name {                                                                                      \
        struct Handle *handle;                                                                                         \
    } Array_##name;                                                                                                    \
                                                                                                                       \
    static inline Array_##name array_##name##_create(void) {                                                           \
        return *((Array_##name *) &LVALUE(array_create(sizeof(type))));                                                \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_destroy(Array_##name *array) { array_destroy((Array *) array); }                 \
                                                                                                                       \
    static inline void array_##name##_add(Array_##name *array, type element) { array_add((Array *) array, &element); } \
                                                                                                                       \
    static inline void array_##name##_add_at(Array_##name *array, const int32_t index, type element) {                 \
        array_add_at((Array *) array, index, &element);                                                                \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_add_all(Array_##name *dest, const Array_##name *src) {                           \
        array_add_all((Array *) dest, (Array *) src);                                                                  \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_concat(Array_##name *dest, const type *elements, int32_t count) {                \
        array_concat((Array *) dest, elements, count);                                                                 \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_remove(Array_##name *array, type element) {                                      \
        array_remove((Array *) array, &element);                                                                       \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_remove_at(Array_##name *array, const int32_t index) {                            \
        array_remove_at((Array *) array, index);                                                                       \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_remove_all(Array_##name *dest, const Array_##name *src) {                        \
        array_remove_all((Array *) dest, (Array *) src);                                                               \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_remove_if(Array_##name *array, bool (*predicate)(const type *element)) {         \
        array_remove_if((Array *) array, (bool (*)(const void *element)) predicate);                                   \
    }                                                                                                                  \
                                                                                                                       \
    static inline bool array_##name##_contains(const Array_##name *array, type element) {                              \
        return array_contains((Array *) array, &element);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    static inline bool array_##name##_is_empty(const Array_##name *array) { return array_is_empty((Array *) array); }  \
                                                                                                                       \
    static inline int32_t array_##name##_index_of(const Array_##name *array, type element) {                           \
        return array_index_of((Array *) array, &element);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_clear(Array_##name *array) { array_clear((Array *) array); }                     \
                                                                                                                       \
    static inline type array_##name##_get(const Array_##name *array, const int32_t index) {                            \
        return *(type *) array_get((Array *) array, index);                                                            \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_set(Array_##name *array, const int32_t index, type element) {                    \
        array_set((Array *) array, index, &element);                                                                   \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_replace(Array_##name *array, type element, type by) {                            \
        array_replace((Array *) array, &element, &by);                                                                 \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_reserve(Array_##name *array, const int32_t new_capacity) {                       \
        array_reserve((Array *) array, new_capacity);                                                                  \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_sort(Array_##name *array, int32_t (*comparator)(const type *a, const type *b)) { \
        array_sort((Array *) array, (int32_t(*)(const void *a, const void *b))(comparator));                           \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_stable_sort(Array_##name *array,                                                 \
                                                  int32_t (*comparator)(const type *a, const type *b)) {               \
        array_stable_sort((Array *) array, (int32_t(*)(const void *a, const void *b)) comparator);                     \
    }                                                                                                                  \
    static inline void array_##name##_shrink(Array_##name *array) { array_shrink((Array *) array); }                   \
                                                                                                                       \
    static inline Array_##name array_##name##_copy(const Array_##name *array) {                                        \
        return *((Array_##name *) &LVALUE(array_copy((Array *) array)));                                               \
    }                                                                                                                  \
                                                                                                                       \
    static inline Array_##name array_##name##_slice(const Array_##name *array, int32_t begin, int32_t end) {           \
        return *((Array_##name *) &LVALUE(array_slice((Array *) array, begin, end)));                                  \
    }                                                                                                                  \
                                                                                                                       \
    static inline void array_##name##_reverse(Array_##name *array) { array_reverse((Array *) array); }                 \
                                                                                                                       \
    static inline void array_##name##_for_each(const Array_##name *array, void (*callback)(type element)) {            \
        for (int32_t i = 0; i < array_size((Array *) array); ++i) {                                                    \
            callback(*(type *) array_get((Array *) array, i));                                                         \
        }                                                                                                              \
    }                                                                                                                  \
                                                                                                                       \
    static inline type *array_##name##_to_ptr(const Array_##name *array) { return array_to_ptr((Array *) array); }     \
                                                                                                                       \
    static inline type *array_##name##_data(const Array_##name *array) { return array_data((Array *) array); }         \
                                                                                                                       \
    static inline int32_t array_##name##_size(const Array_##name *array) { return array_size((Array *) array); }       \
                                                                                                                       \
    static inline int32_t array_##name##_capacity(const Array_##name *array) {                                         \
        return array_capacity((Array *) array);                                                                        \
    }                                                                                                                  \
                                                                                                                       \
    static inline int32_t array_##name##_element_size(const Array_##name *array) {                                     \
        return array_element_size((Array *) array);                                                                    \
    }

ARRAY_DEFINE(int8_t, int8)

ARRAY_DEFINE(int16_t, int16)

ARRAY_DEFINE(int32_t, int32)

ARRAY_DEFINE(int64_t, int64)

ARRAY_DEFINE(uint8_t, uint8)

ARRAY_DEFINE(uint16_t, uint16)

ARRAY_DEFINE(uint32_t, uint32)

ARRAY_DEFINE(uint64_t, uint64)

ARRAY_DEFINE(size_t, size)

ARRAY_DEFINE(float, float)

ARRAY_DEFINE(double, double)

ARRAY_DEFINE(bool, bool)

ARRAY_DEFINE(char, char)

ARRAY_DEFINE(void *, ptr)

ARRAY_DEFINE(char *, char_ptr)

ARRAY_DEFINE(String, string)
