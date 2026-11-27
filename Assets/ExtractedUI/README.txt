UI extracted from D:/Download/calendar.jpg and D:/Download/setting.jpg

Calendar: 41 separate PNG assets.
Setting: 50 separate PNG assets, including the version panel and delete button.

The PNG files use RGBA transparency. Visible pixels are copied from the JPEG
sources at their original resolution, without recolouring or rescaling.
Text is separated by UI label, not by individual alphabet glyph.

IMPORTANT: These are visible-layer cutouts, not reconstructed source artwork.
Backgrounds have transparent holes where foreground text and icons were removed.
Parts obscured by other artwork (notably the first two day labels and reward
icons under the wax seals) cannot be recovered from the screenshots and are not
invented. JPEG compression and antialiasing remain in retained source pixels.
Masks are approximate along textured and overlapping boundaries.

manifest.json records source-image x/y positions, dimensions and layer parents.
Coordinates use the upper-left corner of the original 2796 x 1290 screenshot.
To reconstruct a layout, place parent backgrounds first and foreground assets
above them at the recorded coordinates. Do not stretch individual assets.

Calendar_preview.png and Setting_preview.png are overview sheets with a solid
blue-grey backdrop for inspection. That backdrop is not in the individual PNGs.

Extraction: Tools/extract_ui_assets.ps1 (PowerShell and System.Drawing).
Validation: every nontransparent exported pixel is checked against its source
RGB value. This check verifies pixel fidelity, not completeness of hidden art.
