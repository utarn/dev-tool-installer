# Node.js Development Tool Installers

เอกสารนี้อธิบายการทำงานของตัวติดตั้งในหมวด Node.js ของแอปพลิเคชัน DevToolInstaller หลังการเปลี่ยนแนวทางจากการติดตั้ง Node.js แบบ MSI ไปเป็นการใช้งาน nvm-windows

## Overview

ชุดเครื่องมือ Node.js ปัจจุบันประกอบด้วย 4 installer หลัก:

1. **NvmWindowsInstaller** - ติดตั้ง NVM for Windows
2. **NodeJsInstaller** - ติดตั้งและสลับไปใช้ Node.js 20.19.6 (64-bit) ผ่าน nvm
3. **NpmInstaller** - อัปเดต npm ให้เป็นเวอร์ชันล่าสุด
4. **NodeJsToolsInstaller** - ติดตั้งเครื่องมือพัฒนา Node.js แบบ global

> หมายเหตุ: **NodeJs22Installer** ถูกถอดออกจากการลงทะเบียนใน ToolRegistry แล้ว (ไม่ได้ถูกเรียกใช้งานใน flow หลัก) แต่ไฟล์โค้ดยังคงอยู่ในโปรเจกต์

## Dependency Chain

Dependency chain ใหม่สำหรับหมวด Node.js คือ:

**NVM for Windows → Node.js 20 → NPM / Node.js Tools**

รายละเอียด dependency:
- **NvmWindowsInstaller**: ไม่มี dependency
- **NodeJsInstaller**: ขึ้นกับ NVM for Windows
- **NpmInstaller**: ขึ้นกับ Node.js 20
- **NodeJsToolsInstaller**: ขึ้นกับ Node.js 20

## NvmWindowsInstaller

### Purpose
ติดตั้ง **nvm-windows v1.2.2** เพื่อใช้จัดการหลายเวอร์ชันของ Node.js บน Windows

### Details
- **Package Source**: winget
- **Package ID**: `CoreyButler.NVMforWindows`
- **Target Version**: `1.2.2`
- **Installation Method**: ติดตั้งผ่าน winget แบบ silent/non-interactive
- **Category**: NodeJS
- **Dependencies**: None

### Installation Process
1. ตรวจสอบว่า `nvm` ถูกติดตั้งแล้วหรือไม่
2. ติดตั้งผ่านคำสั่ง winget โดยใช้แพ็กเกจ `CoreyButler.NVMforWindows` เวอร์ชัน `1.2.2`
3. ตรวจสอบความพร้อมใช้งานของคำสั่ง `nvm`

## NodeJsInstaller

### Purpose
ติดตั้ง Node.js runtime เวอร์ชันที่กำหนดสำหรับโปรเจกต์ โดยใช้ nvm แทน MSI installer

### Details
- **Target Node Version**: `20.19.6` (64-bit)
- **Installation Method**: nvm commands
- **Category**: NodeJS
- **Dependencies**: NVM for Windows (NvmWindowsInstaller)
- **Commands Used**:
  - `nvm install 20.19.6`
  - `nvm use 20.19.6`

### Installation Process
1. เรียก `nvm install 20.19.6` เพื่อติดตั้ง Node.js เวอร์ชันที่ต้องการ
2. เรียก `nvm use 20.19.6` เพื่อสลับ active version
3. ตรวจสอบการติดตั้งด้วยคำสั่ง `node --version`

## NodeJs20Installer

### Purpose
ติดตั้ง Node.js 20 สำหรับ workflow ที่อ้างอิง installer เฉพาะเวอร์ชัน

### Details
- **Target Node Version**: `20.19.6` (64-bit)
- **Installation Method**: nvm commands
- **Category**: NodeJS
- **Dependencies**: NVM for Windows (NvmWindowsInstaller)
- **Commands Used**:
  - `nvm install 20.19.6`
  - `nvm use 20.19.6`

### Installation Process
1. เรียก `nvm install 20.19.6`
2. เรียก `nvm use 20.19.6`
3. ตรวจสอบผลด้วย `node --version`

## NpmInstaller

### Purpose
ตรวจสอบและอัปเดต npm (Node Package Manager) ให้พร้อมใช้งาน

### Details
- **Installation Method**: ใช้ npm อัปเดตตัวเอง
- **Category**: NodeJS
- **Dependencies**: Node.js 20
- **Features**:
  - ตรวจสอบการมีอยู่ของ npm
  - อัปเดต npm ผ่าน `npm install -g npm@latest`
  - ตรวจสอบด้วย `npm --version`

### Installation Process
1. ตรวจสอบว่า npm พร้อมใช้งานแล้วหรือไม่
2. รัน `npm install -g npm@latest`
3. ตรวจสอบผลการติดตั้ง

## NodeJsToolsInstaller

### Purpose
ติดตั้งเครื่องมือพัฒนาที่ใช้บ่อยสำหรับงาน Node.js

### Details
- **Installation Method**: ใช้ npm ติดตั้ง global packages
- **Category**: NodeJS
- **Dependencies**: Node.js 20
- **Tools Installed**:
  - **nodemon**: รีสตาร์ตแอป Node.js อัตโนมัติเมื่อไฟล์เปลี่ยน
  - **express-generator**: สร้างโครง Express.js
  - **typescript**: TypeScript compiler
  - **ts-node**: รัน TypeScript ได้โดยตรง

### Installation Process
1. ตรวจสอบแต่ละแพ็กเกจว่าเคยติดตั้งแล้วหรือไม่
2. ติดตั้งแพ็กเกจที่ยังขาดด้วย `npm install -g <package-name>`
3. รายงานสถานะแต่ละขั้นตอนจนจบ

## Usage in Application

Installers เหล่านี้ถูกลงทะเบียนใน ToolRegistry และเข้าถึงได้ผ่าน:
- เมนูหมวด "Node.js Development"
- การเลือกติดตั้งรายเครื่องมือ
- กลไก dependency-aware installation ตาม chain ใหม่:
  - NVM for Windows ก่อน
  - ตามด้วย Node.js 20
  - แล้วจึง npm และ Node.js tools

## Error Handling and Verification

Installer ในกลุ่มนี้มีการจัดการข้อผิดพลาดและการตรวจสอบผลลัพธ์อย่างครอบคลุม:
- ตรวจสอบเครื่องมือก่อนติดตั้งซ้ำ
- จัดการความผิดพลาดจาก package manager/command execution
- ตรวจสอบคำสั่งสำคัญหลังติดตั้ง (`nvm`, `node`, `npm`)
- รองรับการรายงานสถานะแบบละเอียดเพื่อการดีบัก