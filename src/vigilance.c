#include "include/vigilance.h"

#include <gc.h>

#ifdef _WIN32
#include <windows.h>
#endif

#undef main

void vigilance_main(ArrayCharPtr args);

int32_t main(const int32_t argc, char **argv) {
    GC_INIT();
#ifdef _WIN32
    const HANDLE console_handle = GetStdHandle(STD_OUTPUT_HANDLE);
    DWORD console_mode;
    GetConsoleMode(console_handle, &console_mode);
    console_mode |= ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
    SetConsoleMode(console_handle, console_mode);
    SetConsoleCP(CP_UTF8);
    SetConsoleOutputCP(CP_UTF8);
#endif
    const ArrayCharPtr args = array_char_ptr_create();
    array_char_ptr_concat(args, argv, argc);
    vigilance_main(args);
    return 0;
}
