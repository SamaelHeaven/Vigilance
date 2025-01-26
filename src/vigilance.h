#pragma once

#include <assert.h>
#include <ctype.h>
#include <errno.h>
#include <inttypes.h>
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
        volatile const typeof(rvalue) VALUE__ = (rvalue);                                                              \
        &VALUE__;                                                                                                      \
    }))

#define CAST(type, value) *(type *) &LVALUE(value)
