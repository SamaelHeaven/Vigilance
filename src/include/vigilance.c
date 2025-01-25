#include "vigilance.h"

#include <gc.h>

#ifdef _WIN32
#include <windows.h>
#endif

#undef main

void vigilance_main(CharPtrArray args);

int main(const int32_t argc, char **argv) {
    GC_INIT();
#ifdef _WIN32
    SetConsoleCP(CP_UTF8);
    SetConsoleOutputCP(CP_UTF8);
#endif
    CharPtrArray args = char_ptr_array_create();
    char_ptr_array_reserve(&args, argc);
    for (int32_t i = 0; i < argc; ++i) {
        char_ptr_array_add(&args, argv[i]);
    }
    vigilance_main(args);
    return 0;
}
