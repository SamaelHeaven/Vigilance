#pragma once

#include <assert.h>
#include <ctype.h>
#include <stdint.h>
#include <stdio.h>
#include <stdbool.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>

#define LVALUE(rvalue)                                                                                                 \
    (*({                                                                                                               \
        typeof(rvalue) VALUE__ = (rvalue);                                                                             \
        &VALUE__;                                                                                                      \
    }))
