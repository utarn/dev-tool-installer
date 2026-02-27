# Oh My Posh Installer Integration

**วันที่:** 2026-02-26  
**เวลา:** 11:15 (UTC+7)

## บริบท

ผู้ใช้มี PowerShell profile ที่ตั้งค่า oh-my-posh ด้วย custom Paradox theme ที่รองรับ Python virtual environment display และ PSReadLine history prediction ต้องการให้ installer ตั้งค่าเหล่านี้ให้เครื่องอื่นได้อัตโนมัติ

## การตั้งค่าเดิมของผู้ใช้

### PowerShell Profile (`Microsoft.PowerShell_profile.ps1`)
```powershell
oh-my-posh init pwsh --config "C:\Users\Korn\Documents\PowerShell\paradox.omp.json" | Invoke-Expression
Set-PSReadLineOption -PredictionSource History
Set-PSReadLineOption -PredictionViewStyle ListView
```

### Custom Paradox Theme (`paradox.omp.json`)
- แก้จากต้นฉบับ: เพิ่ม Python segment ที่แสดง venv name
- Properties: `home_enabled`, `display_virtual_env`
- Template แสดงเฉพาะ venv name ไม่แสดง Python version

## สิ่งที่สร้าง

### ไฟล์ใหม่
| ไฟล์ | คำอธิบาย |
|------|----------|
| `config/paradox.omp.json` | Custom theme bundled กับ project |
| `Installers/OhMyPoshInstaller.cs` | Installer ครบวงจร (oh-my-posh + theme + profile) |
| `context/docs/oh-my-posh-installer.md` | เอกสารรายละเอียด |

### ไฟล์ที่แก้ไข
| ไฟล์ | การเปลี่ยนแปลง |
|------|----------------|
| `DevToolInstaller.csproj` | เพิ่ม `<Content>` element สำหรับ bundle `config/` folder |
| `context/docs/INDEX.md` | เพิ่มลิงก์ไปยังเอกสาร |
| `context/dev-tool-installer/INDEX.md` | เพิ่ม journal entry |

### ไม่ต้องแก้ไข
- `ToolRegistry.cs` - มี `new OhMyPoshInstaller()` อยู่แล้วที่บรรทัด 25

## การตัดสินใจ

1. **รวม 3 ฟีเจอร์ใน installer เดียว** - แทนที่จะแยก OhMyPosh / Profile เป็น 2 installer เพราะมัน tightly coupled กัน
2. **Bundle theme file** - ใช้ `config/` directory แทนที่จะ hardcode ใน C# เพื่อให้แก้ไข theme ได้ง่าย
3. **Preserve existing profile** - ถ้ามี profile อยู่แล้ว จะรักษาบรรทัดที่ไม่เกี่ยวกับ OMP/PSReadLine
4. **Fallback theme** - ถ้าไม่พบ bundled file จะใช้ built-in `$env:POSH_THEMES_PATH\paradox.omp.json`
5. **ลบ JSON comments** - ไฟล์ต้นฉบับมี `//` comments ซึ่งไม่ถูกต้องตาม JSON spec จึงลบออก

## หมายเหตุ

- ไม่สามารถ build ได้เพราะเครื่องพัฒนาไม่มี .NET 10 SDK (มีแค่ 8.0.418)
- `ProcessHelper.GetCommandOutput()` เป็น async method จึงต้องใช้ `await`