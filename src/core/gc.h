#pragma once

#include "../vigilance.h"

typedef void (*GCFinalizer)(void *ptr);

void *gc_malloc(size_t size);

void *gc_realloc(void *ptr, size_t size);

void gc_free(void *ptr);

void gc_register_finalizer(void *ptr, GCFinalizer finalizer);

void gc_collect();

void gc_collect_a_little();
