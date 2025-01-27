#pragma once

#define MIN(a, b)                                                                                                      \
    ({                                                                                                                 \
        const typeof(a) a_ = a;                                                                                        \
        const typeof(b) b_ = b;                                                                                        \
        a_ < b_ ? a_ : b_;                                                                                             \
    })

#define MAX(a, b)                                                                                                      \
    ({                                                                                                                 \
        const typeof(a) a_ = a;                                                                                        \
        const typeof(b) b_ = b;                                                                                        \
        a_ > b_ ? a_ : b_;                                                                                             \
    })

#define ABS(value)                                                                                                     \
    ({                                                                                                                 \
        const typeof(value) value_ = value;                                                                            \
        value_ < 0 ? -value_ : value_;                                                                                 \
    })

#define CLAMP(value, min, max)                                                                                         \
    ({                                                                                                                 \
        const typeof(value) value_ = value;                                                                            \
        const typeof(min) min_ = min;                                                                                  \
        const typeof(max) max_ = max;                                                                                  \
        value_<min_ ? min_ : value_> max_ ? max_ : value_;                                                             \
    })
