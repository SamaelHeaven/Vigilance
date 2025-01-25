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

void array_concat(Array *array, const void *elements, int32_t count);

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

const void *array_data(const Array *array);

int32_t array_size(const Array *array);

int32_t array_capacity(const Array *array);

int32_t array_element_size(const Array *array);

#define DECLARE_ARRAY(type_name, namespace, el_type, ...)                                                              \
                                                                                                                       \
    typedef struct type_name {                                                                                         \
        struct Handle *handle;                                                                                         \
    } type_name;                                                                                                       \
                                                                                                                       \
    inline static type_name namespace##_create(void) {                                                                 \
        return CAST(type_name, array_create(sizeof(__VA_ARGS__ el_type)));                                             \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_destroy(type_name *array) { array_destroy((Array *) array); }                       \
                                                                                                                       \
    inline static void namespace##_add(type_name *array COMMA __VA_ARGS__ el_type element) {                           \
        array_add((Array *) array, &element);                                                                          \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_add_at(type_name *array, const int32_t index COMMA __VA_ARGS__ el_type element) {   \
        array_add_at((Array *) array, index, &element);                                                                \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_add_all(type_name *dest, const type_name *src) {                                    \
        array_add_all((Array *) dest, (Array *) src);                                                                  \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_concat(type_name *array COMMA __VA_ARGS__ el_type const *elements, int32_t count) { \
        array_concat((Array *) array, elements, count);                                                                \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove(type_name *array COMMA __VA_ARGS__ el_type element) {                        \
        array_remove((Array *) array, &element);                                                                       \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_at(type_name *array, const int32_t index) {                                  \
        array_remove_at((Array *) array, index);                                                                       \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_all(type_name *dest, const type_name *src) {                                 \
        array_remove_all((Array *) dest, (Array *) src);                                                               \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_remove_if(type_name *array,                                                         \
                                             bool (*predicate)(__VA_ARGS__ el_type const *element)) {                  \
        array_remove_if((Array *) array, (bool (*)(const void *element)) predicate);                                   \
    }                                                                                                                  \
                                                                                                                       \
    inline static bool namespace##_contains(const type_name *array COMMA __VA_ARGS__ el_type element) {                \
        return array_contains((Array *) array, &element);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static bool namespace##_is_empty(const type_name *array) { return array_is_empty((Array *) array); }        \
                                                                                                                       \
    inline static int32_t namespace##_index_of(const type_name *array COMMA __VA_ARGS__ el_type element) {             \
        return array_index_of((Array *) array, &element);                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_clear(type_name *array) { array_clear((Array *) array); }                           \
                                                                                                                       \
    inline static __VA_ARGS__ el_type namespace##_get(const type_name *array, const int32_t index) {                   \
        return *(__VA_ARGS__ el_type *) array_get((Array *) array, index);                                             \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_set(type_name *array, const int32_t index COMMA __VA_ARGS__ el_type element) {      \
        array_set((Array *) array, index, &element);                                                                   \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_replace(                                                                            \
            type_name *array COMMA __VA_ARGS__ el_type element COMMA __VA_ARGS__ el_type by) {                         \
        array_replace((Array *) array, &element, &by);                                                                 \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_reserve(type_name *array, const int32_t new_capacity) {                             \
        array_reserve((Array *) array, new_capacity);                                                                  \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_sort(                                                                               \
            type_name *array,                                                                                          \
            int32_t (*comparator)(__VA_ARGS__ el_type const *a COMMA __VA_ARGS__ el_type const *b)) {                  \
        array_sort((Array *) array, (int32_t(*)(const void *a, const void *b))(comparator));                           \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_stable_sort(                                                                        \
            type_name *array,                                                                                          \
            int32_t (*comparator)(__VA_ARGS__ el_type const *a COMMA __VA_ARGS__ el_type const *b)) {                  \
        array_stable_sort((Array *) array, (int32_t(*)(const void *a, const void *b)) comparator);                     \
    }                                                                                                                  \
    inline static void namespace##_shrink(type_name *array) { array_shrink((Array *) array); }                         \
                                                                                                                       \
    inline static type_name namespace##_copy(const type_name *array) {                                                 \
        return CAST(type_name, array_copy((Array *) array));                                                           \
    }                                                                                                                  \
                                                                                                                       \
    inline static type_name namespace##_slice(const type_name *array, int32_t begin, int32_t end) {                    \
        return CAST(type_name, array_slice((Array *) array, begin, end));                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static void namespace##_reverse(type_name *array) { array_reverse((Array *) array); }                       \
                                                                                                                       \
    inline static void namespace##_for_each(const type_name *array, void (*callback)(__VA_ARGS__ el_type element)) {   \
        for (int32_t i = 0; i < array_size((Array *) array); ++i) {                                                    \
            callback(*(__VA_ARGS__ el_type *) array_get((Array *) array, i));                                          \
        }                                                                                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static __VA_ARGS__ el_type *namespace##_to_ptr(const type_name *array) {                                    \
        return array_to_ptr((Array *) array);                                                                          \
    }                                                                                                                  \
                                                                                                                       \
    inline static __VA_ARGS__ el_type const *namespace##_data(const type_name *array) {                                \
        return (__VA_ARGS__ el_type const *) array_data((Array *) array);                                              \
    }                                                                                                                  \
                                                                                                                       \
    inline static int32_t namespace##_size(const type_name *array) { return array_size((Array *) array); }             \
                                                                                                                       \
    inline static int32_t namespace##_capacity(const type_name *array) { return array_capacity((Array *) array); }     \
                                                                                                                       \
    inline static int32_t namespace##_element_size(const type_name *array) {                                           \
        return array_element_size((Array *) array);                                                                    \
    }

DECLARE_ARRAY(Int8Array, int8_array, int8_t)

DECLARE_ARRAY(Int16Array, int16_array, int16_t)

DECLARE_ARRAY(Int32Array, int32_array, int32_t)

DECLARE_ARRAY(Int64Array, int64_array, int64_t)

DECLARE_ARRAY(UInt8Array, uint8_array, uint8_t)

DECLARE_ARRAY(UInt16Array, uint16_array, uint16_t)

DECLARE_ARRAY(UInt32Array, uint32_array, uint32_t)

DECLARE_ARRAY(UInt64Array, uint64_array, uint64_t)

DECLARE_ARRAY(SizeArray, size_array, size_t)

DECLARE_ARRAY(FloatArray, float_array, float)

DECLARE_ARRAY(DoubleArray, double_array, double)

DECLARE_ARRAY(BoolArray, bool_array, bool)

DECLARE_ARRAY(CharArray, char_array, char)

DECLARE_ARRAY(PtrArray, ptr_array, void *)

DECLARE_ARRAY(ConstPtrArray, const_ptr_array, void *, const)

DECLARE_ARRAY(CharPtrArray, char_ptr_array, char *)

DECLARE_ARRAY(ConstCharPtrArray, const_char_ptr_array, char *, const)

DECLARE_ARRAY(StringArray, string_array, String)
