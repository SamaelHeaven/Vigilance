#include "gc.h"

#include <gc.h>

static void gc_finalizer(void *ptr, void *client_data) {
    const GCFinalizer finalizer = client_data;
    finalizer(ptr);
}

void *gc_malloc(const size_t size) {
    void *result = GC_MALLOC(size);
    ASSERT(result);
    return result;
}

void *gc_realloc(void *ptr, const size_t size) {
    ASSERT(ptr);
    void *result = GC_REALLOC(ptr, size);
    ASSERT(result);
    return result;
}

void gc_free(void *ptr) { GC_FREE(ptr); }

void gc_register_finalizer(void *ptr, const GCFinalizer finalizer) {
    ASSERT(ptr && finalizer);
    GC_REGISTER_FINALIZER(ptr, gc_finalizer, finalizer, nullptr, nullptr);
}

void gc_collect() { GC_gcollect(); }

void gc_collect_a_little() { GC_collect_a_little(); }
