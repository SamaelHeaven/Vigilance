#pragma once

#define MIN(a, b)                                                                                                      \
    ({                                                                                                                 \
        const typeof(a) A__ = (a);                                                                                     \
        const typeof(b) B__ = (b);                                                                                     \
        (A__ < B__) ? A__ : B__;                                                                                       \
    })

#define MAX(a, b)                                                                                                      \
    ({                                                                                                                 \
        const typeof(a) A__ = (a);                                                                                     \
        const typeof(b) B__ = (b);                                                                                     \
        (A__ > B__) ? A__ : B__;                                                                                       \
    })

#define ABS(value)                                                                                                     \
    ({                                                                                                                 \
        const typeof(value) VALUE__ = (value);                                                                         \
        (VALUE__ < 0) ? -VALUE__ : VALUE__;                                                                            \
    })

#define CLAMP(value, min, max)                                                                                         \
    ({                                                                                                                 \
        const typeof(value) VALUE__ = (value);                                                                         \
        const typeof(min) MIN__ = (min);                                                                               \
        const typeof(max) MAX__ = (max);                                                                               \
        (VALUE__ < MIN__) ? MIN__ : ((VALUE__ > MAX__) ? MAX__ : VALUE__);                                             \
    })
