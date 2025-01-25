#include "vigilance.h"

#include <gc.h>

#undef main

void vigilance_main(Array_string args);

int main(const int32_t argc, const char **argv) {
    GC_INIT();
    Array_string args = array_string_create();
    array_string_reserve(&args, argc);
    for (int32_t i = 0; i < argc; ++i) {
        array_string_add(&args, string_create(argv[i]));
    }
    vigilance_main(args);
    return 0;
}
