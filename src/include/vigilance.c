#include "vigilance.h"

#include <gc.h>

#undef main

void lib_main(Array_char_ptr args);

int main(const int32_t argc, char **argv) {
    GC_INIT();
    Array_char_ptr args = array_char_ptr_new();
    array_char_ptr_reserve(&args, argc);
    for (int32_t i = 0; i < argc; ++i) {
        array_char_ptr_add(&args, argv[i]);
    }
    lib_main(args);
    return 0;
}
