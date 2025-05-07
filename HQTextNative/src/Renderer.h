#ifndef HQTEXTTEST_RENDERER_H
#define HQTEXTTEST_RENDERER_H

#include <pango/pango.h>
#include <pango/pangocairo.h>
#include "RenderData.h"
#include "Unity/IUnityInterface.h"

namespace HQText {
extern "C" UNITY_INTERFACE_EXPORT void ReleaseSurface(cairo_surface_t* surface);
extern "C" UNITY_INTERFACE_EXPORT void WriteToPNG(char* filepath,
                                                  cairo_surface_t* surface);
extern "C" UNITY_INTERFACE_EXPORT cairo_surface_t* RenderToSurface(
    RenderData* r,
    int surfaceWidth,
    int surfaceHeight,
    bool fillBackground);

extern "C" UNITY_INTERFACE_EXPORT int GetRenderedClusterRects(
    HQText::RenderData* renderData,
    int surfaceWidth,
    int surfaceHeight,
    PangoRectangle* rects,
    int count);

}  // namespace HQText
#endif  // HQTEXTTEST_RENDERER_H
