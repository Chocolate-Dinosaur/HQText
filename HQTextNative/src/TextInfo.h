#include <pango/pango.h>
#ifndef HQTEXTTEST_TEXTINFO_H
#define HQTEXTTEST_TEXTINFO_H
namespace HQText {
struct TextInfo {
  int width;
  int height;
  int widthLogical;
  int heightLogical;
  int widthInk;
  int heightInk;
  PangoDirection direction;
  int lineCount;
  int characterCount;
  int ascent;
  int descent;
  int lineHeight;

  TextInfo(int w,
           int h,
           int wLogical,
           int hLogical,
           int wInk,
           int hInk,
           PangoDirection dir,
           int lCount,
           int cCount,
           int asc,
           int des,
           int lHeight) {
    width = w;
    height = h;
    widthLogical = wLogical;
    heightLogical = hLogical;
    widthInk = wInk;
    heightInk = hInk;
    direction = dir;
    lineCount = lCount;
    characterCount = cCount;
    ascent = asc;
    descent = des;
    lineHeight = lHeight;
  }
};
}  // namespace HQText
#endif  // HQTEXTTEST_TEXTINFO_H
