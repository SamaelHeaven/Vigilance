#pragma once

#include <ctype.h>
#include <errno.h>
#include <inttypes.h>
#include <math.h>
#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#define COMMA ,

#define LVALUE(rvalue)                                                                                                 \
    (*({                                                                                                               \
        volatile const typeof(rvalue) value_ = rvalue;                                                                 \
        &value_;                                                                                                       \
    }))

#define CAST(type, value) (*(type *) &LVALUE(value))

#define BASENAME(file)                                                                                                 \
    ({                                                                                                                 \
        char *basename_ = strrchr(file, '/');                                                                          \
        basename_ = basename_ ? basename_ : strrchr(file, '\\');                                                       \
        basename_ ? basename_ + 1 : file;                                                                              \
    })

#ifdef NDEBUG
#define ASSERT(expr) ((void) 0)
#else
#define ASSERT(expr)                                                                                                   \
    ((expr) ? (void) 0                                                                                                 \
            : (fprintf(stderr, "[ASSERTION FAILED] %s:%d %s(): %s\n    at %s\n", BASENAME(__FILE__), __LINE__,         \
                       __func__, #expr, __FILE__),                                                                     \
               abort()))
#endif
