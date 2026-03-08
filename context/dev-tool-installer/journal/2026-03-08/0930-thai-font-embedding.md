# Thai Font Embedding

**Date:** 2026-03-08  
**Task:** Embed Thai fonts directly in DevToolInstaller and rewrite ThaiFontInstaller

## Summary

Replaced the external binary delegation approach for Thai font installation with self-contained embedded TTF files. The fonts are now bundled directly with the published executable.

## Old Approach (External Binary)

- `ThaiFontInstaller.cs` delegated to an external `FontInstaller.Console.exe` binary
- Tried multiple strategies to find the binary:
  1. Hardcoded local dev path (`C:\Users\utarn\projects\font-installer\...`)
  2. Search PATH for `FontInstaller.Console.exe`
  3. Download from GitHub releases (`utarn/font-installer`)
  4. Check alternative local paths (macOS/Linux development paths)
- Launched the external process with `--embedded` flag and `runas` verb
- Required network access or pre-existing local binary

## New Approach (Embedded Fonts)

- All 50 Thai font TTF files are copied into `font/thai/` directory in the project
- `DevToolInstaller.csproj` includes them as `Content` items with `CopyToPublishDirectory`
- `ThaiFontInstaller.cs` directly installs fonts using Win32 P/Invoke (same pattern as `FontInstaller.cs`):
  1. Reads TTF files from `font/thai/` relative to `AppContext.BaseDirectory`
  2. Copies each font to `C:\Windows\Fonts` (skips if same size already exists)
  3. Registers in registry at `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts`
  4. Calls `AddFontResource()` for each new font
  5. Broadcasts `WM_FONTCHANGE` once at the end
- `IsInstalledAsync` checks for representative font `THSarabunNew.ttf` in Windows Fonts
- No network access required, no external dependencies

## Font Families Included (50 TTF files)

| Family | Variants |
|--------|----------|
| TH Baijam | Regular, Bold, Italic, Bold Italic |
| TH Chakra Petch | Regular, Bold, Italic, Bold Italic |
| TH Charm of AU | Regular |
| TH Charmonman | Regular, Bold |
| TH Fahkwang | Regular, Bold, Italic, Bold Italic |
| TH K2D July8 | Regular, Bold, Italic, Bold Italic |
| TH Kodchasal | Regular, Bold, Italic, Bold Italic |
| TH KoHo | Regular, Bold, Italic, Bold Italic |
| TH Krub | Regular, Bold, Italic, Bold Italic |
| TH Mali Grade6 | Regular, Bold, Italic, Bold Italic |
| TH Niramit AS | Regular, Bold, Italic, Bold Italic |
| TH Srisakdi | Regular, Bold |
| THSarabun | Regular, Bold, Italic, Bold Italic, BoldItalic |
| THSarabunNew | Regular, Bold, Italic, BoldItalic |

## Files Changed

- **Added:** `font/thai/*.ttf` (50 files)
- **Modified:** `DevToolInstaller.csproj` — added Content item for `font/thai/**/*.ttf`
- **Rewritten:** `Installers/ThaiFontInstaller.cs` — self-contained P/Invoke font installation
- **Updated:** `DevToolInstaller.Tests/Installers/FontInstallersTests.cs` — updated tests for new implementation
- **Updated:** `context/docs/font-installer.md` — documentation reflects new approach

## Technical Notes

- Project uses AOT (`PublishAot`), so fonts are bundled as Content files (not EmbeddedResource)
- P/Invoke declarations are duplicated from `FontInstaller.cs` since `partial class` with `LibraryImport` requires them per class
- Skip logic compares file size to avoid unnecessary overwrites of already-installed fonts
- Existing `font/THSARABUN_PSK.zip` is preserved (not deleted)