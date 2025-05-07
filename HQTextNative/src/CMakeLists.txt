cmake_minimum_required (VERSION 3.15)
project(libHQText)

set(CMAKE_CXX_FLAGS_RELEASE "/MT")
set(CMAKE_CXX_FLAGS_DEBUG "/MTd")
set(CMAKE_CXX_STANDARD 17)

#include pango
add_library(Pango INTERFACE)

if(CMAKE_SYSTEM_NAME STREQUAL "Darwin")
    find_package(PkgConfig REQUIRED)
    pkg_check_modules(PANGO REQUIRED pango)
    pkg_check_modules(PANGOCAIRO REQUIRED pangocairo)
    pkg_check_modules(FONTCONFIG REQUIRED fontconfig)
    target_include_directories(Pango INTERFACE ${PANGO_INCLUDE_DIRS} ${PANGOCAIRO_INCLUDE_DIRS}  ${FONTCONFIG_INCLUDE_DIRS})
    target_link_directories(Pango INTERFACE ${PANGO_LIBRARY_DIRS} ${PANGOCAIRO_LIBRARY_DIRS} } ${FONTCONFIG_LIBRARY_DIRS})
    target_link_libraries(Pango INTERFACE ${PANGO_LIBRARIES} ${PANGOCAIRO_LIBRARIES} ${FONTCONFIG_LIBRARIES})

elseif(CMAKE_SYSTEM_NAME STREQUAL "Windows")
    #find_package does not seem to work with pango, so doing it  with find path
    set(CMAKE_CXX_FLAGS_RELEASE "/MT")
    set(CMAKE_CXX_FLAGS_DEBUG "/MTd")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/pango_x64-windows-static/include/pango-1.0")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/glib_x64-windows-static/include/glib-2.0/")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/glib_x64-windows-static/lib/glib-2.0/include")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/harfbuzz_x64-windows-static/include/harfbuzz")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/cairo_x64-windows-static/include/cairo")
    target_include_directories(Pango INTERFACE "${vcpkgpath}/packages/fontconfig_x64-windows-static/include/")
    target_link_directories(Pango INTERFACE "${vcpkgpath}/installed/x64-windows-static/$<$<CONFIG:Debug>:debug>/lib")
    target_link_libraries(Pango INTERFACE brotlicommon)
    target_link_libraries(Pango INTERFACE brotlidec)
    target_link_libraries(Pango INTERFACE brotlienc)
    target_link_libraries(Pango INTERFACE bz2$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE cairo)
    target_link_libraries(Pango INTERFACE cairo-gobject)
    target_link_libraries(Pango INTERFACE cairo-script-interpreter)
    target_link_libraries(Pango INTERFACE charset)
    target_link_libraries(Pango INTERFACE ffi)
    target_link_libraries(Pango INTERFACE fontconfig)
    target_link_libraries(Pango INTERFACE freetype$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE fribidi)
    target_link_libraries(Pango INTERFACE getopt)
    target_link_libraries(Pango INTERFACE gio-2.0)
    target_link_libraries(Pango INTERFACE glib-2.0)
    target_link_libraries(Pango INTERFACE gmodule-2.0)
    target_link_libraries(Pango INTERFACE gobject-2.0)
    target_link_libraries(Pango INTERFACE gthread-2.0)
    target_link_libraries(Pango INTERFACE harfbuzz)
    target_link_libraries(Pango INTERFACE harfbuzz-subset)
    target_link_libraries(Pango INTERFACE iconv)
    target_link_libraries(Pango INTERFACE intl)
    target_link_libraries(Pango INTERFACE libexpat$<$<CONFIG:Debug>:d>MT)
    target_link_libraries(Pango INTERFACE libpng16$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pango-1.0)
    target_link_libraries(Pango INTERFACE pangocairo-1.0)
    target_link_libraries(Pango INTERFACE pangoft2-1.0)
    target_link_libraries(Pango INTERFACE pangowin32-1.0)
    target_link_libraries(Pango INTERFACE pcre2-8$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pcre2-16$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pcre2-32$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pcre2-posix$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pixman-1)
    target_link_libraries(Pango INTERFACE pthreadVC3$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pthreadVCE3$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE pthreadVSE3$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE zlib$<$<CONFIG:Debug>:d>)
    target_link_libraries(Pango INTERFACE Ws2_32.lib)
    target_link_libraries(Pango INTERFACE Dwrite.lib)
endif()

add_library (libHQText SHARED
        TextInfo.h
        VerticalAlignment.h
        Color.h
        Color16.h
        TextSize.h
        RenderData.h
        Unity/IUnityInterface.h
        Unity/IUnityGraphics.h
        Unity/IUnityRenderingExtensions.h
        HorizontalWrapping.h
        VerticalWrapping.h
        FontConfig.cpp
        FontConfig.h
        Renderer.cpp
        Renderer.h
        Plugin.cpp
        Plugin.h)
set_target_properties(libHQText PROPERTIES LINKER_LANGUAGE C)
set_target_properties(libHQText PROPERTIES RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build/bin)
set_target_properties(libHQText PROPERTIES LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build/lib)
if (MSVC)
set_target_properties(libHQText PROPERTIES MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

link_directories(${PANGO_LIBRARY_DIRS})
link_directories(${PANGOCAIRO_LIBRARY_DIRS})

target_link_libraries(libHQText PUBLIC Pango)

if (MSVC)
target_link_options(libHQText PUBLIC "/DELAYLOAD:dwrite.dll")
target_link_options(libHQText PUBLIC "/DELAYLOAD:ws2_32.dll")
endif()
