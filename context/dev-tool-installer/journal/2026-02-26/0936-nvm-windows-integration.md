# Journal: nvm-windows integration for Node.js installers

- **Date**: 2026-02-26
- **Time**: 09:36 (Asia/Bangkok)
- **Task**: เปลี่ยนกลไกติดตั้ง Node.js จาก MSI ไปเป็น nvm-windows และปรับเอกสารประกอบ

## Summary of Changes

วันนี้มีการปรับโครงสร้างการติดตั้งในหมวด Node.js ให้ใช้ nvm-windows เป็นฐานในการจัดการเวอร์ชัน Node.js แทนการดาวน์โหลดและติดตั้ง MSI โดยตรง เพื่อให้รองรับการสลับหลายเวอร์ชันได้สะดวกขึ้น

## Implemented Code Changes

1. **สร้าง `NvmWindowsInstaller.cs`**
   - เพิ่ม installer สำหรับติดตั้ง nvm-windows ผ่าน winget
   - ใช้แพ็กเกจ `CoreyButler.NVMforWindows`
   - เวอร์ชันที่กำหนด: `1.2.2`

2. **แก้ `NodeJs20Installer.cs` จาก MSI เป็น nvm**
   - เปลี่ยน flow จากการติดตั้งไฟล์ MSI ไปเป็นคำสั่ง nvm
   - ใช้คำสั่ง:
     - `nvm install 20.19.6`
     - `nvm use 20.19.6`

3. **แก้ `NodeJsInstaller.cs` จาก MSI เป็น nvm**
   - ปรับการติดตั้งหลักของ Node.js ให้ใช้ nvm เช่นเดียวกับตัวเฉพาะเวอร์ชัน
   - ใช้คำสั่ง:
     - `nvm install 20.19.6`
     - `nvm use 20.19.6`

4. **แก้ `ToolRegistry.cs`**
   - ถอด `NodeJs22Installer` ออกจาก ToolRegistry
   - เพิ่ม `NvmWindowsInstaller` เข้า registry เพื่อให้เป็น dependency ต้นทางของสาย Node.js
   - หมายเหตุ: ไฟล์ `NodeJs22Installer.cs` ยังอยู่ในโปรเจกต์ แต่ไม่ถูกใช้งานใน registration flow ปัจจุบัน

5. **แก้ dependency ของ installers ที่เกี่ยวข้อง**
   - ปรับ `NpmInstaller` ให้ dependency เป็น **Node.js 20**
   - ปรับ `NodeJsToolsInstaller` ให้ dependency เป็น **Node.js 20**

## Selected Runtime Version

- **Node.js Version**: `20.19.6`
- **Architecture**: `64-bit`

## Reasoning / Decision

เลือกใช้ nvm-windows เพื่อให้จัดการหลายเวอร์ชันของ Node.js ได้ยืดหยุ่นกว่าเดิม ลดภาระการผูกกับ MSI เวอร์ชันเดียว และทำให้การอัปเกรด/สลับเวอร์ชันในอนาคตทำได้ง่ายขึ้น โดยยังคงชัดเจนเรื่อง dependency chain ภายในระบบ installer.