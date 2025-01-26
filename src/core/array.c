#include "array.h"

#include "gc.h"

typedef struct Handle {
    void *elements;
    int32_t size;
    int32_t capacity;
    int32_t element_size;
} Handle;

static void merge(void *elements, const int32_t left, const int32_t mid, const int32_t right,
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

static void merge_sort(void *elements, const int32_t left, const int32_t right, const int32_t element_size,
                       int32_t (*comparator)(const void *a, const void *b)) {
    if (left < right) {
        const int32_t mid = left + (right - left) / 2;
        merge_sort(elements, left, mid, element_size, comparator);
        merge_sort(elements, mid + 1, right, element_size, comparator);
        merge(elements, left, mid, right, element_size, comparator);
    }
}

Array array_create(const int32_t element_size) {
    assert(element_size > 0);
    const Array array = {.handle = gc_malloc(sizeof(Handle))};
    Handle *handle = array.handle;
    handle->elements = gc_malloc(element_size * 1);
    handle->size = 0;
    handle->capacity = 1;
    handle->element_size = element_size;
    return array;
}

void array_destroy(Array *array) {
    if (array) {
        if (array->handle) {
            gc_free(array->handle->elements);
            gc_free(array->handle);
        }
        gc_free(array);
    }
}

void array_add(Array *array, const void *element) {
    assert(array);
    Handle *handle = array->handle;
    assert(handle);
    if (handle->size == handle->capacity) {
        handle->capacity = ceilf((float) handle->capacity * 1.5f);
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    memcpy(handle->elements + handle->size * handle->element_size, element, handle->element_size);
    handle->size++;
}

void array_add_at(Array *array, const int32_t index, const void *element) {
    assert(array);
    Handle *handle = array->handle;
    assert(handle && index >= 0 && index <= handle->size);
    if (handle->size == handle->capacity) {
        handle->capacity = ceilf((float) handle->capacity * 1.5f);
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    if (index < handle->size) {
        memmove(handle->elements + (index + 1) * handle->element_size, handle->elements + index * handle->element_size,
                (handle->size - index) * handle->element_size);
    }
    memcpy(handle->elements + index * handle->element_size, element, handle->element_size);
    handle->size++;
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_add_all(Array *dest, const Array *src) {
    assert(dest && src);
    const Handle *src_handle = src->handle;
    assert(src_handle);
    array_concat(dest, src_handle->elements, src_handle->size);
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_concat(Array *array, const void *elements, const int32_t count) {
    assert(array && elements);
    Handle *handle = array->handle;
    assert(handle);
    if (handle->size + count > handle->capacity) {
        handle->capacity = handle->size + count;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
    memcpy(handle->elements + handle->size * handle->element_size, elements, count * handle->element_size);
    handle->size += count;
}

void array_remove(Array *array, const void *element) {
    assert(array);
    int32_t index;
    do {
        index = array_index_of(array, element);
        if (index != -1) {
            array_remove_at(array, index);
        }
    } while (index != -1);
}

void array_remove_at(Array *array, const int32_t index) {
    assert(array);
    Handle *handle = array->handle;
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

void array_remove_all(Array *dest, const Array *src) {
    assert(dest && src);
    const Handle *src_handle = src->handle;
    assert(src_handle);
    for (int32_t i = 0; i < src_handle->size; ++i) {
        array_remove(dest, src_handle->elements + i * src_handle->element_size);
    }
}

void array_remove_if(Array *array, bool (*predicate)(const void *element)) {
    assert(array);
    Handle *handle = array->handle;
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        if (predicate(handle->elements + i * handle->element_size)) {
            array_remove_at(array, i);
            i--;
        }
    }
}

bool array_contains(const Array *array, const void *element) {
    assert(array);
    return array_index_of(array, element) != -1;
}

bool array_is_empty(const Array *array) {
    assert(array && array->handle);
    return array->handle->size == 0;
}

int32_t array_index_of(const Array *array, const void *element) {
    assert(array && element);
    const Handle *handle = array->handle;
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        const void *current_element = handle->elements + i * handle->element_size;
        if (memcmp(current_element, element, handle->element_size) == 0) {
            return i;
        }
    }
    return -1;
}

void array_clear(Array *array) {
    assert(array);
    Handle *handle = array->handle;
    assert(handle);
    handle->elements = gc_realloc(handle->elements, handle->element_size * 1);
    handle->size = 0;
    handle->capacity = 1;
}

void *array_get(const Array *array, const int32_t index) {
    assert(array);
    const Handle *handle = array->handle;
    assert(handle);
    if (index < 0 || index >= handle->size) {
        return nullptr;
    }
    return handle->elements + index * handle->element_size;
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_set(Array *array, const int32_t index, const void *element) {
    assert(array && element);
    const Handle *handle = array->handle;
    assert(handle && index >= 0 && index < handle->size);
    memcpy(handle->elements + index * handle->element_size, element, handle->element_size);
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_replace(Array *array, const void *element, const void *by) {
    assert(array && element && by);
    const Handle *handle = array->handle;
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        if (memcmp(handle->elements + i * handle->element_size, element, handle->element_size) == 0) {
            memcpy(handle->elements + i * handle->element_size, by, handle->element_size);
        }
    }
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_reserve(Array *array, const int32_t new_capacity) {
    assert(array && new_capacity >= 0);
    Handle *handle = array->handle;
    assert(handle);
    if (new_capacity > handle->capacity) {
        handle->elements = gc_realloc(handle->elements, handle->element_size * new_capacity);
        handle->capacity = new_capacity;
    }
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_sort(Array *array, int32_t (*comparator)(const void *a, const void *b)) {
    assert(array && comparator);
    const Handle *handle = array->handle;
    assert(handle);
    qsort(handle->elements, handle->size, handle->element_size, comparator);
}

// ReSharper disable once CppParameterMayBeConstPtrOrRef
void array_stable_sort(Array *array, int32_t (*comparator)(const void *a, const void *b)) {
    assert(array && comparator);
    const Handle *handle = array->handle;
    assert(handle);
    if (handle->size > 1) {
        merge_sort(handle->elements, 0, handle->size - 1, handle->element_size, comparator);
    }
}

void array_shrink(Array *array) {
    assert(array);
    Handle *handle = array->handle;
    assert(handle);
    if (handle->size < handle->capacity) {
        handle->capacity = handle->size;
        handle->elements = gc_realloc(handle->elements, handle->element_size * handle->capacity);
    }
}

Array array_copy(const Array *array) {
    assert(array);
    const Handle *src_handle = array->handle;
    assert(src_handle);
    const Array copy = array_create(src_handle->element_size);
    Handle *copy_handle = copy.handle;
    copy_handle->size = src_handle->size;
    copy_handle->capacity = src_handle->capacity;
    copy_handle->elements = gc_realloc(copy_handle->elements, copy_handle->element_size * copy_handle->capacity);
    memcpy(copy_handle->elements, src_handle->elements, src_handle->size * src_handle->element_size);
    return copy;
}

Array array_slice(const Array *array, const int32_t begin, const int32_t end) {
    assert(array);
    const Handle *handle = array->handle;
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


void array_reverse(Array *array) {
    assert(array);
    const Handle *handle = array->handle;
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

void array_for_each(const Array *array, void (*callback)(void *element)) {
    assert(array && callback);
    const Handle *handle = array->handle;
    assert(handle);
    for (int32_t i = 0; i < handle->size; ++i) {
        callback(handle->elements + i * handle->element_size);
    }
}

void *array_to_ptr(const Array *array) {
    assert(array);
    const Handle *handle = array->handle;
    assert(handle);
    void *result = gc_malloc(handle->element_size * handle->size);
    memcpy(result, handle->elements, handle->element_size * handle->size);
    return result;
}

const void *array_data(const Array *array) {
    assert(array && array->handle);
    return array->handle->elements;
}

int32_t array_size(const Array *array) {
    assert(array && array->handle);
    return array->handle->size;
}

int32_t array_capacity(const Array *array) {
    assert(array && array->handle);
    return array->handle->capacity;
}

int32_t array_element_size(const Array *array) {
    assert(array && array->handle);
    return array->handle->element_size;
}
