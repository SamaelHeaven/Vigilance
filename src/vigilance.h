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
    *({                                                                                                                \
        volatile const typeof(rvalue) value_ = rvalue;                                                                 \
        &value_;                                                                                                       \
    })

#define CAST(type, value) *(type *) &LVALUE(value)

#ifdef NDEBUG
#define ASSERT(expr) ((void) 0)
#else
#define ASSERT(expr)                                                                                                   \
    ((expr) ? (void) 0                                                                                                 \
            : (fprintf(stderr, "Assertion failed: %s, function %s, file %s, line %d.\n", #expr, __func__, __FILE__,    \
                       __LINE__),                                                                                      \
               abort()))
#endif
