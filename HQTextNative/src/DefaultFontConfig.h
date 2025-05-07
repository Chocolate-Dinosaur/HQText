#ifndef HQTEXT_DEFAULTFONTCONFIG
#define HQTEXT_DEFAULTFONTCONFIG

const unsigned char defaultFontConfig[] = "<\?xml version=\"1.0\"\?><!DOCTYPE fontconfig SYSTEM \"fonts.dtd\"><fontconfig>\n\n<match target="
                    "\"font\" >\n    <!-- autohint was the old automatic hinter when hinting was patent\n    protected"
                    ", so turn it off to ensure any hinting information in the font\n    itself is used, this is the d"
                    "efault -->\n    <edit mode=\"assign\" name=\"autohint\">  <bool>false</bool></edit>\n\n\t<edit na"
                    "me=\"hintstyle\" mode=\"assign\">\n      <const>hintfull</const>\n    </edit>\n\n\t  <!-- antiali"
                    "asing is on by default and really helps for faint characters\n    and also for \'xft:\' fonts use"
                    "d in rxvt-unicode -->\n    <edit mode=\"assign\" name=\"antialias\"> <bool>true</bool></edit>\n\n"
                    "    <edit name=\"lcdfilter\" mode=\"assign\">\n      <const>lcddefault</const>\n    </edit>\n    "
                    "<edit name=\"rgba\" mode=\"assign\">\n      <const>rgh</const>\n    </edit>\n</match>\n<match tar"
                    "get=\"font\">\n\t\t<!-- check to see if the font is roman -->\n\t\t<test name=\"slant\">\n\t\t\t<"
                    "const>roman</const>\n\t\t</test>\n\t\t<!-- check to see if the pattern requested non-roman -->\n"
                    "\t\t<test target=\"pattern\" name=\"slant\" compare=\"not_eq\">\n\t\t\t<const>roman</const>\n\t\t"
                    "</test>\n\t\t<!-- multiply the matrix to slant the font -->\n\t\t<edit name=\"matrix\" mode=\"ass"
                    "ign\">\n\t\t\t<times>\n\t\t\t\t<name>matrix</name>\n\t\t\t\t<matrix><double>1</double><double>0.2"
                    "</double>\n\t\t\t\t\t<double>0</double><double>1</double>\n\t\t\t\t</matrix>\n\t\t\t</times>\n\t"
                    "\t</edit>\n\t\t<!-- pretend the font is oblique now -->\n\t\t<edit name=\"slant\" mode=\"assign\""
                    ">\n\t\t\t<const>oblique</const>\n\t\t</edit>\n\t\t<!-- and disable embedded bitmaps for artificia"
                    "l oblique -->\n\t\t<edit name=\"embeddedbitmap\" mode=\"assign\">\n\t\t\t<bool>false</bool>\n\t\t"
                    "</edit>\n\t</match>\n<!--\n Synthetic emboldening for fonts that do not have bold face available"
                    "\n -->\n\t<match target=\"font\">\n\t\t<!-- check to see if the weight in the font is less than m"
                    "edium which possibly need emboldening -->\n\t\t<test name=\"weight\" compare=\"less_eq\">\n\t\t\t"
                    "<const>medium</const>\n\t\t</test>\n\t\t<!-- check to see if the pattern requests bold -->\n\t\t<"
                    "test target=\"pattern\" name=\"weight\" compare=\"more_eq\">\n\t\t\t<const>bold</const>\n\t\t</te"
                    "st>\n\t\t<!--\n\t\t  set the embolden flag\n\t\t  needed for applications using cairo, e.g. gucha"
                    "rmap, gedit, ...\n\t\t-->\n\t\t<edit name=\"embolden\" mode=\"assign\">\n\t\t\t<bool>true</bool>"
                    "\n\t\t</edit>\n\t\t<!--\n\t\t set weight to bold\n\t\t needed for applications using Xft directly"
                    ", e.g. Firefox, ...\n\t\t-->\n\t\t<edit name=\"weight\" mode=\"assign\">\n\t\t\t<const>bold</cons"
                    "t>\n\t\t</edit>\n\t</match>\n</fontconfig>\n";

#endif