# Oh My Posh + PowerShell Profile Installer

## ภาพรวม

ตัวติดตั้งนี้รวม 3 ฟีเจอร์ในขั้นตอนเดียว:

1. **Oh My Posh** - prompt theme engine สำหรับ terminal
2. **Custom Paradox Theme** - theme ที่ปรับแต่งแล้วรองรับ Python virtual environment display
3. **PowerShell Profile** - ตั้งค่า PSReadLine history + ListView prediction

## ไฟล์ที่เกี่ยวข้อง

| ไฟล์ | คำอธิบาย |
|------|----------|
| `Installers/OhMyPoshInstaller.cs` | Installer หลัก |
| `config/paradox.omp.json` | Custom Paradox theme (bundled) |
| `DevToolInstaller.csproj` | มี `<Content>` element สำหรับ bundle config/ folder |

## ขั้นตอนการติดตั้ง

### 1. ติดตั้ง Oh My Posh binary

- ใช้ `winget install --id=JanDeDobbeleer.OhMyPosh`
- ถ้ามีอยู่แล้วจะข้ามขั้นตอนนี้

### 2. คัดลอก Custom Theme

- คัดลอก `config/paradox.omp.json` จาก bundled files ไปยัง `Documents\PowerShell\paradox.omp.json`
- ถ้าไม่พบไฟล์ bundled จะ fallback ไปใช้ built-in paradox theme

### 3. สร้าง/อัปเดต PowerShell Profile

สร้างไฟล์ `Documents\PowerShell\Microsoft.PowerShell_profile.ps1` พร้อมเนื้อหา:

```powershell
oh-my-posh init pwsh --config "C:\Users\<user>\Documents\PowerShell\paradox.omp.json" | Invoke-Expression
Set-PSReadLineOption -PredictionSource History
Set-PSReadLineOption -PredictionViewStyle ListView
```

- ถ้ามี profile อยู่แล้ว จะ **รักษาบรรทัดที่ไม่เกี่ยวข้อง** และแทนที่เฉพาะ OMP + PSReadLine config

### 4. ตั้งค่า Windows Terminal

แก้ไขไฟล์ `settings.json` ของ Windows Terminal ให้:

| การตั้งค่า | ค่า | คำอธิบาย |
|-----------|-----|----------|
| `defaultProfile` | `{574e775e-4f2a-5b96-ac1e-a2962a402336}` | ตั้ง PowerShell 7 (pwsh) เป็น default shell |
| `profiles.defaults.font.face` | `CaskaydiaCove Nerd Font` | ฟอนต์ที่รองรับ Powerline/Nerd Font icons |
| `profiles.defaults.opacity` | `70` | ความโปร่งใส 70% |
| `profiles.defaults.useAcrylic` | `false` | ไม่ใช้ Acrylic effect |
| `copyFormatting` | `none` | คัดลอกแบบ plain text |
| `copyOnSelect` | `false` | ไม่คัดลอกอัตโนมัติเมื่อเลือกข้อความ |

**ตำแหน่งไฟล์ที่รองรับ:**
- Stable: `%LOCALAPPDATA%\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json`
- Preview: `%LOCALAPPDATA%\Packages\Microsoft.WindowsTerminalPreview_8wekyb3d8bbwe\LocalState\settings.json`

ถ้าไม่พบ Windows Terminal จะข้ามขั้นตอนนี้โดยไม่ทำให้ installer fail

## Custom Paradox Theme

Theme นี้แก้ไขจาก paradox theme ต้นฉบับโดย:

- **เพิ่ม Python segment** ที่แสดง virtual environment name (`.Venv`) แทนที่จะแสดง Python version
- Properties: `home_enabled: true`, `display_virtual_env: true`
- Template: แสดงเฉพาะ venv name ไม่แสดง Python version เพื่อความกระชับ

### Segments ที่แสดง (ซ้ายไปขวา):

1. ⚡ Root indicator (เหลือง)
2. User@Host (ขาว)
3. Full path (ฟ้า)
4. Git branch/status (เขียว)
5. Python venv name (ม่วง)
6. Error status (แดง)
7. ❯ prompt (บรรทัดใหม่, สีน้ำเงิน)

## การตรวจสอบว่าติดตั้งแล้ว

- ตรวจว่า `oh-my-posh` อยู่ใน PATH
- ตรวจว่า profile มีบรรทัด `oh-my-posh init pwsh`
- ถ้าทั้งสองเงื่อนไขเป็นจริง จะรายงานว่าติดตั้งแล้ว

## Dependencies

- PowerShell 7 (ต้องติดตั้งก่อน)
- winget (สำหรับติดตั้ง oh-my-posh)
- Nerd Font (แนะนำ CaskaydiaMono Nerd Font จาก Font Installer)