#include "random.h"

#include "gc.h"

#define MT_N 624
#define MT_M 397
#define MT_MATRIX_A 0x9908b0dfU
#define MT_UPPER_MASK 0x80000000U
#define MT_LOWER_MASK 0x7fffffffU

typedef struct Handle {
    uint32_t mt[MT_N];
    int32_t index;
} Handle;

static void random_twist(const Random random) {
    Handle *handle = random.handle;
    for (int32_t i = 0; i < MT_N; ++i) {
        const uint32_t y = handle->mt[i] & MT_UPPER_MASK | handle->mt[(i + 1) % MT_N] & MT_LOWER_MASK;
        handle->mt[i] = handle->mt[(i + MT_M) % MT_N] ^ y >> 1;
        if (y % 2 != 0) {
            handle->mt[i] ^= MT_MATRIX_A;
        }
    }
    handle->index = 0;
}

static uint32_t random_next_uint32(const Random random) {
    Handle *handle = random.handle;
    assert(handle);
    if (handle->index >= MT_N) {
        random_twist(random);
    }
    uint32_t y = handle->mt[handle->index++];
    y ^= y >> 11;
    y ^= y << 7 & 0x9d2c5680U;
    y ^= y << 15 & 0xefc60000U;
    y ^= y >> 18;
    return y;
}

Random random_create(const uint32_t seed) {
    const Random random = {.handle = gc_malloc(sizeof(Handle))};
    Handle *handle = random.handle;
    handle->mt[0] = seed;
    for (int32_t i = 1; i < MT_N; ++i) {
        handle->mt[i] = 1812433253U * (handle->mt[i - 1] ^ handle->mt[i - 1] >> 30) + i;
    }
    handle->index = MT_N;
    return random;
}

void random_destroy(const Random random) {
    if (random.handle) {
        gc_free(random.handle);
    }
}

Random random(void) {
    static Random random = {};
    if (!random.handle) {
        random = random_create(time(nullptr));
    }
    return random;
}

bool random_bool(const Random random) { return random_next_uint32(random) % 2; }

int8_t random_int8(const Random random, const int8_t min, const int8_t max) {
    assert(min < max);
    return (int8_t) (random_next_uint32(random) % (max - min)) + min;
}

int16_t random_int16(const Random random, const int16_t min, const int16_t max) {
    assert(min < max);
    return (int16_t) (random_next_uint32(random) % (max - min)) + min;
}

int32_t random_int32(const Random random, const int32_t min, const int32_t max) {
    assert(min < max);
    return (int32_t) (random_next_uint32(random) % (max - min)) + min;
}

int64_t random_int64(const Random random, const int64_t min, const int64_t max) {
    assert(min < max);
    return ((int64_t) random_next_uint32(random) << 32 | random_next_uint32(random)) % (max - min) + min;
}

uint8_t random_uint8(const Random random, const uint8_t min, const uint8_t max) {
    assert(min < max);
    return (uint8_t) (random_next_uint32(random) % (max - min)) + min;
}

uint16_t random_uint16(const Random random, const uint16_t min, const uint16_t max) {
    assert(min < max);
    return (uint16_t) (random_next_uint32(random) % (max - min)) + min;
}

uint32_t random_uint32(const Random random, const uint32_t min, const uint32_t max) {
    assert(min < max);
    return random_next_uint32(random) % (max - min) + min;
}

uint64_t random_uint64(const Random random, const uint64_t min, const uint64_t max) {
    assert(min < max);
    return ((uint64_t) random_next_uint32(random) << 32 | random_next_uint32(random)) % (max - min) + min;
}

float random_float(const Random random, const float min, const float max) {
    assert(min < max);
    return (float) random_next_uint32(random) / 0xFFFFFFFF * (max - min) + min;
}

double random_double(const Random random, const double min, const double max) {
    assert(min < max);
    return (double) random_next_uint32(random) / 0xFFFFFFFF * (max - min) + min;
}
