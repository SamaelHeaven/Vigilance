#pragma once

#define MIN(a, b)                                                                                                      \
    ({                                                                                                                 \
        typeof(a) A__ = (a);                                                                                           \
        typeof(b) B__ = (b);                                                                                           \
        (A__ < B__) ? A__ : B__;                                                                                       \
    })

#define MAX(a, b)                                                                                                      \
    ({                                                                                                                 \
        typeof(a) A__ = (a);                                                                                           \
        typeof(b) B__ = (b);                                                                                           \
        (A__ > B__) ? A__ : B__;                                                                                       \
    })

#define ABS(value)                                                                                                     \
    ({                                                                                                                 \
        typeof(value) VALUE__ = (value);                                                                               \
        (VALUE__ < 0) ? -VALUE__ : VALUE__;                                                                            \
    })

#define CLAMP(value, min, max)                                                                                         \
    ({                                                                                                                 \
        typeof(value) VALUE__ = (value);                                                                               \
        typeof(min) MIN__ = (min);                                                                                     \
        typeof(max) MAX__ = (max);                                                                                     \
        (VALUE__ < MIN__) ? MIN__ : ((VALUE__ > MAX__) ? MAX__ : VALUE__);                                             \
    })
