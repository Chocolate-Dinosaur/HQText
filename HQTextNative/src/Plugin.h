#ifndef HQTEXTTEST_PLUGIN_H
#define HQTEXTTEST_PLUGIN_H

#include <pango/pango.h>
#include <pango/pangocairo.h>
#include <vector>
#include "RenderData.h"
#include "TextInfo.h"
#include "TextSize.h"
#include "Unity/IUnityInterface.h"
#include "Unity/IUnityRenderingExtensions.h"

namespace HQText {

extern "C" UNITY_INTERFACE_EXPORT unsigned int Initialize();
extern "C" UNITY_INTERFACE_EXPORT void Teardown(unsigned int index);
extern "C" UNITY_INTERFACE_EXPORT RenderData* GetRenderData(unsigned int index);
extern "C" UNITY_INTERFACE_EXPORT TextSize GetTextSize(char* data,
                                                       char* fontname,
                                                       char* fontface,
                                                       int fontSize,
                                                       float lineSpacing,
                                                       _cairo_font_type ft,
                                                       gboolean useMarkup);
extern "C" UNITY_INTERFACE_EXPORT TextInfo
SetTextData(unsigned int index,
            char* data,
            char* fontname,
            char* facename,
            int fontSize,
            int textBoxWidth,
            int textBoxHeight,
            Color color,
            PangoAlignment textAlignment,
            float lineSpacing,
            gboolean justify,
            gboolean autoDir,
            PangoDirection dir,
            VerticalAlignment va,
            _cairo_font_type ft,
            HorizontalWrapping wrappingH,
            VerticalWrapping wrappingV,
            gboolean useMarkup,
            float resolutionMultiplier,
            gboolean automaticPadding = true,
            int paddingLeft = 0,
            int paddingRight = 0,
            int paddingTop = 0,
            int paddingBottom = 0);

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT
GetTextureUpdateCallback();

extern "C" UNITY_INTERFACE_EXPORT void GetCharacterRects(unsigned int index,
                                                         PangoRectangle* rects,
                                                         int count);
}  // namespace HQText
#endif  // HQTEXTTEST_PLUGIN_H
