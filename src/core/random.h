#pragma once

#include "../vigilance.h"

typedef struct Random {
    struct Handle *handle;
} Random;

Random random_create(uint32_t seed);

Random random();

bool random_bool(Random random);

int8_t random_int8(Random random, int8_t min, int8_t max);

int16_t random_int16(Random random, int16_t min, int16_t max);

int32_t random_int32(Random random, int32_t min, int32_t max);

int64_t random_int64(Random random, int64_t min, int64_t max);

uint8_t random_uint8(Random random, uint8_t min, uint8_t max);

uint16_t random_uint16(Random random, uint16_t min, uint16_t max);

uint32_t random_uint32(Random random, uint32_t min, uint32_t max);

uint64_t random_uint64(Random random, uint64_t min, uint64_t max);

float random_float(Random random, float min, float max);

double random_double(Random random, double min, double max);
