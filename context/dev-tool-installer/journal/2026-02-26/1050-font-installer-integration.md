# 1050 - Font Installer Integration

## สรุปงาน

เพิ่มตัวติดตั้งฟอนต์ใหม่ในระบบ โดยอ่านไฟล์ zip จากโฟลเดอร์ `font/` และติดตั้งไฟล์ฟอนต์ลง Windows Fonts

## ไฟล์ที่เปลี่ยนแปลง

- `Installers/BrowserInstallers.cs`
  - เพิ่มคลาส `FontInstaller : IInstaller`
  - แตก zip ด้วย `System.IO.Compression.ZipFile`
  - คัดลอกไฟล์ `.ttf` / `.otf` ไป `C:\Windows\Fonts`
  - ใช้ `File.Copy(..., overwrite: true)` เพื่อ replace ไฟล์ชื่อเดิม
  - บังคับเงื่อนไขสิทธิ์ Administrator
- `ToolRegistry.cs`
  - ลงทะเบียน `FontInstaller` ในรายการเครื่องมือ
- `context/docs/INDEX.md`
  - เพิ่มลิงก์เอกสาร `font-installer.md`
- `context/dev-tool-installer/INDEX.md`
  - เพิ่มรายการ journal ฉบับนี้

## พฤติกรรมสำคัญ

- ไม่ skip โดยสถานะติดตั้งเดิม (`IsInstalledAsync` คืน `false` เสมอ) เพื่อให้ user สามารถสั่งรันติดตั้งฟอนต์ซ้ำเพื่อ replace ได้
- ถ้ามีไฟล์ฟอนต์ชื่อเดียวกันอยู่แล้ว ระบบจะ replace ให้

## ข้อสังเกต

การ replace เป็นระดับไฟล์ (ตามชื่อไฟล์) ไม่ได้อ่าน metadata เพื่อเทียบ version ภายในฟอนต์