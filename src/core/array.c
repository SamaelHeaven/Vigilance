#include "array.h"

#include "gc.h"

#define READONLY_TAG 0x1

typedef struct Handle {
    void *elements;
    int32_t size;
    int32_t capacity;
    int32_t element_size;
} Handle;

static Handle *array_handle_mark_readonly(Handle *handle) { return (Handle *) ((uintptr_t) handle | READONLY_TAG); }

static Handle *array_handle_decode(Handle *handle) { return (Handle *) ((uintptr_t) handle & ~READONLY_TAG); }

static bool array_handle_is_readonly(Handle *handle) { return ((uintptr_t) handle & READONLY_TAG) != 0; }

static void array_merge(void *elements, const int32_t left, const int32_t mid, const int32_t right,
                        const int32_t element_size, int32_t (*comparator)(const void *a, const void *b)) {
    const int32_t n1 = mid - left + 1;
    const int32_t n2 = right - mid;
    void *left_buffer = malloc(n1 * element_size);
    void *right_buffer = malloc(n2 * element_size);
    memcpy(left_buffer, elements + left * element_size, n1 * element_size);
    memcpy(right_buffer, elements + (mid + 1) * element_size, n2 * element_size);
    int32_t i = 0, j = 0, k = left;
    while (i < n1 && j < n2) {
        if (comparator(left_buffer + i * element_size, right_buffer + j * element_size) <= 0) {
            memcpy(elements + k * element_size, left_buffer + i * element_size, element_size);
            i++;
        } else {
            memcpy(elements + k * element_size, right_buffer + j * element_size, element_size);
            j++;
        }
        k++;
    }
    while (i < n1) {
        memcpy(elements + k * element_size, left_buffer + i * element_size, element_size);
        i++;
        k++;
    }
    while (j < n2) {
        memcpy(elements + k * element_size, right_buffer + j * element_size, element_size);
        j++;
        k++;
    }
    free(left_buffer);
    free(right_buffer);
}

static void array_merge_sort(void *elements, const int32_t left, const int32_t right, const int32_t element_size,
                             int32_t (*comparator)(const void *a, const void *b)) {
    if (left < right) {
        const int32_t mid = left + (right - left) / 2;
        array_merge_sort(elements, left, mid, element_size, comparator);
        array_merge_sort(elements, mid + 1, right, element_size, comparator);
        array_merge(elements, left, mid, right, element_size, comparator);
    }
}

WritableArray array_create(const int32_t element_size) {
    assert(element_size > 0);
    const Array array = {.handle = gc_malloc(sizeof(Handle))};
    Handle *handle = array.handle;
    handle->elements = gc_malloc(element_size * 1);
    handle->size = 0;
    handle->capacity = 1;
    handle->element_size = element_size;
    return array;
}

void array_destroy(const WritableArray array) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    if (handle) {
        gc_free(handle->elements);
        gc_free(handle);
    }
}

ReadonlyArray array_readonly(Array array) {
    Handle *handle = array.handle;
    assert(handle);
    if (!array_handle_is_readonly(handle)) {
        array.handle = array_handle_mark_readonly(handle);
    }
    return array;
}

bool array_is_readonly(const Array array) {
    assert(array.handle);
    return array_handle_is_readonly(array.handle);
}

void array_assert_writable(const Array array) { assert(!array_is_readonly(array) && "Array is readonly"); }

void array_add(const WritableArray array, const void *element) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (handle->size == handle->capacity) {
        handle->capacity = handle->capacity * 3 / 2 + 1;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    memcpy(handle->elements + handle->size * handle->element_size, element, handle->element_size);
    handle->size++;
}

void array_add_at(const WritableArray array, const int32_t index, const void *element) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle && index >= 0 && index <= handle->size);
    if (handle->size == handle->capacity) {
        handle->capacity = handle->capacity * 3 / 2 + 1;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    if (index < handle->size) {
        memmove(handle->elements + (index + 1) * handle->element_size, handle->elements + index * handle->element_size,
                (handle->size - index) * handle->element_size);
    }
    memcpy(handle->elements + index * handle->element_size, element, handle->element_size);
    handle->size++;
}

void array_add_all(const WritableArray dest, const Array src) {
    array_assert_writable(dest);
    const Handle *src_handle = array_handle_decode(src.handle);
    assert(src_handle);
    array_concat(dest, src_handle->elements, src_handle->size);
}

void array_concat(const WritableArray array, const void *elements, const int32_t count) {
    array_assert_writable(array);
    assert(elements);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (handle->size + count > handle->capacity) {
        handle->capacity = handle->size + count;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    memcpy(handle->elements + handle->size * handle->element_size, elements, count * handle->element_size);
    handle->size += count;
}

void array_remove(const WritableArray array, const void *element) {
    int32_t index;
    do {
        index = array_index_of(array, element);
        if (index != -1) {
            array_remove_at(array, index);
        }
    } while (index != -1);
}

void array_remove_at(const WritableArray array, const int32_t index) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle && index >= 0 && index < handle->size);
    if (index < handle->size - 1) {
        memmove(handle->elements + index * handle->element_size, handle->elements + (index + 1) * handle->element_size,
                (handle->size - index - 1) * handle->element_size);
    }
    handle->size--;
    if (handle->size > 0 && handle->size < handle->capacity / 4) {
        handle->capacity /= 2;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
}

void array_remove_all(const WritableArray dest, const Array src) {
    array_assert_writable(dest);
    const Handle *src_handle = array_handle_decode(src.handle);
    assert(src_handle);
    for (int32_t i = 0; i < src_handle->size; ++i) {
        array_remove(dest, src_handle->elements + i * src_handle->element_size);
    }
}

void array_remove_if(const WritableArray array, bool (*predicate)(const void *element)) {
    array_assert_writable(array);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        if (predicate(handle->elements + i * handle->element_size)) {
            array_remove_at(array, i);
            i--;
        }
    }
}

bool array_contains(const Array array, const void *element) { return array_index_of(array, element) != -1; }

bool array_is_empty(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    return handle->size == 0;
}

int32_t array_index_of(const Array array, const void *element) {
    assert(element);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        const void *current_element = handle->elements + i * handle->element_size;
        if (memcmp(current_element, element, handle->element_size) == 0) {
            return i;
        }
    }
    return -1;
}

void array_clear(const WritableArray array) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    handle->elements = gc_realloc(handle->elements, handle->element_size * 1);
    handle->size = 0;
    handle->capacity = 1;
}

void *array_get(const Array array, const int32_t index) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (index < 0 || index >= handle->size) {
        return nullptr;
    }
    return handle->elements + index * handle->element_size;
}

void array_set(const WritableArray array, const int32_t index, const void *element) {
    array_assert_writable(array);
    assert(element);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle && index >= 0 && index < handle->size);
    memcpy(handle->elements + index * handle->element_size, element, handle->element_size);
}

void array_replace(const WritableArray array, const void *element, const void *by) {
    array_assert_writable(array);
    assert(element && by);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        if (memcmp(handle->elements + i * handle->element_size, element, handle->element_size) == 0) {
            memcpy(handle->elements + i * handle->element_size, by, handle->element_size);
        }
    }
}

void array_reserve(const WritableArray array, const int32_t new_capacity) {
    array_assert_writable(array);
    assert(new_capacity >= 0);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (new_capacity > handle->capacity) {
        handle->elements = gc_realloc(handle->elements, handle->element_size * new_capacity);
        handle->capacity = new_capacity;
    }
}

void array_sort(const WritableArray array, int32_t (*comparator)(const void *a, const void *b)) {
    array_assert_writable(array);
    assert(comparator);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    qsort(handle->elements, handle->size, handle->element_size, comparator);
}

void array_stable_sort(const WritableArray array, int32_t (*comparator)(const void *a, const void *b)) {
    array_assert_writable(array);
    assert(comparator);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (handle->size > 1) {
        array_merge_sort(handle->elements, 0, handle->size - 1, handle->element_size, comparator);
    }
}

void array_shrink(const WritableArray array) {
    array_assert_writable(array);
    Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    if (handle->size < handle->capacity) {
        handle->capacity = handle->size;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
}

WritableArray array_copy(const Array array) {
    const Handle *src_handle = array_handle_decode(array.handle);
    assert(src_handle);
    const Array copy = array_create(src_handle->element_size);
    Handle *copy_handle = copy.handle;
    copy_handle->size = src_handle->size;
    copy_handle->capacity = src_handle->capacity;
    copy_handle->elements = gc_realloc(copy_handle->elements, copy_handle->element_size * copy_handle->capacity);
    memcpy(copy_handle->elements, src_handle->elements, src_handle->size * src_handle->element_size);
    return copy;
}

WritableArray array_slice(const Array array, const int32_t begin, const int32_t end) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle && begin >= 0 && end >= begin && end <= handle->size);
    const int32_t slice_size = end - begin;
    const Array slice = array_create(handle->element_size);
    Handle *slice_handle = slice.handle;
    slice_handle->size = slice_size;
    slice_handle->capacity = slice_size;
    slice_handle->elements = gc_realloc(slice_handle->elements, handle->element_size * slice_size);
    memcpy(slice_handle->elements, handle->elements + begin * handle->element_size, slice_size * handle->element_size);
    return slice;
}


void array_reverse(const WritableArray array) {
    array_assert_writable(array);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    const int32_t element_size = handle->element_size;
    const int32_t size = handle->size;
    void *temp = malloc(element_size);
    for (int32_t i = 0; i < size / 2; ++i) {
        void *front = handle->elements + i * element_size;
        void *back = handle->elements + (size - 1 - i) * element_size;
        memcpy(temp, front, element_size);
        memcpy(front, back, element_size);
        memcpy(back, temp, element_size);
    }
    free(temp);
}

void array_for_each(const Array array, void (*callback)(void *element)) {
    assert(callback);
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        callback(handle->elements + i * handle->element_size);
    }
}

void *array_to_ptr(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    void *result = gc_malloc(handle->element_size * handle->size);
    memcpy(result, handle->elements, handle->element_size * handle->size);
    return result;
}

const void *array_data(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    return handle->elements;
}

int32_t array_size(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    return handle->size;
}

int32_t array_capacity(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    return handle->capacity;
}

int32_t array_element_size(const Array array) {
    const Handle *handle = array_handle_decode(array.handle);
    assert(handle);
    return handle->element_size;
}
