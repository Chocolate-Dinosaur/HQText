#ifndef HQTEXT_TEXTSIZE_H
#define HQTEXT_TEXTSIZE_H
namespace HQText {
struct TextSize {
  int widthLogical;
  int heightLogical;
  int widthInk;
  int heightInk;

  TextSize(int wLogical, int hLogical, int wInk, int hInk) {
    widthLogical = wLogical;
    heightLogical = hLogical;
    widthInk = wInk;
    heightInk = hInk;
  }
};
}  // namespace HQText
#endif  // HQTEXT_TEXTSIZE_H
