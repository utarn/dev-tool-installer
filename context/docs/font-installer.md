# Font Installer

เอกสารนี้อธิบายการทำงานของตัวติดตั้งฟอนต์ในโปรเจกต์ DevToolInstaller

## ตัวติดตั้งที่มีอยู่

### 1. CascadiaMono Nerd Font (`FontInstaller.cs`)

ดาวน์โหลดและติดตั้ง CascadiaMono Nerd Font จาก GitHub releases

- **ที่มา:** ดาวน์โหลด zip จาก `github.com/ryanoasis/nerd-fonts`
- **พฤติกรรม:** `AlwaysRun = true` — ติดตั้งใหม่ทุกครั้ง (overwrite)
- **IsInstalled:** คืนค่า `false` เสมอ

### 2. Thai Fonts (`ThaiFontInstaller.cs`)

ติดตั้งฟอนต์ไทย 50 ไฟล์ที่ฝังมากับตัว exe โดยตรง

- **ที่มา:** ไฟล์ TTF ใน `font/thai/` ที่ถูก bundle มากับ publish output
- **พฤติกรรม:** `AlwaysRun = false` — ข้ามถ้าติดตั้งแล้ว
- **IsInstalled:** ตรวจสอบว่ามีไฟล์ `THSarabunNew.ttf` ใน `C:\Windows\Fonts` หรือไม่

## ที่มาไฟล์ฟอนต์

### CascadiaMono
ดาวน์โหลดจาก GitHub releases ขณะติดตั้ง

### Thai Fonts
ไฟล์ TTF ฝังอยู่ในโฟลเดอร์ `font/thai/` ของโปรเจกต์ (50 ไฟล์, 14 ตระกูลฟอนต์):

| ตระกูลฟอนต์ | รูปแบบ |
|-------------|--------|
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

ไฟล์เหล่านี้คัดลอกมาจากโปรเจกต์ `utarn/font-installer`

> หมายเหตุ: ไฟล์ `font/THSARABUN_PSK.zip` ยังคงอยู่ในโปรเจกต์ (legacy)

## วิธีติดตั้ง

ทั้งสองตัวติดตั้งใช้วิธีเดียวกัน (Win32 P/Invoke):

1. ตรวจสอบสิทธิ์แอดมิน (จำเป็นสำหรับเขียนลงโฟลเดอร์ฟอนต์ของ Windows)
2. ค้นหาไฟล์ฟอนต์นามสกุล `.ttf` (CascadiaMono: จาก zip ที่ดาวน์โหลด, Thai Fonts: จาก `font/thai/`)
3. คัดลอกไฟล์ไปยังโฟลเดอร์ `C:\Windows\Fonts`
4. ลงทะเบียนฟอนต์ใน Registry (`HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts`)
5. เรียก `AddFontResource()` เพื่อให้ฟอนต์พร้อมใช้ทันที
6. ส่ง `WM_FONTCHANGE` broadcast เพื่อแจ้งแอปพลิเคชันทั้งหมด

## พฤติกรรมเมื่อมีฟอนต์อยู่แล้ว

- **CascadiaMono:** ใช้ `File.Copy(..., overwrite: true)` — แทนที่ทุกครั้ง
- **Thai Fonts:** เปรียบเทียบขนาดไฟล์ก่อน — ข้ามถ้าขนาดเท่ากัน, แทนที่ถ้าต่างกัน

## ข้อจำกัด

- รองรับเฉพาะ Windows
- ต้องรันโปรแกรมด้วยสิทธิ์ Administrator
- โปรเจกต์ใช้ AOT (`PublishAot`) จึงใช้ Content files แทน EmbeddedResource