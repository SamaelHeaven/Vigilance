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

#ifdef NDEBUG
#define ASSERT(expr) ((void) 0)
#else
#define ASSERT(expr)                                                                                                   \
    ((expr) ? (void) 0                                                                                                 \
            : (fprintf(stderr, "[ASSERTION FAILED] %s:%d %s(): %s\n    at %s\n", strrchr("/"__FILE__, '/') + 1,        \
                       __LINE__, __func__, #expr, __FILE__),                                                           \
               abort()))
#endif
