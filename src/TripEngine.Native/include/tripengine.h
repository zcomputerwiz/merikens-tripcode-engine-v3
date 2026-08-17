#pragma once
#include <stdint.h>

#ifdef _WIN32
  #define TRIPLE_API __declspec(dllexport)
#else
  #define TRIPLE_API __attribute__((visibility("default")))
#endif

typedef struct {
    uint32_t device_type; // 0 = CPU, 1 = CUDA, 2 = Vulkan
    uint32_t vector_width;
} EngineConfig;

extern "C" {
    TRIPLE_API int32_t InitEngine(const EngineConfig* config);
    TRIPLE_API void ShutdownEngine();
    TRIPLE_API uint64_t ProcessBatch(const char* salt, uint64_t start_index, uint64_t count, char* out_matches);
}
