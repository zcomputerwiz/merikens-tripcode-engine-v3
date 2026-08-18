#include "tripengine.h"
#include <string.h>

extern "C" {
    TRIPLE_API int32_t InitEngine(const EngineConfig* config) {
        // Scalar stub implementation
        if (!config) return -1;
        return 0; // Success
    }

    TRIPLE_API void ShutdownEngine() {
        // Stub implementation
    }

    TRIPLE_API uint64_t ProcessBatch(const char* salt, uint64_t start_index, uint64_t count, char* out_matches) {
        // Stub implementation: returns 0 matches
        if (out_matches) {
            out_matches[0] = '\0';
        }
        return 0;
    }
}
