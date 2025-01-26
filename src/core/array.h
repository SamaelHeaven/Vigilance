#pragma once

#include "../vigilance.h"
#include "string.h"

typedef struct Array {
    struct Handle *handle;
} Array;

typedef Array ReadonlyArray;

Array array_create(int32_t element_size);

void array_destroy(Array array);

ReadonlyArray array_readonly(Array array);

bool array_is_readonly(Array array);

void array_add(Array array, const void *element);

void array_add_at(Array array, int32_t index, const void *element);

void array_add_all(Array dest, Array src);

void array_concat(Array array, const void *elements, int32_t count);

void array_remove(Array array, const void *element);

void array_remove_at(Array array, int32_t index);

void array_remove_all(Array dest, Array src);

void array_remove_if(Array array, bool (*predicate)(const void *element));

bool array_contains(Array array, const void *element);

bool array_is_empty(Array array);

int32_t array_index_of(Array array, const void *element);

void array_clear(Array array);

void *array_get(Array array, int32_t index);

void array_set(Array array, int32_t index, const void *element);

void array_replace(Array array, const void *element, const void *by);

void array_reserve(Array array, int32_t new_capacity);

void array_shrink(Array array);

Array array_copy(Array array);

Array array_slice(Array array, int32_t begin, int32_t end);

void array_reverse(Array array);

void array_sort(Array array, int32_t (*comparator)(const void *a, const void *b));

void array_stable_sort(Array array, int32_t (*comparator)(const void *a, const void *b));

void array_for_each(Array array, void (*callback)(void *element));

void *array_to_ptr(Array array);

const void *array_data(Array array);

int32_t array_size(Array array);

int32_t array_capacity(Array array);

int32_t array_element_size(Array array);

#define DECLARE_ARRAY(type_name, namespace, el_type)                                                                   \
                                                                                                                       \
    typedef struct type_name {                                                                                         \
        struct Handle *handle;                                                                                         \
    } type_name;                                                                                                       \
                                                                                                                       \
    typedef type_name Readonly##type_name;                                                                             \
                                                                                                                       \
    inline static type_name namespace##_create(void) { return CAST(type_name, array_create(sizeof(el_type))); }        \
                                                                                                                       \
    inline static void namespace##_destroy(type_name array) { array_destroy(*(Array *) &array); }                      \
                                                                                                                       \
    inline static Readonly##type_name namespace##_readonly(type_name array) {                                          \
        return CAST(Readonly##type_name, array_readonly(*(Array *) &array));                                           \
    }                                                                                                                  \
                                                                                                                       \
    inline static bool namespace##_is_readonly(type_name array) { return array_is_readonly(*(Array *) &array); }       \
                                                                                                                       \
    inline static void namespace##_add(type_name array, el_type element) { array_add(*(Array *) &array, &element); }   \
                                                                                                                       \
    inline static void namespace##_add_at(type_name array, const int32_t index, el_type element) {                     \
        array_add_at(*(Array *) &array, index, &element);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_add_all(type_name dest, type_name src) {                                            \
        array_add_all(*(Array *) &dest, *(Array *) &src);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_concat(type_name array, el_type const *elements, int32_t count) {                   \
        array_concat(*(Array *) &array, elements, count);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove(type_name array, el_type element) {                                          \
        array_remove(*(Array *) &array, &element);                                                                     \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_at(type_name array, const int32_t index) {                                   \
        array_remove_at(*(Array *) &array, index);                                                                     \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_all(type_name dest, type_name src) {                                         \
        array_remove_all(*(Array *) &dest, *(Array *) &src);                                                           \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_if(type_name array, bool (*predicate)(el_type const *element)) {             \
        array_remove_if(*(Array *) &array, (bool (*)(const void *element)) predicate);                                 \
    }                                                                                                                  \
                                                                                                                       \
    inline static bool namespace##_contains(type_name array, el_type element) {                                        \
        return array_contains(*(Array *) &array, &element);                                                            \
    }                                                                                                                  \
                                                                                                                       \
    inline static bool namespace##_is_empty(type_name array) { return array_is_empty(*(Array *) &array); }             \
                                                                                                                       \
    inline static int32_t namespace##_index_of(type_name array, el_type element) {                                     \
        return array_index_of(*(Array *) &array, &element);                                                            \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_clear(type_name array) { array_clear(*(Array *) &array); }                          \
                                                                                                                       \
    inline static el_type namespace##_get(type_name array, const int32_t index) {                                      \
        return *(el_type *) array_get(*(Array *) &array, index);                                                       \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_set(type_name array, const int32_t index, el_type element) {                        \
        array_set(*(Array *) &array, index, &element);                                                                 \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_replace(type_name array, el_type element, el_type by) {                             \
        array_replace(*(Array *) &array, &element, &by);                                                               \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_reserve(type_name array, const int32_t new_capacity) {                              \
        array_reserve(*(Array *) &array, new_capacity);                                                                \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_sort(type_name array, int32_t (*comparator)(el_type const *a, el_type const *b)) {  \
        array_sort(*(Array *) &array, (int32_t(*)(const void *a, const void *b))(comparator));                         \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_stable_sort(type_name array,                                                        \
                                               int32_t (*comparator)(el_type const *a, el_type const *b)) {            \
        array_stable_sort(*(Array *) &array, (int32_t(*)(const void *a, const void *b)) comparator);                   \
    }                                                                                                                  \
    inline static void namespace##_shrink(type_name array) { array_shrink(*(Array *) &array); }                        \
                                                                                                                       \
    inline static type_name namespace##_copy(type_name array) {                                                        \
        return CAST(type_name, array_copy(*(Array *) &array));                                                         \
    }                                                                                                                  \
                                                                                                                       \
    inline static type_name namespace##_slice(type_name array, int32_t begin, int32_t end) {                           \
        return CAST(type_name, array_slice(*(Array *) &array, begin, end));                                            \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_reverse(type_name array) { array_reverse(*(Array *) &array); }                      \
                                                                                                                       \
    inline static void namespace##_for_each(type_name array, void (*callback)(el_type element)) {                      \
        for (int32_t i = 0; i < array_size(*(Array *) &array); ++i) {                                                  \
            callback(*(el_type *) array_get(*(Array *) &array, i));                                                    \
        }                                                                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static el_type *namespace##_to_ptr(type_name array) { return array_to_ptr(*(Array *) &array); }             \
                                                                                                                       \
    inline static el_type const *namespace##_data(type_name array) {                                                   \
        return (el_type const *) array_data(*(Array *) &array);                                                        \
    }                                                                                                                  \
                                                                                                                       \
    inline static int32_t namespace##_size(type_name array) { return array_size(*(Array *) &array); }                  \
                                                                                                                       \
    inline static int32_t namespace##_capacity(type_name array) { return array_capacity(*(Array *) &array); }          \
                                                                                                                       \
    inline static int32_t namespace##_element_size(type_name array) { return array_element_size(*(Array *) &array); }

DECLARE_ARRAY(ArrayInt8, array_int8, int8_t)

DECLARE_ARRAY(ArrayInt16, array_int16, int16_t)

DECLARE_ARRAY(ArrayInt32, array_int32, int32_t)

DECLARE_ARRAY(ArrayInt64, array_int64, int64_t)

DECLARE_ARRAY(ArrayUInt8, array_uint8, uint8_t)

DECLARE_ARRAY(ArrayUInt16, array_uint16, uint16_t)

DECLARE_ARRAY(ArrayUInt32, array_uint32, uint32_t)

DECLARE_ARRAY(ArrayUInt64, array_uint64, uint64_t)

DECLARE_ARRAY(ArraySize, array_size, size_t)

DECLARE_ARRAY(ArrayFloat, array_float, float)

DECLARE_ARRAY(ArrayDouble, array_double, double)

DECLARE_ARRAY(ArrayBool, array_bool, bool)

DECLARE_ARRAY(ArrayChar, array_char, char)

DECLARE_ARRAY(ArrayPtr, array_ptr, void *)

DECLARE_ARRAY(ArrayConstPtr, array_const_ptr, const void *)

DECLARE_ARRAY(ArrayCharPtr, array_char_ptr, char *)

DECLARE_ARRAY(ArrayConstCharPtr, array_const_char_ptr, const char *)

DECLARE_ARRAY(ArrayString, array_string, String)

DECLARE_ARRAY(ArrayReadonlyString, array_readonly_string, ReadonlyString)
