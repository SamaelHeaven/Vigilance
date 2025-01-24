cmake_minimum_required(VERSION 3.28)
project(vigilance C)

set(CMAKE_C_STANDARD 23)
set(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} -DNDEBUG")
set(BUILD_SHARED_LIBS OFF CACHE BOOL "Build shared libraries" FORCE)

set(gc_VERSION 8.2.8)
find_package(gc ${gc_VERSION} QUIET)
if (NOT gc_FOUND)
    include(FetchContent)
    FetchContent_Declare(
            gc
            DOWNLOAD_EXTRACT_TIMESTAMP OFF
            URL https://github.com/ivmai/bdwgc/archive/refs/tags/v${gc_VERSION}.tar.gz
    )
    FetchContent_GetProperties(gc)
    if (NOT gc_POPULATED)
        set(FETCHCONTENT_QUIET NO)
        FetchContent_Populate(gc)
        add_subdirectory(${gc_SOURCE_DIR} ${gc_BINARY_DIR})
    endif ()
endif ()

set(raylib_VERSION 5.5)
find_package(raylib ${raylib_VERSION} QUIET)
if (NOT raylib_FOUND)
    include(FetchContent)
    FetchContent_Declare(
            raylib
            DOWNLOAD_EXTRACT_TIMESTAMP OFF
            URL https://github.com/raysan5/raylib/archive/refs/tags/${raylib_VERSION}.tar.gz
    )
    FetchContent_GetProperties(raylib)
    if (NOT raylib_POPULATED)
        set(FETCHCONTENT_QUIET NO)
        FetchContent_Populate(raylib)
        set(BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
        add_subdirectory(${raylib_SOURCE_DIR} ${raylib_BINARY_DIR})
    endif ()
endif ()

file(GLOB_RECURSE SOURCES src/*.h src/*.c)

add_library(${PROJECT_NAME} STATIC ${SOURCES})

target_link_libraries(${PROJECT_NAME} PUBLIC gc raylib)

target_include_directories(${PROJECT_NAME} PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src/include>
        $<INSTALL_INTERFACE:include>
)
