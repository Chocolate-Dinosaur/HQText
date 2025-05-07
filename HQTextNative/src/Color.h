#ifndef HQTEXT_COLOR_H
#define HQTEXT_COLOR_H
namespace HQText {
struct Color {
  double r;
  double g;
  double b;
  double a;

  Color() {
    r = 0;
    g = 0;
    b = 0;
    a = 0;
  }

  Color(double red, double green, double blue, double alpha) {
    r = red;
    g = green;
    b = blue;
    a = alpha;
  }
};
}  // namespace HQText
#endif  // HQTEXT_COLOR_H
