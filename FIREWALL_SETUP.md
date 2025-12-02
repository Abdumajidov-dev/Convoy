# Firewall Setup - Port 5084 ochish

## 🔥 Muammo
Flutter device'dan API'ga ulanolmayapsiz chunki Windows Firewall port 5084'ni bloklayapti.

## ✅ Yechim: Firewall Sozlash

### Option 1: Automatic Setup (RECOMMENDED)

**1. Admin sifatida ishga tushiring:**
- `setup-firewall.bat` faylini o'ng tugma bilan bosing
- "Run as administrator" ni tanlang
- UAC prompt'da "Yes" bosing

**2. Script avtomatik bajaradi:**
- Firewall rule qo'shadi
- Port 5084'ni ochadi
- Natijani ko'rsatadi

### Option 2: PowerShell (Manual)

**1. PowerShell'ni Admin sifatida oching:**
- Start Menu > PowerShell
- O'ng tugma > Run as administrator

**2. Quyidagi buyruqni kiriting:**
```powershell
New-NetFirewallRule -DisplayName "Convoy API Port 5084" -Direction Inbound -Protocol TCP -LocalPort 5084 -Action Allow
```

**3. Tasdiqlash:**
```powershell
Get-NetFirewallRule -DisplayName "Convoy API Port 5084"
```

### Option 3: Windows Defender Firewall GUI

**1. Windows Security'ni oching:**
- Start Menu > Windows Security
- Yoki: `Win + I` > Privacy & Security > Windows Security

**2. Firewall & network protection:**
- "Advanced settings" ni bosing

**3. Inbound Rules:**
- O'ng panelda "New Rule..." ni bosing
- "Port" ni tanlang > Next
- "TCP" va "Specific local ports: 5084" > Next
- "Allow the connection" > Next
- Domain, Private, Public - hammasini belgilang > Next
- Name: "Convoy API Port 5084" > Finish

## 🧪 Test Qilish

### 1. Firewall rule tekshirish
```powershell
netsh advfirewall firewall show rule name="Convoy API Port 5084"
```

### 2. API'ni ishga tushiring
```bash
dotnet run --launch-profile http
```

### 3. Browser'dan test qiling
```
http://10.100.104.128:5084/swagger
```

### 4. Telefoningizdan test qiling
**Flutter app'da yoki Browser'da:**
```
http://10.100.104.128:5084/api/user
```

## 🔍 Port Tekshirish

### Port ochiqligini tekshirish:
```powershell
# PowerShell
Test-NetConnection -ComputerName 10.100.104.128 -Port 5084
```

```bash
# CMD yoki Git Bash
curl http://10.100.104.128:5084/api/user
```

### Telefon'dan test (Browser):
```
http://10.100.104.128:5084/swagger
```

## 🐛 Troubleshooting

### Problem 1: "Access Denied"
**Sabab:** Admin huquqlari yo'q

**Yechim:**
- PowerShell yoki CMD'ni "Run as administrator" bilan oching
- `setup-firewall.bat`ni o'ng tugma > Run as administrator

### Problem 2: Rule qo'shilmadi
**Sabab:** Antivirus bloklayapti

**Yechim:**
- Antivirus settings'ga kiring
- Firewall/Network protection'ni vaqtincha o'chiring
- Rule qo'shgandan keyin yoqing

### Problem 3: Hali ham ulanmayapti
**Tekshirish:**

1. **API ishga tushganmi?**
```bash
dotnet run --launch-profile http
```

2. **Port to'g'rimi?**
```
Should see: "Now listening on: http://0.0.0.0:5084"
```

3. **Firewall rule bormi?**
```powershell
netsh advfirewall firewall show rule name="Convoy API Port 5084"
```

4. **Bir xil network'damisiz?**
```bash
# Kompyuter IP
ipconfig

# Telefon Settings > WiFi > IP address
# Ikkalasi ham bir xil subnet'da bo'lishi kerak (masalan: 10.100.104.x)
```

### Problem 4: Public WiFi
Ba'zi public WiFi'lar device'lar orasida kommunikatsiyani bloklaydi (AP Isolation).

**Yechim:**
- Private/home WiFi ishlating
- Yoki telefon hotspot'idan foydalaning

## 📋 Firewall Rule Details

Qo'shilgan rule:
- **Name:** Convoy API Port 5084
- **Direction:** Inbound (kiruvchi)
- **Action:** Allow
- **Protocol:** TCP
- **Port:** 5084
- **Profile:** Domain, Private, Public (hamma)

## 🗑️ Firewall Rule O'chirish

Agar test tugagandan keyin rule'ni o'chirmoqchi bo'lsangiz:

### PowerShell:
```powershell
Remove-NetFirewallRule -DisplayName "Convoy API Port 5084"
```

### CMD:
```bash
netsh advfirewall firewall delete rule name="Convoy API Port 5084"
```

### GUI:
1. Windows Defender Firewall > Advanced settings
2. Inbound Rules
3. "Convoy API Port 5084" ni toping
4. O'ng tugma > Delete

## ✅ Success Checklist

- [ ] `setup-firewall.bat` Admin sifatida ishga tushirildi
- [ ] Firewall rule muvaffaqiyatli qo'shildi
- [ ] API `dotnet run --launch-profile http` bilan ishga tushdi
- [ ] Browser'da `http://10.100.104.128:5084/swagger` ochildi
- [ ] Telefon bir xil WiFi'da
- [ ] Flutter'dan `http://10.100.104.128:5084/api/user` ishlayapti

## 🎓 Qo'shimcha

### Multiple Network Interfaces
Agar kompyuteringizda bir nechta network interface bo'lsa (WiFi + Ethernet), to'g'ri IP'ni ishlating:

```bash
ipconfig

# WiFi adapter'ni toping va IPv4 Address'ni oling
# Masalan: 10.100.104.128
```

### Mobile Hotspot
Agar telefon hotspot'idan foydalanadigan bo'lsangiz:
1. Telefon'da hotspot yoqing
2. Kompyuter'ni hotspot'ga ulang
3. Kompyuter IP'sini oling: `ipconfig`
4. Flutter'da bu IP'ni ishlating

## 📞 Help

Hali ham muammo bo'lsa:
1. `setup-firewall.bat` log'larini yuboring
2. `ipconfig` natijasini tekshiring
3. Antivirus holatingizni tekshiring
4. Network type'ini tekshiring (Private/Public)

---

**Firewall sozlangandan keyin Flutter ishlaydi!** ✅
