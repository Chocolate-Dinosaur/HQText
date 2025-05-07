#include <pango/pango.h>
#include <pango/pangocairo.h>
#include <cmath>
#include <iostream>
#include <vector>
#include "RenderData.h"
#include "Unity/IUnityInterface.h"
#define LINE_IS_VALID(line) ((line) && (line)->layout != NULL)

struct layoutOffset {
  double x = 0;
  double y = 0;
};

layoutOffset calculateOffset(
    PangoLayout* pangoLayout,
    unsigned int w,
    unsigned int h,
    PangoAlignment horAlignment,
    HQText::VerticalAlignment verAlignment,
    HQText::RenderPadding padding = HQText::RenderPadding{}) {
  PangoRectangle inkRect, logicalRect;
  pango_layout_get_extents(pangoLayout, &inkRect, &logicalRect);

  double layoutWidth = pango_layout_get_width(pangoLayout);
  if (layoutWidth < 0) {
    layoutWidth = (double)logicalRect.width;
  }
  layoutWidth /= PANGO_SCALE;
  double layoutHeight = (double)logicalRect.height / PANGO_SCALE;

  PangoAlignment alignment = horAlignment;
  PangoLayoutLine* line = pango_layout_get_line(pangoLayout, 0);
  PangoDirection direction = PangoDirection::PANGO_DIRECTION_NEUTRAL;
  if (LINE_IS_VALID(line)) {
    direction = (PangoDirection)line->resolved_dir;
  }
  if (line != nullptr) {
    g_object_unref(line);
  }
  // If the direction is left to right, swap the alignment.
  if (direction == PangoDirection::PANGO_DIRECTION_RTL ||
      direction == PangoDirection::PANGO_DIRECTION_WEAK_RTL) {
    if (alignment == PangoAlignment::PANGO_ALIGN_LEFT) {
      alignment = PangoAlignment::PANGO_ALIGN_RIGHT;
    } else if (alignment == PangoAlignment::PANGO_ALIGN_RIGHT) {
      alignment = PangoAlignment::PANGO_ALIGN_LEFT;
    }
  }

  double usableWidth = w - padding.left - padding.right;
  double usableHeight = h - padding.top - padding.bottom;

  double offsetX = padding.left;
  if (alignment == PangoAlignment::PANGO_ALIGN_CENTER) {
    offsetX += (usableWidth - layoutWidth) / 2;
  } else if (alignment == PangoAlignment::PANGO_ALIGN_RIGHT) {
    offsetX += usableWidth - layoutWidth;
  }
  double offsetY = padding.top;
  if (verAlignment == HQText::VerticalAlignment::middle) {
    offsetY += (usableHeight - layoutHeight) / 2;
  } else if (verAlignment == HQText::VerticalAlignment::bottom) {
    offsetY += usableHeight - layoutHeight;
  }
  layoutOffset result;
  result.x = offsetX;
  result.y = offsetY;
  return result;
}

namespace HQText {

extern "C" UNITY_INTERFACE_EXPORT cairo_surface_t* RenderToSurface(
    RenderData* r,
    int surfaceWidth,
    int surfaceHeight,
    bool fillBackground) {
    cairo_surface_t* surface = cairo_image_surface_create(
      CAIRO_FORMAT_ARGB32, surfaceWidth, surfaceHeight);
  cairo_t* cr = cairo_create(surface);
 
  if (fillBackground) {
    cairo_set_source_rgba(cr, 1, 1, 1, 1);
  } else {
    cairo_set_source_rgba(cr, 0, 0, 0, 0);
  }
  cairo_paint(cr);

  //cairo_scale(cr, 1, -1);
  //cairo_translate(cr, 0, -surfaceHeight); // replace SURFACE_HEIGHT
  /*
  cairo_matrix_t x_reflection_matrix;
  cairo_matrix_init_identity(&x_reflection_matrix); // could not find a oneliner
    x_reflection_matrix.yy = -1.0;
  cairo_set_matrix(cr, &x_reflection_matrix);
  // This would result in your drawing being done on top of the destination 
  // surface, so we translate the surface down the full height
  
  */

 

  // Draw TRIAL VERSION text
  
   {
      cairo_select_font_face(cr, "@cairo:monospace", CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_BOLD);
      cairo_set_font_size(cr, r->fontSize * 0.25);

      cairo_text_extents_t extents;
      cairo_text_extents(cr, "HQTEXT TRIAL VERSION", &extents);

      cairo_move_to(cr, (surfaceWidth / 2) - (extents.width / 2), (surfaceHeight / 2) + (extents.height / 2));
      cairo_set_source_rgba(cr, 0.5, 0.0, 0.0, 0.5);

      cairo_show_text(cr, "HQTEXT TRIAL VERSION");
  }

  pango_cairo_update_layout(cr, r->pangoLayout);
  auto offset =
      calculateOffset(r->pangoLayout, surfaceWidth, surfaceHeight,
                      r->textAlignment, r->verticalAlignment, r->padding);

  // TODO: Factor in font ascent and line height into the the calculations.
  // position of the topMargin-left corner of the layout
  cairo_move_to(cr, offset.x, offset.y);
  auto color = r->fontColor;
  cairo_set_source_rgba(cr, color.r, color.g, color.b, color.a);    // not premultiplied alpha
  pango_cairo_show_layout(cr, r->pangoLayout);  // draw layout

  cairo_destroy(cr);

  return surface;
}

extern "C" UNITY_INTERFACE_EXPORT void WriteToPNG(char* filepath,
                                                  cairo_surface_t* surface) {
  cairo_surface_write_to_png(surface, filepath);
}

extern "C" UNITY_INTERFACE_EXPORT void ReleaseSurface(
    cairo_surface_t* surface) {
  cairo_surface_destroy(surface);
}

struct lineRange {
  int start = -1;
  int end = -1;
};

std::vector<lineRange> generateClusterToLineMapping(PangoLayout* l) {
  std::vector<PangoRectangle> lineRects;
  std::vector<lineRange> lineRanges;

  PangoLayoutIter* lineIt = pango_layout_get_iter(l);
  do {
    PangoRectangle lineRect;
    pango_layout_iter_get_line_extents(lineIt, nullptr, &lineRect);
    lineRects.push_back(lineRect);
    lineRanges.push_back({});
  } while (pango_layout_iter_next_line(lineIt));
  pango_layout_iter_free(lineIt);

  PangoLayoutIter* clusterIt = pango_layout_get_iter(l);
  int clusterIndex = 0;
  int lineIndex = 0;
  do {
    PangoRectangle clusterRect;
    pango_layout_iter_get_cluster_extents(clusterIt, nullptr, &clusterRect);
    // Assume that cluster won't be in previous lines
    for (int li = lineIndex; li < lineRects.size(); ++li) {
      auto lineRect = lineRects[li];
      if (clusterRect.y >= lineRect.y &&
          clusterRect.y + clusterRect.height <= lineRect.y + lineRect.height) {
        if (lineRanges[li].start < 0) {
          lineRanges[li].start = clusterIndex;
        }
        if (clusterIndex > lineRanges[li].end) {
          lineRanges[li].end = clusterIndex;
        }
        break;
      }
    }
    clusterIndex++;
  } while (pango_layout_iter_next_cluster(clusterIt));
  pango_layout_iter_free(clusterIt);
  return lineRanges;
}

// GetRenderedClusterRects calculates the cluster int rectangles and populates
// rects with the information. The value returned is the actual number of
// rectangles populated.
extern "C" UNITY_INTERFACE_EXPORT int GetRenderedClusterRects(
    RenderData* renderData,
    int surfaceWidth,
    int surfaceHeight,
    PangoRectangle* rects,
    int count) {
  // Initialize the rectangles
  for (int c = 0; c < count; c++) {
    rects[c] = PangoRectangle{0, 0, 0, 0};
  }

  int characterCount =
      pango_layout_get_character_count(renderData->pangoLayout);
  if (characterCount == 0) {
    return 0;
  }

  auto offset =
      calculateOffset(renderData->pangoLayout, surfaceWidth, surfaceHeight,
                      renderData->textAlignment, renderData->verticalAlignment,
                      renderData->padding);

  PangoLayoutIter* it = pango_layout_get_iter(renderData->pangoLayout);
  std::vector<PangoRectangle> clusterRects;
  do {
    PangoRectangle rect;
    pango_layout_iter_get_cluster_extents(it, &rect, nullptr);

    rect.x /= PANGO_SCALE;
    rect.y /= PANGO_SCALE;
    rect.width /= PANGO_SCALE;
    rect.height /= PANGO_SCALE;
    rect.x += std::floor(offset.x);
    rect.y += std::floor(offset.y);

    clusterRects.push_back(rect);
  } while (pango_layout_iter_next_cluster(it) && clusterRects.size() <= count);

  pango_layout_iter_free(it);

  // We need to swap the position of characters in each line if the
  // direction
  // is right to left, to have them in the correct order from begin to end.
  if ((PangoDirection)pango_layout_get_line_readonly(renderData->pangoLayout, 0)
          ->resolved_dir == PANGO_DIRECTION_RTL) {
    auto clusterLineMap = generateClusterToLineMapping(renderData->pangoLayout);
    for (auto lineRange : clusterLineMap) {
      int half = (lineRange.end - lineRange.start + 1) / 2;
      // Flip positions of the characters in the line
      for (int ci = 0; ci < half; ci++) {
        std::swap(clusterRects[lineRange.start + ci],
                  clusterRects[lineRange.end - ci]);
      }
    }
  }
  std::copy(clusterRects.begin(), clusterRects.end(), rects);
  return (int)clusterRects.size();  // add one to index to return amount
}

}  // namespace HQText
