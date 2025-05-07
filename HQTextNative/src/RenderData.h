#ifndef HQTEXT_RENDERDATA_H
#define HQTEXT_RENDERDATA_H

#include <cairo.h>
#include <pango/pango.h>
#include <pango/pangocairo.h>
#include <cmath>
#include <string>
#include <utility>
#include "Color.h"
#include "FontConfig.h"
#include "HorizontalWrapping.h"
#include "VerticalAlignment.h"
#include "VerticalWrapping.h"

namespace HQText {

struct RenderPadding {
  int left = 0;
  int right = 0;
  int top = 0;
  int bottom = 0;
};

struct RenderData {
#define DEVICE_DPI 72

 private:
  int renderWidth = 0;
  int renderHeight = 0;

 public:
  std::string text;
  int textBoxWidth = 0;
  int textBoxHeight = 0;
  int fontSize;
  std::string fontName;
  float lineSpacingFactor;
  Color fontColor;
  PangoAlignment textAlignment;  // PangoAlignment
  _cairo_font_type fontType;
  gboolean justify;
  gboolean autoDir;
  PangoDirection dir;
  VerticalAlignment verticalAlignment;
  PangoFontDescription* fontDescription;
  PangoFontMap* fontMap;
  PangoContext* pangoContext;
  PangoLayout* pangoLayout;
  HorizontalWrapping horizontalWrapping;
  VerticalWrapping verticalWrapping;
  gboolean useMarkup;
  float resolutionMultiplier;
  RenderPadding padding;

  int RenderWidthPixels() { return renderWidth / PANGO_SCALE; }
  int RenderHeightPixels() { return renderHeight / PANGO_SCALE; }

  RenderData(std::string t,
             int tbw,
             int tbh,
             int fs,
             PangoAlignment ta,
             std::string fn,
             std::string face,
             Color fc,
             float lsf,
             gboolean j,
             gboolean autoDirection,
             PangoDirection direction,
             VerticalAlignment va,
             _cairo_font_type ft,
             HorizontalWrapping wrappingH,
             VerticalWrapping wrappingV,
             gboolean shouldUseMarkup,
             float resolutionMultp,
             bool automaticPadding = true,
             RenderPadding _padding = {}) {
    fontType = ft;
    text = std::move(t);
    textBoxWidth = tbw;
    textBoxHeight = tbh;
    fontSize = fs;
    textAlignment = ta;
    fontName = fn;
    fontColor = fc;
    lineSpacingFactor = lsf;
    justify = j;
    autoDir = autoDirection;
    dir = direction;
    verticalAlignment = va;
    horizontalWrapping = wrappingH;
    verticalWrapping = wrappingV;
    useMarkup = shouldUseMarkup;
    resolutionMultiplier = resolutionMultp;
    padding = _padding;

    fontMap = pango_cairo_font_map_new_for_font_type(ft);
    pangoContext = pango_font_map_create_context(fontMap);
    pangoLayout = pango_layout_new(pangoContext);

    // Disable ClearType antialiasing
    // TODO: only do this for win32 as it doesn't seem to affect FreeType (perhaps due to setting on font.conf?)
    {
        auto font_options = cairo_font_options_create();
        cairo_font_options_set_antialias(font_options, CAIRO_ANTIALIAS_GRAY);
        pango_cairo_context_set_font_options(pangoContext, font_options);
        cairo_font_options_destroy(font_options);
    }

    char* fnAsChar = nullptr;
    char* faceAsChar = nullptr;
    if (!fn.empty()) {
      fnAsChar = const_cast<char*>(fn.data());
    }

    if (!face.empty()) {
      faceAsChar = const_cast<char*>(face.data());
    }

    if (fnAsChar == nullptr || faceAsChar == nullptr) {
      pango_font_description_from_string("Sans");
    } else {
      fontDescription =
          GetFontDescriptionFromString(fnAsChar, faceAsChar, fontType);
    }
    if (fontDescription == nullptr) {
      printf("Could not find font (%s:%s), falling back to default font.\n",
             fontName.c_str(), face.c_str());

      // Create a default font description if GetFontDescriptionFromString
      // returns null
      fontDescription = pango_font_description_from_string("Sans");
      if (fontDescription == nullptr) {
        printf("Could not find default font (Sans)");
      }
    }

    faceAsChar = nullptr;
    fnAsChar = nullptr;
    double scaledFontSize =
        std::max((double)fontSize, 1.0) * resolutionMultiplier * PANGO_SCALE;

    pango_font_description_set_absolute_size(fontDescription, scaledFontSize);

    int ascent = 0;
    int lineHeight = 0;
    PangoFontMetrics* metrics = pango_context_get_metrics(
        pango_layout_get_context(pangoLayout),
        pango_layout_get_font_description(pangoLayout), nullptr);
    if (metrics) {
      ascent = pango_font_metrics_get_ascent(metrics) / PANGO_SCALE;
      lineHeight = (pango_font_metrics_get_ascent(metrics) +
                    pango_font_metrics_get_descent(metrics)) /
                   PANGO_SCALE;

      if (lineSpacingFactor != 0) {
        lineHeight = lineSpacingFactor - lineHeight;
        pango_layout_set_spacing(pangoLayout, lineHeight * PANGO_SCALE);
      }
      pango_font_metrics_unref(metrics);
    }
    if (useMarkup) {
      pango_layout_set_markup(pangoLayout, text.c_str(), -1);
    } else {
      pango_layout_set_text(pangoLayout, text.c_str(), -1);
    }
    pango_layout_set_alignment(pangoLayout, textAlignment);

    pango_layout_set_wrap(pangoLayout, PangoWrapMode::PANGO_WRAP_WORD_CHAR);
    pango_font_description_set_absolute_size(fontDescription, scaledFontSize);
    pango_layout_set_justify(pangoLayout, justify);
    pango_layout_set_font_description(pangoLayout, fontDescription);
    pango_context_set_base_dir(pangoContext, dir);
    pango_layout_set_auto_dir(pangoLayout, autoDir);

    int scaledTextBoxWidth =
        (int)((float)tbw * PANGO_SCALE * resolutionMultiplier);

    int scaledTextBoxHeight =
        (int)((float)tbh * PANGO_SCALE * resolutionMultiplier);

    // If automatic padding is enabled, calculate the padding
    if (automaticPadding) {
      // Here we set the width and height to the full size available, to
      // calculate the padding required for any overhanging characters.
      // NOTE: By adding any padding we calculate here later on, the characters
      // may move to other lines because of the wrapping settings. We won't be
      // accounting for that.
      pango_layout_set_width(pangoLayout, wrappingH == HorizontalWrapping::WrapH
                                              ? scaledTextBoxWidth
                                              : -1);
      pango_layout_set_height(
          pangoLayout,
          wrappingV == VerticalWrapping::ExpandV ? -1 : scaledTextBoxHeight);

      PangoRectangle inkRect;
      PangoRectangle logicalRect;
      pango_layout_get_extents(pangoLayout, &inkRect, &logicalRect);
      // Scale to pixel values...
      inkRect.x /= PANGO_SCALE;
      inkRect.y /= PANGO_SCALE;
      inkRect.width /= PANGO_SCALE;
      inkRect.height /= PANGO_SCALE;
      logicalRect.x /= PANGO_SCALE;
      logicalRect.y /= PANGO_SCALE;
      logicalRect.width /= PANGO_SCALE;
      logicalRect.height /= PANGO_SCALE;
      // For some fonts the characters' ink fall outside of the logical rect. We
      // use the ink rectangle values to calculate the automatic margins
      padding.top = (inkRect.y < 0) ? -inkRect.y : 0;
      int inkOverflowBottom =
          (inkRect.height + inkRect.y) - (logicalRect.height + logicalRect.y);
      padding.bottom = inkOverflowBottom > 0 ? inkOverflowBottom : 0;
      padding.left = (inkRect.x < 0) ? -inkRect.x : 0;
      int inkRightOverflow =
          (inkRect.width + inkRect.x) - (logicalRect.width + logicalRect.x);
      padding.right = inkRightOverflow > 0 ? inkRightOverflow : 0;
    }
    int availableWidth =
        scaledTextBoxWidth - (padding.left + padding.right) * PANGO_SCALE;
    // Only set the width if we want the text to wrap
    pango_layout_set_width(pangoLayout, wrappingH == HorizontalWrapping::WrapH
                                            ? availableWidth
                                            : -1);
    int availableHeight =
        scaledTextBoxHeight - (padding.top + padding.bottom) * PANGO_SCALE;
    pango_layout_set_height(pangoLayout, wrappingV == VerticalWrapping::ExpandV
                                             ? -1
                                             : availableHeight);

    PangoRectangle inkRect;
    PangoRectangle logicalRect;
    pango_layout_get_extents(pangoLayout, &inkRect, &logicalRect);

    renderWidth =
        logicalRect.width + (padding.left + padding.right) * PANGO_SCALE;
    // limit the renderWidth to the text box's size if horizontal expansion
    // isn't enabled
    if (renderWidth > scaledTextBoxWidth &&
        horizontalWrapping != HorizontalWrapping::ExpandH) {
      renderWidth = scaledTextBoxWidth;
    }
    renderHeight =
        logicalRect.height + (padding.top + padding.bottom) * PANGO_SCALE;
    // limit the renderHeight to the text box's size if vertical expansion
    // isn't enabled
    if (renderHeight > scaledTextBoxHeight &&
        verticalWrapping != HQText::VerticalWrapping::ExpandV) {
      renderHeight = scaledTextBoxHeight;
    }
  }

  ~RenderData() {
    pango_font_description_free(fontDescription);
    if (pangoLayout != nullptr) {
      g_object_unref(pangoLayout);
    } else {
      printf("Renderer.cpp line is null ( g_object_unref(pangoLayout);)");
    }
    if (pangoContext != nullptr) {
      g_object_unref(pangoContext);
    } else {
      printf("Renderer.cpp line is null ( g_object_unref(pangoContext);)");
    }
    if (fontMap != nullptr) {
      g_object_unref(fontMap);
    } else {
      printf("Renderer.cpp line is null ( g_object_unref(fontMap);)");
    }
  }
};
};      // namespace HQText
#endif  // HQTEXT_RENDERDATA_H
