add_library(tripengine SHARED
    src/tripengine.cpp
)

target_include_directories(tripengine PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

set_target_properties(tripengine PROPERTIES
    POSITION_INDEPENDENT_CODE ON
)
