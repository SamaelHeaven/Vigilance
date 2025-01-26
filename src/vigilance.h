#pragma once

#include <assert.h>
#include <ctype.h>
#include <errno.h>
#include <float.h>
#include <limits.h>
#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define COMMA ,

#define LVALUE(rvalue)                                                                                                 \
    (*({                                                                                                               \
        volatile typeof(rvalue) VALUE__ = (rvalue);                                                                    \
        &VALUE__;                                                                                                      \
    }))

#define CAST(type, value) *(type *) &LVALUE(value)

#define MIN(a, b) (((a) < (b)) ? (a) : (b))

#define MAX(a, b) (((a) > (b)) ? (a) : (b))

#define ABS(a) (((a) < 0) ? -(a) : (a))

#define CLAMP(value, min, max) MAX(min, MIN(max, value))
