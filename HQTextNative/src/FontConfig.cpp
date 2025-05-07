#include <fontconfig/fontconfig.h>
#include <pango/pango.h>
#include <pango/pangocairo.h>
#include <mutex>
#include <vector>
#include "Unity/IUnityInterface.h"
#include "DefaultFontConfig.h"

namespace HQText {
static FcConfig* fontConfig = nullptr;
std::mutex configMutex;

void setConfig(FcConfig* config) {
  configMutex.lock();
  fontConfig = config;
  FcConfigSetCurrent(fontConfig);
  configMutex.unlock();
}

extern "C" UNITY_INTERFACE_EXPORT FcBool FontConfigInitialized() {
  return fontConfig != nullptr;
}
extern "C" UNITY_INTERFACE_EXPORT void DeinitializeFontConfig() {
  if (fontConfig != nullptr) {
    configMutex.lock();
    FcConfigDestroy(fontConfig);
    fontConfig = nullptr;
    configMutex.unlock();
  }
}

extern "C" UNITY_INTERFACE_EXPORT bool InitializeFontConfig(
    const FcChar8* filePath) {
  // Clear any existing config
  DeinitializeFontConfig();

  FcConfig* config = FcConfigCreate();


  FcBool success = FcFalse;
  //success = FcConfigParseAndLoad(config, filePath, true) != 0;
  success = FcConfigParseAndLoadFromMemory(config, defaultFontConfig, true) != 0;
  if (!success) {
    FcConfigDestroy(config);
    return false;
  }

  setConfig(config);
  return true;
}

extern "C" UNITY_INTERFACE_EXPORT FcBool AddFontDir(const char* dirPath) {
    FcConfigAppFontClear(FcConfigGetCurrent());
  return FcConfigAppFontAddDir(FcConfigGetCurrent(), (FcChar8*)dirPath);
}

extern "C" UNITY_INTERFACE_EXPORT PangoFontFamily** GetAvailableFontFamilies(
    int& n_families,
    _cairo_font_type backendType) {
  PangoFontMap* fontMap;
  PangoFontFamily** families;

  fontMap = pango_cairo_font_map_new_for_font_type(backendType);
  pango_font_map_list_families(fontMap, &families, &n_families);

  return families;
}

extern "C" UNITY_INTERFACE_EXPORT PangoFontDescription*
GetFontDescriptionFromString(char* family,
                             char* face,
                             _cairo_font_type backendType) {
  // Get the list of font families
  PangoFontFamily** families;
  int n_families;

  auto fontMap = pango_cairo_font_map_new_for_font_type(backendType);
  pango_font_map_list_families(fontMap, &families, &n_families);

  PangoFontDescription* description = nullptr;

  // Find the desired font family
  for (int i = 0; i < n_families; i++) {
    const char* family_name = pango_font_family_get_name(families[i]);
    if (strcmp(family_name, family) == 0) {
      // Find the desired font face
      PangoFontFace** faces;
      int n_faces;
      pango_font_family_list_faces(families[i], &faces, &n_faces);

      for (int j = 0; j < n_faces; j++) {
        const char* face_name = pango_font_face_get_face_name(faces[j]);
        if (strcmp(face_name, face) == 0) {
          // Get the font description for the desired font face
          description = pango_font_face_describe(faces[j]);
          break;
        }
      }

      // Free the font faces array
      g_free(faces);
      break;
    }
  }

  // Free the font families array
  g_free(families);
  g_object_unref(fontMap);

  return description;
}

extern "C" UNITY_INTERFACE_EXPORT PangoFontFace** GetAvailableFontFacesAtIndex(
    PangoFontFamily** families,
    int index,
    int& n_faces) {
  PangoFontFace** all_faces;
  pango_font_family_list_faces(families[index], &all_faces, &n_faces);
  return all_faces;
  // optional: this code below filters out synthetic faces.
  /*
std::vector<PangoFontFace*> non_synthetic_faces =
    std::vector<PangoFontFace*>();
for (int i = 0; i < n_faces; ++i) {
  if (!pango_font_face_is_synthesized(all_faces[i])) {
    non_synthetic_faces.push_back(all_faces[i]);
  }
}

n_faces = non_synthetic_faces.size();
PangoFontFace** filtered_faces = new PangoFontFace*[n_faces];
for (int i = 0; i < n_faces; ++i) {
  filtered_faces[i] = non_synthetic_faces[i];
}

g_free(all_faces);
return filtered_faces;*/
}
extern "C" UNITY_INTERFACE_EXPORT void FreeFontFaces(PangoFontFace*** faces) {
  g_free(faces);
}

extern "C" UNITY_INTERFACE_EXPORT const char*
GetFontFaceAtIndex(PangoFontFace** faces, int index, int& sizeOfNameInBytes) {
  const char* faceName = pango_font_face_get_face_name(faces[index]);
  sizeOfNameInBytes = strlen(faceName);
  return faceName;
}

extern "C" UNITY_INTERFACE_EXPORT void GetFontFaceDescriptionAtIndex(
    PangoFontFace** faces,
    int index,
    PangoWeight& weight,
    PangoStretch& stretch,
    PangoVariant& variant,
    PangoStyle& style,
    PangoGravity& gravity,
    gboolean& isSynthesized) {
  PangoFontDescription* description = pango_font_face_describe(faces[index]);
  weight = pango_font_description_get_weight(description);
  stretch = pango_font_description_get_stretch(description);
  variant = pango_font_description_get_variant(description);
  style = pango_font_description_get_style(description);
  gravity = pango_font_description_get_gravity(description);
  pango_font_description_free(description);
  isSynthesized = pango_font_face_is_synthesized(faces[index]);
}

extern "C" UNITY_INTERFACE_EXPORT void FreeFontFamilies(
    PangoFontFamily** families) {
  g_free(families);
}

extern "C" UNITY_INTERFACE_EXPORT const char* GetFontFamilyAtIndex(
    PangoFontFamily** families,
    int index,
    int& sizeOfNameInBytes) {
  const char* familyName = pango_font_family_get_name(families[index]);
  sizeOfNameInBytes = strlen(familyName);
  return familyName;
}

// helper function to just print all the fonts
extern "C" UNITY_INTERFACE_EXPORT void ListFonts() {
  int i;
  PangoFontFamily** families;
  int n_families;
  PangoFontMap* fontmap;

  fontmap = pango_cairo_font_map_get_default();
  pango_font_map_list_families(fontmap, &families, &n_families);
  for (i = 0; i < n_families; i++) {
    PangoFontFamily* family = families[i];
    const char* family_name;
    family_name = pango_font_family_get_name(family);
    delete family_name;
  }

  g_free(families);
}
}  // namespace HQText
