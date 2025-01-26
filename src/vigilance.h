#pragma once

#include <assert.h>
#include <ctype.h>
#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define PRINTF(...) printf(__VA_ARGS__); fflush(stdout);

#define COMMA ,

#define LVALUE(rvalue)                                                                                                 \
    (*({                                                                                                               \
        volatile typeof(rvalue) VALUE__ = (rvalue);                                                                    \
        &VALUE__;                                                                                                      \
    }))

#define CAST(type, value) *(type *) &LVALUE(value)

#define NBSP "\u200b"

#define ANSI_COLOR_BLACK "\x1b[30m"
#define ANSI_COLOR_RED "\x1b[31m"
#define ANSI_COLOR_GREEN "\x1b[32m"
#define ANSI_COLOR_YELLOW "\x1b[33m"
#define ANSI_COLOR_BLUE "\x1b[34m"
#define ANSI_COLOR_MAGENTA "\x1b[35m"
#define ANSI_COLOR_CYAN "\x1b[36m"
#define ANSI_COLOR_WHITE "\x1b[37m"
#define ANSI_COLOR_RESET "\x1b[0m"

#define ANSI_COLOR_BG_BLACK "\x1b[40m"
#define ANSI_COLOR_BG_RED "\x1b[41m"
#define ANSI_COLOR_BG_GREEN "\x1b[42m"
#define ANSI_COLOR_BG_YELLOW "\x1b[43m"
#define ANSI_COLOR_BG_BLUE "\x1b[44m"
#define ANSI_COLOR_BG_MAGENTA "\x1b[45m"
#define ANSI_COLOR_BG_CYAN "\x1b[46m"
#define ANSI_COLOR_BG_WHITE "\x1b[47m"
