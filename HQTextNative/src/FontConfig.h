#ifndef HQTEXTTEST_FONTCONFIG_H
#define HQTEXTTEST_FONTCONFIG_H

#include <fontconfig/fontconfig.h>
#include <pango/pango.h>
#include <pango/pangocairo.h>
#include "Unity/IUnityInterface.h"
namespace HQText {
extern "C" UNITY_INTERFACE_EXPORT FcBool FontConfigInitialized();
extern "C" UNITY_INTERFACE_EXPORT void DeinitializeFontConfig();
extern "C" UNITY_INTERFACE_EXPORT bool InitializeFontConfig(
    const FcChar8* filePath);
extern "C" UNITY_INTERFACE_EXPORT FcBool AddFontDir(const char* dirPath);
extern "C" UNITY_INTERFACE_EXPORT PangoFontFamily** GetAvailableFontFamilies(
    int& n_families,
    _cairo_font_type backendType);
extern "C" UNITY_INTERFACE_EXPORT PangoFontFace** GetAvailableFontFacesAtIndex(
    PangoFontFamily** families,
    int index,
    int& n_faces);
extern "C" UNITY_INTERFACE_EXPORT void FreeFontFaces(PangoFontFace*** faces);
extern "C" UNITY_INTERFACE_EXPORT void FreeFontFamilies(
    PangoFontFamily** families);

extern "C" UNITY_INTERFACE_EXPORT const char*
GetFontFaceAtIndex(PangoFontFace** faces, int index, int& sizeOfNameInBytes);
extern "C" UNITY_INTERFACE_EXPORT void GetFontFaceDescriptionAtIndex(
    PangoFontFace** faces,
    int index,
    PangoWeight& weight,
    PangoStretch& stretch,
    PangoVariant& variant,
    PangoStyle& style,
    PangoGravity& gravity,
    gboolean& isSynthesized);
extern "C" UNITY_INTERFACE_EXPORT void ListFonts();
extern "C" UNITY_INTERFACE_EXPORT const char* GetFontFamilyAtIndex(
    PangoFontFamily** families,
    int index,
    int& sizeOfNameInBytes);

extern "C" UNITY_INTERFACE_EXPORT PangoFontDescription*
GetFontDescriptionFromString(char* family,
                             char* face,
                             _cairo_font_type backendType);

}  // namespace HQText

// namespace HQText
#endif  // HQTEXTTEST_FONTCONFIG_H
