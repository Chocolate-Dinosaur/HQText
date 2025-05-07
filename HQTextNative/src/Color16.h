#ifndef HQTEXT_COLOR16_H
#define HQTEXT_COLOR16_H

#include <glibconfig.h>

namespace HQText {
struct Color16 {
  guint16 r;
  guint16 g;
  guint16 b;
  guint16 a;

  Color16() {
    r = 0;
    g = 0;
    b = 0;
    a = 1;
  }
};
}  // namespace HQText

#endif  // HQTEXT_COLOR16_H
