#include <fontconfig/fontconfig.h>
#include <map>
#include <mutex>
#include <vector>
#include "RenderData.h"
#include "Renderer.h"
#include "TextInfo.h"
#include "TextSize.h"
#include "Unity/IUnityRenderingExtensions.h"

#ifdef ISDLL
#define DLL __declspec(dllexport)
#else
#define DLL __declspec(dllimport)
#endif

#define LINE_IS_VALID(line) ((line) && (line)->layout != NULL)
typedef unsigned char u8;
namespace HQText {

static std::map<unsigned int, RenderData*> renderDataLUT = {};
static unsigned int initCounter = 7690;
static FcConfig* currentFontConfig = NULL;
std::mutex m;


// NOTE: There was a CRASH when using Win32 for rendering - this was because of a bug in cairo where it wasn't calling InitializeCriticalSection, causing the DebugInfo field to be NULL which is not valid.
// To fix this I had to add this code:
//
//  #ifdef CAIRO_WIN32_STATIC_BUILD
//      CAIRO_MUTEX_INITIALIZE();
//  #endif
//
//  to the top of _cairo_win32_font_face_hash_table_lock() and _cairo_win32_font_face_hash_table_destroy()
//  this appears to be an oversight as in the FreeType implementation, the corresponding function _cairo_ft_unscaled_font_map_lock() does this.
//  also most people use dynamic linking which calls CAIRO_MUTEX_INITIALIZE(); in DllMain.


extern "C" UNITY_INTERFACE_EXPORT unsigned int Initialize() {
  initCounter++;
  int index = initCounter;
  return index;
}

extern "C" UNITY_INTERFACE_EXPORT void Teardown(unsigned int index) {
  auto it = renderDataLUT.find(index);
  if (it != renderDataLUT.end()) {
    RenderData* data = it->second;
    delete data;
    renderDataLUT.erase(it);
  }
}

extern "C" UNITY_INTERFACE_EXPORT RenderData* GetRenderData(
    unsigned int index) {
  auto it = renderDataLUT.find(index);
  if (it != renderDataLUT.end())
    return it->second;
  return NULL;
}

extern "C" UNITY_INTERFACE_EXPORT TextSize GetTextSize(char* data,
                                                       char* fontname,
                                                       char* fontface,
                                                       int fontSize,
                                                       float lineSpacing,
                                                       _cairo_font_type ft,
                                                       gboolean useMarkup) {
  PangoFontDescription* desc;
  PangoFontMap* fontMap = pango_cairo_font_map_new_for_font_type(ft);
  PangoContext* pangoContext = pango_font_map_create_context(fontMap);
  PangoLayout* pangoLayout = pango_layout_new(pangoContext);

  // no wrapping
  pango_layout_set_width(pangoLayout, -1);
  desc = GetFontDescriptionFromString(fontname, fontface, ft);
  if (desc == nullptr) {
    desc = pango_font_description_from_string("Sans");
  }
  if (fontSize <= 0) {
    fontSize = 1;
  }

  pango_font_description_set_absolute_size(
      desc, fontSize * DEVICE_DPI * PANGO_SCALE / DEVICE_DPI);
  pango_layout_set_font_description(pangoLayout, desc);
  if (useMarkup) {
    pango_layout_set_markup(pangoLayout, data, -1);
  } else {
    pango_layout_set_text(pangoLayout, data, -1);
  }

  pango_layout_set_spacing(pangoLayout, lineSpacing);

  PangoRectangle inkRect;
  PangoRectangle logicalRect;

  pango_layout_get_extents(pangoLayout, &inkRect, &logicalRect);
  TextSize t = TextSize(
      logicalRect.width / PANGO_SCALE, logicalRect.height / PANGO_SCALE,
      inkRect.width / PANGO_SCALE, inkRect.height / PANGO_SCALE);
  if (pangoContext != nullptr) {
    g_object_unref(pangoContext);
  } else {
    printf("Pango context is null");
  }
  if (pangoLayout != nullptr) {
    g_object_unref(pangoLayout);
  } else {
    printf("Pango layout is null");
  }
  if (fontMap != nullptr) {
    g_object_unref(fontMap);
  } else {
    printf("Pango font map is null");
  }
  pango_font_description_free(desc);
  return t;
}
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
            int paddingBottom = 0) {
  RenderPadding padding = {paddingLeft, paddingRight, paddingTop,
                           paddingBottom};
  m.lock();
  auto it = renderDataLUT.find(index);
  if (it != renderDataLUT.end()) {
    RenderData* renderData = it->second;
    delete renderData;
    renderDataLUT.erase(it);
  }

  auto r = new RenderData(
      data, textBoxWidth, textBoxHeight, fontSize, textAlignment, fontname,
      facename, color, lineSpacing, justify, autoDir, dir, va, ft, wrappingH,
      wrappingV, useMarkup, resolutionMultiplier, automaticPadding, padding);
  renderDataLUT[index] = r;
  PangoRectangle inkRect;
  PangoRectangle logicalRect;
  pango_layout_get_extents(r->pangoLayout, &inkRect, &logicalRect);
  PangoLayoutLine* line;
  line = pango_layout_get_line(r->pangoLayout, 0);
  PangoDirection direction = r->dir;
  if (r->autoDir && LINE_IS_VALID(line)) {
    direction = (PangoDirection)line->resolved_dir;
  }

  int lineCount = pango_layout_get_line_count(r->pangoLayout);
  int characterCount = pango_layout_get_character_count(r->pangoLayout);
  int ascent = 0;
  int descent = 0;
  int lineHeight = 0;
  PangoFontMetrics* metrics = pango_context_get_metrics(
      pango_layout_get_context(r->pangoLayout),
      pango_layout_get_font_description(r->pangoLayout), nullptr);
  if (metrics) {
    ascent = pango_font_metrics_get_ascent(metrics) / PANGO_SCALE;
    descent = pango_font_metrics_get_descent(metrics) / PANGO_SCALE;
    lineHeight = pango_font_metrics_get_height(metrics) / PANGO_SCALE;
    pango_font_metrics_unref(metrics);
  }

  TextInfo t = TextInfo(
      r->RenderWidthPixels(), r->RenderHeightPixels(),
      logicalRect.width / PANGO_SCALE, logicalRect.height / PANGO_SCALE,
      inkRect.width / PANGO_SCALE, inkRect.height / PANGO_SCALE, direction,
      lineCount, characterCount, ascent, descent, lineHeight);
  m.unlock();
  return t;
}

void renderToTexture(void* data) {
  auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
  m.lock();

  auto it = renderDataLUT.find(params->userData);
  // only render something if we find the matching render data.
  if (it == renderDataLUT.end()) {
    m.unlock();
    auto tex = new uint32_t[params->width * params->height];
    for (unsigned int i = 0; i < params->width * params->height; ++i) {
      tex[i] = 0x00000000;
    }
    params->texData = tex;
    return;
  }

  RenderData* rd = it->second;
  cairo_surface_t* surface =
      RenderToSurface(rd, (int)params->width, (int)params->height, false);
  auto surfaceData = cairo_image_surface_get_data(surface);
  int height = cairo_image_surface_get_height(surface);
  int stride = cairo_image_surface_get_stride(surface);
  int width = cairo_image_surface_get_width(surface);
  
  auto img = new uint32_t[params->width * params->height];

  //memcpy(img, surfaceData, width * height * 4);

  // this loop flips the code on the y axis so it is the right orientation
  // for unity, and removed the premultiplied alpha.
  for (int y = 0; y < height; ++y) {
    for (int x = 0; x < width; ++x) {
      int flippedY = height - y - 1;
      int srcIndex = y * stride + x * 4;
      int destIndex = flippedY * stride + x * 4;

      u8 a = ((u8)surfaceData[srcIndex + 3]);
      u8 r = ((u8)surfaceData[srcIndex + 0]);
      u8 g = ((u8)surfaceData[srcIndex + 1]);
      u8 b = (u8)surfaceData[srcIndex + 2];
      float aFloat = ((float)a / 255.0);
      if (a > 0) {
        r /= aFloat;
        g /= aFloat;
        b /= aFloat;
      }
      img[destIndex / 4] = a << 24 | r << 16 | g << 8 | b;
    }
  }
  /*
  for (int y = 0; y < height; ++y) {
      for (int x = 0; x < width; ++x) {
          int flippedY = height - y - 1;
          int srcIndex = y * stride + x * 4;
          int destIndex = flippedY * stride + x * 4;

          u8 a = ((u8)surfaceData[srcIndex + 3]);
          u8 r = ((u8)surfaceData[srcIndex + 0]);
          u8 g = ((u8)surfaceData[srcIndex + 1]);
          u8 b = (u8)surfaceData[srcIndex + 2];
          img[destIndex / 4] = a << 24 | r << 16 | g << 8 | b;
      }
  }*/

  m.unlock();
  params->texData = img;
  cairo_surface_destroy(surface);
}

void releaseTexture(void* data) {
  auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
  delete[] reinterpret_cast<uint32_t*>(params->texData);
}

void TextureUpdateCallback(int eventID, void* data) {
  auto event = static_cast<UnityRenderingExtEventType>(eventID);

  if (event == kUnityRenderingExtEventUpdateTextureBeginV2) {
    renderToTexture(data);
  } else if (event == kUnityRenderingExtEventUpdateTextureEndV2) {
    releaseTexture(data);
  }
}

// GetCharacterRects returns a list of ink extents for each character in the
// text.
extern "C" UNITY_INTERFACE_EXPORT void GetCharacterRects(unsigned int index,
                                                         PangoRectangle* rects,
                                                         int count) {
  m.lock();
  auto it = renderDataLUT.find(index);

  if (it == renderDataLUT.end()) {
    m.unlock();
    return;
  }

  RenderData* renderData = it->second;
  // TODO: Get the width and height from the client, or always use the render
  //  width & height of the RenderData, and enforce it when rendering to a
  //  texture.
  GetRenderedClusterRects(renderData, renderData->RenderWidthPixels(),
                          renderData->RenderHeightPixels(), rects, count);
  m.unlock();
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT
GetTextureUpdateCallback() {
  return TextureUpdateCallback;
}

}  // namespace HQText
